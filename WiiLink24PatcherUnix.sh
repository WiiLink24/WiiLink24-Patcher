#!/bin/bash

# I am not sure if this works on Linux yet. Tested perfectly on macOS

FilesHostedOn1=https://raw.githubusercontent.com/RiiConnect24/IOS-Patcher/master/UNIX
FilesHostedOn2=https://kcrpl.github.io/Patchers_Auto_Update/WiiLink24-Patcher/v1

version=0.1

last_build=2021/01/08
at=8:30PM

header="WiiLink24 Patcher v$version Created by Noah Pistilli Updated on $last_build at $at"
header2="-----------------------------------------------------------------------------------------------------------------------------"
helpmsg="Please contact SketchMaster2001#0024 on Discord regarding this error." #change the name to whoever I guess

function check_dependency {
  if [ -z "$2" ]; then
    # Expect that the package name is the same as the command being searched for.
    package_name=$1
  else
    # The package name was specified to be different.
    package_name=$2
  fi

  if ! command -v $1 &> /dev/null; then
    case "$OSTYPE" in
      darwin*)
        echo >&2 "Cannot find the command $1. You can use 'brew install $package_name' to get this required package. If you don't have brew installed, please install at https://brew.sh/" ;;
      *)
        echo >&2 "Cannot find the command $1. Please install $package_name with your package manager, or compile and add it to your path." ;;
    esac

    exit 1
  fi
}

function check_dependencies {
  case "$OSTYPE" in
  linux*) check_dependency xdelta3 ;;
  # Via Homebrew, xdelta3's binary is named exactly such, but the package is "xdelta".
  darwin*) check_dependency xdelta3 xdelta ;;
  esac

  check_dependency mono
  check_dependency curl
}

function main {
    clear
    printf "\"$header"\"$header2"\nWiiLink24 Patcher\n\n1. Start\n2. Credits\n\n"
    read -p "Choose:" b
}


# Reset if possible
rm -rf WiinoMa_Patcher unpack
check_dependencies
main

function number_1 {
    clear
    printf "\"$header"\"$header2"\nHello $(whoami). Welcome to the WiiLink24 Patcher.\nThe patcher will guide you through the process of installing WiiLink24.\n\nWhat are we doing today?\n\n1. Install WiiLink24 on your Wii:\n\n"
    read -p "Choose:" s

    if [ "$s" == "1" ]; then lang_choose; fi
}

function lang_choose {
    clear
    printf "\"$header"\"$header2"\nHello $(whoami). Welcome to the WiiLink24 Installation Process.\n\nThe patcher will download any files that are required to run the patcher.\n\nThe entire process should take about 1 to 3 minutes depending on your computer CPU and internet speed.\n\nBut before starting, you need to tell me one thing:\nFor Wii no Ma, what language do you want to download?\n\n1. English\n2. Japanese\n\n "
    read -p "Choose:" s

    if [ "$s" == "1" ]; then reg=1; lang=English; sd_status
    elif [ "$s" == "2" ]; then reg=2; lang=Japanese; sd_status; fi
}

function sd_status {
    clear
    printf "\"$header"\"$header2"\nAfter passing this screen, any user interraction won't be needed so you can relax and let me do the work!\n\nTo make patching even easier, I can download everything straight to your SD Card.\n\nPlug in your SD Card right now.\n\n1. Connected\n2. I can't connect my SD Card to my computer\n\n"
    read -p "Choose:" s

    if [ "$s" == "1" ]; then sdstatus=1; detect_sd_card
    elif [ "$s" == "2" ]; then sdstatus=0; pre_patch; fi
}

