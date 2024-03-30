# Quick script I whipped up to build the project for all supported platforms
# NOTE: macOS-arm64 needs to be signed on Mac hardware, otherwise it won't run on Apple Silicon Macs

# Define the version and nightly as parameters
param (
    [Parameter(Mandatory=$false)]
    [switch]$nightly,
    [Parameter(Mandatory=$false)]
    [string]$version
)

# Check if dotnet is installed
& dotnet --version 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Error "dotnet is not installed. Please download and install it from https://dotnet.microsoft.com/en-us/ before running this script."
    exit 1
}

# Define the nightly string
$nightlyString = ""
if ($nightly) {
    if (-not $version) {
        Write-Error "Version is required when nightly is specified"
        exit 1
    }
    $nightlyString = "_Nightly_$version"
}

# Build the project for all supported platforms
dotnet publish -c Release -r win-x64 --self-contained /p:AssemblyName="WiiLinkPatcher_Windows$nightlyString"
dotnet publish -c Release -r osx-x64 --self-contained /p:AssemblyName="WiiLinkPatcher_macOS-x64$nightlyString"
dotnet publish -c Release -r osx-arm64 --self-contained /p:AssemblyName="WiiLinkPatcher_macOS-arm64$nightlyString"
dotnet publish -c Release -r linux-x64 --self-contained /p:AssemblyName="WiiLinkPatcher_Linux-x64$nightlyString"
dotnet publish -c Release -r linux-arm64 --self-contained /p:AssemblyName="WiiLinkPatcher_Linux-arm64$nightlyString"

# Open the folder where the builds are located
explorer.exe .\bin\Release\net6.0-windows10.0.22621.0\