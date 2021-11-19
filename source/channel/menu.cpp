/****************************************************************************
 * libwiigui Template
 * Tantric 2009
 *
 * menu.cpp
 * Menu flow routines - handles all menu logic
 ***************************************************************************/

#include <unistd.h>

#include "gui/gui.h"
#include <string>
#include <curl/curl.h>
#include <fat.h>
#include <fstream>
#include <iostream>
#include "gui/gettext.h"
#include <gccore.h>
#include "download.h"
#include "main.h"
#include "menu.h"

extern "C" {
    #include "wad/wad.h"
}


#define THREAD_SLEEP 100
// 48 KiB was chosen after many days of testing.
// It horrifies the author.
#define GUI_STACK_SIZE 48 * 1024
#define _(string) gettext (string)

static GuiImageData *pointer[4];
static GuiImage *bgImg = NULL;
static GuiWindow *mainWindow = NULL;
static lwp_t guithread = LWP_THREAD_NULL;
static bool guiHalt = true;
static bool ExitRequested = false;
static int RegionCode = 0;
static int LanguageCode = 0;
static bool isRoom = false;
static bool isDigicam = false;

tm *getTime() {
    time_t rawtime;
    struct tm *timeinfo;

    time ( &rawtime );
    timeinfo = localtime(&rawtime);

    return timeinfo;
}

/****************************************************************************
 * ResumeGui
 *
 * Signals the GUI thread to start, and resumes the thread. This is called
 * after finishing the removal/insertion of new elements, and after initial
 * GUI setup.
 ***************************************************************************/
static void ResumeGui() {
    guiHalt = false;
    LWP_ResumeThread(guithread);
}

/****************************************************************************
 * HaltGui
 *
 * Signals the GUI thread to stop, and waits for GUI thread to stop
 * This is necessary whenever removing/inserting new elements into the GUI.
 * This eliminates the possibility that the GUI is in the middle of accessing
 * an element that is being changed.
 ***************************************************************************/
static void HaltGui() {
    guiHalt = true;

    // wait for thread to finish
    while (!LWP_ThreadIsSuspended(guithread))
        usleep(THREAD_SLEEP);
}

/****************************************************************************
 * WindowPrompt
 *
 * Displays a prompt window to user, with information, an error message, or
 * presenting a user with a choice
 ***************************************************************************/
int WindowPrompt(const char *title, const char *msg, const char *btn1Label,
                 const char *btn2Label) {
    int choice = -1;

    GuiWindow promptWindow(448, 288);
    promptWindow.SetAlignment(ALIGN_CENTRE, ALIGN_MIDDLE);
    promptWindow.SetPosition(0, -10);
    GuiImageData btnOutline(button_png);
    GuiImageData btnOutlineOver(button_over_png);
    GuiTrigger trigA;
    trigA.SetSimpleTrigger(-1, WPAD_BUTTON_A | WPAD_CLASSIC_BUTTON_A,
                           PAD_BUTTON_A);

    GuiImageData dialogBox(dialogue_box_png);
    GuiImage dialogBoxImg(&dialogBox);

    GuiText titleTxt(title, 26, (GXColor){0, 0, 0, 255});
    titleTxt.SetAlignment(ALIGN_CENTRE, ALIGN_TOP);
    titleTxt.SetPosition(0, 40);
    GuiText msgTxt(msg, 22, (GXColor){0, 0, 0, 255});
    msgTxt.SetAlignment(ALIGN_CENTRE, ALIGN_MIDDLE);
    msgTxt.SetPosition(0, -20);
    msgTxt.SetWrap(true, 400);

    GuiText btn1Txt(btn1Label, 22, (GXColor){0, 0, 0, 255});
    GuiImage btn1Img(&btnOutline);
    GuiImage btn1ImgOver(&btnOutlineOver);
    GuiButton btn1(btnOutline.GetWidth(), btnOutline.GetHeight());

    if (btn2Label) {
        btn1.SetAlignment(ALIGN_LEFT, ALIGN_BOTTOM);
        btn1.SetPosition(20, -25);
    } else {
        btn1.SetAlignment(ALIGN_CENTRE, ALIGN_BOTTOM);
        btn1.SetPosition(0, -25);
    }

    btn1.SetLabel(&btn1Txt);
    btn1.SetImage(&btn1Img);
    btn1.SetImageOver(&btn1ImgOver);
    btn1.SetTrigger(&trigA);
    btn1.SetState(STATE_SELECTED);
    btn1.SetEffectGrow();

    GuiText btn2Txt(btn2Label, 22, (GXColor){0, 0, 0, 255});
    GuiImage btn2Img(&btnOutline);
    GuiImage btn2ImgOver(&btnOutlineOver);
    GuiButton btn2(btnOutline.GetWidth(), btnOutline.GetHeight());
    btn2.SetAlignment(ALIGN_RIGHT, ALIGN_BOTTOM);
    btn2.SetPosition(-20, -25);
    btn2.SetLabel(&btn2Txt);
    btn2.SetImage(&btn2Img);
    btn2.SetImageOver(&btn2ImgOver);
    btn2.SetTrigger(&trigA);
    btn2.SetEffectGrow();

    promptWindow.Append(&dialogBoxImg);
    promptWindow.Append(&titleTxt);
    promptWindow.Append(&msgTxt);
    promptWindow.Append(&btn1);

    if (btn2Label)
        promptWindow.Append(&btn2);

    promptWindow.SetEffect(EFFECT_SLIDE_TOP | EFFECT_SLIDE_IN, 50);
    HaltGui();
    mainWindow->SetState(STATE_DISABLED);
    mainWindow->Append(&promptWindow);
    mainWindow->ChangeFocus(&promptWindow);
    ResumeGui();

    while (choice == -1) {
        usleep(THREAD_SLEEP);

        if (btn1.GetState() == STATE_CLICKED)
            choice = 1;
        else if (btn2.GetState() == STATE_CLICKED)
            choice = 0;
    }

    promptWindow.SetEffect(EFFECT_SLIDE_TOP | EFFECT_SLIDE_OUT, 50);
    while (promptWindow.GetEffect() > 0)
        usleep(THREAD_SLEEP);
    HaltGui();
    mainWindow->Remove(&promptWindow);
    mainWindow->SetState(STATE_DEFAULT);
    ResumeGui();
    return choice;
}

