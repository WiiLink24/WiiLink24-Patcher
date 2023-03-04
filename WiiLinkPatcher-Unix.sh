#!/usr/bin/env bash

# New links to use
WiiLinkPatcherURL=https://patcher.wiilink24.com
PabloURL=http://pabloscorner.akawah.net/WL24-Patcher # Temporary host for some files

### Build info ###
version=1.0.8.1n

last_build_en="February 28th, 2023"
at_en="5:19 PM"

# Title bar text
printf "\033]0;WiiLink Patcher v%s\007" "$version"
##################

cd "$(dirname "${0}")" || exit

# Set inital language to English
prog_language=en

# For patching the core Japanese-exclusive channels
patch_core_channel() {
    local title_id="${2}"
    local title_folder="unpack"
    local patch_folder="WiiLink_Patcher/${1}"
    local output_wad="${title_folder}/${title_id}.wad"
    local channel_name="${9}"

    # Take folder name and make all lowercase version of it to a new variable
    case $1 in
        "WiinoMa") local url_subdir="wiinoma" ;;
        "Digicam") local url_subdir="digicam" ;;
        "Demae") local url_subdir="demae" ;;
        "Dominos") local url_subdir="dominos" ;;
    esac
    
    # Check if the directory of the output file exists, create it if it doesn't
    [ ! -d "$title_folder" ] && mkdir -p "$title_folder"
    [ ! -d "$patch_folder" ] && mkdir -p "$patch_folder"
    
    task="Downloading and extracting stuff for ${channel_name}"
    ./WiiLink_Patcher/Sharpii nusd -id "$title_id" -o "$output_wad" -wad -q
    ./WiiLink_Patcher/Sharpii wad -u "$output_wad" "$title_folder" -q
    
    # Download patched TMD file and rename to ${title_id}.tmd
    curl --create-dirs --insecure -s -o ${title_folder}/"${title_id}".tmd ${WiiLinkPatcherURL}/"${url_subdir}"/"$1".tmd
    
    task="Applying delta patches for ${channel_name}"
    [ "$reg" == "EN" ] || [ "${1}" == "dominos" ] && xdelta3 -f -d -s "$title_folder/$3.app" "$patch_folder/$4.delta" "$title_folder/$3.app"
    xdelta3 -f -d -s "$title_folder/$5.app" "$patch_folder/$6.delta" "$title_folder/$5.app"
    [ "$reg" == "EN" ] || [ "${1}" == "dominos" ] || [ "${1}" == "wiinoma" ] && xdelta3 -f -d -s "$title_folder/$7.app" "$patch_folder/$8.delta" "$title_folder/$7.app"
    
    task="Repacking the title for ${channel_name}"
    ./WiiLink_Patcher/Sharpii wad -p "$title_folder" "WAD/${channel_name} ($lang).wad" -f
}

# For patching the WiiConnect24-based channels
patch_wc24_channel() {
    local title_id="${2}"
    local title_folder="unpack"
    local patch_folder="WiiLink_Patcher/${1}"
    local channel_name="${5}"
    local channel_region="${6}"
    local channel_version="${7}"
    
    # Check if the directory of the output file exists, create it if it doesn't
    [ ! -d "$title_folder" ] && mkdir -p "$title_folder"
    [ ! -d "$patch_folder" ] && mkdir -p "$patch_folder"

    task="Downloading necessary files for ${channel_name}"
                                         curl --create-dirs --insecure -s -f "${PabloURL}/WC24_Patcher/${1}/cert/${title_id}.cert" -o "${title_folder}/${title_id}.cert"
    [ "$title_id" == "$nc_title_id" ] && curl --create-dirs --insecure -s -f "${PabloURL}/WC24_Patcher/${1}/tik/${title_id}.tik" -o "${title_folder}/cetk"
        
    task="Extracting files from ${channel_name}"
    ./WiiLink_Patcher/Sharpii nusd -id "$title_id" -o "$title_folder" -q -decrypt

    task="Renaming stuff for ${channel_name}"
    mv $title_folder/tmd."$channel_version" $title_folder/"$title_id".tmd
    mv $title_folder/cetk $title_folder/"$title_id".tik
    
    task="Applying ${channel_name} patches"
    xdelta3 -f -d -s "$title_folder/${3}.app" "$patch_folder/${4}.delta" "$title_folder/${3}.app"
    
    task="Repacking the title for ${channel_name}"
    ./WiiLink_Patcher/Sharpii wad -p "$title_folder" "WAD/${channel_name} (${channel_region}) (WiiLink).wad" -f
}

download_patch() {
    local patch_url="${WiiLinkPatcherURL}/${1}/${2}"
    local patch_destination_path="WiiLink_Patcher/${4}/${3}"
    
    curl --create-dirs --insecure -f -s "$patch_url" -o "$patch_destination_path"
}

# Download the correct SPD WAD for the chosen platform
spd_download() {
    case "$platform_type" in
        "Wii") curl --create-dirs --insecure -f -s "${WiiLinkPatcherURL}/spd/SPD_Wii.wad" -o "WAD/WiiLink_SPD (Wii).wad" ;;
        "vWii") curl --create-dirs --insecure -f -s "${WiiLinkPatcherURL}/spd/SPD_vWii.wad" -o "WAD/WiiLink_SPD (vWii).wad" ;;
    esac
}

