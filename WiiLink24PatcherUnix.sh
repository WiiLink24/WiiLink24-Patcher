#!/usr/bin/env bash

FilesHostedOn1=https://raw.githubusercontent.com/RiiConnect24/IOS-Patcher/master/UNIX
FilesHostedOn2=https://kcrpl.github.io/Patchers_Auto_Update/WiiLink24-Patcher/v1

version=1.1

path=`dirname -- "$0"`

last_build=2021/01/20
at=1:30PM

helpmsg="Please contact SketchMaster2001#0024 on Discord regarding this error." 

header() {
        clear
        printf "\033[1mWiiLink24 Patcher v$version Created by Noah Pistilli. Copyright(c) 2021 Noah Pistilli\033[0m\nUpdated on $last_build at $at\n" | fold -s -w "$(tput cols)"
        printf -- "=%.0s" $(seq "$(tput cols)") && printf "\n\n"
}

choose() {
        read -p "Choose: " s
}
    
check_dependency() {
        if [ -z "$2" ]; then
         # Expect that the package name is the same as the command being searched for.
        package_name=$1
        else
         # The package name was specified to be different.
         package_name=$2
        fi

        if ! command -v $1 &> /dev/null; then
        case "$OSTYPE" in
                darwin*) echo >&2 "Cannot find the command $1. You can use 'brew install $package_name' to get this required package. If you don't have brew installed, please install at https://brew.sh/" ;;
                *) echo >&2 "Cannot find the command $1. Please install $package_name with your package manager, or compile and add it to your path." ;;
        esac

        exit 1
        fi
}

check_dependencies() {
        case "$OSTYPE" in
                linux*) check_dependency xdelta3 ;;
                # Via Homebrew, xdelta3's binary is named exactly such, but the package is "xdelta".
                darwin*) check_dependency xdelta3 xdelta ;;
        esac

        check_dependency mono
        check_dependency curl
}

main() {
        clear
        header 1 "Start"
        printf "WiiLink24 Patcher\n\n1. Start\n2. Credits\n\n"
        read -p "Choose:" b
}

# Reset if possible
rm -rf $path/WiinoMa_Patcher $path/unpack
check_dependencies
main

number_1() {
        clear
        header 
        printf "Hello $(whoami). Welcome to the WiiLink24 Patcher.\nThe patcher will guide you through the process of installing WiiLink24.\n\nWhat are we doing today?\n\n1. Install WiiLink24 on your Wii:\n\n" | fold -s -w "$(tput cols)"
        choose

        if [ "$s" == "1" ]; then lang_choose; fi
}

lang_choose() {
        clear
        header
        printf "Hello $(whoami). Welcome to the WiiLink24 Installation Process.\n\nThe patcher will download any files that are required to run the patcher.\n\nThe entire process should take about 1 to 3 minutes depending on your computer CPU and internet speed.\n\nBut before starting, you need to tell me one thing:\nFor Wii no Ma, what language do you want to download?\n\n1. English\n2. Japanese\n\n" | fold -s -w "$(tput cols)"
        choose

        case $s in
                1) reg=EN; lang=English; sd_status ;;
                2) reg=JPN; lang=Japanese; sd_status ;;
        esac
}

sd_status() {
        clear
        header  
        printf "After passing this screen, any user interraction won't be needed so you can relax and let me do the work!\n\nTo make patching even easier, I can download everything straight to your SD Card.\n\nPlug in your SD Card right now.\n\n1. Connected\n2. I can't connect my SD Card to my computer\n\n" | fold -s -w "$(tput cols)"
        choose

        case $s in
                1) sdstatus=1; detect_sd_card ;;
                2) sdstatus=0; sdcard=null; pre_patch
        esac
}       