/****************************************************************************
 * UpdateGUI
 *
 * Primary thread to allow GUI to respond to state changes, and draws GUI
 ***************************************************************************/

static void *UpdateGUI(void *arg) {
    int i;

    while (1) {
        if (guiHalt) {
            LWP_SuspendThread(guithread);
        } else {
            UpdatePads();
            mainWindow->Draw();

            // so that player 1's cursor appears on top!
            for (i = 3; i >= 0; i--) {
                if (userInput[i].wpad->ir.valid)
                    Menu_DrawImg(userInput[i].wpad->ir.x - 48,
                                 userInput[i].wpad->ir.y - 48, 96, 96,
                                 pointer[i]->GetImage(),
                                 userInput[i].wpad->ir.angle, 1, 1, 255);
            }

            Menu_Render();

            for (i = 0; i < 4; i++)
                mainWindow->Update(&userInput[i]);

            if (ExitRequested) {
                for (i = 0; i <= 255; i += 15) {
                    mainWindow->Draw();
                    Menu_DrawRectangle(0, 0, screenwidth, screenheight,
                                       (GXColor){0, 0, 0, (u8)i}, 1);
                    Menu_Render();
                }
                ExitApp();
            }
        }
    }
    return NULL;
}

/****************************************************************************
 * InitGUIThread
 *
 * Startup GUI threads
 ***************************************************************************/
static u8 *_gui_stack[GUI_STACK_SIZE] ATTRIBUTE_ALIGN(8);
void InitGUIThreads() {
    LWP_CreateThread(&guithread, UpdateGUI, NULL, _gui_stack, GUI_STACK_SIZE,
                     70);
}


/****************************************************************************
 * Credits
 ***************************************************************************/