# System/Architecture Detector
case "$(uname -m),$(uname)" in
    x86_64,Darwin)
        sys="macOS-x64"
        mount=/Volumes
    ;;
    arm64,Darwin)
        sys="macOS-x64"
        mount=/Volumes
    ;;
    x86_64,Linux)
        sys="linux-x64"
        mount=/mnt
    ;;
    aarch64,Linux)
        sys="linux-arm64"
        mount=/mnt
    ;;
    x86_32,*)
        header
        printf "The x86_32 architecture is not supported by Sharpii. Please contact Sketch#4374 or PablosCorner#3037 on Discord for help.\n\n" | fold -s -w "$(tput cols)"
        exit
    ;;
    *)
        sys="linux-arm"
        mount=/mnt
    ;;
esac


# The information you see at the top of the console
header() {
    clear
    
	case $prog_language in
        "en") printf "\e[1mWiiLink Patcher v%s - (c) 2023 WiiLink\e[0m (Updated on %s at %s EST)\n" "$version" "$last_build_en" "$at_en" | fold -s -w "$(tput cols)" ;;
    esac
    printf -- "=%.0s" $(seq "$(tput cols)") | fold -s -w "$(tput cols)"
    printf "\n\n"
}

choose() {
    case $prog_language in
        "en") local message="Choose: " ;;
    esac
    
    read -n 1 -r -p "$message" choice
    printf "\n"
}

# Important in case you need to report any issues with the patcher!
announcement() {
    case $prog_language in
        "en")
            printf "\e[1;32m--- Announcement ---\e[0m\n" | fold -s -w "$(tput cols)"
            printf "If you have any issues with the patcher or services offered by WiiLink, please report them here:\n" | fold -s -w "$(tput cols)"
            printf "\e[1mhttps://discord.gg/WiiLink\e[0m - Thank you.\n" | fold -s -w "$(tput cols)"
            printf "\e[1;32m--------------------\e[0m\n\n" | fold -s -w "$(tput cols)"
            ;;
    esac
}

# Checks some things that are needed for the patcher to work
check_dependency() {
    header
    
    # Set the package name
    if [ -z "$2" ]; then
        # Expect that the package name is the same as the command being searched for.
        package_name=$1
    else
        # The package name was specified to be different.
        package_name=$2
    fi
    
    # Check if the command exists.
    if ! command -v "$1" &>/dev/null; then
        printf "\e[5;31mDependency Error:\e[0m\n\n"
        case "$OSTYPE" in
            darwin*) printf "Cannot find the command \e[1m%s\e[0m.\n\nYou can use '\e[1mbrew install %s\e[0m' to get this required package. If you don't have brew installed, please install at https://brew.sh/\n\n" "$1" "$package_name" ;;
            *) printf "Cannot find the command \e[1m%s\e[0m.\n\nPlease install \e[1m%s\e[0m with your package manager, or compile and add it to your path.\n\n" "$1" "$package_name" ;;
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

# Your journey starts here
install_choose() {
    header
    announcement

    local greeting=""
    
    # Change the greeting based on the time of day.
    current_time=$(date +%H)
    if [ "$current_time" -ge 0 ] && [ "$current_time" -lt 12 ]; then
        case $prog_language in
            "en") greeting="Good morning" ;;
        esac
    elif [ "$current_time" -ge 12 ] && [ "$current_time" -lt 18 ]; then
        case $prog_language in
            "en") greeting="Good afternoon" ;;
        esac
    else
        case $prog_language in
            "en") greeting="Good evening" ;;
        esac
    fi
    
    case $prog_language in
        "en") 
            printf "$greeting %s! Welcome to the WiiLink Patcher.\n\n" "$(whoami)" | fold -s -w "$(tput cols)"
            printf "What are we doing today?\n\n" | fold -s -w "$(tput cols)"
            printf "1. Install WiiLink on your Wii.\n" | fold -s -w "$(tput cols)"
            printf "  The patcher will guide you through the process of installing WiiLink.\n\n" | fold -s -w "$(tput cols)"
            printf "2. Go Back to Main Menu\n\n" | fold -s -w "$(tput cols)"
            ;;
    esac
    
    choose
    case $choice in
        1) lang_choose ;;
        2) main ;;
        *) printf "\n\e[1;31mInvalid Selection\e[0m\n\n"
            sleep 2
            install_choose
        ;;
    esac
}

