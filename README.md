# WiiLink Patcher (.NET Version)

WiiLink Patcher is a program made for easier installation of WiiLink. With it, you can just sit back and relax while the patcher does everything for you.

It utilizes [libWiiSharp](https://github.com/WiiDatabase/libWiiSharp) to download channel contents, and patches them with [VCDiff](https://github.com/SnowflakePowered/vcdiff), to create the patched WADs that you can install on your Wii, and use with WiiLink24!

>This patcher may contain bugs. If you spot any or are just having problems with the patcher in general, report them on our [Issues Page](https://github.com/WiiLink24/WiiLink24-Patcher/issues), or ask us for help on our [Discord server](https://discord.gg/wiilink)!

### Download
You can download the latest version of the patcher from the [Releases Page](https://github.com/WiiLink24/WiiLink24-Patcher/releases).

>**NOTE:** In **Windows**, your antivirus may flag the **patcher**, as malware. This is a **false positive**, and you can safely ignore it. If you are still unsure, you can inspect the source code, and/or compile it yourself for extra verification. You can also temporarily disable your antivirus to download the patcher, or add an exception for it if you put it in a dedicated folder.

### Features
* Works with **Wii Room**, **Digicam Print Channel**, **Food Channel**, and more!
* The patcher will automatically download the required files.
* Copying patched files to an SD Card that is already connected to the PC.
* Downloads **Wii Mod Lite** and putting it on SD Card along with the WADs.
* You can get **Food Channel (Domino's)**, along with the **Get Console ID** homebrew app for easy console ID registration on our [Discord server](https://discord.gg/wiilink), for ***free***!

>(**Food Channel (Domino's)** is only available in the US and Canada!)

Compatible with Windows (**10** and **11**), macOS, and Linux!

## Compiling

Clone or download the repository, and open the solution file in Visual Studio. You will need to make sure you have .NET 6.0 set up to compile it.

Alternatively, you can use the [dotnet](https://dotnet.microsoft.com) to compile it. You will need to have at least .NET 6.0 installed. You can compile it by running `dotnet publish -c Release -r <RID>` in the root directory of the project. You can find a list of RIDs [here](https://docs.microsoft.com/en-us/dotnet/core/rid-catalog).

## Screenshot
![C# Version](https://i.imgur.com/DlH8c0V.png)