static int MenuCredits() {

    int menu = MENU_NONE;

    GuiImageData btnOutline(button_png);
    GuiImageData btnOutlineOver(button_over_png);
    GuiImageData btnLargeOutline(button_large_png);
    GuiImageData btnLargeOutlineOver(button_large_over_png);

    GuiTrigger trigA;
    trigA.SetSimpleTrigger(-1, WPAD_BUTTON_A | WPAD_CLASSIC_BUTTON_A,
                           PAD_BUTTON_A);

    GuiText titleTxt(_("Credits"), 28, (GXColor){255, 255, 255, 255});
    titleTxt.SetAlignment(ALIGN_CENTRE, ALIGN_TOP);
    titleTxt.SetPosition(0, 25);

    GuiText nameTxt1("-SketchMaster2001", 28, (GXColor){255, 255, 255, 255});
    nameTxt1.SetPosition(0, 100);
    nameTxt1.SetAlignment(ALIGN_CENTRE, ALIGN_TOP);

    GuiText saveBtnTxt(_("Back"), 22, (GXColor){0, 0, 0, 255});
    GuiImage saveBtnImg(&btnOutline);
    GuiImage saveBtnImgOver(&btnOutlineOver);
    GuiButton saveBtn(btnOutline.GetWidth(), btnOutline.GetHeight());
    saveBtn.SetAlignment(ALIGN_CENTRE, ALIGN_BOTTOM);
    saveBtn.SetPosition(0, -15);
    saveBtn.SetLabel(&saveBtnTxt);
    saveBtn.SetImage(&saveBtnImg);
    saveBtn.SetImageOver(&saveBtnImgOver);
    saveBtn.SetTrigger(&trigA);
    saveBtn.SetEffectGrow();

    GuiImage *logo = new GuiImage(new GuiImageData(logo_png));
    logo->SetAlignment(ALIGN_CENTRE, ALIGN_BOTTOM);
    logo->SetPosition(0, -150);

    HaltGui();
    GuiWindow w(screenwidth, screenheight);
    mainWindow->Append(logo);
    mainWindow->Append(&w);
    mainWindow->Append(&titleTxt);

    w.Append(&nameTxt1);
    w.Append(&saveBtn);
    ResumeGui();

    while (menu == MENU_NONE) {
        usleep(THREAD_SLEEP);

        if (saveBtn.GetState() == STATE_CLICKED) {
            menu = MENU_PRIMARY;
        }
    }

    HaltGui();

    mainWindow->Remove(&w);
    mainWindow->Remove(&titleTxt);
    mainWindow->Remove(logo);
    return menu;
}

