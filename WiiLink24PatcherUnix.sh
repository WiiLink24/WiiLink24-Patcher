#!/usr/bin/env bash

FilesHostedOn1="https://sketchmaster2001.github.io/RC24_Patcher/Sharpii"
FilesHostedOn2=https://kcrpl.github.io/Patchers_Auto_Update/WiiLink24-Patcher/v1

version=1.0.2
last_build=2021/01/20
at=1:30PM

helpmsg="Please contact SketchMaster2001#0024 on Discord regarding this error." 

cd $(dirname ${0})

#Uses 1 function instead of rewriting "Sharpii...xdelta...curl" for when WiiLink Supports more than 1 Channel
patchtitle () {
	./WiiLink_Patcher/Sharpii nusd -id ${2} -o WiiLink_Patcher/${1} -wad -q
	./WiiLink_Patcher/Sharpii wad -u WiiLink_Patcher/${1}/${2}v1025.wad WiiLink_Patcher/${1} -q
	
	xdelta3 -f -d -s WiiLink_Patcher/${1}/${3}.app WiiLink_Patcher/${4}.delta WiiLink_Patcher/${1}/${3}.app
	xdelta3 -f -d -s WiiLink_Patcher/${1}/${5}.app WiiLink_Patcher/${6}.delta WiiLink_Patcher/${1}/${5}.app
        xdelta3 -f -d -s WiiLink_Patcher/${1}/${7} WiiLink_Patcher/${8}.delta WiiLink_Patcher/${1}/${7}
        xdelta3 -f -d -s WiiLink_Patcher/${1}/${9} WiiLink_Patcher/${10}.delta WiiLink_Patcher/${1}/${9}
	
	./WiiLink_Patcher/Sharpii wad -p WiiLink_Patcher/${1} "WAD/${11} ($lang).wad" -f -q
} 

#Downloads Patches
dwnpatch(){
    curl --create-dirs -f -s $FilesHostedOn2/patches/${1} -o WiiLink_Patcher/${3}/${2}
}

#System/Architecture Detector
case $(uname -m),$(uname) in
	x86_64,Darwin)
		sys="macOS"
		mount=/Volumes
		;;
	x86_64,*)
		sys="linux-x64"
		mount=/mnt
		;;
	*,*)
		sys="linux-arm"
		mount=/mnt
		;;
esac

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

        check_dependency curl
}

# Reset if possible

rm -rf WiiLink_Patcher 
check_dependencies

lang_choose() {
        clear
        header
        printf "Hello $(whoami). Welcome to the WiiLink24 Installation Process.\n\nThe patcher will download any files that are required to run the patcher.\n\nThe entire process should take about 1 to 3 minutes depending on your computer CPU and internet speed.\n\nBut before starting, you need to tell me one thing:\nFor Wii no Ma, what language do you want to download?\n\n1. English\n2. Japanese\n\n" | fold -s -w "$(tput cols)"
        choose

        case $s in
                1) reg=EN; lang=English; sd_status ;;
                2) reg=JPN; lang=Japanese; sd_status ;;
                *) printf "Invalid Selection\n"; sleep 2; lang_choose ;;
        esac
}

sd_status() {
        clear
        header  
        printf "After passing this screen, any user interraction won't be needed so you can relax and let me do the work!\n\nTo make patching even easier, I can download everything straight to your SD Card.\n\nPlug in your SD Card right now.\n\n1. Connected\n2. I can't connect my SD Card to my computer\n\n" | fold -s -w "$(tput cols)"
        choose

        case $s in
                1) sdstatus=1; detect_sd_card ;;
                2) sdstatus=0; sdcard=null; pre_patch ;;
                *) printf "Invalid Selection\n"; sleep 2; sd_status ;;
        esac
}       

detect_sd_card() {
        for f in ${mount}/*/; do
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
                1) patch ;; 
                2) main ;;
                3) vol_name  ;;
                *) printf "Invalid Selection\n"; sleep 2; pre_patch ;;
        esac
}

vol_name() {
        clear
        header 
        printf "[*] SD Card\n\nCurrent SD Card Volume Name: $sdcard\n\nType in the new volume name (e.g. /Volumes/Wii)\n\n"
        read -p "" sdcard

        pre_patch
}

#Will serve more of a purpose when more channels are added
refresh() {
    clear
    header
    printf "Patching... This may take some time depending on your CPU and Internet speed\n\n"
    if [ "$patch0" == "1" ]
	then
		printf "[X] Patching Wii no Ma\n"
	else
		printf "[ ] Patching Wii no Ma\n"
	fi
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

#Looks more like the batch patcher without all of the percentages
patch() {
        refresh
        patch0=0
    
        mkdir -p WAD 
        mkdir -p apps; mkdir -p apps/wiimodlite 
        mkdir WiiLink_Patcher 
    
        #Downloading Files
        task="Downloading Files"
        curl -f -s -o "WiiLink_Patcher/Sharpii" "$FilesHostedOn1/sharpii($sys)"
        chmod +x WiiLink_Patcher/Sharpii
    
        dwnpatch "WiiNoMa_1_$lang.delta" "WiinoMa_1.delta" 
        dwnpatch "WiiNoMa_2_$lang.delta" "WiinoMa_2.delta"
        dwnpatch "WiinoMa_tmd_$reg.delta" "WiinoMa_tmd.delta" 
        dwnpatch "WiinoMa_tik_$reg.delta" "WiinoMa_tik.delta" 
    
        #Patching WAD
        task="Patching Wii no Ma" 
        patchtitle WiinoMa 000100014843494A  00000001 WiinoMa_1 00000002 WiinoMa_2 000100014843494a.tmd WiinoMa_tmd 000100014843494a.tik WiinoMa_tik "Wii no Ma" 

        patch0=1

        refresh
                 
        #Downloading Wii Mod Lite
        task="Downloading Wii Mod Lite" 
        curl -f -s --insecure "$FilesHostedOn2/apps/WiiModLite/boot.dol" -o apps/wiimodlite/boot.dol 
        curl -f -s --insecure "$FilesHostedOn2/apps/WiiModLite/meta.xml" -o apps/wiimodlite/meta.xml 
        curl -f -s --insecure "$FilesHostedOn2/apps/WiiModLite/icon.png" -o apps/wiimodlite/icon.png 
        if [ $sdcard != null ]; then cp -r WAD $sdcard; fi 
        if [ $sdcard != null ]; then cp -r $apps $sdcard; fi 
               
        #Clean up, Clean up
        rm -rf WiiLink_Patcher

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

        read -n 1 -p "Press any key to exit."

        exit
}

credits() {
        clear 
        header
        printf "Credits:\n\n    - SketchMaster2001: Unix Patcher\n\n    - TheShadowEevee: Sharpii-NetCore\n\n    - person66, and leathl: Original Sharpii, and libWiiSharp developers\n\n     WiiLink24 website: https://wiilink24.com\n\n"

        read -n 1 -p "Press any key to return to the main menu."
}

while true
do  
        clear
        header 1 "Start"
        printf "WiiLink24 Patcher\n\n1. Start\n2. Credits\n\n"
        read -p "Choose: " b

        case $b in
                1) lang_choose ;;
                2) credits ;;
                *) printf "Invalid Selection\n"; sleep 2 ;;
        esac
done
