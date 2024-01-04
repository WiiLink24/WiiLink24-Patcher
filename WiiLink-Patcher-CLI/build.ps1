# Quick script I whipped up to build the project for all supported platforms

# Define the version as a parameter
param (
    [Parameter(Mandatory=$true)]
    [string]$version
)

# Build the project for all supported platforms
dotnet publish -c Release -r win-x64 --self-contained /p:AssemblyName="WiiLink_Patcher_Windows_v$version"
dotnet publish -c Release -r osx-x64 --self-contained /p:AssemblyName="WiiLink_Patcher_macOS_v$version"
dotnet publish -c Release -r osx-arm64 --self-contained /p:AssemblyName="WiiLink_Patcher_macOS-arm64_v$version"
dotnet publish -c Release -r linux-x64 --self-contained /p:AssemblyName="WiiLink_Patcher_Linux-x64_v$version"
dotnet publish -c Release -r linux-arm64 --self-contained /p:AssemblyName="WiiLink_Patcher_Linux-arm64_v$version"

# Open the folder where the builds are located
explorer.exe .\bin\Release\net6.0-windows10.0.22621.0\
