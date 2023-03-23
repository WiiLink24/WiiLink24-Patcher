# WiiLink Patcher (C# Version)

WiiLink Patcher is a program made for easier installation of WiiLink. With it, you can just sit back and relax while the patcher does everything for you.

It utilizes [Sharpii-NetCore](https://github.com/TheShadowEevee/Sharpii-NetCore) to download channel contents, and patches them with [Xdelta3](https://github.com/jmacd/xdelta), to create the patched WADs that you can install on your Wii, and use with WiiLink24!

>This patcher may contain bugs. If you spot any or are just having problems with the patcher in general, report them on our [Issues Page](https://github.com/WiiLink24/WiiLink24-Patcher/issues), or ask us for help on our [Discord server](https://discord.gg/wiilink)!

### Download
* [**[Windows]** v1.1.0 (RC2)](https://cdn.discordapp.com/attachments/253286648291393536/1088539858026373180/WiiLink_Patcher_Windows_v1.1.0.exe)
* [**[macOS]** v1.1.0 (RC2)](https://cdn.discordapp.com/attachments/253286648291393536/1088539858454184036/WiiLink_Patcher_macOS_v1.1.0)
* [**[Linux]** v1.1.0 (RC2)](https://cdn.discordapp.com/attachments/253286648291393536/1088539859033006200/WiiLink_Patcher_Linux_v1.1.0)


>In **macOS** and **Linux**, you'll need to give the app execution permissions, either by setting it in the file permission settings, or doing:<br>`chmod +x WiiLink_Patcher_macOS_v1.1.0` or `chmod +x WiiLink_Patcher_Linux_v1.1.0`<br> in the terminal in the same directory as the app.

>**NOTE:** In **Windows**, your antivirus may flag the **patcher**, **Sharpii**, or **xdelta3** as a virus. These are a false positive, and you can safely ignore it. If you are still unsure, you can inspect the source code, and/or compile it yourself to make sure it's safe. You can also temporarily disable your antivirus to download the patcher, or add an exception for it if you put it in a dedicated folder.

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
![C# Version](https://imgur.com/SIR3QUk.png)
