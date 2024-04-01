#!/bin/bash

# NOTE: macOS-arm64 needs to be signed on Mac hardware, otherwise it won't run on Apple Silicon Macs

# Checks to see if dotnet is installed
function check_dotnet {
    if ! command -v dotnet &> /dev/null
    then
        echo "dotnet could not be found. Please download and install it from https://dotnet.microsoft.com/download/dotnet before running this script."
        exit
    fi
}

# Function to build for a specific platform
function build_for_platform {
    case "$1" in
        win-x64)
            platform="Windows"
        ;;
        osx-x64)
            platform="macOS-x64"
        ;;
        osx-arm64)
            platform="macOS-ARM64"
        ;;
        linux-x64)
            platform="Linux-x64"
        ;;
        linux-arm64)
            platform="Linux-ARM64"
        ;;
        *)
            echo "Invalid platform: $1"
            echo "Available platforms:"
            for platform in "${platforms[@]}"; do
                echo "- $platform"
            done
            exit 1
        ;;
    esac
    
    dotnet publish -c Release -r "$1" --self-contained /p:AssemblyName="WiiLinkPatcher_$platform$nightlyString"
}

# Initialize variables
nightlyString=""
platforms=("win-x64" "osx-x64" "osx-arm64" "linux-x64" "linux-arm64")

# Parse command line arguments
build_invoked=false
nightly=false
while (( "$#" )); do
    case "$1" in
        -b|--build)
            build_invoked=true
            if [ -n "$2" ] && [ "${2:0:1}" != "-" ]; then
                platform="$2"
                shift 2
            else
                echo "Available platforms:"
                for platform in "${platforms[@]}"; do
                    echo "- $platform"
                done
                exit 1
            fi
        ;;
        -n|--nightly)
            nightly=true
            shift
        ;;
        -v|--version)
            if [ "$nightly" = false ]; then
                echo "The -v/--version option can only be used when -n/--nightly is also used"
                exit 1
            fi
            if [[ $2 =~ ^v[0-9]+ ]]; then
                version="$2"
                shift 2
            else
                echo "Invalid version: $2. Version should start with 'v' followed by a number (e.g., v100 for version 1.0.0)"
                exit 1
            fi
        ;;
        -h|--help)
            echo "Usage: ./build.sh [options]"
            echo ""
            echo "Options:"
            echo "  -h, --help                Show this help message and exit"
            echo "  -b, --build <platform>    Build for a specific platform"
            echo "  -n, --nightly             Mark the build as a nightly build"
            echo "  -v, --version <version>   Specify the version for the nightly build"
            echo ""
            echo "Examples:"
            echo "  ./build.sh -b win-x64"
            echo "  ./build.sh --build osx-x64"
            echo "  ./build.sh -n -v v100"
            echo "  ./build.sh --nightly --version v100"
            exit 0
        ;;
        --)
            shift
        ;;
        *)
            echo "Invalid option: $1" >&2
            exit 1
        ;;
    esac
done

# Check if dotnet is installed
check_dotnet

# Check if nightly is specified
if [ "$nightly" == "true" ]; then
    if [ -z "$version" ]; then
        echo "Version is required when nightly is specified"
        exit 1
    fi
    nightlyString="_Nightly_$version"
fi

# Build the project for the specified platform or all supported platforms if none is specified
if [ -z "$platform" ]; then
    if [ "$build_invoked" == "true" ]; then
        echo "Available platforms: ${platforms[*]}"
        exit 1
    fi
    echo "No platform specified. Building for all supported platforms."
    for platform in "${platforms[@]}"; do
        build_for_platform "$platform"
    done
else
    echo "Building for platform: $platform"
    build_for_platform "$platform"
fi

# Open the folder where the builds are located
if [[ "$OSTYPE" == "linux-gnu"* ]]; then
    xdg-open ./bin/Release/net6.0-windows10.0.22621.0/ &> /dev/null
    elif [[ "$OSTYPE" == "darwin"* ]]; then
    open ./bin/Release/net6.0-windows10.0.22621.0/ &> /dev/null
else
    echo "Cannot open the directory automatically on this OS."
    echo "You can find the builds in the bin/Release/net6.0-windows10.0.22621.0/ directory."
fi