/****************************************************************************
 * WiiRoomLangs
 ***************************************************************************/
 static int WiiRoomLangs() {
     isRoom = true;
    int menu = MENU_NONE;

    GuiText titleTxt(_("Select your language"), 28, (GXColor){255, 255, 255, 255});
    titleTxt.SetAlignment(ALIGN_CENTRE, ALIGN_TOP);
    titleTxt.SetPosition(0, 25);

    GuiImageData btnOutline(button_png);
    GuiImageData btnOutlineOver(button_over_png);
    GuiImageData btnLargeOutline(button_large_png);
    GuiImageData btnLargeOutlineOver(button_large_over_png);

    GuiTrigger trigA;
    trigA.SetSimpleTrigger(-1, WPAD_BUTTON_A | WPAD_CLASSIC_BUTTON_A,
                           PAD_BUTTON_A);
    GuiTrigger trigHome;
    trigHome.SetButtonOnlyTrigger(
            -1, WPAD_BUTTON_HOME | WPAD_CLASSIC_BUTTON_HOME, 0);

    GuiText englishBtnTxt(_("English"), 22, (GXColor){0, 0, 0, 255});
    englishBtnTxt.SetWrap(true, btnLargeOutline.GetWidth() - 30);
    GuiImage englishBtnImg(&btnLargeOutline);
    GuiImage englishBtnImgOver(&btnLargeOutlineOver);
    GuiButton englishBtn(btnLargeOutline.GetWidth(),
                         btnLargeOutline.GetHeight());
    englishBtn.SetAlignment(ALIGN_CENTRE, ALIGN_TOP);
    englishBtn.SetPosition(-125, 120);
    englishBtn.SetLabel(&englishBtnTxt);
    englishBtn.SetImage(&englishBtnImg);
    englishBtn.SetImageOver(&englishBtnImgOver);
    englishBtn.SetTrigger(&trigA);
    englishBtn.SetEffectGrow();

    GuiText jpnBtnTxt(_("Japanese"), 22, (GXColor){0, 0, 0, 255});
    jpnBtnTxt.SetWrap(true, btnLargeOutline.GetWidth() - 30);
    GuiImage jpnBtnImg(&btnLargeOutline);
    GuiImage jpnImgOver(&btnLargeOutlineOver);
    GuiButton jpnBtn(btnLargeOutline.GetWidth(),
                     btnLargeOutline.GetHeight());
    jpnBtn.SetAlignment(ALIGN_CENTRE, ALIGN_TOP);
    jpnBtn.SetPosition(125, 120);
    jpnBtn.SetLabel(&jpnBtnTxt);
    jpnBtn.SetImage(&jpnBtnImg);
    jpnBtn.SetImageOver(&jpnImgOver);
    jpnBtn.SetTrigger(&trigA);
    jpnBtn.SetEffectGrow();

    GuiText backBtnTxt(_("Back"), 22, (GXColor){0, 0, 0, 255});
    GuiImage backBtnImg(&btnOutline);
    GuiImage backBtnImgOver(&btnOutlineOver);
    GuiButton backBtn(btnOutline.GetWidth(), btnOutline.GetHeight());
    backBtn.SetAlignment(ALIGN_LEFT, ALIGN_BOTTOM);
    backBtn.SetPosition(220, -15);
    backBtn.SetLabel(&backBtnTxt);
    backBtn.SetImage(&backBtnImg);
    backBtn.SetImageOver(&backBtnImg);
    backBtn.SetTrigger(&trigA);
    backBtn.SetEffectGrow();

    HaltGui();
    GuiWindow w(screenwidth, screenheight);
    w.Append(&titleTxt);
    w.Append(&englishBtn);
    w.Append(&jpnBtn);

    w.Append(&backBtn);
    mainWindow->Append(&w);

    ResumeGui();

    ResumeGui();

    while (menu == MENU_NONE) {
        usleep(THREAD_SLEEP);

        if (backBtn.GetState() == STATE_CLICKED) {
            isRoom = false;
            menu = MENU_PRIMARY;
        } else if (englishBtn.GetState() == STATE_CLICKED) {
            LanguageCode = 1;
            menu = DOWNLOAD_WAD;
        } else if (jpnBtn.GetState() == STATE_CLICKED) {
            LanguageCode = 0;
            menu = DOWNLOAD_WAD;
        }
    }

    HaltGui();
    mainWindow->Remove(&w);
    return menu;
 }

 static int DigicamLangs() {
     isRoom = false;
     isDigicam = true;

     int menu = MENU_NONE;

     GuiText titleTxt(_("Select your language"), 28, (GXColor){255, 255, 255, 255});
     titleTxt.SetAlignment(ALIGN_CENTRE, ALIGN_TOP);
     titleTxt.SetPosition(0, 25);

     GuiImageData btnOutline(button_png);
     GuiImageData btnOutlineOver(button_over_png);
     GuiImageData btnLargeOutline(button_large_png);
     GuiImageData btnLargeOutlineOver(button_large_over_png);

     GuiTrigger trigA;
     trigA.SetSimpleTrigger(-1, WPAD_BUTTON_A | WPAD_CLASSIC_BUTTON_A,
                            PAD_BUTTON_A);
     GuiTrigger trigHome;
     trigHome.SetButtonOnlyTrigger(
             -1, WPAD_BUTTON_HOME | WPAD_CLASSIC_BUTTON_HOME, 0);

     GuiText englishBtnTxt(_("English"), 22, (GXColor){0, 0, 0, 255});
     englishBtnTxt.SetWrap(true, btnLargeOutline.GetWidth() - 30);
     GuiImage englishBtnImg(&btnLargeOutline);
     GuiImage englishBtnImgOver(&btnLargeOutlineOver);
     GuiButton englishBtn(btnLargeOutline.GetWidth(),
                          btnLargeOutline.GetHeight());
     englishBtn.SetAlignment(ALIGN_CENTRE, ALIGN_TOP);
     englishBtn.SetPosition(-125, 120);
     englishBtn.SetLabel(&englishBtnTxt);
     englishBtn.SetImage(&englishBtnImg);
     englishBtn.SetImageOver(&englishBtnImgOver);
     englishBtn.SetTrigger(&trigA);
     englishBtn.SetEffectGrow();

     GuiText jpnBtnTxt(_("Japanese"), 22, (GXColor){0, 0, 0, 255});
     jpnBtnTxt.SetWrap(true, btnLargeOutline.GetWidth() - 30);
     GuiImage jpnBtnImg(&btnLargeOutline);
     GuiImage jpnImgOver(&btnLargeOutlineOver);
     GuiButton jpnBtn(btnLargeOutline.GetWidth(),
                         btnLargeOutline.GetHeight());
     jpnBtn.SetAlignment(ALIGN_CENTRE, ALIGN_TOP);
     jpnBtn.SetPosition(125, 120);
     jpnBtn.SetLabel(&jpnBtnTxt);
     jpnBtn.SetImage(&jpnBtnImg);
     jpnBtn.SetImageOver(&jpnImgOver);
     jpnBtn.SetTrigger(&trigA);
     jpnBtn.SetEffectGrow();

     GuiText backBtnTxt(_("Back"), 22, (GXColor){0, 0, 0, 255});
     GuiImage backBtnImg(&btnOutline);
     GuiImage backBtnImgOver(&btnOutlineOver);
     GuiButton backBtn(btnOutline.GetWidth(), btnOutline.GetHeight());
     backBtn.SetAlignment(ALIGN_LEFT, ALIGN_BOTTOM);
     backBtn.SetPosition(220, -15);
     backBtn.SetLabel(&backBtnTxt);
     backBtn.SetImage(&backBtnImg);
     backBtn.SetImageOver(&backBtnImg);
     backBtn.SetTrigger(&trigA);
     backBtn.SetEffectGrow();

     HaltGui();
     GuiWindow w(screenwidth, screenheight);
     w.Append(&titleTxt);
     w.Append(&englishBtn);
     w.Append(&jpnBtn);

     w.Append(&backBtn);
     mainWindow->Append(&w);

     ResumeGui();

     while (menu == MENU_NONE) {
         usleep(THREAD_SLEEP);

         if (backBtn.GetState() == STATE_CLICKED) {
             isDigicam = false;
             menu = MENU_PRIMARY;
         } else if (englishBtn.GetState() == STATE_CLICKED) {
             LanguageCode = 1;
             menu = DOWNLOAD_WAD;
         } else if (jpnBtn.GetState() == STATE_CLICKED) {
             LanguageCode = 0;
             menu = DOWNLOAD_WAD;
         }
     }

     HaltGui();
     mainWindow->Remove(&w);
     return menu;
 }

 static int DownloadWAD(int region, int language) {
     const char *wadFile;
     const char *channelName;
     std::string channelUrl = "https://patcher.wiilink24.com/";

     if (isRoom) {
         wadFile = "fat:/Wii_Room.wad";
         channelName = "Installing Wii Room";
         channelUrl.append(std::to_string(language));
         channelUrl.append("/Wii_Room.wad");
     } else if (isDigicam) {
         wadFile = "fat:/Digicam.wad";
         channelName = "Installing Digicam Print Channel";
         channelUrl.append(std::to_string(language));
         channelUrl.append("/Digicam_Print_Channel.wad");
     } else {
         wadFile = "fat:/SPD.wad";
         channelName = "Installing Set Personal Data";
         channelUrl.append("WiiLink24_SPD.wad");
     }

     int menu = MENU_NONE;

     // We keep a state on the progress of the download
     int state;
     int install_code;
     int attempts = 0;


     GuiWindow promptWindow(448, 288);
     promptWindow.SetAlignment(ALIGN_CENTRE, ALIGN_MIDDLE);
     promptWindow.SetPosition(0, -10);
     GuiImageData btnOutline(button_png);
     GuiImageData btnOutlineOver(button_over_png);
     GuiTrigger trigA;
     trigA.SetSimpleTrigger(-1, WPAD_BUTTON_A | WPAD_CLASSIC_BUTTON_A,
                            PAD_BUTTON_A);

     GuiImageData dialogBox(dialogue_box_png);
     GuiImage dialogBoxImg(&dialogBox);

     GuiText titleTxt(channelName, 26, (GXColor){0, 0, 0, 255});
     titleTxt.SetAlignment(ALIGN_CENTRE, ALIGN_TOP);
     titleTxt.SetPosition(0, 40);

     GuiText msgTxt("Downloading contents...", 22, (GXColor){0, 0, 0, 255});
     msgTxt.SetAlignment(ALIGN_CENTRE, ALIGN_MIDDLE);
     msgTxt.SetPosition(0, -20);
     msgTxt.SetWrap(true, 400);

     GuiText btn1Txt("Return to Menu", 22, (GXColor){0, 0, 0, 255});
     GuiImage btn1Img(&btnOutline);
     GuiImage btn1ImgOver(&btnOutlineOver);
     GuiButton btn1(btnOutline.GetWidth(), btnOutline.GetHeight());
     btn1.SetAlignment(ALIGN_CENTRE, ALIGN_BOTTOM);
     btn1.SetPosition(0, -25);
     btn1.SetLabel(&btn1Txt);
     btn1.SetImage(&btn1Img);
     btn1.SetImageOver(&btn1ImgOver);
     btn1.SetTrigger(&trigA);
     btn1.SetEffectGrow();

     promptWindow.Append(&dialogBoxImg);
     promptWindow.Append(&titleTxt);
     promptWindow.Append(&msgTxt);


     promptWindow.SetEffect(EFFECT_SLIDE_TOP | EFFECT_SLIDE_IN, 50);
     HaltGui();
     mainWindow->SetState(STATE_DISABLED);
     mainWindow->Append(&promptWindow);
     mainWindow->ChangeFocus(&promptWindow);
     ResumeGui();

     // Download WAD
     FILE *fp = fopen(wadFile, "wb");
     CURLcode ret = curl_download(channelUrl.c_str(), fp);
     fclose(fp);
     if (ret != CURLE_OK) {
         // Handle cURL errors
         state = STATE_INTERNET_ERROR;
     } else {
         // Install WAD
         state = STATE_INSTALL;
     }


     while (menu == MENU_NONE) {
         usleep(THREAD_SLEEP);

         if (state == STATE_INSTALL) {
             msgTxt.SetText("Installing WAD...");
             fp = fopen(wadFile, "rb");
             install_code = install_WAD(fp);
             fclose(fp);
             if (install_code != 0) {
                state = STATE_INSTALL_ERROR;
             } else {
                 state = STATE_FINISHED;
             }
         } else if (state == STATE_FINISHED) {
             mainWindow->Remove(&promptWindow);
             promptWindow.Append(&btn1);
             if (isDigicam) {
                 msgTxt.SetText("Finished! "
                                "If you haven't yet, install SPD.");
             } else {
                 msgTxt.SetText("Finished!");
             }
             HaltGui();
             mainWindow->Append(&promptWindow);
             mainWindow->ChangeFocus(&promptWindow);
             ResumeGui();
             // Remove WAD from I/O
             unlink(wadFile);
             state = STATE_NONE;
         } else if (state == STATE_INTERNET_ERROR) {
             // Log our error
             // I am using std::ofstream because fwrite is not writing the full string
             std::ofstream fw("fat:/WiiLink_Patcher_Log.txt", std::ofstream::app);
             tm *timeInfo = getTime();

             fw << asctime(timeInfo) << "An Internet Error has occurred. cURL error code: " << (int)ret << "\n\n";
             fw.close();

             mainWindow->Remove(&promptWindow);
             promptWindow.Append(&btn1);
             titleTxt.SetText("An Internet error has occurred");
             msgTxt.SetText("Please contact WiiLink support and supply the log file found on the root of your SD Card.");
             HaltGui();
             mainWindow->Append(&promptWindow);
             mainWindow->ChangeFocus(&promptWindow);
             ResumeGui();
             state = STATE_NONE;
         } else if (state == STATE_INSTALL_ERROR) {
             // If the install code is -1022, we will try to uninstall the WAD, then reinstall
             if ((install_code == -1022) && (attempts != 1)) {
                 attempts += 1;
                 state = STATE_UNINSTALL;
             } else {
                 std::ofstream fw("fat:/WiiLink_Patcher_Log.txt", std::ofstream::app);
                 tm *timeInfo = getTime();

                 fw << asctime(timeInfo) << "An WAD install error has occurred. /dev/es error code: " << install_code << "\n\n";
                 fw.close();

                 mainWindow->Remove(&promptWindow);
                 promptWindow.Append(&btn1);
                 titleTxt.SetText("A WAD install error has occurred");
                 msgTxt.SetText("Please contact WiiLink support and supply the log file found on the root of your SD Card.");
                 HaltGui();
                 mainWindow->Append(&promptWindow);
                 mainWindow->ChangeFocus(&promptWindow);
                 ResumeGui();
                 state = STATE_NONE;
             }
         } else if (state == STATE_UNINSTALL) {
             titleTxt.SetText("-1022 has occurred");
             msgTxt.SetText("Attempting to uninstall then install...");
             fp = fopen(wadFile, "rb");
             int uninstall_ret = Wad_Uninstall(fp);
             fclose(fp);
             if (uninstall_ret != 0) {
                 // Uninstall failed
                 state = STATE_INSTALL_ERROR;
             } else {
                 // Now install the WAD
                 state = STATE_INSTALL;
             }
         }

         if (btn1.GetState() == STATE_CLICKED) {
             isDigicam = false;
             isRoom = false;
             menu = MENU_PRIMARY;
         }
     }

     promptWindow.SetEffect(EFFECT_SLIDE_TOP | EFFECT_SLIDE_OUT, 50);
     while (promptWindow.GetEffect() > 0)
         usleep(THREAD_SLEEP);
     HaltGui();
     mainWindow->Remove(&promptWindow);
     mainWindow->SetState(STATE_DEFAULT);
     ResumeGui();

     return menu;
 }

