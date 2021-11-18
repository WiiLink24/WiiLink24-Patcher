#include <gccore.h>
#include <malloc.h>
#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>
#include <wiiuse/wpad.h>
#include <sdcard/wiisd_io.h>
#include <fat.h>

// IOS patches
extern "C" {
#include <wiisocket.h>
#include <libpatcher/libpatcher.h>
}

// GUI
#include "gui/gui.h"
#include "menu.h"
#include "noto_sans_jp_regular_otf.h"


#define TITLE_ID(x, y) (((u64)(x) << 32) | (y))

static void power_cb(void) {
    StopGX();
    STM_ShutdownToIdle();
}

static void reset_cb(u32 irq, void *ctx) {
    StopGX();
    STM_RebootSystem();
}

void ExitApp() {
    StopGX();
    WII_ReturnToMenu();
}

void ExitLULZ() {
    StopGX();
    WII_LaunchTitle(HBC_LULZ);
}

void ExitOHBC() {
    StopGX();
    WII_LaunchTitle(HBC_OHBC);
}

const DISC_INTERFACE *sd_slot = &__io_wiisd;
const DISC_INTERFACE *usb = &__io_usbstorage;

void Init_IO() {
    // Initialize IO
    usb->startup();
    sd_slot->startup();

    // Check if the SD Card is inserted
    bool isInserted = __io_wiisd.isInserted();

    // Try to mount the SD Card before the USB
    if (isInserted) {
        fatMountSimple("fat", sd_slot);
    } else {
        // Since the SD Card is not inserted, we will attempt to mount the USB.
        bool USB = __io_usbstorage.isInserted();
        if (USB) {
            fatMountSimple("fat", usb);
        } else {
            // No input devices were inserted OR it failed to mount either
            // device.
            printf("Please insert either an SD Card or USB.\n");
            sleep(5);
            WII_ReturnToMenu();
        }
    }
}


int main(void) {
    // Make hardware buttons functional.
    SYS_SetPowerCallback(power_cb);
    SYS_SetResetCallback(reset_cb);

    InitVideo();

    bool success = apply_patches();
    if (!success) {
        printf("Failed to apply patches!\n");
        sleep(5);
        WII_ReturnToMenu();
    }

    ISFS_Initialize();
    CONF_Init();
    Init_IO();
    wiisocket_init();
    SetupPads();
    InitFreeType((u8 *)noto_sans_jp_regular_otf, noto_sans_jp_regular_otf_size);
    InitGUIThreads();


    MainMenu(1);

    return 0;
}
