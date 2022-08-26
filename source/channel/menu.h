/****************************************************************************
 * libwiigui Template
 * Tantric 2009
 *
 * menu.h
 * Menu flow routines - handles all menu logic
 ***************************************************************************/

#ifndef _MENU_H_
#define _MENU_H_

#include <ogcsys.h>

void InitGUIThreads();
void MainMenu(int menuitem);

enum {
    MENU_EXIT = -1,
    MENU_NONE,
    MENU_PRIMARY,
    MENU_CREDITS,
    MENU_ROOM_LANGUAGE,
    MENU_DIGICAM_LANGUAGE,
    DOWNLOAD_WAD
};

enum {
    STATE_NONE = 0,
    STATE_INSTALL,
    STATE_DOLPHIN_MESSAGE,
    STATE_FINISHED,
    STATE_INTERNET_ERROR,
    STATE_INSTALL_ERROR,
    STATE_UNINSTALL
};

// I swear to god nobody better have anything other than these
#define HBC_OHBC 0x000100014f484243ULL
#define HBC_LULZ 0x000100014c554c5aULL

#endif