/****************************************************************************
 * MenuSettings
 ***************************************************************************/
static int MenuSettings() {
    int menu = MENU_NONE;

    GuiText titleTxt(_("WiiLink Patcher"), 28, (GXColor){255, 255, 255, 255});
    titleTxt.SetAlignment(ALIGN_CENTRE, ALIGN_TOP);
    titleTxt.SetPosition(0, 25);

    GuiImageData btnOutline(button_png);
    GuiImageData btnOutlineOver(button_over_png);
    GuiImageData btnLargeOutline(button_large_png);
    GuiImageData btnLargeOutlineOver(button_large_over_png);

    GuiTrigger trigA;
    trigA.SetSimpleTrigger(-1, WPAD_BUTTON_A | WPAD_CLASSIC_BUTTON_A,
                           PAD_BUTTON_A);
    GuiTrigger trigHome;
    trigHome.SetButtonOnlyTrigger(
        -1, WPAD_BUTTON_HOME | WPAD_CLASSIC_BUTTON_HOME, 0);

    GuiText roomBtnTxt(_("Wii Room"), 22, (GXColor){0, 0, 0, 255});
    roomBtnTxt.SetWrap(true, btnLargeOutline.GetWidth() - 30);
    GuiImage roomBtnImg(&btnLargeOutline);
    GuiImage roomBtnImgOver(&btnLargeOutlineOver);
    GuiButton roomBtn(btnLargeOutline.GetWidth(),
                           btnLargeOutline.GetHeight());
    roomBtn.SetAlignment(ALIGN_CENTRE, ALIGN_TOP);
    roomBtn.SetPosition(-175, 120);
    roomBtn.SetLabel(&roomBtnTxt);
    roomBtn.SetImage(&roomBtnImg);
    roomBtn.SetImageOver(&roomBtnImgOver);
    roomBtn.SetTrigger(&trigA);
    roomBtn.SetEffectGrow();

    GuiText digicamBtnTxt(_("Digicam Print Channel"), 22, (GXColor){0, 0, 0, 255});
    digicamBtnTxt.SetWrap(true, btnLargeOutline.GetWidth() - 30);
    GuiImage digicamImg(&btnLargeOutline);
    GuiImage digicamImgOver(&btnLargeOutlineOver);
    GuiButton digicamBtn(btnLargeOutline.GetWidth(),
                          btnLargeOutline.GetHeight());
    digicamBtn.SetAlignment(ALIGN_CENTRE, ALIGN_TOP);
    digicamBtn.SetPosition(0, 120);
    digicamBtn.SetLabel(&digicamBtnTxt);
    digicamBtn.SetImage(&digicamImg);
    digicamBtn.SetImageOver(&digicamImgOver);
    digicamBtn.SetTrigger(&trigA);
    digicamBtn.SetEffectGrow();

    GuiText spdBtnTxt(_("Set Personal Data"), 22, (GXColor){0, 0, 0, 255});
    spdBtnTxt.SetWrap(true, btnLargeOutline.GetWidth() - 30);
    GuiImage spdImg(&btnLargeOutline);
    GuiImage spdImgOver(&btnLargeOutlineOver);
    GuiButton spdBtn(btnLargeOutline.GetWidth(),
                         btnLargeOutline.GetHeight());
    spdBtn.SetAlignment(ALIGN_CENTRE, ALIGN_TOP);
    spdBtn.SetPosition(175, 120);
    spdBtn.SetLabel(&spdBtnTxt);
    spdBtn.SetImage(&spdImg);
    spdBtn.SetImageOver(&spdImgOver);
    spdBtn.SetTrigger(&trigA);
    spdBtn.SetEffectGrow();


    GuiText creditsBtnTxt(_("Credits"), 22, (GXColor){0, 0, 0, 255});
    GuiImage creditsBtnImg(&btnLargeOutline);
    GuiImage creditsBtnImgOver(&btnLargeOutlineOver);
    GuiButton creditsBtn(btnLargeOutline.GetWidth(),
                         btnLargeOutline.GetHeight());
    creditsBtn.SetAlignment(ALIGN_CENTRE, ALIGN_TOP);
    creditsBtn.SetPosition(0, 250);
    creditsBtn.SetLabel(&creditsBtnTxt);
    creditsBtn.SetImage(&creditsBtnImg);
    creditsBtn.SetImageOver(&creditsBtnImgOver);
    creditsBtn.SetTrigger(&trigA);
    creditsBtn.SetEffectGrow();

    GuiText wiiBtnTxt(_("Wii Menu"), 22, (GXColor){0, 0, 0, 255});
    GuiImage wiiBtnImg(&btnOutline);
    GuiImage wiiBtnImgOver(&btnOutlineOver);
    GuiButton wiiBtn(btnOutline.GetWidth(), btnOutline.GetHeight());
    wiiBtn.SetAlignment(ALIGN_RIGHT, ALIGN_BOTTOM);
    wiiBtn.SetPosition(-100, -15);
    wiiBtn.SetLabel(&wiiBtnTxt);
    wiiBtn.SetImage(&wiiBtnImg);
    wiiBtn.SetImageOver(&wiiBtnImgOver);
    wiiBtn.SetTrigger(&trigA);
    wiiBtn.SetTrigger(&trigHome);
    wiiBtn.SetEffectGrow();

    GuiText hbcBtnTxt(_("HBC"), 22, (GXColor){0, 0, 0, 255});
    GuiImage hbcBtnImg(&btnOutline);
    GuiImage hbcBtnImgOver(&btnOutlineOver);
    GuiButton hbcBtn(btnOutline.GetWidth(), btnOutline.GetHeight());
    hbcBtn.SetAlignment(ALIGN_LEFT, ALIGN_BOTTOM);
    hbcBtn.SetPosition(100, -15);
    hbcBtn.SetLabel(&hbcBtnTxt);
    hbcBtn.SetImage(&hbcBtnImg);
    hbcBtn.SetImageOver(&hbcBtnImgOver);
    hbcBtn.SetTrigger(&trigA);
    hbcBtn.SetEffectGrow();

    HaltGui();
    GuiWindow w(screenwidth, screenheight);
    w.Append(&titleTxt);
    w.Append(&roomBtn);
    w.Append(&digicamBtn);
    w.Append(&spdBtn);
    w.Append(&creditsBtn);

    w.Append(&wiiBtn);
    w.Append(&hbcBtn);

    mainWindow->Append(&w);

    ResumeGui();

    while (menu == MENU_NONE) {
        usleep(THREAD_SLEEP);

        if (creditsBtn.GetState() == STATE_CLICKED) {
            menu = MENU_CREDITS;
        } else if (roomBtn.GetState() == STATE_CLICKED) {
            menu = MENU_ROOM_LANGUAGE;
        } else if (hbcBtn.GetState() == STATE_CLICKED) {
            ExitLULZ();
            ExitOHBC();
        } else if (digicamBtn.GetState() == STATE_CLICKED) {
            menu = MENU_DIGICAM_LANGUAGE;
        } else if (spdBtn.GetState() == STATE_CLICKED) {
            menu = DOWNLOAD_WAD;
        } else if (wiiBtn.GetState() == STATE_CLICKED) {
            ExitApp();
        }
    }

    HaltGui();
    mainWindow->Remove(&w);
    return menu;
}


