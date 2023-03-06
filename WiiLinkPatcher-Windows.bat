setlocal
@echo OFF
chcp 65001 > nul
setlocal EnableDelayedExpansion

:: Links to use
set "WiiLinkPatcherURL=https://patcher.wiilink24.com"
set "PabloURL=http://pabloscorner.akawah.net/WL24-Patcher"

:: Set inital language to English
set prog_language=en

:: ######### Build info #########
set version=1.0.8.2n
set copyright_year=2023

set "last_build_long=March 6, 2023"
set "last_build_short=3/6/2023"
set "at_en=2:12 PM"

title WiiLink Patcher v%version%
:: ##############################

:: New line character
(set \n=^
%=This is Mandatory Space=%
)

:: Windows Compatibility Check
for /f "tokens=2 delims==" %%a in ('wmic os get version /value') do set "OSVersion=%%a"
for /f "tokens=1-2 delims=." %%a in ("%OSVersion%") do (
    set "MajorVersion=%%a"
)
if not %MajorVersion%==10 goto :compatibility_warning

:: Check if WiiLink server is up
curl --silent --head --fail --insecure !WiiLinkPatcherURL!/wiinoma/WiinoMa_1_English.delta > NUL
if %errorlevel% neq 0 goto :server_down

:: Remove temporary folders if they exist
if exist unpack rmdir /s /q unpack
if exist unpack-patched rmdir /s /q unpack-patched
if exist WiiLink_Patcher rmdir /s /q WiiLink_Patcher

:: After all the checks are done, we'll go to the main screen
goto main