# Choose your language for the Japanese channels
lang_choose() {
    header
    
    case $prog_language in
        "en")
            printf "Hello %s, welcome to the express installation of WiiLink!\n\n" "$(whoami)" | fold -s -w "$(tput cols)"
            printf "The patcher will download any files that are required to run the patcher.\n" | fold -s -w "$(tput cols)"
            printf "The entire process should take about 1 to 3 minutes depending on your computer CPU and internet speed.\n\n" | fold -s -w "$(tput cols)"
            printf "But before starting, you need to tell me one thing:\n\n" | fold -s -w "$(tput cols)"
            printf "\e[1mFor Wii no Ma (Wii Room), Digicam Print Channel, and Demae Channel - what language of the channels do you want to download?\e[0m\n\n" | fold -s -w "$(tput cols)"
            printf "1. English\n" | fold -s -w "$(tput cols)"
            printf "2. Japanese\n\n" | fold -s -w "$(tput cols)"
            printf "3. Go Back to Main Menu\n\n" | fold -s -w "$(tput cols)"
            ;;
    esac
    
    choose
    case $choice in
        1)	reg=EN
            lang=English
            demae_configuration
        ;;
        2)	reg=JPN
            lang=Japan
            demae_version=standard
            nc_setup
        ;;
        3) 	main ;;
        *)	printf "\n\e[1;31mInvalid Selection\e[0m\n\n"
            sleep 2
            lang_choose
        ;;
    esac
}

# Choose which version of Demae Channel you want
demae_configuration() {
    header
    
    case $prog_language in
        "en")
            printf "Alright, what version of Demae Channel do you want?\n\n" | fold -s -w "$(tput cols)"
            printf "1. Standard\n" | fold -s -w "$(tput cols)"
            printf "2. Domino's\n\n" | fold -s -w "$(tput cols)"
            printf "3. Go Back to Main Menu\n\n" | fold -s -w "$(tput cols)"
            ;;
    esac
    
    choose
    case $choice in
        1)	demae_version=standard
            nc_setup
        ;;
        2)	demae_version=dominos
            nc_setup
        ;;
        3) 	main ;;
        *)	printf "\n\e[1;31mInvalid Selection\e[0m\n\n"
            sleep 2
            demae_configuration
        ;;
    esac
}


# Nintendo Channel Region Choice
nc_setup() {
    header
    
    case $prog_language in
        "en")
            printf "Alright, what region of Nintendo Channel do you want?\n\n" | fold -s -w "$(tput cols)"
            printf "1. Europe (E)\n" | fold -s -w "$(tput cols)"
            printf "2. USA (U)\n" | fold -s -w "$(tput cols)"
            printf "3. Japan (J)\n\n" | fold -s -w "$(tput cols)"
            printf "4. Go Back to Main Menu\n\n" | fold -s -w "$(tput cols)"
            ;;
    esac
    
    choose
    case $choice in
        e|E|1)	
            nc_region="PAL"
            fc_setup ;;
        u|U|2)	
            nc_region="USA"
            fc_setup ;;
        j|J|3)	
            nc_region="Japan"
            fc_setup ;;
        4)  
            main ;;
        *)	
            printf "\n\e[1;31mInvalid Selection\e[0m\n\n"
            sleep 2
            nc_setup ;;
    esac
}

# Forecast Channel Region Choice
fc_setup(){
    header
    
    case $prog_language in
        "en")
            printf "Cool! What region of the Forecast Channel do you want?\n\n" | fold -s -w "$(tput cols)"
            printf "1. Europe (E)\n" | fold -s -w "$(tput cols)"
            printf "2. USA (U)\n" | fold -s -w "$(tput cols)"
            printf "3. Japan (J)\n\n" | fold -s -w "$(tput cols)"
            printf "4. Go Back to Main Menu\n\n" | fold -s -w "$(tput cols)"
            ;;
    esac
    
    choose
    case $choice in
        e|E|1)	
            fc_region="PAL"
            platform_choice ;;
        u|U|2)	
            fc_region="USA"
            platform_choice ;;
        j|J|3)	
            fc_region="Japan"
            platform_choice ;;
        4)
            main ;;
        *)
            printf "\n\e[1;31mInvalid Selection\e[0m\n\n"
            sleep 2
            fc_setup ;;
    esac
}

# Choose your platform
platform_choice() {
    header
    
    case $prog_language in
        "en")
            printf "Before we begin, I need to know what platform you're installing WiiLink on\n" | fold -s -w "$(tput cols)"
            printf "This setting will change the version of SPD that I will download so channels like Demae works.\n\n" | fold -s -w "$(tput cols)"
            printf "What platform are you using? (Only applies to non-Japanese Wii)\n\n" | fold -s -w "$(tput cols)"
            printf "1. Wii (or Dolphin Emulator)\n" | fold -s -w "$(tput cols)"
            printf "2. Wii U (vWii)\n\n" | fold -s -w "$(tput cols)"
            printf "3. Go Back to Main Menu\n\n" | fold -s -w "$(tput cols)"
            ;;
    esac
    
    choose
    case $choice in
        1)	platform_type="Wii"
            sd_status
        ;;
        2)	platform_type="vWii"
            sd_status
        ;;
        3)	main
        ;;
        *)	printf "\n\e[1;31mInvalid Selection\e[0m\n\n"
            sleep 2
            wl_setup_1
        ;;
    esac
}

