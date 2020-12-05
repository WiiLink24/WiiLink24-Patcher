@echo off
setlocal EnableDelayedExpansion
cd /d "%~dp0"

echo 	Starting up...
echo	The program is starting...

:: ===========================================================================
:: WiiLink24 Patcher for Windows
set version=1.0.2
:: AUTHORS: KcrPL
:: ***************************************************************************
:: Copyright (c) 2020 KcrPL
:: ===========================================================================

::if exist update_assistant.bat del /q update_assistant.bat
:script_start
echo 	.. Setting up the variables
:: Window size (Lines, columns)
set mode=128,37
mode %mode%
set s=NUL

set /a sdcardstatus=0
set sdcard=NUL
set line=---------------------------------------------------------------------------------------------------------------------------

:: Free space requirements
	set cd_temp=%cd%
	set running_on_drive=%cd_temp:~0,1%
	
	:: WiiLink24 Installation
	set wii_patching_requires=120
		set /a size1=%wii_patching_requires%*1024
		set /a patching_size_required_wii_bytes=%size1%*1024

:: Window Title
set title=WiiLink24 Patcher v%version% Created by @KcrPL

title %title%

set last_build=2020/12/05
set at=22:20
:: ### Auto Update ###
:: 1=Enable 0=Disable
:: Update_Activate - If disabled, patcher will not even check for updates, default=1
:: offlinestorage - Only used while testing of Update function, default=0
:: FilesHostedOn - The website and path to where the files are hosted. WARNING! DON'T END WITH "/"
:: MainFolder/TempStorage - folder that is used to keep version.txt and whatsnew.txt. These two files are deleted every startup but if offlinestorage will be set 1, they won't be deleted.
set /a Update_Activate=1
set /a offlinestorage=0 
set FilesHostedOn=https://kcrpl.github.io/Patchers_Auto_Update/WiiLink24-Patcher/v1

set MainFolder=%appdata%\WiiLink24Patcher
set TempStorage=%appdata%\WiiLink24Patcher\internet\temp

if exist "%TempStorage%" del /s /q "%TempStorage%">NUL

set header=WiiLink24 Patcher - (C) KcrPL v%version% (Updated on %last_build% at %at%)

if not exist "%MainFolder%" md "%MainFolder%"
if not exist "%TempStorage%" md "%TempStorage%"

set chcp_enable=0

echo.
echo .. Checking for SD Card
echo   :--------------------------------------------------------------------------------:
echo   : Can you see an error box? Press `Continue`.                                    :
echo   : There's nothing to worry about, everything is going ok. This error is normal.  :
echo   :--------------------------------------------------------------------------------:
echo.
echo Checking now...
goto begin_main_refresh_sdcard

:detect_sd_card
setlocal enableDelayedExpansion
set sdcard=NUL
set counter=-1
set letters=ABDEFGHIJKLMNOPQRSTUVWXYZ
set looking_for=
:detect_sd_card_2
set /a counter=%counter%+1
set looking_for=!letters:~%counter%,1!
if exist %looking_for%:/apps (
set sdcard=%looking_for%
call :%tempgotonext%
exit
exit
)

if %looking_for%==Z (
set sdcard=NUL
call :%tempgotonext%
exit
exit
)
goto detect_sd_card_2
:begin_main_refresh_sdcard
set sdcard=NUL
set tempgotonext=begin_main
goto detect_sd_card

