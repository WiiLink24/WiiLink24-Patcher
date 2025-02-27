# NOTE: macOS-arm64 needs to be signed on Mac hardware, otherwise it won't run on Apple Silicon Macs

# Checks to see if dotnet is installed
function Test-Dotnet {
    & dotnet --version 2>$null
    if ($LASTEXITCODE -ne 0) {
        Write-Error "dotnet is not installed. Please download and install it from https://dotnet.microsoft.com/download/dotnet before running this script."
        exit 1
    }
}

# Builds the project for the specified platform
function Build-For-Platform {
    param (
        [Parameter(Mandatory=$true)]
        [string]$Platform
    )

    switch ($Platform) {
        "win-x64" { $PlatformName = "Windows-x64" }
        "win-arm64" { $PlatformName = "Windows-ARM64" }
        "osx-x64" { $PlatformName = "macOS-x64" }
        "osx-arm64" { $PlatformName = "macOS-ARM64" }
        "linux-x64" { $PlatformName = "Linux-x64" }
        "linux-arm64" { $PlatformName = "Linux-ARM64" }
        default {
            Write-Error "Invalid platform: $Platform"
            Write-Output "Available platforms:"
            foreach ($platform in $Global:Platforms) {
                Write-Output "- $platform"
            }
            exit 1
        }
    }

    dotnet publish -c Release -r $Platform /p:AssemblyName="WiiLinkPatcher_$PlatformName$versionString"
}

# Shows the help message
function Show-Help {
    Write-Output "Usage: .\build.ps1 [options]"
    Write-Output ""
    Write-Output "Options:"
    Write-Output "  -h, --help                Show this help message and exit"
    Write-Output "  -b, --build <platform>    Build for a specific platform"
    Write-Output "  -n, --nightly             Mark the build as a nightly build"
    Write-Output "  -v, --version <version>   Specify the version for said nightly build"
    Write-Output ""
    Write-Output "Examples:"
    Write-Output "  .\build.ps1 -b win-x64"
    Write-Output "  .\build.ps1 --build osx-x64"
    Write-Output "  .\build.ps1 -n -v v100"
    Write-Output "  .\build.ps1 --nightly --version v100"
}

# Initialize variables
$versionString = ""
$Global:Platforms = "win-x64", "win-arm64", "osx-x64", "osx-arm64", "linux-x64", "linux-arm64"

# Parse command line arguments
$build_invoked = $false
$nightly = $false
$version = ""
$platform = ""

for ($i = 0; $i -lt $args.Count; $i++) {
    switch ($args[$i]) {
        "-b" { $build_invoked = $true; $platform = $args[++$i] }
        "--build" { $build_invoked = $true; $platform = $args[++$i] }
        "-n" { $nightly = $true }
        "--nightly" { $nightly = $true }
        "-v" {
            $version = $args[++$i]
            if ($version -notmatch "^v\d+") {
                Write-Error "Invalid version: $version. Version should start with 'v' followed by a number (e.g., v100 for version 1.0.0)"
                exit 1
            }
        }
        "--version" {
            $version = $args[++$i]
            if ($version -notmatch "^v\d+") {
                Write-Error "Invalid version: $version. Version should start with 'v' followed by a number (e.g., v100 for version 1.0.0)"
                exit 1
            }
        }
        "-h" { Show-Help; exit 0 }
        "--help" { Show-Help; exit 0 }
        default { Write-Error "Invalid option: $($args[$i])"; exit 1 }
    }
}

# Check if dotnet is installed
Test-Dotnet

# Check if nightly is specified
if ($nightly) {
    if (-not $version) {
        Write-Error "Version is required when nightly is specified"
        exit 1
    }
    $versionString = "_Nightly_$version"
} else {
    $versionString = "_$version"
}

# Build the project for the specified platform or all supported platforms if none is specified
if (-not $platform) {
    if ($build_invoked) {
        Write-Output "Available platforms:"
        foreach ($platform in $Global:Platforms) {
            Write-Output "- $platform"
        }
        exit 1
    }
    Write-Output "No platform specified. Building for all supported platforms."
    foreach ($platform in $Global:Platforms) {
        Build-For-Platform -Platform $platform
    }
} else {
    Write-Output "Building for platform: $platform"
    Build-For-Platform -Platform $platform
}

# Open the folder where the builds are located
explorer.exe .\bin\Release\net9.0-windows10.0.22621.0\