detect_sd_card() {
        for f in /Volumes/*/; do
                if [[ -d $f/apps ]]; then
                sdcard="$f"
                echo $sdcard
        fi
        done

        pre_patch
}

pre_patch() {
        clear
        header 
        case $sdstatus,$sdcard in
                0,null) printf "Aww, no worries. You will be able to copy files later after patching.\n\nThe entire patching process will download about 100MB of data.\n\nWhat's next?\n\n1. Start Patching\n2. Exit\n\n" | fold -s -w "$(tput cols)" ;;
                1,null) printf "Hmm... looks like an SD Card wasn't found in your system.\n\nPlease choose the Change volume name option to set your SD Card volume name manually\n\nOtherwise, you will have to copy them later\n\nThe entire patching process will download about 100MB of data.\n\nWhat's next?\n\n1. Start Patching\n2. Exit\n3. Change Volume Name\n\n" | fold -s -w "$(tput cols)" ;;
                1,*) printf "Congrats! I've successfully detected your SD Card! Volume name: "$sdcard".\n\nI will be able to automatically download and install everything on your SD Card!\n\nThe entire patching process will download about 100MB of data.\n\nWhat's next?\n\n1. Start Patching\n2. Exit\n3. Change Volume Name\n\n" | fold -s -w "$(tput cols)" ;;
        esac
        read -p "Choose: " s

        case $s in
                1) patch_1 ;; 
                2) main ;;
                3) vol_name ;;
                *) printf "Invalid Selection\n"; sleep 2; pre_patch ;;
        esac
}

vol_name() {
        clear
        header 
        printf "\[*] SD Card\n\nCurrent SD Card Volume Name: $sdcard\n\nType in the new volume name (e.g. /Volumes/Wii)\n\n"
        read -p "" sdcard

        pre_patch
}

patch_1() {
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
        header 
        printf "\033[1;91mAn error has occurred.\033[0m\n\nERROR DETAILS:\n\t* Task: %s\n\t* Command: %s\n\t* Line: %s\n\t* Exit code: %s\n\n" "$task" "$BASH_COMMAND" "$1" "$2" | fold -s -w "$(tput cols)"

        printf "%s\n" "$helpmsg" | fold -s -w "$(tput cols)"
        exit
}

trap 'error $LINENO $?' ERR
set -o pipefail
set -o errtrace

patch_2() {
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
        header 
        printf "[*] Patching... this can take some time\n\n Progress: "
        
        case $counter_done in
                0) echo ":          : $percent" ;;
                1) echo ":-         : $percent" ;;
                2) echo ":--        : $percent" ;;
                3) echo ":---       : $percent" ;;
                4) echo ":----      : $percent" ;;
                5) echo ":-----     : $percent" ;;
                6) echo ":------    : $percent" ;;
                7) echo ":-------   : $percent" ;;
                8) echo ":--------  : $percent" ;;
                9) echo ":--------- : $percent" ;;
                10) echo ":----------: $percent" ;;
        esac
 
        case $percent in
                1) if [ ! -d $path/WAD ]; then mkdir $path/WAD; fi ;;
                2) if [ ! -d $path/apps ]; then mkdir $path/apps; mkdir $path/apps/wiimodlite; fi ;;
                3) mkdir $path/WiinoMa_Patcher ;;
                4) mkdir $path/unpack ;;
                #Downloading Files
                5) task="Downloading Files"; curl -f -s -o $path/WiinoMa_Patcher/libWiiSharp.dll "$FilesHostedOn1/libWiiSharp.dll" ;;
                8) curl -f -s -o $path/WiinoMa_Patcher/WadInstaller.dll "$FilesHostedOn1/WadInstaller.dll" ;;
                12) curl -f -s -o $path/WiinoMa_Patcher/Sharpii.exe "$FilesHostedOn1/Sharpii.exe" ;;
                13) curl -f -s -o $path/WiinoMa_Patcher/Sharpii.exe.config "$FilesHostedOn1/Sharpii.exe.config" ;;
                15) curl -f -s "$FilesHostedOn2/patches/WiiNoMa_1_$lang.delta" -o $path/WiinoMa_Patcher/WiinoMa_1.delta ;;
                18) curl -f -s "$FilesHostedOn2/patches/WiiNoMa_2_$lang.delta" -o $path/WiinoMa_Patcher/WiinoMa_2.delta ;;
                21) curl -f -s "$FilesHostedOn2/patches/WiinoMa_tmd_$reg.delta" -o $path/WiinoMa_Patcher/WiinoMa_tmd.delta ;;
                24) curl -f -s "$FilesHostedOn2/patches/WiinoMa_tik_$reg.delta" -o $path/WiinoMa_Patcher/WiinoMa_tik.delta ;;
                #Patching WAD
                38) task="Patching Wii no Ma"; mono $path/WiinoMa_Patcher/Sharpii.exe NUSD -ID 000100014843494A -o $path/WiinoMa_Patcher -all ;;
                41) mono $path/WiinoMa_Patcher/Sharpii.exe WAD -u $path/WiinoMa_Patcher/000100014843494av1025.wad $path/unpack ;;
                44) xdelta3 -f -d -s $path/unpack/00000001.app $path/WiinoMa_Patcher/WiinoMa_1.delta $path/unpack/00000001.app ;;
                47) xdelta3 -f -d -s $path/unpack/00000002.app $path/WiinoMa_Patcher/WiinoMa_2.delta $path/unpack/00000002.app ;;
                50) xdelta3 -f -d -s $path/unpack/000100014843494a.tmd $path/WiinoMa_Patcher/WiinoMa_tmd.delta $path/unpack/000100014843494a.tmd ;;
                53) xdelta3 -f -d -s $path/unpack/000100014843494a.tik $path/WiinoMa_Patcher/WiinoMa_tik.delta $path/unpack/000100014843494a.tik ;;
                56) mono $path/WiinoMa_Patcher/Sharpii.exe WAD -p $path/unpack $path/WAD/"Wii no Ma ($lang) (WiiLink24).wad" ;;
                #Downloading Wii Mod Lite
                59) task="Downloading Wii Mod Lite"; curl -f -s --insecure "$FilesHostedOn2/apps/WiiModLite/boot.dol" -o $path/apps/wiimodlite/boot.dol ;;
                62) curl -f -s --insecure "$FilesHostedOn2/apps/WiiModLite/meta.xml" -o $path/apps/wiimodlite/meta.xml ;;
                65) curl -f -s --insecure "$FilesHostedOn2/apps/WiiModLite/icon.png" -o $path/apps/wiimodlite/icon.png ;;
                68) if [ $sdcard != null ]; then cp -r $path/WAD $sdcard; fi ;;
                72) if [ $sdcard != null ]; then cp -r $path/apps $sdcard; fi ;;
                #Clean up, Clean up
                81) rm -rf $path/unpack ;;
                84) rm -rf $path/WiinoMa_Patcher;;
        esac
        finish 
}

finish() {
        clear
        header 
        case $sdstatus,$sdcard in
                0,null) printf "Please connect your Wii SD Card and copy the "apps" and "WAD" folders to the root (main folder) of your SD Card. You can find these folders in the Downloads folder of your computer.\n\nPlease proceed with the tutorial that you can find on https://wii.guide/wiilink24.\n\n" | fold -s -w "$(tput cols)" ;;
                1,null) printf "Please connect your Wii SD Card and copy the "apps" and "WAD" folders to the root (main folder) of your SD Card. You can find these folders in the Downloads folder of your computer.\n\nPlease proceed with the tutorial that you can find on https://wii.guide/wiilink24.\n\n" | fold -s -w "$(tput cols)" ;;
                1,*) printf "Every file is in its place on your SD Card!\n\nPlease proceed with the tutorial that you can find on https://wii.guide/wiilink24.\n\n" | fold -s -w "$(tput cols)" ;;
        esac
}

case $b in
        1) number_1 ;;
        2) credits ;;
esac