:begin_main
cls
mode %mode%
echo %header%
echo              `..````
echo              yNNNNNNNNMNNmmmmdddhhhyyyysssooo+++/:--.`
echo              ddmNNd:dNMMMMNMMMMMMMMMMMMMMMMMMMMMMMMMMs
echo              hNNNNNNNNMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMd
echo             `mdmNNy dNMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM+    WiiLink24 Patcher
echo             .mmmmNs mNMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM:
echo             :mdmmN+`mNMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM.  1. Start
echo             /mmmmN:-mNMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMN   2. Credits
echo             ommmmN.:mMMMMMMMMMMMMmNMMMMMMMMMMMMMMMMMd   
if not "%sdcard%"=="NUL" echo             smmmmm`+mMMMMMMMMMNhMNNMNNMMMMMMMMMMMMMMy   Detected SD Card: %sdcard%:\
if "%sdcard%"=="NUL" echo             smmmmm`+mMMMMMMMMMNhMNNMNNMMMMMMMMMMMMMMy   Could not detect your SD Card.
echo             hmmmmh omMMMMMMMMMmhNMMMmNNNNMMMMMMMMMMM+   R. Refresh ^| If incorrect, you can change later.   	
echo             hmmmmh omMMMMMMMMMmhNMMMmNNNNMMMMMMMMMMM+
echo             mmmmms smMMMMMMMMMmddMMmmNmNMMMMMMMMMMMM;
echo            `mmmmmo hNMMMMMMMMMmddNMMMNNMMMMMMMMMMMMM.  
echo            -mmmmm/ dNMMMMMMMMMNmddMMMNdhdMMMMMMMMMMN
echo            :mmmmm-`mNMMMMMMMMNNmmmNMMNmmmMMMMMMMMMMd   
echo            :mmmmm-`mNMMMMMMMMNNmmmNMMNmmmMMMMMMMMMMd
echo            +mmmmN.-mNMMMMMMMMMNmmmmMMMMMMMMMMMMMMMMy
echo            smmmmm`/mMMMMMMMMMNNmmmmNMMMMNMMNMMMMMNmy.
echo            hmmmmd`omMMMMMMMMMNNmmmNmMNNMmNNNNMNdhyhh.
echo            mmmmmh ymMMMMMMMMMNNmmmNmNNNMNNMMMMNyyhhh`
echo           `mmmmmy hmMMNMNNMMMNNmmmmmdNMMNmmMMMMhyhhy
echo           -mddmmo`mNMNNNNMMMNNNmdyoo+mMMMNmNMMMNyyys
echo           :mdmmmo-mNNNNNNNNNNdyo++sssyNMMMMMMMMMhs+-
echo          .+mmdhhmmmNNNNNNmdysooooosssomMMMNNNMMMm
echo          o/ossyhdmmNNmdyo+++oooooosssoyNMMNNNMMMM+
echo          o/::::::://++//+++ooooooo+oo++mNMMmNNMMMm
echo         `o//::::::::+////+++++++///:/+shNMMNmNNmMM+
echo         .o////////::+++++++oo++///+syyyymMmNmmmNMMm
echo         -+//////////o+ooooooosydmdddhhsosNMMmNNNmho            `:/
echo         .+++++++++++ssss+//oyyysso/:/shmshhs+:.          `-/oydNNNy
echo           `..-:/+ooss+-`          +mmhdy`           -/shmNNNNNdy+:`
echo                   `.              yddyo++:    `-/oymNNNNNdy+:`
echo                                   -odhhhhyddmmmmmNNmhs/:`
echo                                     :syhdyyyyso+/-`
set /p s=Type a number that you can see above next to the command and hit ENTER: 
if %s%==1 goto begin_main1
if %s%==2 goto credits
if %s%==r goto begin_main_refresh_sdcard
if %s%==R goto begin_main_refresh_sdcard
if %s%==exit exit
goto begin_main
:credits
cls
echo %header%
echo              `..````                                     
echo %line%
echo  WiiLink24 Patcher v%version%
echo   Created by:
echo.
echo  - KcrPL
echo    Windows Patcher, UI, scripts.
echo. 
echo  - Joshua MacDonald
echo    xdelta
echo.    
echo  - person66
echo    Sharpii
echo.    
echo  For the entire WiiLink24 Community.
echo  Want to contact us? Join our Discord server at https://discord.gg/n4ta3w6
echo %line%
echo            hmmmmd`omMMMMMMMMMNNmmmNmMNNMmNNNNMNdhyhh.
echo            mmmmmh ymMMMMMMMMMNNmmmNmNNNMNNMMMMNyyhhh`
echo           `mmmmmy hmMMNMNNMMMNNmmmmmdNMMNmmMMMMhyhhy
echo           -mddmmo`mNMNNNNMMMNNNmdyoo+mMMMNmNMMMNyyys
echo           :mdmmmo-mNNNNNNNNNNdyo++sssyNMMMMMMMMMhs+-
echo          .+mmdhhmmmNNNNNNmdysooooosssomMMMNNNMMMm
echo          o/ossyhdmmNNmdyo+++oooooosssoyNMMNNNMMMM+
echo          o/::::::://++//+++ooooooo+oo++mNMMmNNMMMm
echo         `o//::::::::+////+++++++///:/+shNMMNmNNmMM+
echo         .o////////::+++++++oo++///+syyyymMmNmmmNMMm
echo         -+//////////o+ooooooosydmdddhhsosNMMmNNNmho            `:/
echo         .+++++++++++ssss+//oyyysso/:/shmshhs+:.          `-/oydNNNy
echo           `..-:/+ooss+-`          +mmhdy`           -/shmNNNNNdy+:`
echo                   `.              yddyo++:    `-/oymNNNNNdy+:`
echo                                   -odhhhhyddmmmmmNNmhs/:`
echo                                     :syhdyyyyso+/-`
pause>NUL
goto begin_main
:begin_main_download_curl
cls
echo %header%
echo.
echo              `..````                                     :-------------------------:
echo              yNNNNNNNNMNNmmmmdddhhhyyyysssooo+++/:--.`    Downloading curl.
echo              hNNNNNNNNMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMd    Please wait...
echo              ddmNNd:dNMMMMNMMMMMMMMMMMMMMMMMMMMMMMMMMs   :-------------------------:
echo             `mdmNNy dNMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM+   
echo             .mmmmNs mNMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM:   File 1 [3.5MB] out of 1
echo             :mdmmN+`mNMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM.   0%% [          ]
echo             /mmmmN:-mNMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMN
echo             ommmmN.:mMMMMMMMMMMMMmNMMMMMMMMMMMMMMMMMd
echo             smmmmm`+mMMMMMMMMMNhMNNMNNMMMMMMMMMMMMMMy
echo             hmmmmh omMMMMMMMMMmhNMMMmNNNNMMMMMMMMMMM+
echo             mmmmms smMMMMMMMMMmddMMmmNmNMMMMMMMMMMMM:
echo            `mmmmmo hNMMMMMMMMMmddNMMMNNMMMMMMMMMMMMM.
echo            -mmmmm/ dNMMMMMMMMMNmddMMMNdhdMMMMMMMMMMN
echo            :mmmmm-`mNMMMMMMMMNNmmmNMMNmmmMMMMMMMMMMd
echo            +mmmmN.-mNMMMMMMMMMNmmmmMMMMMMMMMMMMMMMMy
echo            smmmmm`/mMMMMMMMMMNNmmmmNMMMMNMMNMMMMMNmy.
echo            hmmmmd`omMMMMMMMMMNNmmmNmMNNMmNNNNMNdhyhh.
echo            mmmmmh ymMMMMMMMMMNNmmmNmNNNMNNMMMMNyyhhh`
echo           `mmmmmy hmMMNMNNMMMNNmmmmmdNMMNmmMMMMhyhhy
echo           -mddmmo`mNMNNNNMMMNNNmdyoo+mMMMNmNMMMNyyys
echo           :mdmmmo-mNNNNNNNNNNdyo++sssyNMMMMMMMMMhs+-
echo          .+mmdhhmmmNNNNNNmdysooooosssomMMMNNNMMMm
echo          o/ossyhdmmNNmdyo+++oooooosssoyNMMNNNMMMM+
echo          o/::::::://++//+++ooooooo+oo++mNMMmNNMMMm
echo         `o//::::::::+////+++++++///:/+shNMMNmNNmMM+
echo         .o////////::+++++++oo++///+syyyymMmNmmmNMMm
echo         -+//////////o+ooooooosydmdddhhsosNMMmNNNmho            `:/
echo         .+++++++++++ssss+//oyyysso/:/shmshhs+:.          `-/oydNNNy
echo           `..-:/+ooss+-`          +mmhdy`           -/shmNNNNNdy+:`
echo                   `.              yddyo++:    `-/oymNNNNNdy+:`
echo                                   -odhhhhyddmmmmmNNmhs/:`
echo                                     :syhdyyyyso+/-`
call powershell -command (new-object System.Net.WebClient).DownloadFile('%FilesHostedOn%/curl.exe', 'curl.exe')
set /a temperrorlev=%errorlevel%
if not %temperrorlev%==0 goto begin_main_download_curl_error

goto begin_main1

:begin_main_download_curl_error
cls
echo %header%                                                                
echo              `..````                                                  
echo              yNNNNNNNNMNNmmmmdddhhhyyyysssooo+++/:--.`                
echo              hNNNNNNNNMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMd                
echo              ddmNNd:dNMMMMNMMMMMMMMMMMMMMMMMMMMMMMMMMs                
echo             `mdmNNy dNMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM+        
echo             .mmmmNs mNMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM:                
echo             :mdmmN+`mNMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM.                
echo             /mmmmN:-mNMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMN            
echo             ommmmN.:mMMMMMMMMMMMMmNMMMMMMMMMMMMMMMMMd                 
echo             smmmmm`+mMMMMMMMMMNhMNNMNNMMMMMMMMMMMMMMy                 
echo             hmmmmh omMMMMMMMMMmhNMMMmNNNNMMMMMMMMMMM+                 
echo ---------------------------------------------------------------------------------------------------------------------------
echo    /---\   ERROR.
echo   /     \  There was an error while downloading curl.
echo  /   ^!   \ 
echo  --------- We will now open a website that will download curl.exe.
echo            Please move curl.exe to the folder where WiiLink24Patcher.bat is and restart the patcher.
echo.
echo       Press any key to open download page in browser and to return to menu.
echo ---------------------------------------------------------------------------------------------------------------------------
echo          .+mmdhhmmmNNNNNNmdysooooosssomMMMNNNMMMm                     
echo          o/ossyhdmmNNmdyo+++oooooosssoyNMMNNNMMMM+                    
echo          o/::::::://++//+++ooooooo+oo++mNMMmNNMMMm                    
echo         `o//::::::::+////+++++++///:/+shNMMNmNNmMM+                   
echo         .o////////::+++++++oo++///+syyyymMmNmmmNMMm                   
echo         -+//////////o+ooooooosydmdddhhsosNMMmNNNmho            `:/    
echo         .+++++++++++ssss+//oyyysso/:/shmshhs+:.          `-/oydNNNy   
echo           `..-:/+ooss+-`          +mmhdy`           -/shmNNNNNdy+:`   
echo                   `.              yddyo++:    `-/oymNNNNNdy+:`        
echo                                   -odhhhhyddmmmmmNNmhs/:`             
echo                                     :syhdyyyyso+/-`                   
pause>NUL
start %FilesHostedOn%/curl.exe
goto begin_main


:begin_main1
curl
if not %errorlevel%==2 goto begin_main_download_curl

cls
echo %header%
echo.
echo              `..````                                     :-------------------------:
echo              yNNNNNNNNMNNmmmmdddhhhyyyysssooo+++/:--.`    Checking for updates...
echo              hNNNNNNNNMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMd   :-------------------------:
echo              ddmNNd:dNMMMMNMMMMMMMMMMMMMMMMMMMMMMMMMMs
echo             `mdmNNy dNMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM+
echo             .mmmmNs mNMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM:
echo             :mdmmN+`mNMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM.
echo             /mmmmN:-mNMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMN
echo             ommmmN.:mMMMMMMMMMMMMmNMMMMMMMMMMMMMMMMMd
echo             smmmmm`+mMMMMMMMMMNhMNNMNNMMMMMMMMMMMMMMy
echo             hmmmmh omMMMMMMMMMmhNMMMmNNNNMMMMMMMMMMM+
echo             mmmmms smMMMMMMMMMmddMMmmNmNMMMMMMMMMMMM:
echo            `mmmmmo hNMMMMMMMMMmddNMMMNNMMMMMMMMMMMMM.
echo            -mmmmm/ dNMMMMMMMMMNmddMMMNdhdMMMMMMMMMMN
echo            :mmmmm-`mNMMMMMMMMNNmmmNMMNmmmMMMMMMMMMMd
echo            +mmmmN.-mNMMMMMMMMMNmmmmMMMMMMMMMMMMMMMMy
echo            smmmmm`/mMMMMMMMMMNNmmmmNMMMMNMMNMMMMMNmy.
echo            hmmmmd`omMMMMMMMMMNNmmmNmMNNMmNNNNMNdhyhh.
echo            mmmmmh ymMMMMMMMMMNNmmmNmNNNMNNMMMMNyyhhh`
echo           `mmmmmy hmMMNMNNMMMNNmmmmmdNMMNmmMMMMhyhhy
echo           -mddmmo`mNMNNNNMMMNNNmdyoo+mMMMNmNMMMNyyys
echo           :mdmmmo-mNNNNNNNNNNdyo++sssyNMMMMMMMMMhs+-
echo          .+mmdhhmmmNNNNNNmdysooooosssomMMMNNNMMMm
echo          o/ossyhdmmNNmdyo+++oooooosssoyNMMNNNMMMM+
echo          o/::::::://++//+++ooooooo+oo++mNMMmNNMMMm
echo         `o//::::::::+////+++++++///:/+shNMMNmNNmMM+
echo         .o////////::+++++++oo++///+syyyymMmNmmmNMMm
echo         -+//////////o+ooooooosydmdddhhsosNMMmNNNmho            `:/
echo         .+++++++++++ssss+//oyyysso/:/shmshhs+:.          `-/oydNNNy
echo           `..-:/+ooss+-`          +mmhdy`           -/shmNNNNNdy+:`
echo                   `.              yddyo++:    `-/oymNNNNNdy+:`
echo                                   -odhhhhyddmmmmmNNmhs/:`
echo                                     :syhdyyyyso+/-`
		title %string78% :          :
:: Update script.
set updateversion=0.0.0
:: Delete version.txt and whatsnew.txt
if %offlinestorage%==0 if exist "%TempStorage%\version.txt" del "%TempStorage%\version.txt" /q
if %offlinestorage%==0 if exist "%TempStorage%\whatsnew.txt" del "%TempStorage%\whatsnew.txt" /q

if not exist "%TempStorage%" md "%TempStorage%"
:: Commands to download files from server.

		title %string78% :-         :

call curl -f -L -s --insecure "http://www.msftncsi.com/ncsi.txt">NUL
	if "%errorlevel%"=="6" title %title%& goto no_internet_connection

		title %string78% :---       :

if %Update_Activate%==1 if %offlinestorage%==0 call curl -f -L -s -S --insecure "%FilesHostedOn%/UPDATE/whatsnew.txt" --output "%TempStorage%\whatsnew.txt"
if %Update_Activate%==1 if %offlinestorage%==0 call curl -f -L -s -S --insecure "%FilesHostedOn%/UPDATE/version.txt" --output "%TempStorage%\version.txt"
	set /a temperrorlev=%errorlevel%

		title %string78% :----      :

::if %Update_Activate%==1 if %offlinestorage%==0 if %chcp_enable%==1 call curl -f -L -s -S --insecure "%FilesHostedOn%/UPDATE/Translation_Files/Language_%language%.bat" --output "%TempStorage%\Language_%language%.bat"
::if %Update_Activate%==1 if %offlinestorage%==0 if %chcp_enable%==0 call curl -f -L -s -S --insecure "%FilesHostedOn%/UPDATE/Translation_Files_CHCP_OFF/Language_%language%.bat" --output "%TempStorage%\Language_%language%.bat"
if not %errorlevel%==0 set /a translation_download_error=1

::if %chcp_enable%==1 if exist "%TempStorage%\Language_%language%.bat" call "%TempStorage%\Language_%language%.bat" -chcp
::if %chcp_enable%==0 if exist "%TempStorage%\Language_%language%.bat" call "%TempStorage%\Language_%language%.bat"

		title %string78% :-----     :
		
set /a updateserver=1
	::Bind exit codes to errors here

	if not %temperrorlev%==0 set /a updateserver=0

if exist "%TempStorage%\version.txt`" ren "%TempStorage%\version.txt`" "version.txt"
if exist "%TempStorage%\whatsnew.txt`" ren "%TempStorage%\whatsnew.txt`" "whatsnew.txt"
:: Copy the content of version.txt to variable.

		title %string78% :------    :

if exist "%TempStorage%\version.txt" set /p updateversion=<"%TempStorage%\version.txt"
if not exist "%TempStorage%\version.txt" set /a updateavailable=0
if %Update_Activate%==1 if exist "%TempStorage%\version.txt" set /a updateavailable=1
:: If version.txt doesn't match the version variable stored in this batch file, it means that update is available.
if %updateversion%==%version% set /a updateavailable=0

if exist "%TempStorage%\annoucement.txt" del /q "%TempStorage%\annoucement.txt"
curl -f -L -s --insecure "%FilesHostedOn%/UPDATE/annoucement.txt" --output %TempStorage%\annoucement.txt"

		title %string78% :-------   :

if %Update_Activate%==1 if %updateavailable%==1 set /a updateserver=2
if %Update_Activate%==1 if %updateavailable%==1 title %title%& goto update_notice

		title %string78% :--------- :
	set /a prerelease_status=0
For /F "Delims=" %%A In ('curl -f -L -s --insecure "%FilesHostedOn%/UPDATE/prerelease_status.txt"') do set "prerelease_status=%%A"
	title %title%

goto 1
:no_internet_connection
cls
echo %header%                                                                
echo.                 
echo.                 
echo.                 
echo.                 
echo.                 
echo.                 
echo.                 
echo.                 
echo.                 
echo.                 
echo.                 
echo.
echo %line%
echo    /---\   ERROR.
echo   /     \  There is no internet connection.
echo  /   ^^!   \ 
echo  --------- Could not connect to remote server.
echo            Check your internet connection or check if your firewall isn't blocking curl.
echo.
echo       Press any key to return to main menu.
echo %line%
pause>NUL
goto begin_main


:update_notice
set /a update=1
cls	
echo %header%
echo.                                                                       
echo              `..````                                                  
echo              yNNNNNNNNMNNmmmmdddhhhyyyysssooo+++/:--.`                
echo              hNNNNNNNNMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMd                
echo              ddmNNd:dNMMMMNMMMMMMMMMMMMMMMMMMMMMMMMMMs                
echo             `mdmNNy dNMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM+        
echo             .mmmmNs mNMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM:                
echo             :mdmmN+`mNMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM.                
echo             /mmmmN:-mNMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMN            
echo             ommmmN.:mMMMMMMMMMMMMmNMMMMMMMMMMMMMMMMMd                 
echo             smmmmm`+mMMMMMMMMMNhMNNMNNMMMMMMMMMMMMMMy                 
echo             hmmmmh omMMMMMMMMMmhNMMMmNNNNMMMMMMMMMMM+                 
echo ------------------------------------------------------------------------------------------------------------------------------
echo    /---\   An Update is available.
echo   /     \  An Update for this program is available. We suggest updating the WiiLink24 Patcher to the latest version.
echo  /   ^^!   \ 
echo  ---------  Current version: %version%
echo             New version: %updateversion%
echo                       1. Update                      2. Dismiss               3. What's new in this update?
echo ------------------------------------------------------------------------------------------------------------------------------
echo           -mddmmo`mNMNNNNMMMNNNmdyoo+mMMMNmNMMMNyyys                  
echo           :mdmmmo-mNNNNNNNNNNdyo++sssyNMMMMMMMMMhs+-                  
echo          .+mmdhhmmmNNNNNNmdysooooosssomMMMNNNMMMm                     
echo          o/ossyhdmmNNmdyo+++oooooosssoyNMMNNNMMMM+                    
echo          o/::::::://++//+++ooooooo+oo++mNMMmNNMMMm                    
echo         `o//::::::::+////+++++++///:/+shNMMNmNNmMM+                   
echo         .o////////::+++++++oo++///+syyyymMmNmmmNMMm                   
echo         -+//////////o+ooooooosydmdddhhsosNMMmNNNmho            `:/    
echo         .+++++++++++ssss+//oyyysso/:/shmshhs+:.          `-/oydNNNy   
echo           `..-:/+ooss+-`          +mmhdy`           -/shmNNNNNdy+:`   
echo                   `.              yddyo++:    `-/oymNNNNNdy+:`        
echo                                   -odhhhhyddmmmmmNNmhs/:`             
echo                                     :syhdyyyyso+/-`
set /p s=
if %s%==1 goto update_files
if %s%==2 goto 1
if %s%==3 goto whatsnew
goto update_notice
:update_files
cls
echo %header%
echo.                                                                       
echo              `..````                                                  
echo              yNNNNNNNNMNNmmmmdddhhhyyyysssooo+++/:--.`                
echo              hNNNNNNNNMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMd                
echo              ddmNNd:dNMMMMNMMMMMMMMMMMMMMMMMMMMMMMMMMs                
echo             `mdmNNy dNMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM+        
echo             .mmmmNs mNMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM:                
echo             :mdmmN+`mNMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM.                
echo             /mmmmN:-mNMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMN            
echo             ommmmN.:mMMMMMMMMMMMMmNMMMMMMMMMMMMMMMMMd                 
echo             smmmmm`+mMMMMMMMMMNhMNNMNNMMMMMMMMMMMMMMy                 
echo             hmmmmh omMMMMMMMMMmhNMMMmNNNNMMMMMMMMMMM+                 
echo ------------------------------------------------------------------------------------------------------------------------------
echo    /---\   Updating...
echo   /     \  Please wait...
echo  /   ^^!   \ 
echo  --------- WiiLink24 Patcher will restart shortly...
echo.  
echo.
echo ------------------------------------------------------------------------------------------------------------------------------
echo           -mddmmo`mNMNNNNMMMNNNmdyoo+mMMMNmNMMMNyyys                  
echo           :mdmmmo-mNNNNNNNNNNdyo++sssyNMMMMMMMMMhs+-                  
echo          .+mmdhhmmmNNNNNNmdysooooosssomMMMNNNMMMm                     
echo          o/ossyhdmmNNmdyo+++oooooosssoyNMMNNNMMMM+                    
echo          o/::::::://++//+++ooooooo+oo++mNMMmNNMMMm                    
echo         `o//::::::::+////+++++++///:/+shNMMNmNNmMM+                   
echo         .o////////::+++++++oo++///+syyyymMmNmmmNMMm                   
echo         -+//////////o+ooooooosydmdddhhsosNMMmNNNmho            `:/    
echo         .+++++++++++ssss+//oyyysso/:/shmshhs+:.          `-/oydNNNy   
echo           `..-:/+ooss+-`          +mmhdy`           -/shmNNNNNdy+:`   
echo                   `.              yddyo++:    `-/oymNNNNNdy+:`        
echo                                   -odhhhhyddmmmmmNNmhs/:`             
echo                                     :syhdyyyyso+/-`
:update_1
curl -f -L -s -S --insecure "%FilesHostedOn%/UPDATE/update_assistant.bat" --output "update_assistant.bat"
	set temperrorlev=%errorlevel%
	if not %temperrorlev%==0 goto error_updating
start update_assistant.bat -WiiLink24_Patcher
exit
:error_updating
cls
echo %header%
echo.                                                                       
echo              `..````                                                  
echo              yNNNNNNNNMNNmmmmdddhhhyyyysssooo+++/:--.`                
echo              hNNNNNNNNMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMd                
echo              ddmNNd:dNMMMMNMMMMMMMMMMMMMMMMMMMMMMMMMMs                
echo             `mdmNNy dNMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM+        
echo             .mmmmNs mNMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM:                
echo             :mdmmN+`mNMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM.                
echo             /mmmmN:-mNMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMN            
echo             ommmmN.:mMMMMMMMMMMMMmNMMMMMMMMMMMMMMMMMd                 
echo             smmmmm`+mMMMMMMMMMNhMNNMNNMMMMMMMMMMMMMMy                 
echo             hmmmmh omMMMMMMMMMmhNMMMmNNNNMMMMMMMMMMM+                 
echo ------------------------------------------------------------------------------------------------------------------------------
echo    /---\   ERROR.
echo   /     \  There was an error while downloading the update assistant.
echo  /   ^^!   \ 
echo  --------- Press any key to return to main menu.
echo.  
echo.
echo ------------------------------------------------------------------------------------------------------------------------------
echo           -mddmmo`mNMNNNNMMMNNNmdyoo+mMMMNmNMMMNyyys                  
echo           :mdmmmo-mNNNNNNNNNNdyo++sssyNMMMMMMMMMhs+-                  
echo          .+mmdhhmmmNNNNNNmdysooooosssomMMMNNNMMMm                     
echo          o/ossyhdmmNNmdyo+++oooooosssoyNMMNNNMMMM+                    
echo          o/::::::://++//+++ooooooo+oo++mNMMmNNMMMm                    
echo         `o//::::::::+////+++++++///:/+shNMMNmNNmMM+                   
echo         .o////////::+++++++oo++///+syyyymMmNmmmNMMm                   
echo         -+//////////o+ooooooosydmdddhhsosNMMmNNNmho            `:/    
echo         .+++++++++++ssss+//oyyysso/:/shmshhs+:.          `-/oydNNNy   
echo           `..-:/+ooss+-`          +mmhdy`           -/shmNNNNNdy+:`   
echo                   `.              yddyo++:    `-/oymNNNNNdy+:`        
echo                                   -odhhhhyddmmmmmNNmhs/:`             
echo                                     :syhdyyyyso+/-`
pause>NUL
goto begin_main
:whatsnew
cls
if not exist %TempStorage%\whatsnew.txt goto whatsnew_notexist
echo %header%
echo ------------------------------------------------------------------------------------------------------------------------------
echo.
echo What's new in update %updateversion%?
echo.
type "%TempStorage%\whatsnew.txt"
pause>NUL
goto update_notice
:whatsnew_notexist
cls
echo %header%
echo -----------------------------------------------------------------------------------------------------------------------------
echo.
echo Error. What's new file is not available.
echo.
echo Press any button to go back.
pause>NUL
goto update_notice

:1
cls
echo %header%
echo %line%
if exist "%TempStorage%\annoucement.txt" (
	echo --- Announcement ---
	type "%TempStorage%\annoucement.txt"
	echo.
	echo --------------------
	)
::if "%translation_download_error%"=="1" if not "%language%"=="English" ( 
::echo :-----------------------------------------------------------------------:
::echo : There was an error while downloading the up-to-date translation.      :
::echo : Your language was reverted to English.                                :
::echo :-----------------------------------------------------------------------:
::echo.
::set /a translation_download_error=0
::)
	set current_time=%time:~0,5%
	if /i "%current_time%" GEQ " 5:00" if /i "%current_time%" LSS "13:00" echo Good morning %username%^^! Welcome to the WiiLink24 Patcher.
	if /i "%current_time%" GEQ "13:00" if /i "%current_time%" LSS "18:00" echo Good afternoon %username%^^! Welcome to the WiiLink24 Patcher.
	if /i "%current_time%" GEQ "18:00" if /i "%current_time%" LEQ "23:59" echo Good evening %username%^^! Welcome to the WiiLink24 Patcher.
	if /i "%current_time%" GEQ " 0:00" if /i "%current_time%" LSS " 5:00" echo Good evening %username%^^! Welcome to the WiiLink24 Patcher.
echo.
echo What are we doing today?
echo.
echo 1. Install WiiLink24 on your Wii.
echo   The patcher will guide you through the process of installing WiiLink24.
echo.
set /p s=Choose: 
if "%s%"=="1" goto 1_install_wiilink24_1
goto 1

:disk_space_insufficient
cls
echo %header%                                                                
echo.                 
echo.                 
echo.                 
echo.                 
echo.                 
echo.                 
echo.                 
echo.                 
echo.                 
echo.                 
echo.                 
echo.
echo %line%
echo    /---\   ERROR.
echo   /     \  There is not enough space on the disk to perform the operation.
echo  /   ^!   \ Please free up some space and try again.
echo  --------- 
echo            Amount of free space required: %patching_size_required_megabytes%MB
echo.
echo       Press any key to return to main menu.
echo %line%
pause>NUL
goto begin_main


:1_install_wiilink24_1
cls
echo %header%
echo %line%
echo.
echo Please wait...
echo Preparing...
:: Check if NUS is up
curl -i -s http://nus.cdn.shop.wii.com/ccs/download/0001000248414741/tmd | findstr "HTTP/1.1" | findstr "500 Internal Server Error"
if %errorlevel%==0 goto error_NUS_DOWN
:: If returns 0, 500 HTTP code it is

:: Checking disk space
set /a patching_size_required_bytes=%patching_size_required_wii_bytes%
set /a patching_size_required_megabytes=%wii_patching_requires%

for /f "usebackq delims== tokens=2" %%x in (`wmic logicaldisk where "DeviceID='%running_on_drive%:'" get FreeSpace /format:value`) do set free_drive_space_bytes=%%x
if /i %free_drive_space_bytes% LSS %patching_size_required_bytes% goto disk_space_insufficient

goto 1_install_wiilink24_2

:1_install_wiilink24_2
cls
goto 1_install_wiilink24_3
::Reserved for later	
echo %header%
echo %line%
echo.
echo Install WiiLink24.
echo.
echo Choose installation type:
echo 1. Express (Recommended)
echo   - This will patch every channel for later use on your Wii. This includes:
echo     - Wii no Ma
echo.
echo 2. Custom
echo   - You will be asked what you want to patch.
set /p s=
if %s%==1 goto 1_install_wiilink24_3
if %s%==2 goto 1_install_wiilink24_choose_custom_install_type
goto 2_auto_ask

:1_install_wiilink24_3
set /a wiinoma_enable=1

cls
echo %header%
echo %line%
echo.
echo Hello %username%, welcome to the express installation of WiiLink24.
echo.
echo The patcher will download any files that are required to run the patcher.
echo The entire process should take about 1 to 3 minutes depending on your computer CPU and internet speed.
echo.
echo But before starting, you need to tell me one thing:
echo.
echo For Wii no Ma, what language do you want to download? 
echo.
echo 1. English
echo 2. Japanese
echo.
set /p s=Choose: 
if %s%==1 set /a region=1& goto 1_install_wiilink24_4
if %s%==2 set /a region=2& goto 1_install_wiilink24_4
goto 1_install_wiilink24_3

:1_install_wiilink24_4
cls
echo %header%
echo %line%
echo.
echo Great^^!
echo After passing this screen, any user interraction won't be needed so you can relax and let me do the work^^! :)
echo.
echo Hmm... one more thing. What was it? Ah^^! To make patching even easier, I can download everything straight to your SD Card.
echo Just plug in your SD Card right now.
echo.
echo 1. Connected^^!
echo 2. I can't connect the SD Card to my computer.
echo.
set /p s=Choose: 
if %s%==1 set /a sdcardstatus=1& set tempgotonext=1_install_wiilink24_4_summary& goto detect_sd_card
if %s%==2 set /a sdcardstatus=0& set /a sdcard=NUL& goto 1_install_wiilink24_4_summary

goto 1_install_wiilink24_4
:1_install_wiilink24_4_summary
cls
echo %header%
echo %line%
echo.
if %sdcardstatus%==0 echo Ayy caramba^^! No worries, though. You will be able to copy files later after patching.
if %sdcardstatus%==1 if %sdcard%==NUL echo Hmm... looks like an SD Card wasn't found in your system. Please choose the `Change drive letter` option
if %sdcardstatus%==1 if %sdcard%==NUL echo to set your SD Card drive letter manually.
if %sdcardstatus%==1 if %sdcard%==NUL echo.
if %sdcardstatus%==1 if %sdcard%==NUL echo Otherwise, starting patching will set copying to manual so you will have to copy them later.
if %sdcardstatus%==1 if not %sdcard%==NUL echo Congrats^^! I've successfully detected your SD Card^^! Drive letter: %sdcard%
if %sdcardstatus%==1 if not %sdcard%==NUL echo I will be able to automatically download and install everything on your SD Card^^!
echo.
echo The following process will download about 100MB of data.
echo.

echo What's next?
if %sdcardstatus%==0 echo 1. Start Patching  2. Exit
if %sdcardstatus%==1 if %sdcard%==NUL echo 1. Start patching 2. Exit 3. Change drive letter
if %sdcardstatus%==1 if not %sdcard%==NUL echo 1. Start Patching 2. Exit 3. Change drive letter

set /p s=Choose: 
if %s%==1 goto 1_install_wiilink24_5_wad_folder
if %s%==2 goto begin_main
if %s%==3 goto 1_install_wiilink24_change_drive_letter
goto 2_1_summary
:1_install_wiilink24_change_drive_letter
cls
echo %header%
echo %line%
echo [*] SD Card
echo.
echo Current SD Card Letter: %sdcard%
echo.
echo Type in the new drive letter (e.g H) 
set /p sdcard=
goto 1_install_wiilink24_4_summary

:1_install_wiilink24_5_wad_folder
if not exist "WAD" goto 1_install_wiilink24_6
cls
echo %header%
echo %line%
echo.
echo One more thing^^! I've detected WAD folder.
echo I need to delete it.
echo.
echo Can I?
echo 1. Yes
echo 2. No
set /p s=Choose: 
if %s%==1 rmdir /s /q "WAD"& goto 1_install_wiilink24_6
if %s%==2 goto 2_1_summary_wiiu
goto 1_install_wiilink24_5_wad_folder
:1_install_wiilink24_6
cls
set /a temperrorlev=0
set /a counter_done=0
set /a percent=0
set /a temperrorlev=0

::
set /a progress_downloading=0
set /a progress_wiinoma=0
set /a progress_finishing=0

if "%prerelease_status%"=="1" goto 1_install_wiilink24_6_prerelease
goto 1_install_wiilink24_7

:1_install_wiilink24_6_prerelease
cls
echo %header%
echo %line%
echo.
echo Hold up^^!
echo You're using the patcher before it's official release.
echo.
echo Please extract the .zip containing .delta patches to this folder.
echo They can be found in the developer channels in the Discord server.
echo.
echo 1. They're in place^^!
echo 2. It's all just a mistake.
echo.
set /p s=Choose: 
if %s%==1 goto 1_install_wiilink24_7
if %s%==2 goto begin_main

:1_install_wiilink24_7
::if /i %percent% GTR 0 if /i %percent% LSS 10 set /a counter_done=0
::if /i %percent% GTR 10 if /i %percent% LSS 20 set /a counter_done=1
::if /i %percent% GTR 20 if /i %percent% LSS 30 set /a counter_done=2
::if /i %percent% GTR 30 if /i %percent% LSS 40 set /a counter_done=3
::if /i %percent% GTR 40 if /i %percent% LSS 50 set /a counter_done=4
::if /i %percent% GTR 50 if /i %percent% LSS 60 set /a counter_done=5
::if /i %percent% GTR 60 if /i %percent% LSS 70 set /a counter_done=6
::if /i %percent% GTR 70 if /i %percent% LSS 80 set /a counter_done=7
::if /i %percent% GTR 80 if /i %percent% LSS 90 set /a counter_done=8
::if /i %percent% GTR 90 if /i %percent% LSS 100 set /a counter_done=9
::if %percent%==100 set /a counter_done=10

if %percent%==1 set counter_done=3
if %percent%==2 set counter_done=7
if %percent%==3 set counter_done=9


cls
echo.
echo %header%
echo ---------------------------------------------------------------------------------------------------------------------------
echo  [*] Patching... this can take some time depending on the processing speed (CPU) of your computer.
echo.
echo    Progress:
if %counter_done%==0 echo :          : 
if %counter_done%==1 echo :-         : 
if %counter_done%==2 echo :--        : 
if %counter_done%==3 echo :---       : 
if %counter_done%==4 echo :----      : 
if %counter_done%==5 echo :-----     : 
if %counter_done%==6 echo :------    : 
if %counter_done%==7 echo :-------   : 
if %counter_done%==8 echo :--------  : 
if %counter_done%==9 echo :--------- : 
if %counter_done%==10 echo :----------:
echo.
if "%progress_downloading%"=="0" echo [ ] Downloading files
if "%progress_downloading%"=="1" echo [X] Downloading files
if "%progress_wiinoma%"=="0" echo [ ] Wii no Ma
if "%progress_wiinoma%"=="1" echo [X] Wii no Ma
if "%progress_finishing%"=="0" echo [ ] Finishing...
if "%progress_finishing%"=="1" echo [X] Finishing...

call :patching_fast_travel_%percent%
if %percent%==3 goto 1_install_wiilink24_8

set /a percent=%percent%+1
goto 1_install_wiilink24_7

:files_cleanup

if exist WiinoMa_Patcher rmdir /s /q WiinoMa_Patcher
if exist 000100014843494Av1025.wad del /q 000100014843494Av1025.wad
if exist WiinoMa_tik.delta del /q WiinoMa_tik.delta
if exist WiinoMa_tmd.delta del /q WiinoMa_tmd.delta
if exist 000100014843494a.tik del /q 000100014843494a.tik
if exist 000100014843494a.tmd del /q 000100014843494a.tmd
if exist 00000001.app del /q 00000001.app
if exist 00000002.app del /q 00000002.app
if exist WiiNoMa_1.delta del /q WiiNoMa_1.delta
if exist WiiNoMa_2.delta del /q WiiNoMa_2.delta
if exist unpack rmdir /s /q unpack
exit /b 0

:patching_fast_travel_1
call :files_cleanup

::Create folders
if not exist WAD md WAD
if not exist unpack md unpack
if not exist WiinoMa_Patcher md WiinoMa_Patcher
if not exist apps\WiiModLite md apps\WiiModLite

::Download tools
curl -s -f -L --insecure "%FilesHostedOn%/WiinoMa_Patcher/{libWiiSharp.dll,Sharpii.exe,WadInstaller.dll,xdelta3.exe}" -O --remote-name-all
			set /a temperrorlev=%errorlevel%
	set modul=Downloading tools
	if not %temperrorlev%==0 goto error_patching
	move libWiiSharp.dll WiinoMa_Patcher\libWiiSharp.dll>NUL
	move Sharpii.exe WiinoMa_Patcher\Sharpii.exe>NUL
	move WADInstaller.dll WiinoMa_Patcher\WADInstaller.dll>NUL
	move xdelta3.exe WiinoMa_Patcher\xdelta3.exe>NUL
:: Download patch
:: 1 - English
:: 2 - Japanese
if %region%==1 (
curl -f -L -s -S --insecure "%FilesHostedOn%/patches/WiiNoMa_1_English.delta" -o "WiinoMa_1.delta"
	set /a temperrorlev=%errorlevel%
	set modul=Downloading English Delta
	if not %temperrorlev%==0 goto error_patching
curl -f -L -s -S --insecure "%FilesHostedOn%/patches/WiiNoMa_2_English.delta" -o "WiinoMa_2.delta"
	set /a temperrorlev=%errorlevel%
	set modul=Downloading English Delta
	if not %temperrorlev%==0 goto error_patching
curl -f -L -s -S --insecure "%FilesHostedOn%/patches/WiinoMa_tmd_EN.delta" -o "WiinoMa_tmd.delta"
	set /a temperrorlev=%errorlevel%
	set modul=Downloading English Delta
	if not %temperrorlev%==0 goto error_patching
curl -f -L -s -S --insecure "%FilesHostedOn%/patches/WiinoMa_tik_EN.delta" -o "WiinoMa_tik.delta"
	set /a temperrorlev=%errorlevel%
	set modul=Downloading English Delta
	if not %temperrorlev%==0 goto error_patching
	
set language_wiinoma=English
	)

if %region%==2 (
curl -f -L -s -S --insecure "%FilesHostedOn%/patches/WiiNoMa_1_Japanese.delta" -o "WiinoMa_1.delta"
	set /a temperrorlev=%errorlevel%
	set modul=Downloading Japanese Delta
	if not %temperrorlev%==0 goto error_patching
curl -f -L -s -S --insecure "%FilesHostedOn%/patches/WiiNoMa_2_Japanese.delta" -o "WiinoMa_2.delta"
	set /a temperrorlev=%errorlevel%
	set modul=Downloading Japanese Delta
	if not %temperrorlev%==0 goto error_patching
curl -f -L -s -S --insecure "%FilesHostedOn%/patches/WiinoMa_tmd_JPN.delta" -o "WiinoMa_tmd.delta"
	set /a temperrorlev=%errorlevel%
	set modul=Downloading Japanese Delta
	if not %temperrorlev%==0 goto error_patching
curl -f -L -s -S --insecure "%FilesHostedOn%/patches/WiinoMa_tik_JPN.delta" -o "WiinoMa_tik.delta"
	set /a temperrorlev=%errorlevel%
	set modul=Downloading Japanese Delta
	if not %temperrorlev%==0 goto error_patching
set language_wiinoma=Japanese
	)


::Wii Mod Lite
curl -f -L -s -S --insecure "%FilesHostedOn%/apps/WiiModLite/boot.dol" -o "apps/WiiModLite/boot.dol"
	set /a temperrorlev=%errorlevel%
	set modul=Downloading Wii Mod Lite
	if not %temperrorlev%==0 goto error_patching
curl -f -L -s -S --insecure "%FilesHostedOn%/apps/WiiModLite/meta.xml" -o "apps/WiiModLite/meta.xml"
	set /a temperrorlev=%errorlevel%
	set modul=Downloading Wii Mod Lite
	if not %temperrorlev%==0 goto error_patching
curl -f -L -s -S --insecure "%FilesHostedOn%/apps/WiiModLite/icon.png" -o "apps/WiiModLite/icon.png"
	set /a temperrorlev=%errorlevel%
	set modul=Downloading Wii Mod Lite
	if not %temperrorlev%==0 goto error_patching

set /a progress_downloading=1
exit /b 0

:patching_fast_travel_2
::Download WAD

call WiinoMa_Patcher\Sharpii.exe NUSD -ID 000100014843494A -wad>NUL
	set /a temperrorlev=%errorlevel%
	set modul=Downloading Wii no Ma
	if not %temperrorlev%==0 goto error_patching
call WiinoMa_Patcher\Sharpii.exe WAD -u 000100014843494Av1025.wad unpack>NUL
	set /a temperrorlev=%errorlevel%
	set modul=Downloading Wii no Ma
	if not %temperrorlev%==0 goto error_patching
move unpack\00000001.app 00000001.app>NUL
	set /a temperrorlev=%errorlevel%
	set modul=Moving Wii no Ma .app
	if not %temperrorlev%==0 goto error_patching
move unpack\00000002.app 00000002.app>NUL
	set /a temperrorlev=%errorlevel%
	set modul=Moving Wii no Ma .app
	if not %temperrorlev%==0 goto error_patching
move unpack\000100014843494a.tmd 000100014843494a.tmd>NUL
	set /a temperrorlev=%errorlevel%
	set modul=Moving Wii no Ma .tmd
	if not %temperrorlev%==0 goto error_patching
move unpack\000100014843494a.tik 000100014843494a.tik>NUL
	set /a temperrorlev=%errorlevel%
	set modul=Moving Wii no Ma .tik
	if not %temperrorlev%==0 goto error_patching


call WiinoMa_Patcher\xdelta3.exe -d -s 00000001.app WiinoMa_1.delta unpack\00000001.app>NUL
	set /a temperrorlev=%errorlevel%
	set modul=Applying Wii no Ma patch
	if not %temperrorlev%==0 goto error_patching
call WiinoMa_Patcher\xdelta3.exe -d -s 00000002.app WiinoMa_2.delta unpack\00000002.app>NUL
	set /a temperrorlev=%errorlevel%
	set modul=Applying Wii no Ma patch
	if not %temperrorlev%==0 goto error_patching
call WiinoMa_Patcher\xdelta3.exe -d -s 000100014843494a.tmd WiinoMa_tmd.delta unpack\000100014843494a.tmd>NUL
	set /a temperrorlev=%errorlevel%
	set modul=Applying Wii no Ma patch
	if not %temperrorlev%==0 goto error_patching
call WiinoMa_Patcher\xdelta3.exe -d -s 000100014843494a.tik WiinoMa_tik.delta unpack\000100014843494a.tik>NUL
	set /a temperrorlev=%errorlevel%
	set modul=Applying Wii no Ma patch
	if not %temperrorlev%==0 goto error_patching


call WiinoMa_Patcher\Sharpii.exe WAD -p unpack\ "WAD\Wii no Ma (%language_wiinoma%) (WiiLink24).wad">NUL
	set /a temperrorlev=%errorlevel%
	set modul=Packing Wii no Ma WAD
	if not %temperrorlev%==0 goto error_patching

set /a progress_wiinoma=1
exit /b 0

:patching_fast_travel_3
if not %sdcard%==NUL echo.&echo Copying files. This may take a while. Give me a second.
if not %sdcard%==NUL xcopy /y "WAD" "%sdcard%:\WAD\" /e || set /a errorcopying=1
if not %sdcard%==NUL xcopy /y "apps" "%sdcard%:\apps\" /e|| set /a errorcopying=1

call :files_cleanup

set /a progress_finishing=1
exit /b 0

:1_install_wiilink24_8
cls
echo.
echo %header%
echo %line%
echo Patching done^^!
echo.
if %sdcardstatus%==0 echo Please connect your Wii SD Card and copy apps and WAD folder to the root (main folder) of your SD Card.
if %sdcardstatus%==1 if %sdcard%==NUL echo Please connect your Wii SD Card and copy apps and WAD folder to the root (main folder) of your SD Card.

if %sdcardstatus%==1 if not %sdcard%==NUL if %errorcopying%==0 echo Every file is in it's place on your SD Card^^!
if %sdcardstatus%==1 if not %sdcard%==NUL if %errorcopying%==1 echo You can find these folders next to WiiLink24Patcher.bat
echo.
echo Please install the .WAD file on your Wii by using Wii Mod Lite.
echo I also attached it with the WAD.
echo.
echo What next?
echo.
echo 1. Go back to main menu.
echo 2. Exit
echo.
set /p s=Choose: 
if %s%==1 goto begin_main
if %s%==2 goto end
goto 1_install_wiilink24_8
:end
set /a exiting=10
set /a timeouterror=1
timeout 1 /nobreak >NUL && set /a timeouterror=0
goto end1
:end1
cls
echo.
echo %header%
echo %line%
echo  [*] Thanks for using WiiLink24 Patcher^^!
echo.
echo Closing the patcher in:
echo.
if %exiting%==10 echo :----------: 10
if %exiting%==9 echo :--------- : 9
if %exiting%==8 echo :--------  : 8
if %exiting%==7 echo :-------   : 7
if %exiting%==6 echo :------    : 6
if %exiting%==5 echo :-----     : 5
if %exiting%==4 echo :----      : 4
if %exiting%==3 echo :---       : 3
if %exiting%==2 echo :--        : 2
if %exiting%==1 echo :-         : 1
if %exiting%==0 echo :          :
if %exiting%==0 exit
if %timeouterror%==0 timeout 1 /nobreak >NUL
if %timeouterror%==1 ping localhost -n 2 >NUL
set /a exiting=%exiting%-1
goto end1

:error_patching
cls
echo %header%                                                                
echo              `..````                                                  
echo              yNNNNNNNNMNNmmmmdddhhhyyyysssooo+++/:--.`                
echo              hNNNNNNNNMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMd                
echo              ddmNNd:dNMMMMNMMMMMMMMMMMMMMMMMMMMMMMMMMs                
echo             `mdmNNy dNMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM+        
echo             .mmmmNs mNMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM:                
echo             :mdmmN+`mNMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMM.                
echo             /mmmmN:-mNMMMMMMMMMMMMMMMMMMMMMMMMMMMMMMN            
echo             ommmmN.:mMMMMMMMMMMMMmNMMMMMMMMMMMMMMMMMd                 
echo             smmmmm`+mMMMMMMMMMNhMNNMNNMMMMMMMMMMMMMMy                 
echo             hmmmmh omMMMMMMMMMmhNMMMmNNNNMMMMMMMMMMM+                 
echo ---------------------------------------------------------------------------------------------------------------------------
echo    /---\   ERROR.
echo   /     \  There was an error while patching.
echo  /   ^^!   \ Error Code: %temperrorlev%
echo  --------- Failing module: %modul% / %percent%
echo.
echo	Please contact KcrPL#4625 on Discord regarding this error.
echo       Press any key to return to main menu.
echo ---------------------------------------------------------------------------------------------------------------------------
echo           :mdmmmo-mNNNNNNNNNNdyo++sssyNMMMMMMMMMhs+-                  
echo          .+mmdhhmmmNNNNNNmdysooooosssomMMMNNNMMMm                     
echo          o/ossyhdmmNNmdyo+++oooooosssoyNMMNNNMMMM+                    
echo          o/::::::://++//+++ooooooo+oo++mNMMmNNMMMm                    
echo         `o//::::::::+////+++++++///:/+shNMMNmNNmMM+                   
echo         .o////////::+++++++oo++///+syyyymMmNmmmNMMm                   
echo         -+//////////o+ooooooosydmdddhhsosNMMmNNNmho            `:/    
echo         .+++++++++++ssss+//oyyysso/:/shmshhs+:.          `-/oydNNNy   
echo           `..-:/+ooss+-`          +mmhdy`           -/shmNNNNNdy+:`   
echo                   `.              yddyo++:    `-/oymNNNNNdy+:`        
echo                                   -odhhhhyddmmmmmNNmhs/:`             
pause>NUL
goto begin_main