/****************************************************************************
 * MainMenu
 ***************************************************************************/
void MainMenu(int menu) {
    if (!text_language()) {
        printf("Unable to load language");
        sleep(5);
        ExitApp();
    }
    
    // Set up region code
    switch (CONF_GetRegion()) {
        case CONF_REGION_JP:
            RegionCode = 0;
            break;
        case CONF_REGION_US:
            RegionCode = 1;
            break;
        case CONF_REGION_EU:
            RegionCode = 2;
            break;
        default:
            printf("Unable to get the region");
            break;
    }

    int currentMenu = menu;

    pointer[0] = new GuiImageData(player1_point_png);
    pointer[1] = new GuiImageData(player2_point_png);
    pointer[2] = new GuiImageData(player3_point_png);
    pointer[3] = new GuiImageData(player4_point_png);

    mainWindow = new GuiWindow(screenwidth, screenheight);

    bgImg = new GuiImage(screenwidth, screenheight, (GXColor){0, 0, 0, 255});

    // Create a white stripe beneath the title and above actionable buttons.
    bgImg->ColorStripe(75, (GXColor){0xff, 0xff, 0xff, 255});
    bgImg->ColorStripe(76, (GXColor){0xff, 0xff, 0xff, 255});

    bgImg->ColorStripe(screenheight - 77, (GXColor){0xff, 0xff, 0xff, 255});
    bgImg->ColorStripe(screenheight - 78, (GXColor){0xff, 0xff, 0xff, 255});

    GuiImage *topChannelGradient =
        new GuiImage(new GuiImageData(channel_gradient_top_png));
    topChannelGradient->SetTile(screenwidth / 4);

    GuiImage *bottomChannelGradient =
        new GuiImage(new GuiImageData(channel_gradient_bottom_png));
    bottomChannelGradient->SetTile(screenwidth / 4);
    bottomChannelGradient->SetAlignment(ALIGN_LEFT, ALIGN_BOTTOM);

    mainWindow->Append(bgImg);
    mainWindow->Append(topChannelGradient);
    mainWindow->Append(bottomChannelGradient);

    GuiTrigger trigA;
    trigA.SetSimpleTrigger(-1, WPAD_BUTTON_A | WPAD_CLASSIC_BUTTON_A,
                           PAD_BUTTON_A);

    ResumeGui();


    while (currentMenu != MENU_EXIT) {
        switch (currentMenu) {
            case MENU_CREDITS:
                currentMenu = MenuCredits();
                break;
            case DOWNLOAD_WAD:
                currentMenu = DownloadWAD(RegionCode, LanguageCode);
                break;
            case MENU_ROOM_LANGUAGE:
                currentMenu = WiiRoomLangs();
                break;
            case MENU_DIGICAM_LANGUAGE:
                currentMenu = DigicamLangs();
                break;
            default:
                currentMenu = MenuSettings();
                break;
        }

    }

    ResumeGui();
    ExitRequested = true;
    while (1)
        usleep(THREAD_SLEEP);

    HaltGui();

    delete bgImg;
    delete mainWindow;

    delete pointer[0];
    delete pointer[1];
    delete pointer[2];
    delete pointer[3];

    mainWindow = NULL;
}