function detect_sd_card {
    sdcard=null
    for f in /Volumes/*/; do
        if [[ -d $f/apps ]]; then
            sdcard="$f"
            echo $sdcard
        fi
    done

    pre_patch
}

function pre_patch {
    clear
    if [ $sdstatus == 0 ]; then printf "\"$header"\"$header2"\n\nAww, no worries. You will be able to copy files later after patching.\n\nThe entire patching process will download about 100MB of data.\n\nWhat's next?\n\n1. Start Patching\n2. Exit\n\n"; fi
    if [[ $sdstatus == 1 && $sdcard == null ]]; then printf "\"$header"\"$header2"\n\nHmm... looks like an SD Card wasn't found in your system.\n\nPlease choose the Change volume name option to set your SD Card volume name manually\n\nOtherwise, you will have to copy them later\n\nThe entire patching process will download about 100MB of data.\n\nWhat's next?\n\n1. Start Patching\n2. Exit\n3. Change Volume Name\n\n"; fi
    if [[ $sdstatus == 1 && $sdcard != null ]]; then printf "\"$header"\"$header2"\n\nCongrats! I've successfully detected your SD Card! Volume name: "$sdcard".\n\nI will be able to automatically download and install everything on your SD Card!\n\nThe entire patching process will download about 100MB of data.\n\nWhat's next?\n\n1. Start Patching\n2. Exit\n3. Change Volume Name\n\n"; fi
    read -p "Choose: " s

    if [ "$s" == 1 ]; then patch_1
    elif [ "$s" == 2 ]; then main
    elif [ "$s" == 3 ]; then vol_name; fi
}

function vol_name {
    clear
    echo "\"$header"\"$header2"\n[*] SD Card\n\nCurrent SD Card Volume Name: $sdcard\n\nType in the new volume name (e.g. /Volumes/Wii)\n\n"
    read -p "" sdcard

    pre_patch
}

function patch_1 {
    clear
    counter_done=0
    percent=0

    for i in {0..99}; do
        patch_2
    done
}

#Error Detection
error() {
    clear
    printf "\033[1;91mAn error has occurred.\033[0m\n\nERROR DETAILS:\n\t* Task: %s\n\t* Command: %s\n\t* Line: %s\n\t* Exit code: %s\n\n" "$task" "$BASH_COMMAND" "$1" "$2" | fold -s -w "$(tput cols)"

    printf "%s\n" "$helpmsg" | fold -s -w "$(tput cols)"
    exit
}

trap 'error $LINENO $?' ERR
set -o pipefail
set -o errtrace

function patch_2 {
    percent=$((percent+1))

    if [[ $percent -gt 0 && $percent -lt 10 ]]; then counter_done=0; fi
    if [[ $percent -ge 10 && $percent -lt 20 ]]; then counter_done=1; fi
    if [[ $percent -ge 20 && $percent -lt 30 ]]; then counter_done=2; fi
    if [[ $percent -ge 30 && $percent -lt 40 ]]; then counter_done=3; fi
    if [[ $percent -ge 40 && $percent -lt 50 ]]; then counter_done=4; fi
    if [[ $percent -ge 50 && $percent -lt 60 ]]; then counter_done=5; fi
    if [[ $percent -ge 60 && $percent -lt 70 ]]; then counter_done=6; fi
    if [[ $percent -ge 70 && $percent -lt 80 ]]; then counter_done=7; fi
    if [[ $percent -ge 80 && $percent -lt 90 ]]; then counter_done=8; fi
    if [[ $percent -ge 90 && $percent -lt 100 ]]; then counter_done=9; fi
    if [ $percent == 100 ]; then counter_done=10; fi

    clear
    echo ""
    echo $header
    echo $header2
    echo " [*] Patching... this can take some time"
    echo ""
    echo "  Progress:"

    if [ $counter_done == 0 ]; then echo ":          : $percent"; fi
    if [ $counter_done == 1 ]; then echo ":-         : $percent"; fi
    if [ $counter_done == 2 ]; then echo ":--        : $percent"; fi
    if [ $counter_done == 3 ]; then echo ":---       : $percent"; fi
    if [ $counter_done == 4 ]; then echo ":----      : $percent"; fi
    if [ $counter_done == 5 ]; then echo ":-----     : $percent"; fi
    if [ $counter_done == 6 ]; then echo ":------    : $percent"; fi
    if [ $counter_done == 7 ]; then echo ":-------   : $percent"; fi
    if [ $counter_done == 8 ]; then echo ":--------  : $percent"; fi
    if [ $counter_done == 9 ]; then echo ":--------- : $percent"; fi
    if [ $counter_done == 10 ]; then echo ":----------: $percent"; fi

    #Make Folders

    if [ $percent = 1 ]; then mkdir WiinoMa_Patcher; fi
    if [ $percent = 2 ] && [ ! -d WAD ]; then mkdir WAD; fi
    if [ $percent = 3 ]; then mkdir unpack; fi
    if [ $percent = 4 ] && [ ! -d apps ]; then mkdir apps; mkdir apps/wiimodlite; fi


    task="Downloading Files"

    if [ $percent == 5 ]; then curl -s -o WiinoMa_Patcher/libWiiSharp.dll "$FilesHostedOn1/libWiiSharp.dll"; fi
    if [ $percent == 8 ]; then curl -s -o WiinoMa_Patcher/WadInstaller.dll "$FilesHostedOn1/WadInstaller.dll"; fi
    if [ $percent == 12 ]; then curl -s -o WiinoMa_Patcher/Sharpii.exe "$FilesHostedOn1/Sharpii.exe"; fi
    if [ $percent == 13 ]; then curl -s -o WiinoMa_Patcher/Sharpii.exe.config "$FilesHostedOn1/Sharpii.exe.config"; fi

    #English Patches

    if [[ $percent == 15 && $reg == 1 ]]; then curl -f -s "$FilesHostedOn2/patches/WiiNoMa_1_English.delta" -o WiinoMa_Patcher/WiinoMa_1.delta; fi
    if [[ $percent == 18 && $reg == 1 ]]; then curl -f -s "$FilesHostedOn2/patches/WiiNoMa_2_English.delta" -o WiinoMa_Patcher/WiinoMa_2.delta; fi
    if [[ $percent == 21 && $reg == 1 ]]; then curl -f -s "$FilesHostedOn2/patches/WiinoMa_tmd_EN.delta" -o WiinoMa_Patcher/WiinoMa_tmd.delta; fi
    if [[ $percent == 24 && $reg == 1 ]]; then curl -f -s "$FilesHostedOn2/patches/WiinoMa_tik_EN.delta" -o WiinoMa_Patcher/WiinoMa_tik.delta; fi


    #Japanese Patches

    if [[ $percent == 15 && $reg == 2 ]]; then curl -f -s "$FilesHostedOn2/patches/WiiNoMa_1_Japanese.delta" -o WiinoMa_Patcher/WiinoMa_1.delta; fi
    if [[ $percent == 18 && $reg == 2 ]]; then curl -f -s "$FilesHostedOn2/patches/WiiNoMa_2_Japanese.delta" -o WiinoMa_Patcher/WiinoMa_2.delta; fi
    if [[ $percent == 21 && $reg == 2 ]]; then curl -f -s "$FilesHostedOn2/patches/WiinoMa_tmd_JPN.delta" -o WiinoMa_Patcher/WiinoMa_tmd.delta; fi
    if [[ $percent == 24 && $reg == 2 ]]; then curl -f -s "$FilesHostedOn2/patches/WiinoMa_tik_JPN.delta" -o WiinoMa_Patcher/WiinoMa_tik.delta; fi


   task="Patching Wii no Ma"

    if [[ $percent == 38 && ! -f "WiinoMa_Patcher/000100014843494av1025.wad" ]]; then mono WiinoMa_Patcher/Sharpii.exe NUSD -ID 000100014843494A -o WiinoMa_Patcher -all; fi

    if [[ $percent == 41 ]]; then mono WiinoMa_Patcher/Sharpii.exe WAD -u WiinoMa_Patcher/000100014843494av1025.wad unpack; fi
    if [[ $percent == 44 ]]; then xdelta3 -f -d -s unpack/00000001.app WiinoMa_Patcher/WiinoMa_1.delta unpack/00000001.app; fi
    if [[ $percent == 47 ]]; then xdelta3 -f -d -s unpack/00000002.app WiinoMa_Patcher/WiinoMa_2.delta unpack/00000002.app; fi
    if [[ $percent == 50 ]]; then xdelta3 -f -d -s unpack/000100014843494a.tmd WiinoMa_Patcher/WiinoMa_tmd.delta unpack/000100014843494a.tmd; fi
    if [[ $percent == 53 ]]; then xdelta3 -f -d -s unpack/000100014843494a.tik WiinoMa_Patcher/WiinoMa_tik.delta unpack/000100014843494a.tik; fi
    if [[ $percent == 56 ]]; then mono WiinoMa_Patcher/Sharpii.exe WAD -p unpack/ WAD/"Wii no Ma ($lang) (WiiLink24).wad"; fi

   #Move to SD Card
   task="Moving to SD Card"

    if [ $percent == 69 ] && [ ! -d "$sdcard/WAD" ] && [ $sdcard != null ]; then cp -r WAD $sdcard; fi

   #File Cleanup

   if [[ $percent == 81 ]]; then rm -rf unpack; fi
   if [[ $percent == 84 ]]; then rm -rf WiinoMa_Patcher; finish; fi
}

function finish {
    clear
    echo "Patching done!"
    echo ""
    if [ $sdstatus == 0 ]; then printf "\"$header"\"$header2"\n\nPlease connect your Wii SD Card and copy the "apps" and "WAD" folders to the root (main folder) of your SD Card. You can find these folders in the Downloads folder of your computer.\n\nPlease proceed with the tutorial that you can find on https://wii.guide/wiilink24.\n\n"; fi
    if [[ $sdstatus == 1 && $sdcard == null ]]; then printf "\"$header"\"$header2"\n\nPlease connect your Wii SD Card and copy the "apps" and "WAD" folders to the root (main folder) of your SD Card. You can find these folders in the Downloads folder in your computer.\n\nPlease proceed with the tutorial that you can find on https://wii.guide/wiilink24.\n\n"; fi
    if [[ $sdstatus == 1 && $sdcard != null ]]; then printf "\"$header"\"$header2"\n\nEvery file is in its place on your SD Card!\n\nPlease proceed with the tutorial that you can find on https://wii.guide/wiilink24.\n\n"; fi
    read -p "Press 1 to close this patcher." p

    if [[ $p == "1" ]]; then exit; fi
}

if [ "$b" == "1" ]; then number_1
elif [ "$b" == "2" ]; then credits; fi
