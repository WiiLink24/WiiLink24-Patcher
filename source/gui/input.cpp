/****************************************************************************
 * libwiigui Template
 * Tantric 2009
 *
 * input.cpp
 * Wii/GameCube controller management
 ***************************************************************************/

#include <gccore.h>
#include <math.h>
#include <ogcsys.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <unistd.h>
#include <wiiuse/wpad.h>

#include "gui.h"
#include "input.h"
#include "video.h"

GuiTrigger userInput[4];

/****************************************************************************
 * UpdatePads
 *
 * Scans pad and wpad
 ***************************************************************************/
void UpdatePads() {
    WPAD_ScanPads();
    PAD_ScanPads();

    for (int i = 3; i >= 0; i--) {
        userInput[i].pad.btns_d = PAD_ButtonsDown(i);
        userInput[i].pad.btns_u = PAD_ButtonsUp(i);
        userInput[i].pad.btns_h = PAD_ButtonsHeld(i);
        userInput[i].pad.stickX = PAD_StickX(i);
        userInput[i].pad.stickY = PAD_StickY(i);
        userInput[i].pad.substickX = PAD_SubStickX(i);
        userInput[i].pad.substickY = PAD_SubStickY(i);
        userInput[i].pad.triggerL = PAD_TriggerL(i);
        userInput[i].pad.triggerR = PAD_TriggerR(i);
    }
}

/****************************************************************************
 * SetupPads
 *
 * Sets up userInput triggers for use
 ***************************************************************************/
void SetupPads() {
    PAD_Init();
    WPAD_Init();

    // read wiimote accelerometer and IR data
    WPAD_SetDataFormat(WPAD_CHAN_ALL, WPAD_FMT_BTNS_ACC_IR);
    WPAD_SetVRes(WPAD_CHAN_ALL, screenwidth, screenheight);

    for (int i = 0; i < 4; i++) {
        userInput[i].chan = i;
        userInput[i].wpad = WPAD_Data(i);
    }
}