:: Github Announcement
:announcement
    if %prog_language%==en (
        echo [1;32m--- Announcement ---[0m
        echo If you have any issues with the patcher or services offered by WiiLink^, please report them here:
        echo [1mhttps://discord.gg/WiiLink[0m - Thank you.
        echo [1;32m--------------------[0m
    )
    echo.
goto :EOF


:: Error screen
:errorserver
    call :header

    :: Set error text based on language
    if %prog_language%==en (
        echo [5;31mAn error has occurred![0m
        echo.
        echo ERROR DETAILS:
        echo.
        echo %TAB%* Task: !task!
        echo %TAB%* Command: !cur_command!
        echo %TAB%* Exit code: %errorlevel%
    )
    echo.

    :: Print help message
    if %prog_language% == en (
        echo Please open an issue on our GitHub page ^(https://github.com/WiiLink24/WiiLink24-Patcher/issues^) and describe the issue
        echo you're having.
    )
    echo.
exit

:: If the user is using an unsupported version of Windows, we'll show them this screen
:compatibility_warning
    call :header no_formatting

    :: Set error text based on language
    if %prog_language%==en (
        echo ERROR: Unsupported version of Windows detected
        echo.
        echo WiiLink Patcher is only officially supported on Windows 10 and 11. If you're
        echo using an older version of Windows, you will need to upgrade, or use the
        echo Unix Patcher on a Linux or macOS machine/VM.
        echo.
        echo Please get it here: https://github.com/WiiLink24/WiiLink24-Patcher
        echo.
        echo Apologies for the inconvenience, but I couldn't find a way to make this work
        echo correctly on older versions of Windows. Not without doing another major rewrite.
        echo.
        echo Press any key to exit...
        pause > nul
    )
exit

:: Server down screen
:server_down
    call :header

    :: Set error text based on language
    if %prog_language%==en (
        echo [5;31mThe WiiLink server is currently down^^![0m
        echo.
        echo It seems that our server is currently down. We're trying to get it back up as soon as possible^^!
        echo.
        echo Stay tuned on our Discord server for updates: 
        echo [1;32mhttps://discord.gg/WiiLink[0m
    )
    echo.
    echo Press any key to exit...
    pause > nul
exit


:: Files cleanup
:files_cleanup
    set "clean_choice=%1"

    set "task=Cleaning up files"
    if !clean_choice!==channel_clean (
        if exist unpack rmdir /s /q unpack
        if exist unpack-patched rmdir /s /q unpack-patched
    )
    if !clean_choice!==setup_clean (
        if exist WiiLink_Patcher rmdir /s /q WiiLink_Patcher

        if exist apps (
            if not %sdcard%==NUL rmdir /s /q apps
        )
        if exist WAD (
            if not %sdcard%==NUL rmdir /s /q WAD
        )
    )
goto :EOF


:: For patching the core Japanese-exclusive channels
:patch_core_channel
    set "title_id=%2"
    set "temp_folder=unpack-patched"
    set "unpack_folder=unpack"
    set "patch_folder=WiiLink_Patcher\%1"
    set "output_wad=!temp_folder!\!title_id!.wad"
    set "channel_name=%~9"

    :: Set URL subdir based on channel name
    if %1==WiinoMa set "url_subdir=wiinoma"
    if %1==Digicam set "url_subdir=digicam"
    if %1==Demae set "url_subdir=demae"
    if %1==Dominos set "url_subdir=dominos"

    :: Create folders
    if not exist !temp_folder! mkdir !temp_folder!
    if not exist !patch_folder! mkdir !patch_folder!
    if not exist !unpack_folder! mkdir !unpack_folder!

    set "task=Downloading and Extracting stuff for !channel_name!"
    set cur_command=WiiLink_Patcher\Sharpii.exe nusd -id !title_id! -o !output_wad! -wad -q
    !cur_command! > NUL
    if %errorlevel% neq 0 goto :error

    set cur_command=WiiLink_Patcher\Sharpii.exe wad -u !output_wad! !unpack_folder! -q
    !cur_command! > NUL
    if %errorlevel% neq 0 goto :error

    :: Download patched TMD
    set cur_command=curl --create-dirs --insecure --insecure -s -o !unpack_folder!\!title_id!.tmd !WiiLinkPatcherURL!/!url_subdir!/%1.tmd
    !cur_command! > NUL
    if %errorlevel% neq 0 goto :error

    set "task=Applying delta patches for !channel_name!"
    if "%reg%"=="EN" set "match=1"
    if "%1"=="dominos" set "match=1"

    :: First delta patch
    if defined match (
        set cur_command=WiiLink_Patcher\xdelta3.exe -q -f -d -s !unpack_folder!\%3.app !patch_folder!\%4.delta !temp_folder!\%3.app
        !cur_command! > NUL
        if %errorlevel% neq 0 goto :error
    )
    set match=0
    
    :: Second delta patch
    set cur_command=WiiLink_Patcher\xdelta3.exe -q -f -d -s !unpack_folder!\%5.app !patch_folder!\%6.delta !temp_folder!\%5.app
    !cur_command! > NUL
    if %errorlevel% neq 0 goto :error
    
    if "%reg%"=="EN" set "match=1"
    if "%1"=="Dominos" set "match=1"
    if "%1"=="WiinoMa" set "match=1"

    :: Third delta patch
    if defined match (
        set cur_command=WiiLink_Patcher\xdelta3.exe -q -f -d -s !unpack_folder!\%7.app !patch_folder!\%8.delta !temp_folder!\%7.app
        !cur_command! > NUL
        if %errorlevel% neq 0 goto :error
    )
    set match=0

    set "task=Moving patched files for !channel_name!"
    set cur_command=move /y !temp_folder!\*.* !unpack_folder!
    !cur_command! > NUL
    if %errorlevel% neq 0 goto :error

    :: Output WAD name (that has spaces in it)
    set "output_wad_name=WAD\!channel_name! (!lang!).wad"

    set "task=Repacking the title for !channel_name!"
    set cur_command=WiiLink_Patcher\Sharpii.exe wad -p !unpack_folder! "!output_wad_name!"
    call WiiLink_Patcher\Sharpii.exe wad -p !unpack_folder! "!output_wad_name!"> NUL
    if %errorlevel% neq 0 goto :error
goto :EOF


:: For patching the WiiConnect24 channels
:patch_wc24_channel
    set "title_id=%2"
    set "temp_folder=unpack-patched"
    set "unpack_folder=unpack"
    set "patch_folder=WiiLink_Patcher\%1"
    set "output_wad=!temp_folder!\!title_id!.wad"
    set "channel_name=%~5"
    set "channel_region=%6"
    set "channel_version=%7"

    :: Create folders
    if not exist !temp_folder! mkdir !temp_folder!
    if not exist !patch_folder! mkdir !patch_folder!
    if not exist !unpack_folder! mkdir !unpack_folder!

    set "task=Downloading necessary files for !channel_name!"
    set cur_command=curl -f -L -s -S --insecure --create-dirs !PabloURL!/WC24_Patcher/%1/cert/!title_id!.cert -o !unpack_folder!\!title_id!.cert
    !cur_command! > NUL
    if %errorlevel% neq 0 goto :error

    if "!title_id!"=="%nc_title_id%" (
        set cur_command=curl -f -L -s -S --insecure --create-dirs !PabloURL!/WC24_Patcher/%1/tik/!title_id!.tik -o !unpack_folder!\cetk
        !cur_command! > NUL
        if %errorlevel% neq 0 goto :error
    )

    set "task=Extracting files from !channel_name!"
    set cur_command=WiiLink_Patcher\Sharpii.exe nusd -id !title_id! -o !unpack_folder! -q -decrypt
    !cur_command! > NUL
    if %errorlevel% neq 0 goto :error

    set "task=Renaming stuff for !channel_name!"
    set cur_command=move !unpack_folder!\tmd.%channel_version% !unpack_folder!\!title_id!.tmd
    !cur_command! > NUL
    if %errorlevel% neq 0 goto :error

    set cur_command=move !unpack_folder!\cetk !unpack_folder!\!title_id!.tik
    !cur_command! > NUL
    if %errorlevel% neq 0 goto :error

    set "task=Applying !channel_name! patches"
    set cur_command=WiiLink_Patcher\xdelta3.exe -q -f -d -s !unpack_folder!\%3.app !patch_folder!\%4.delta !temp_folder!\%3.app
    !cur_command! > NUL
    if %errorlevel% neq 0 goto :error

    set "task=Moving patched files for !channel_name!"
    set cur_command=move /y !temp_folder!\*.* !unpack_folder!
    !cur_command! > NUL
    if %errorlevel% neq 0 goto :error

    :: Output WAD name (that has spaces in it)
    set "output_patched_wad=WAD\!channel_name! [!channel_region!] (WiiLink).wad"

    set "task=Repacking the title for !channel_name!"
    set cur_command=WiiLink_Patcher\Sharpii.exe wad -p !unpack_folder! "!output_patched_wad!"
    call WiiLink_Patcher\Sharpii.exe wad -p !unpack_folder! "!output_patched_wad!"> NUL
    if %errorlevel% neq 0 goto :error
goto :EOF

:: Language selection menu
@REM :change_prog_lang
@REM     call :header

@REM     :: Set language text based on language
@REM     if %prog_language%==en set language_text=Please select your language:

@REM     :: Print language text
@REM     echo %language_text%
@REM     echo.
@REM     echo 1. English
@REM     echo 2. Español
@REM     echo 3. 日本語 ^(Japanese^)
@REM     echo.
@REM     if %prog_language%==en echo 4. Go back to the main menu
@REM     echo.
@REM     if %prog_language%==en (
@REM         echo [1;32mNOTE:[0m If the language characters don't look right for the langauge you want, change to a compatible font in
@REM         echo       CMD/Terminal^^!
@REM         echo.
@REM     )
    
@REM     call :user_choose 1234
@REM     if %errorlevel%==1 set prog_language=en & goto main
@REM     if %errorlevel%==2 set prog_language=es & goto main
@REM     if %errorlevel%==3 set prog_language=jp & goto main
@REM     if %errorlevel%==4 goto main
@REM goto :EOF


:: Credits
:credits
    call :header
    if %prog_language%==en (
        echo Credits:
        echo.
        echo   - Sketch: Windows Patcher Developer
        echo.
        echo   - PablosCorner: Windows Patcher Maintainer
        echo.
        echo   - KcrPL: Original WiiLink Patcher Developer
        echo.
        echo   - TheShadowEevee: Sharpii-NetCore
        echo.
        echo   - Joshua MacDonald: Xdelta
        echo.
        echo   - person66, and leathl: Original Sharpii, and libWiiSharp developers
        echo.
        echo [1;32mWiiLink website:[0m https://wiilink24.com
        echo.
        echo [1;32mPress any key to go back to the main menu[0m
    )
    pause > nul
goto main


:: Download patch
:download_patch
    set "patch_url=!WiiLinkPatcherURL!/%1/%2"
    set "patch_destination_path=WiiLink_Patcher/%4/%3"

    set cur_command=curl -f -L -s -S --insecure --create-dirs  "!patch_url!" -o "!patch_destination_path!"
    !cur_command!
    if %errorlevel% neq 0 goto :error
goto :EOF


:: Download the correct SPD WAD for the chosen platform
:download_spd
    if %platform_type%==Wii (
        set cur_command=curl -f -L -s -S --insecure --create-dirs -f -s !WiiLinkPatcherURL!/spd/SPD_Wii.wad -o "WAD/WiiLink_SPD (Wii).wad"
        !cur_command!
        if %errorlevel% neq 0 goto :error
    )
    if %platform_type%==vWii (
        set cur_command=curl -f -L -s -S --insecure --create-dirs !WiiLinkPatcherURL!/spd/SPD_vWii.wad -o "WAD/WiiLink_SPD (vWii).wad"
        !cur_command!
        if %errorlevel% neq 0 goto :error
    )
goto :EOF

:: Header text
:header
    cls

    set header_no_format=%1

    :: Border character
    set "border_char=="
    set "border_line="
    
    :: Set header text based on language
    if !prog_language!==en (
        set "header=[1mWiiLink Patcher v%version% - (c) %copyright_year% WiiLink[0m (Updated on %last_build_long% at %at_en% EST)"
        if "!header_no_format!"=="no_formatting" set "header=WiiLink Patcher v%version% - (c) %copyright_year% WiiLink (Updated: !last_build_short!, !at_en! EST)"
    )

    :: Print header
    echo !header!
    for /f "tokens=2" %%i in ('mode con ^| findstr /R "Columns:"') do set columns=%%i
    for /l %%i in (1,1,!columns!) do set "border_line=!border_line!!border_char!"
    echo !border_line!
    echo.
goto :EOF


:: Choose
:user_choose
    :: Set choose text based on language
    if %prog_language%==en set choose_text=Choose:

    :: Set invalid choice text based on language
    if %prog_language%==en set invalid_choice_text=Invalid choice

    choice /c:%1 /n /m "%choose_text%"
goto :EOF


:: SD card check
:detect_sd_card
    set sdcard=NUL
    for /f "tokens=1" %%i in ('wmic logicaldisk get name ^| findstr /v "^$"') do (
        if exist %%i\apps set sdcard=%%i
    )
goto :EOF


:: Choose install method (Only one at the moment)
:install_choose
    call :header
    call :announcement
    
    :: Time-based greeting
    for /f "tokens=1 delims=:" %%i in ('time /t') do set hour=%%i
    if %prog_language%==en (
        if %hour% gtr 0 (
            if %hour% lss 12 set greeting=Good morning
        )
        if %hour% gtr 11 (
            if %hour% lss 18 set greeting=Good afternoon
        )
        if %hour% gtr 17 (
            if %hour% lss 24 set greeting=Good evening
        )
    )

    if %prog_language%==en (
        echo %greeting% %username%^^! Welcome to the WiiLink Patcher.
        echo.
        echo What are we doing today?
        echo.
        echo 1. Install WiiLink on your Wii.
        echo   The patcher will guide you through the process of installing WiiLink.
        echo.
        echo 2. Go Back to Main Menu
    )
    echo.

    call :user_choose 12
    if %errorlevel%==1 goto channel_lang_choose
    if %errorlevel%==2 goto main
goto :EOF


:: Choose language for core channels
:channel_lang_choose
    call :header
        
    if %prog_language% == en (
        echo Hello %username%, welcome to the express installation of WiiLink!
        echo.
        echo The patcher will download any files that are required to run the patcher.
        echo.
        echo The entire process should take about 1 to 3 minutes depending on your computer CPU and internet speed.
        echo.
        echo But before starting, you need to tell me one thing:
        echo.
        echo For Wii no Ma ^(Wii Room^)^, Digicam Print Channel, and Demae Channel - what language of the channels do you
        echo want to download?
        echo.
        echo 1. English
        echo 2. Japanese
        echo.
        echo 3. Go Back to Main Menu
    )
    echo.

    call :user_choose 123
    if %errorlevel%==1 set "reg=EN" & set "lang=English" & goto demae_configuration
    if %errorlevel%==2 set "reg=JP" & set "lang=Japan" & set demae_version=standard & goto nc_setup
    if %errorlevel%==3 goto main
goto :EOF


:: Choose which version of Demae Channel you want
:demae_configuration
    call :header

    if %prog_language% == en (
        echo Alright, what version of Demae Channel do you want?
        echo.
        echo 1. Standard
        echo 2. Domino's
        echo.
        echo 3. Go Back to Main Menu
    )
    echo.

    call :user_choose 123
    if %errorlevel%==1 set demae_version=standard & goto nc_setup
    if %errorlevel%==2 set demae_version=dominos & goto nc_setup
    if %errorlevel%==3 goto main


:: Choose region for Nintendo Channel
:nc_setup
    call :header

    if %prog_language% == en (
        echo Alright, what region of Nintendo Channel do you want?
        echo.
        echo 1. Europe ^(E^)
        echo 2. USA ^(U^)
        echo 3. Japan ^(J^)
        echo.
        echo 4. Go Back to Main Menu
    )
    echo.

    call :user_choose 1234
    if %errorlevel%==1 set "nc_region=PAL" & goto fc_setup
    if %errorlevel%==2 set "nc_region=USA" & goto fc_setup
    if %errorlevel%==3 set "nc_region=Japan" & goto fc_setup
    if %errorlevel%==4 goto main
goto :EOF


:: Choose region for Forecast Channel
:fc_setup
    call :header

    if %prog_language%==en (
        echo Alright, what region of Forecast Channel do you want?
        echo.
        echo 1. Europe ^(E^)
        echo 2. USA ^(U^)
        echo 3. Japan ^(J^)
        echo.
        echo 4. Go Back to Main Menu
    )
    echo.

    call :user_choose 1234
    if %errorlevel%==1 set "fc_region=PAL" & goto platform_choice
    if %errorlevel%==2 set "fc_region=USA" & goto platform_choice
    if %errorlevel%==3 set "fc_region=Japan" & goto platform_choice
    if %errorlevel%==4 goto main
goto :EOF


:: Choose whether you're using a Wii or Wii U (vWii)
:platform_choice
    call :header

    if %prog_language%==en (
        echo Before we begin, I need to know what platform you're installing WiiLink on.
        echo.
        echo This setting will change the version of SPD that I will download so channels like Demae works.
        echo.
        echo What platform are you using? ^(Only applies to non-Japanese Wii^)
        echo.
        echo 1. Wii ^(or Dolphin Emulator^)
        echo 2. Wii U ^(vWii^)
        echo.
        echo 3. Go Back to Main Menu
    )
    echo.

    call :user_choose 123
    if %errorlevel%==1 set "platform_type=Wii" & goto :sd_status
    if %errorlevel%==2 set "platform_type=vWii" & goto :sd_status
    if %errorlevel%==3 goto main
goto :EOF


:: Check the status of the SD card
:sd_status
    call :header
    
    if %prog_language%==en (
        echo Great^^!
        echo After passing this screen, any user interraction won't be needed so you can relax and let me do the work^^!
        echo.
        echo Hmm... one more thing. What was it? Ah^^! To make patching even easier, I can download everything straight to your
        echo SD Card.
        echo.
        echo Just plug in your SD card right now.
        echo.
        echo 1. Connected^^!
        echo 2. I can't connect my SD Card to my computer..
        echo 3. Go Back to Main Menu
    )
    echo.

    call :user_choose 123
    if %errorlevel% == 1 (
        set sdstatus=connected
        call :detect_sd_card
        goto pre_patch
    ) else if %errorlevel% == 2 (
        set sdstatus=cant_connect
        set sdcard=NUL
        goto pre_patch
    ) else if %errorlevel% == 3 (
        goto main
    )
goto :EOF


:: Pre-patch screen
:pre_patch
    call :header

    :: Check if the user has a SD card connected
    if %sdstatus%==cant_connect (
        if %sdcard%==NUL (
            if %prog_language%==en (
                echo Ayy caramba^^! No worries, though. You will be able to copy files later after patching.
                echo.
                echo The entire patching process will download about 160MB of data.
                echo.
                echo What's next?
                echo.
                echo 1. Start patching
                echo 2. Go Back to Main Menu
            )
        )
    ) else if %sdstatus%==connected (
        if %sdcard%==NUL (
            if %prog_language%==en (
                echo Hmm... looks like an SD Card wasn't found in your system.
                echo.
                echo Please choose the Change volume name option to set your SD Card volume name manually,
                echo otherwise, you will have to copy them later.
                echo.
                echo The entire patching process will download about 160MB of data.
                echo.
                echo What's next?
                echo.
                echo 1. Start Patching
                echo 2. Go Back to Main Menu
                echo 3. Change Volume Name
            )
        ) else (
            if %prog_language%==en (
                echo Congrats^^! I've successfully detected your SD Card^^!
                echo [1mVolume Name:[0m [1;32m%sdcard%\[0m
                echo.
                echo I will be able to automatically download and install everything on your SD Card^!
                echo.
                echo The entire patching process will download about 160MB of data.
                echo.
                echo What's next?
                echo.
                echo 1. Start Patching
                echo 2. Go Back to Main Menu
                echo 3. Change Volume Name
            )
        )
    )
    echo.
    
    call :user_choose 123
    if %errorlevel%==1 goto wad_setup
    if %errorlevel%==2 goto main
    if %errorlevel%==3 (
        if %sdstatus%==cant_connect (
            goto pre_patch
        ) else (
            goto change_drive_letter
        )
    )
goto :EOF


:: Change the drive letter of the SD Card
:change_drive_letter
    call :header

    if %prog_language%==en (
        echo [1mChange SD Card Drive Letter[0m ^(Ex. E^)
        echo ^(Type EXIT to go back to the previous screen^)
    )
    echo.
    set /p sdcard_new=Enter the new drive letter: 

    :: Go back to the previous screen if the user types EXIT or exit
    if %sdcard_new%==EXIT goto pre_patch
    if %sdcard_new%==exit goto pre_patch

    :: Setting it to your boot drive is not a good idea
    if %sdcard_new%==C (
        if %prog_language%==en (
            echo.
            echo [1;31mProbably not a good idea to use your boot drive...[0m
            echo [1;31mPlease choose another drive letter^^![0m
            echo.
            echo Press any key to try again...
        )
        pause >nul
        goto change_drive_letter
    )


    :: Make sure that the drive letter is not more than 1 character (Ex. E, not E: or E:\)
    if not "%sdcard_new:~1%" EQU "" (
        if %prog_language%==en (
            echo.
            echo [1;31mYou can only enter one character^^![0m
            echo.
            echo Press any key to try again...
        )
        pause >nul
        goto change_drive_letter
    )

    
    :: Make sure that the new drive letter is not the same as the old drive letter
    if %sdcard_new%==%sdcard% (
        if %prog_language%==en (
            echo.
            echo [1;31mYou cannot use the same drive letter^^![0m
            echo.
            echo Press any key to try again...
        )
        pause >nul
        goto change_drive_letter
    )

    :: Make sure that the new drive letter is actually mounted on the system
    vol %sdcard_new%: > nul 2>&1 || (
        if %prog_language%==en (
            echo.
            echo [1;31mThe drive letter you entered is not mounted on your system^^![0m
            echo.
            echo Press any key to try again...
        )
        pause >nul
        goto change_drive_letter
    )

    :: Check if \apps folder exists in the new drive letter, if not, display an error message and try again
    if not exist %sdcard_new%:\apps (
        if %prog_language%==en (
            echo.
            echo [1;31mA drive has been detected, however, the \apps folder was not found.[0m
            echo [1;31mPlease create it on the root of the SD Card and try again^^![0m
            echo.
            echo Press any key to try again...
        )
        pause >nul
        goto change_drive_letter
    )

    set sdcard=%sdcard_new%:
goto pre_patch


:: Check if WAD folder exists
:wad_setup
    
    :: If "WAD" folder does not exist in the same directory as the patcher, go to patch_progress label
    if not exist %~dp0WAD (
        goto patch_progress
    )

    call :header

    :: Ask for permission to delete the WAD folder
    if %prog_language%==en (
        echo One more thing! I've detected a WAD folder.
        echo I need to delete it. Can I?
        echo.
        echo 1. Yes
        echo 2. No
    )
    echo.
    
    call :user_choose 12
    if %errorlevel%==1 (
        rmdir /s /q WAD
        goto patch_progress
    )
    if %errorlevel%==2 (
        goto patch_progress
    )
goto :EOF


:: Patch process
:patch_progress
    set counter_done=0
    set percent=0

    :: Change Demae progress message depending on version (Standard or Domino's)
    if %demae_version%==standard (
        if %prog_language%==en (
            set "demae_prog_msg=(Standard)"
        )
    ) else if %demae_version%==dominos (
        if %prog_language%==en (
            set "demae_prog_msg=(Domino's)"
        )
    )

    :: Progress variables
    set "patching_progress[0]=downloading:in_progress"
    set "patching_progress[1]=wiinoma:not_started"
    set "patching_progress[2]=digicam:not_started"
    set "patching_progress[3]=demae:not_started"
    set "patching_progress[4]=nc:not_started"
    set "patching_progress[5]=forecast:not_started"
    set "patching_progress[6]=finishing:not_started"

    :: Progress messages
    if %prog_language% == en (
        set "progress_messages[0]=Downloading files"
        set "progress_messages[1]=Wii no Ma (Wii Room)"
        set "progress_messages[2]=Digicam Print Channel"
        set "progress_messages[3]=Demae Channel %demae_prog_msg%"
        set "progress_messages[4]=Nintendo Channel"
        set "progress_messages[5]=Forecast Channel"
        set "progress_messages[6]=Finishing..."
    )

    :: Progress box
    set "progress_symbols[0]=○"
    set "progress_symbols[1]=►"
    set "progress_symbols[2]=●"

    :: Setup labels
    set setup_labels[0]=download_all_patches
    set setup_labels[1]=wiinoma_patch
    set setup_labels[2]=digicam_patch
    set setup_labels[3]=demae_patch
    set setup_labels[4]=nc_patch
    set setup_labels[5]=forecast_patch
    set setup_labels[6]=finish_sd_copy

    :: Progress bar and completion display
    call :progress_info_display

    :: We're done!
    goto :finish
goto :EOF


:: Progress bar and completion display
:progress_info_display
    :: Progress bar calculation
    if %percent%==0 set counter_done=1
    if %percent%==1 set counter_done=2
    if %percent%==2 set counter_done=4
    if %percent%==3 set counter_done=6
    if %percent%==4 set counter_done=8
    if %percent%==5 set counter_done=9
    if %percent%==6 set counter_done=10

    call :header

    :: Progress bar
    if %counter_done%==0 set progress_bar=[[1;32m          [0m]
    if %counter_done%==1 set progress_bar=[[1;32m=         [0m]
    if %counter_done%==2 set progress_bar=[[1;32m==        [0m]
    if %counter_done%==3 set progress_bar=[[1;32m===       [0m]
    if %counter_done%==4 set progress_bar=[[1;32m====      [0m]
    if %counter_done%==5 set progress_bar=[[1;32m=====     [0m]
    if %counter_done%==6 set progress_bar=[[1;32m======    [0m]
    if %counter_done%==7 set progress_bar=[[1;32m=======   [0m]
    if %counter_done%==8 set progress_bar=[[1;32m========  [0m]
    if %counter_done%==9 set progress_bar=[[1;32m========= [0m]
    if %counter_done%==10 set progress_bar=[[1;32m==========[0m]

    :: Calculate percentage
    set /a percent_done=!counter_done!*100/11

    if %prog_language%==en (
        echo [1m[*] Patching... this can take some time depending on the processing speed ^(CPU^) of your computer.[0m
        echo.
        echo     Progress: !progress_bar! [1m!percent_done!%% completed[0m
    )
    
    echo.
    if %prog_language%==en echo Please wait while the patching process is in progress...
    echo.

    :: Show progress of each channel
    set progress_loop_index=0
    call :progress_loop
    echo.

    :: Call the next setup process by percent value
    call :!setup_labels[%percent%]!

    :: Increment the progress bar
    set /a percent+=1

    :: If we're done, go to finish_sd_copy
    if !patching_progress[6]! == finishing:done goto :finish_sd_copy
    
    :: Loop back to start of label
    goto :progress_info_display
goto :EOF

:: Show progress of each channel
:progress_loop
    if not defined patching_progress[%progress_loop_index%] goto :EOF

    for /f "tokens=2 delims=:" %%a in ("!patching_progress[%progress_loop_index%]!") do set "status=%%a"

    set "message=!progress_messages[%progress_loop_index%]!"
    if "%status%" == "not_started" echo !progress_symbols[0]! !message!
    if "%status%" == "in_progress" echo [5m!progress_symbols[2]![0m !message!
    if "%status%" == "done" echo [32m!progress_symbols[2]![0m !message!

    set /a progress_loop_index+=1
goto progress_loop


:: Download all patches
:download_all_patches
    set "task=Downloading patches"
    
    :: Downloading Sharpii
    set cur_command=curl -f -L -s -S --insecure --create-dirs -o WiiLink_Patcher/Sharpii.exe !PabloURL!/Sharpii/Sharpii.exe
    !cur_command!
    if %errorlevel% neq 0 goto :error

    :: Downloading xdelta3
    set cur_command=curl -f -L -s -S --insecure --create-dirs -o WiiLink_Patcher/xdelta3.exe !PabloURL!/xdelta/xdelta.exe
    !cur_command!
    if %errorlevel% neq 0 goto :error
    
    :: Download SPD if English is selected
    if %reg%==EN call :download_spd
    if %reg%==JP mkdir WAD

    :::: Downloading all patches ::::
    :: Wii no Ma
    if %reg%==EN call :download_patch wiinoma WiinoMa_0_!lang!.delta WiinoMa_0.delta WiinoMa
                 call :download_patch wiinoma WiinoMa_1_!lang!.delta WiinoMa_1.delta WiinoMa
                 call :download_patch wiinoma WiinoMa_2_!lang!.delta WiinoMa_2.delta WiinoMa

    :: Digicam Print Channel
    if %reg%==EN call :download_patch digicam Digicam_0_!lang!.delta Digicam_0.delta Digicam
                 call :download_patch digicam Digicam_1_!lang!.delta Digicam_1.delta Digicam
    if %reg%==EN call :download_patch digicam Digicam_2_!lang!.delta Digicam_2.delta Digicam

    :: Demae Channel (Standard or Dominos)
    if %demae_version%==standard (
        if %reg%==EN call :download_patch demae Demae_0_!lang!.delta DemaeChannel_0_!lang!.delta Demae
                     call :download_patch demae Demae_1_!lang!.delta DemaeChannel_1_!lang!.delta Demae
        if %reg%==EN call :download_patch demae Demae_2_!lang!.delta DemaeChannel_2_!lang!.delta Demae
    ) else if %demae_version%==dominos (
        call :download_patch dominos Dominos_0.delta Dominos_0.delta Dominos
        call :download_patch dominos Dominos_1.delta Dominos_1.delta Dominos
        call :download_patch dominos Dominos_2.delta Dominos_2.delta Dominos
    )

    :: Downloading Wii Mod Lite
    set "task=Downloading Wii Mod Lite"
    set cur_command=curl -f -L -s -S --insecure --create-dirs -o apps/WiiModLite/boot.dol https://hbb1.oscwii.org/unzipped_apps/WiiModLite/apps/WiiModLite/boot.dol
    !cur_command!
    if %errorlevel% neq 0 goto :error

    set cur_command=curl -f -L -s -S --insecure --create-dirs -o apps/WiiModLite/meta.xml https://hbb1.oscwii.org/unzipped_apps/WiiModLite/apps/WiiModLite/meta.xml
    !cur_command!
    if %errorlevel% neq 0 goto :error

    set cur_command=curl -f -L -s -S --insecure --create-dirs -o apps/WiiModLite/icon.png https://hbb1.oscwii.org/hbb/WiiModLite.png
    !cur_command!
    if %errorlevel% neq 0 goto :error

    :: Nintendo Channel
    call :download_patch nc NC_1_!nc_region!.delta Nintendo_Channel_1_!nc_region!.delta Nintendo_Channel

    :: Forecast Channel
    call :download_patch forecast Forecast_1_!platform_type!_!fc_region!.delta ForecastChannel_1.delta Forecast_Channel

    :: Downloading stuff is finished!
    set "patching_progress[0]=downloading:done"
    set "patching_progress[1]=wiinoma:in_progress"
goto :EOF


:: Patching Wii no Ma
:wiinoma_patch
    set "task=Patching Wii no Ma"

    :: Parameters ::
    :: %1 = Title Folder, %2 = Title ID
    :: %3 = First App Number, %4 = First Patch, %5 = Second App Number, %6 = Second Patch, %7 = Third App Number, %8 = Third Patch,
    :: %9 = Title Name
    call :patch_core_channel WiinoMa 000100014843494a 00000000 WiinoMa_0 00000001 WiinoMa_1 00000002 WiinoMa_2 "Wii no Ma"

    :: Finished patching
    call :files_cleanup channel_clean
    set "patching_progress[1]=wiinoma:done"
    set "patching_progress[2]=digicam:in_progress"
goto :EOF


:: Patching Digicam Print Channel
:digicam_patch
    set "task=Patching Digicam Print Channel"

    :: Parameters ::
    :: %1 = Title Folder, %2 = Title ID
    :: %3 = First App Number, %4 = First Patch, %5 = Second App Number, %6 = Second Patch, %7 = Third App Number, %8 = Third Patch,
    :: %9 = Title Name
    call :patch_core_channel Digicam 000100014843444a 00000000 Digicam_0 00000001 Digicam_1 00000002 Digicam_2 "Digicam Print Channel"

    :: Finished patching
    call :files_cleanup channel_clean
    set "patching_progress[2]=digicam:done"
    set "patching_progress[3]=demae:in_progress"
goto :EOF


:: Patching Demae Channel
:demae_patch
    set "task=Patching Demae Channel"

    :: Parameters ::
    :: %1 = Title Folder, %2 = Title ID
    :: %3 = First App Number, %4 = First Patch, %5 = Second App Number, %6 = Second Patch, %7 = Third App Number, %8 = Third Patch,
    :: %9 = Title Name
    if %demae_version%==standard (
        call :patch_core_channel Demae 000100014843484a 00000000 DemaeChannel_0_!lang! 00000001 DemaeChannel_1_!lang! 00000002 DemaeChannel_2_!lang! "Demae Channel (Standard)"
    )
    if %demae_version%==dominos (
        call :patch_core_channel Dominos 000100014843484a 00000000 Dominos_0 00000001 Dominos_1 00000002 Dominos_2 "Demae Channel (Dominos)"
    )

    :: Finished patching
    call :files_cleanup channel_clean
    set "patching_progress[3]=demae:done"
    set "patching_progress[4]=nc:in_progress"
goto :EOF


:: Patching Nintendo Channel
:nc_patch
    set "task=Patching Nintendo Channel"
    
    if %nc_region%==PAL (
        set "nc_title_id=0001000148415450"
        set "app_number=0000002d"
    )
    if %nc_region%==USA (
        set "nc_title_id=0001000148415445"
        set "app_number=0000002c"
    )
    if %nc_region%==Japan (
        set "nc_title_id=000100014841544a"
        set "app_number=0000003e"
    )

    :: Parameters ::
    :: %1 = Channel Folder Name, %2 = Title ID
    :: %3 = App Number, %4 = Patch Name
    :: %5 = Channel Name, %6 = Region, %7 = Version Number
    call :patch_wc24_channel Nintendo_Channel !nc_title_id! !app_number! Nintendo_Channel_1_!nc_region! "Nintendo Channel" !nc_region! 1792

    :: Finished patching
    call :files_cleanup channel_clean
    set "patching_progress[4]=nc:done"
    set "patching_progress[5]=forecast:in_progress"
goto :EOF


:: Patching Forecast Channel
:forecast_patch
    set "task=Patching Forecast Channel"
    
    if %fc_region%==PAL set "fc_title_id=0001000248414650"
    if %fc_region%==USA set "fc_title_id=0001000248414645"
    if %fc_region%==Japan set "fc_title_id=000100024841464a"

    :: Parameters ::
    :: %1 = Channel Folder Name, %2 = Title ID
    :: %3 = App Number, %4 = Patch Name
    :: %5 = Channel Name, %6 = Region, %7 = Version Number
    call :patch_wc24_channel Forecast_Channel %fc_title_id% 0000000d ForecastChannel_1 "Forecast Channel (!platform_type!)" %fc_region% 7

    :: Finished patching
    call :files_cleanup channel_clean
    set "patching_progress[5]=forecast:done"
    set "patching_progress[6]=finishing:in_progress"
goto :EOF


:: Finishing it all up! Ooh wee, I'm so excited!
:finish_sd_copy
    set "task=Copying files to SD card"
    if not %sdcard%==NUL (
        echo.
        if %prog_language%==en echo [1m [*] Copying files. This may take a while. Give me a second.[0m
        echo.

        :: Move apps and WAD folders to SD Card
        set cur_command=xcopy apps\*.* !sdcard!\apps /s /e /y /i /q
        !cur_command! > NUL
        if %errorlevel% neq 0 goto :error

        set cur_command=xcopy WAD\*.* !sdcard!\WAD /s /e /y /i /q
        !cur_command! > NUL
        if %errorlevel% neq 0 goto :error
    )

    :: Final cleanup
    call :files_cleanup setup_clean
    set "patching_progress[6]=finishing:done"
goto :EOF


:: We made it! We're done! Yay! Now go install the channels!
:finish
    call :clear
    call :header

    :: Current folder location script is running from
    set "current_folder=%~dp0"

    if !sdstatus!==cant_connect set "match=1"
    if !sdstatus!==connected (
        if !sdcard!==NUL set "match=1"
    )
    
    if !match! == 1 (
        if %prog_language%==en (
            echo [1;32mPatching Complete^^![0m
            echo.
            echo Please connect your Wii SD Card and copy the apps and WAD folders to the root ^(main folder^) of your SD Card.
            echo You can find these folders in the [1m%current_folder%[0m folder of your computer.
        )
    )

    if !sdstatus!==connected (
        if not !sdcard!==NUL (
            if %prog_language%==en (
                echo [1;32mPatching Complete^^![0m
                echo.
                echo Every file is in its place on your SD Card^^!
            )
        )
    )

    if %prog_language%==en (
        echo.
        echo Please proceed with the tutorial that you can find on https://wii.guide/wiilink.
        echo.
        echo What do you want to do now?
        echo.
        echo 1. Go back to the main menu
        echo 2. Exit
    )
    echo.

    call :user_choose 12
    if %errorlevel%==1 goto :main
    if %errorlevel%==2 clear & exit 0
goto :EOF


:: This is where it all starts
:main
    call :header

    
    :: WiiLink Patcher main menu, based on language
    if %prog_language%==en (
        echo [1mWelcome to the WiiLink Patcher^^![0m
        echo.
        echo 1. Start
        echo 2. Credits
        echo.
        echo 3. Exit Patcher
    )
    echo.

    :: Show if SD card is detected
    call :detect_sd_card
    if %sdcard%==NUL (
        if %prog_language%==en echo [1;31mCould not detect your SD Card.[0m
    ) else (
        if %prog_language%==en echo [1;32mDetected SD Card:[0m %sdcard%\
    )
    
    if %prog_language%==en echo R. Refresh ^| If incorrect^, you can change later.
    echo.

    call :user_choose 123R
    if %errorlevel%==1 goto install_choose
    if %errorlevel%==2 goto credits
    if %errorlevel%==3 cls & exit
    if %errorlevel%==4 call :detect_sd_card & goto main
goto :EOF
endlocal