sd_status() {
    header
    
    case $prog_language in
        "en")
            printf "Great!\n" | fold -s -w "$(tput cols)"
            printf "After passing this screen, any user interraction won't be needed so you can relax and let me do the work!\n\n" | fold -s -w "$(tput cols)"
            printf "Hmm... one more thing. What was it? Ah! To make patching even easier, I can download everything straight to your SD Card.\n" | fold -s -w "$(tput cols)"
            printf "Just plug in your SD card right now.\n\n" | fold -s -w "$(tput cols)"
            printf "1. Connected!\n" | fold -s -w "$(tput cols)"
            printf "2. I can't connect my SD Card to my computer..\n\n" | fold -s -w "$(tput cols)"
            printf "3. Go Back to Main Menu\n\n" | fold -s -w "$(tput cols)"
            ;;
    esac
    
    choose
    case $choice in
        1)
            sdstatus=1
            detect_sd_card
            pre_patch ;;
        2)	
            sdstatus=0
            sdcard=null
            pre_patch ;;
        3)	
            main ;;
        *)	
            printf "\n\e[1;31mInvalid Selection\e[0m\n\n"
            sleep 2
            wl_setup_3
        ;;
    esac
}

# Checks to see if you got an SD Card inserted
# More specifically, if it's got the apps folder
detect_sd_card() { 
    sdcard=null

    # Check if SD Card is connected and if it's a valid SD Card, while checking for the apps folder
    for f in "${mount}"/*/; do
        if [[ -d $f/apps ]]; then
            sdcard="$f"
        fi
    done
}

# The result of the SD Card check
pre_patch() {
    header
    
    # Check if SD Card is connected and if it's a valid SD Card
    case $sdstatus,$sdcard in
        0,null)
            case $prog_language in
                "en")
                    printf "Ayy caramba! No worries, though. You will be able to copy files later after patching.\n" | fold -s -w "$(tput cols)"
                    printf "The entire patching process will download about 160MB of data.\n" | fold -s -w "$(tput cols)"
                    printf "What's next?\n\n" | fold -s -w "$(tput cols)"
                    printf "1. Start Patching\n" | fold -s -w "$(tput cols)"
                    printf "2. Go Back to Main Menu\n\n" | fold -s -w "$(tput cols)"
                    ;;
            esac
            ;;
        1,null)
            case $prog_language in
                "en")
                    printf "Hmm... looks like an SD Card wasn't found in your system.\n\n" | fold -s -w "$(tput cols)"
                    printf "Please choose the Change volume name option to set your SD Card volume name manually,\n" | fold -s -w "$(tput cols)"
                    printf "otherwise, you will have to copy them later.\n\n" | fold -s -w "$(tput cols)"
                    printf "The entire patching process will download about 160MB of data.\n\n" | fold -s -w "$(tput cols)"
                    printf "What's next?\n\n" | fold -s -w "$(tput cols)"
                    printf "1. Start Patching\n" | fold -s -w "$(tput cols)"
                    printf "2. Go Back to Main Menu\n" | fold -s -w "$(tput cols)"
                    printf "3. Change Volume Name\n\n" | fold -s -w "$(tput cols)"
                    ;;
            esac
            ;;
        1,*)
            case $prog_language in
                "en")
                    printf "Congrats! I've successfully detected your SD Card!\n" | fold -s -w "$(tput cols)"
                    printf "\e[1mVolume name:\e[0m \e[1;32m%s\e[0m\n\n" "$sdcard" | fold -s -w "$(tput cols)"
                    printf "I will be able to automatically download and install everything on your SD Card!\n\n" | fold -s -w "$(tput cols)"
                    printf "The entire patching process will download about 160MB of data.\n\n" | fold -s -w "$(tput cols)"
                    printf "What's next?\n\n" | fold -s -w "$(tput cols)"
                    printf "1. Start Patching\n" | fold -s -w "$(tput cols)"
                    printf "2. Go Back to Main Menu\n" | fold -s -w "$(tput cols)"
                    printf "3. Change Volume Name\n\n" | fold -s -w "$(tput cols)"
                    ;;
            esac
            ;;
    esac
    
    choose
    case $choice in
        1) wad_setup ;;
        2) main ;;
        3) 
            case $sdcard in
                null)
                    printf "\n\e[1;31mInvalid Selection\e[0m\n\n"
                    sleep 2
                    pre_patch
                    ;;
                *)
                    vol_name
                    ;;
            esac
            ;;
        *)
            printf "\n\e[1;31mInvalid Selection\e[0m\n\n"
            sleep 2
            pre_patch
        ;;
    esac
}

# Change SD Card Volume Name if you need to
vol_name() {
    header
    
    case $prog_language in
        "en")
            printf "SD Card\n\n" | fold -s -w "$(tput cols)"
            printf "Type in the new volume name (e.g. /Volumes/Wii)\n\n" | fold -s -w "$(tput cols)"
            ;;
    esac

    read -p -r "" sdcard

    # Check if the SD Card Volume Name is mounted
    if [ ! -d "$sdcard" ]; then
        case $prog_language in
            "en")
                printf "\e[1;31mThe SD Card Volume Name you entered is not mounted!\e[0m\n\n"
                printf "Please make sure that the SD Card is mounted and try again.\n\n"
                ;;
        esac
        read -n 1 -s -r -p "Press any key to try again..."
        pre_patch
    fi

    # Check if /apps folder exists in the new drive letter, if not, display an error message and try again
    if [ ! -d "$sdcard/apps" ]; then
        case $prog_language in
            "en")
                printf "\e[1;31mA drive has been detected, however, the /apps folder was not found.\e[0m\n"
                printf "Please create it on the root of the SD Card and try again!\n\n"
                ;;
        esac
        read -n 1 -s -r -p "Press any key to try again..."
        pre_patch
    fi

    pre_patch
}

server_down() {
    header

    case $prog_language in
        "en")
            printf "\e[5;31mThe WiiLink server is currently down!\e[0m\n\n"
            printf "It seems that our server is currently down. We're trying to get it back up as soon as possible.\n\n"
            printf "Stay tuned on our Discord server for updates:\n"
            printf "\e[1;32mhttps://discord.gg/WiiLink\e[0m\n\n"
            ;;
    esac
    read -n 1 -s -r -p "Press any key to exit..."
    printf "\n"
    exit
}

# Check is server is up, if not go to server_down function and pass the exit code to it
if ! curl --silent --head --fail --insecure "${WiiLinkPatcherURL}/wiinoma/WiinoMa_1_English.delta" > /dev/null; then
    server_down
fi


# You should only ever see this if something went really wrong
error() {
    header
    
    local helpmsg=""
    case $prog_language in
        "en") helpmsg="Please open an issue on our GitHub page (https://github.com/WiiLink24/WiiLink24-Patcher/issues) and describe the issue you're having." ;;
    esac

    case $prog_language in
        "en") 
            printf "\e[5;31mAn error has occurred.\e[0m\n\nERROR DETAILS:\n" | fold -s -w "$(tput cols)"
            printf "\t* Task: %s\n" "$task" | fold -s -w "$(tput cols)"
            printf "\t* Command: %s\n" "$BASH_COMMAND" | fold -s -w "$(tput cols)"
            printf "\t* Line: %s\n" "$1" | fold -s -w "$(tput cols)"
            printf "\t* Exit code: %s\n\n" "$2" | fold -s -w "$(tput cols)"
            ;;
    esac
    printf "%s\n" "$helpmsg" | fold -s -w "$(tput cols)"
    exit
}
trap 'error $LINENO $?' ERR
set -o pipefail
set -o errtrace

# Looks like you got a WAD folder. Let's delete it, if you want to.
wad_setup() {
    # Check if WAD folder exists
    if [ ! -d "WAD" ]; then
        patch_progress
    else
        header
        
        # Ask for permission to delete the WAD folder
        case $prog_language in
            "en")
                printf "One more thing! I've detected a WAD folder.\n\n"
                printf "I need to delete it. Can I?\n\n"
                printf "1. Yes\n"
                printf "2. No\n\n"
            ;;
        esac
        
        choose
        
        # Check user input and delete the WAD folder if necessary
        case $choice in
            1)  rm -r "WAD"
                patch_progress
            ;;
            2)  main
            ;;
            *)  printf "\n\e[1;31mInvalid Selection\e[0m\n\n"
                sleep 2
                wad_setup
            ;;
        esac
    fi
}

files_cleanup() {
    local clean_choice=$1
    
    task="Cleaning up files"
    case $clean_choice in
        "channel_clean")
        	[ -d "unpack" ] && rm -r unpack 
			;;
        "setup_clean")
            [ -d "WiiLink_Patcher" ] && rm -r WiiLink_Patcher
            
			# Delete the WAD and apps folder in the current folder if they were copied to the SD Card
			if [ -d "apps" ] && [ "${sdcard}" != "null" ]; then
				rm -rf apps
			fi
			if [ -d "WAD" ] && [ "${sdcard}" != "null" ]; then
				rm -rf WAD
			fi ;;
    esac
}


# Downloading the patches and Wii Mod Lite
download_all_patches() {
    task="Downloading patches"
    
    # Downloading Sharpii
    curl --create-dirs --insecure -f -s -o WiiLink_Patcher/Sharpii "${PabloURL}/Sharpii/sharpii(${sys})"
    chmod +x WiiLink_Patcher/Sharpii
    
    # Download SPD if English is selected
    case $reg in
        "EN")	spd_download ;;
        "JPN") 	mkdir -p WAD ;;
    esac
    
    ## Downloading Patches
    # Wii no Ma
    [ "$reg" == "EN" ] && download_patch "wiinoma" "WiinoMa_0_${lang}.delta" "WiinoMa_0.delta" "WiinoMa"
                          download_patch "wiinoma" "WiinoMa_1_${lang}.delta" "WiinoMa_1.delta" "WiinoMa"
                          download_patch "wiinoma" "WiinoMa_2_${lang}.delta" "WiinoMa_2.delta" "WiinoMa"
    
    # Digicam Print Channel
    [ "$reg" == "EN" ] && download_patch "digicam" "Digicam_0_${lang}.delta" "Digicam_0.delta" "Digicam"
                          download_patch "digicam" "Digicam_1_${lang}.delta" "Digicam_1.delta" "Digicam"
    [ "$reg" == "EN" ] && download_patch "digicam" "Digicam_2_${lang}.delta" "Digicam_2.delta" "Digicam"
    
    # Demae Channel (Standard or Dominos)
    case $demae_version in
        standard)
            [ "$reg" == "EN" ] && download_patch "demae" "Demae_0_${lang}.delta" "DemaeChannel_0_${lang}.delta" "Demae"
                                  download_patch "demae" "Demae_1_${lang}.delta" "DemaeChannel_1_${lang}.delta" "Demae"
            [ "$reg" == "EN" ] && download_patch "demae" "Demae_2_${lang}.delta" "DemaeChannel_2_${lang}.delta" "Demae"
        ;;
        dominos)
            download_patch "dominos" "Dominos_0.delta" "Dominos_0.delta" "Dominos"
            download_patch "dominos" "Dominos_1.delta" "Dominos_1.delta" "Dominos"
            download_patch "dominos" "Dominos_2.delta" "Dominos_2.delta" "Dominos"
        ;;
    esac
    
    #Downloading Wii Mod Lite
    task="Downloading Wii Mod Lite"
    curl --create-dirs --insecure -f -s "https://hbb1.oscwii.org/unzipped_apps/WiiModLite/apps/WiiModLite/boot.dol" -o apps/WiiModLite/boot.dol
    curl --create-dirs --insecure -f -s "https://hbb1.oscwii.org/unzipped_apps/WiiModLite/apps/WiiModLite/meta.xml" -o apps/WiiModLite/meta.xml
    curl --create-dirs --insecure -f -s "https://hbb1.oscwii.org/hbb/WiiModLite.png" -o apps/WiiModLite/icon.png
    
    # Nintendo Channel patch downloading
    download_patch "nc" "NC_1_${nc_region}.delta" "NintendoChannel_1_${nc_region}.delta" "Nintendo_Channel"

    # Forecast Channel patch downloading
    download_patch "forecast" "Forecast_1_${platform_type}_${fc_region}.delta" "ForecastChannel_1.delta" "Forecast_Channel"
    
    # Downloading stuff is finished!
    patching_progress[0]="downloading:done"
    patching_progress[1]="wiinoma:in_progress"
}

# Patching Wii no Ma
wiinoma_patch() {
    task="Patching Wii no Ma"
    
    ## Parameters: ##
    # $1 = Title ID, $2 = Title ID,
    # $3 = First App Number, $4 = First Patch, $5 = Second App Number, $6 = Second Patch, $7 = Third App Number, $8 = Third Patch,
    # $9 = Title Name
    patch_core_channel "WiinoMa" 000100014843494a 00000000 WiinoMa_0 00000001 WiinoMa_1 00000002 WiinoMa_2 "Wii no Ma"
    
    # Finish patching
    files_cleanup "channel_clean"
    patching_progress[1]="wiinoma:done"
    patching_progress[2]="digicam_print_channel:in_progress"
}

# Patching Digicam Print Channel
digicam_patch() {
    task="Patching Digicam Print Channel"
    
    ## Parameters: ##
    # $1 = Title ID, $2 = Title ID,
    # $3 = First App Number, $4 = First Patch, $5 = Second App Number, $6 = Second Patch, $7 = Third App Number, $8 = Third Patch,
    # =$9 = Title Name
    patch_core_channel "Digicam" 000100014843444a 00000000 Digicam_0 00000001 Digicam_1 00000002 Digicam_2 "Digicam Print Channel"
    
    # Finish patching
    files_cleanup "channel_clean"
    patching_progress[2]="digicam_print_channel:done"
    patching_progress[3]="demae_channel:in_progress"
}

# Patching Demae Channel
demae_patch() {
    task="Patching Demae Channel"

    # Depending on whether Standard or Domino's is selected, patch the appropriate files
    case $demae_version in
        standard)
            local demae_0="DemaeChannel_0_${lang}"
            local demae_1="DemaeChannel_1_${lang}"
            local demae_2="DemaeChannel_2_${lang}"
            local folder="Demae"
            local ver="Standard"
            ;;
        dominos)
            local demae_0="Dominos_0"
            local demae_1="Dominos_1"
            local demae_2="Dominos_2"
            local folder="Dominos"
            local ver="Dominos"
            ;;
    esac
    
    ## Parameters: ##
    # $1 = Title ID, $2 = Title ID,
    # $3 = First App Number, $4 = First Patch, $5 = Second App Number, $6 = Second Patch, $7 = Third App Number, $8 = Third Patch,
    # $9 = Title Name
    patch_core_channel "$folder" 000100014843484a 00000000 "$demae_0" 00000001 "$demae_1" 00000002 "$demae_2" "Demae Channel ($ver)"
    
    # Finish patching
    files_cleanup "channel_clean"
    patching_progress[3]="demae_channel:done"
    patching_progress[4]="nintendo_channel:in_progress"
}

# Patching Nintendo Channel
nintendo_channel_patch() {
    task="Patching Nintendo Channel"
    case $nc_region in
        "PAL")
            nc_title_id="0001000148415450"
            local app_number="0000002d" ;;
        "USA")
            nc_title_id="0001000148415445"
            local app_number="0000002c" ;;
        "Japan")
            nc_title_id="000100014841544a"
            local app_number="0000003e" ;;
    esac
    
    ## Parameters: ##
    # $1 = Channel Folder Name, $2 = Title ID,
    # $3 = App Number, $4 = Patch Name,
    # $5 = Channel Name, $6 = Region, $7 = Version Number
    patch_wc24_channel "Nintendo_Channel" "$nc_title_id" "$app_number" NintendoChannel_1_"${nc_region}" "Nintendo Channel" "$nc_region" 1792
    
    # Finish patching
    files_cleanup "channel_clean"
    patching_progress[4]="nintendo_channel:done"
    patching_progress[5]="forecast_channel:in_progress"
}

# Patching Forecast Channel
forecast_channel_patch() {
    task="Patching Forecast Channel"
    case $fc_region in
        "PAL") fc_title_id="0001000248414650" ;;
        "USA") fc_title_id="0001000248414645" ;;
        "Japan") fc_title_id="000100024841464a" ;;
    esac
    
    ## Parameters: ##
    # $1 = Channel Folder Name, $2 = Title ID,
    # $3 = App Number, $4 = Patch Name,
    # $5 = Channel Name, $6 = Region, $7 = Version Number
    patch_wc24_channel "Forecast_Channel" "$fc_title_id" 0000000d ForecastChannel_1 "Forecast Channel ($platform_type)" "$fc_region" 7
    
    # Finish patching
    files_cleanup "channel_clean"
    patching_progress[5]="forecast_channel:done"
    patching_progress[6]="finishing:in_progress"
}

# Finishing it all up! Ooh wee, I'm so excited!
finish_sd_copy() {
    task="Copying files to SD card"
    if [ "${sdcard}" != null ]; then
        case $prog_language in
            "en") printf "\n \e[1m[*] Copying files. This may take a while. Give me a second.\e[0m\n" | fold -s -w "$(tput cols)" ;;
        esac
        cp -r "apps" "${sdcard}"
        cp -r "WAD" "${sdcard}"
    fi
    
    # Final cleanup
    files_cleanup "setup_clean"
    patching_progress[6]="finishing:done"
}

# What you're looking at while channels are being patched
patch_progress() {
    # Progress Bar variables
    counter_done=0
    percent=0

    # Change Demae progress message depending on version
    case $demae_version in
        standard)
            case $prog_language in
                "en") demae_prog_message="(Standard)" ;;
            esac ;;
        dominos)
            case $prog_language in
                "en") demae_prog_message="(Dominos)" ;;
            esac ;;
    esac
    
    # Progress variables
    patching_progress=(
        "downloading:in_progress"
        "wiinoma:not_started"
        "digicam_print_channel:not_started"
        "demae_channel:not_started"
        "nintendo_channel:not_started"
        "forecast_channel:not_started"
        "finishing:not_started"
    )
    
    case $prog_language in
        "en")
            progress_messages=(
                "Downloading files"
                "Wii no Ma (Wii Room)"
                "Digicam Print Channel"
                "Demae Channel $demae_prog_message"
                "Nintendo Channel"
                "Forecast Channel"
                "Finishing..."
            )
            ;;
    esac

    # 0 = Not Started, 1 = In Progress, 2 = Done
    progress_box=( "○" "►" "●" )
    
    # The setup processes that will be run sequentially
    setup_functions=(
        download_all_patches
        wiinoma_patch
        digicam_patch
        demae_patch
        nintendo_channel_patch
        forecast_channel_patch
        finish_sd_copy
    )
    
    # Progress bar and completion display
    while [[ ${patching_progress[6]} != "finishing:done" ]]; do
        
        # Progress bar calculation
        case $percent in
            0) counter_done=1 ;;
            1) counter_done=2 ;;
            2) counter_done=4 ;;
            3) counter_done=6 ;;
            4) counter_done=8 ;;
            5) counter_done=9 ;;
            6) counter_done=10 ;;
        esac
        
        header

        case $prog_language in
            "en")
                printf " \e[1m[*] Patching... this can take some time depending on the processing speed (CPU) of your computer.\e[0m\n\n" | fold -s -w "$(tput cols)"
                printf "    Progress: " | fold -s -w "$(tput cols)"
                ;;
        esac
        
        case $counter_done in
            0) printf "[\e[1;32m          \e[0m]" ;;
            1) printf "[\e[1;32m=         \e[0m]" ;;
            2) printf "[\e[1;32m==        \e[0m]" ;;
            3) printf "[\e[1;32m===       \e[0m]" ;;
            4) printf "[\e[1;32m====      \e[0m]" ;;
            5) printf "[\e[1;32m=====     \e[0m]" ;;
            6) printf "[\e[1;32m======    \e[0m]" ;;
            7) printf "[\e[1;32m=======   \e[0m]" ;;
            8) printf "[\e[1;32m========  \e[0m]" ;;
            9) printf "[\e[1;32m========= \e[0m]" ;;
            10) printf "[\e[1;32m==========\e[0m]" ;;
        esac

        # Calculate percentage2
        percent_done=$((counter_done*100/11))
        printf " \e[1m%s%% completed\e[0m\n" "$percent_done"

        case $prog_language in
            "en") printf "\nPlease wait while the patching process is in progress...\n\n" ;;
        esac
        
        # Show progress of each channel
        for i in "${!patching_progress[@]}"; do
            status="${patching_progress[$i]##*:}"
            message="${progress_messages[$i]}"

            case "$status" in
                "not_started") printf "%s %s\n" "${progress_box[0]}" "$message" ;;
                "in_progress") printf "\e[5m%s\e[0m %s\n" "${progress_box[2]}" "$message" ;;
                "done") printf "\e[32m%s\e[0m %s\n" "${progress_box[2]}" "$message" ;;
            esac
        done
        printf "\n"

        # Go to the next setup process
        ${setup_functions[percent]}
        
        # Increment the progress bar
        percent=$((percent+1))
    done
    
    # After all the channels are patched, we're done!
    finish
}


# We made it! We're done! Yay! Now go install the channels!
finish() {
    header
    
    case $prog_language in
        "en") printf "\e[1;32mPatching Complete!\e[0m\n\n" | fold -s -w "$(tput cols)";;
    esac
    
    # Current folder location script is running from
    local current_folder=null
	current_folder="$(basename "$(pwd)")"
    
    if [ "${sdstatus}" = "0" ] || [ "${sdstatus}" = "1" ] && [ "${sdcard}" = "null" ]; then
        case $prog_language in
            "en")
                printf "Please connect your Wii SD Card and copy the apps and WAD folders to the root (main folder) of your SD Card.\n" | fold -s -w "$(tput cols)"
                printf "You can find these folders in the /\e[1m%s\e[0m folder of your computer.\n\n" "${current_folder}" | fold -s -w "$(tput cols)"
                ;;
        esac
    elif [ "${sdstatus}" = "1" ] && [ "${sdcard}" != "null" ]; then
        case $prog_language in
            "en") printf "Every file is in its place on your SD Card!\n\n" | fold -s -w "$(tput cols)" ;;
        esac
    fi
    
    case $prog_language in
        "en")
            printf "Please proceed with the tutorial that you can find on https://wii.guide/wiilink.\n\n" | fold -s -w "$(tput cols)"
            
            printf "What do you want to do now?\n\n"
            printf "1. Go back to the main menu\n"
            printf "2. Exit\n\n"
            ;;
    esac

    choose
    case $choice in
        1) main ;;
        2) clear; exit 0 ;;
        *) printf "\n\n\e[1;31mInvalid input!\e[0m\n\n"
        finish ;;
    esac
}

# Language selection menu
# set_installer_language() {
#     clear
#     header
    
#     case $prog_language in
#         "en") local message="Please select your language:" ;;
#     esac
    
#     printf "%s\n\n" "$message"
#     printf "1. English\n"
#     printf "2. Español\n"
#     printf "3. 日本語\n\n"
    
#     choose
#     case $choice in
#         1) prog_language="en" ; main ;;
#         2) prog_language="es" ; main ;;
#         3) prog_language="jp" ; main ;;
#         *) printf "\n\n\e[1;31mInvalid input!\e[0m\n\n"
#         set_installer_language ;;
#     esac
# }


# The wonderful folks who contributed to this patcher
credits() {
    header
    
    case $prog_language in
        "en")
            printf "\e[1;32mCredits\e[0m:\n\n"
            printf "  - Sketch: Original Unix Patcher Code\n\n"
            printf "  - PablosCorner: Unix Patcher Maintainer\n\n"
            printf "  - TheShadowEevee: Sharpii-NetCore\n\n"
            printf "  - Joshua MacDonald: Xdelta\n\n"
            printf "  - person66, and leathl: Original Sharpii, and libWiiSharp developers\n\n"
            printf "\e[1;32mWiiLink\e[0m \e[1mwebsite:\e[0m https://wiilink24.com\n\n"
            printf "\e[1;32mPress any key to go back to the main menu\e[0m"
            ;;
    esac
    read -n 1 -r -p ""
}


# This is where it all begins
main() {
    while true; do
        header
        
        case $prog_language in
            "en")
                printf "\e[1mWelcome to the WiiLink Patcher!\e[0m\n\n"
                printf "1. Start\n"
                printf "2. Credits\n\n"
                printf "3. Exit Patcher\n\n"
                ;;
        esac

        detect_sd_card
        
        # Show if SD card is detected or not, if it is, show the path to it.
        case "${sdcard}" in
            null) 
                local message_en="Could not detect your SD Card."
                color="\e[1;31m" # Red
                ;;
            *) 
                local message_en="Detected SD Card: ${sdcard}"
                color="\e[1;32m" # Green
                ;;
        esac
        
        case "${prog_language}" in
            "en") printf "${color}%s\e[0m\n" "${message_en}"
                  printf "R. Refresh | If incorrect, you can change later.\n\n"
                  ;;
        esac
        
        choose
        case $choice in
            1) install_choose ;;
            2) credits ;;
            3) clear; exit ;;
            r|R) detect_sd_card ;;
            *)
                printf "\n\e[1;31mInvalid Selection\e[0m\n\n"
                sleep 2
            ;;
        esac
    done
}

main