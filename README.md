# WiiLink Patcher

WiiLink Patcher is a program made for easier installation of WiiLink. With it, you can just sit back and relax while the patcher does everything for you.

It utilizes [libWiiSharp](https://github.com/WiiDatabase/libWiiSharp) to download channel contents, and patches them with [VCDiff](https://github.com/SnowflakePowered/vcdiff), to create the patched WADs that you can install on your Wii, and use with WiiLink24!

>This patcher may contain bugs. If you spot any or are just having problems with the patcher in general, report them on our [Issues Page](https://github.com/WiiLink24/WiiLink24-Patcher/issues), or ask us for help on our [Discord server](https://discord.gg/wiilink)!

### Download
You can download the latest version of the patcher from the [Releases Page](https://github.com/WiiLink24/WiiLink24-Patcher/releases).

>**NOTE:** In **Windows**, your antivirus may flag the **patcher**, as malware. This is a **false positive**, and you can safely ignore it. If you are still unsure, you can inspect the source code, and/or compile it yourself for extra verification. You can also temporarily disable your antivirus to download the patcher, or add an exception for it if you put it in a dedicated folder.

>**NOTE:** For **OSX/MacOS**, you need Rosetta installed on your system if your Mac is using an **M1 or newer** chip. Rosetta can be installed with:
>`/usr/sbin/softwareupdate --install-rosetta --agree-to-license`

### Features
* Patches WiiConnect24-based channels to work with WiiLink!
* Works with **Wii Room**, **Photo Prints Channel**, **Food Channel**, and more!
* The patcher will automatically download the required files.
* Copies patched files to an SD Card that is already connected to the PC.
* Downloads **YAWM ModMii Edition (yawmME)** and puts it on the SD Card along with the WADs.
* You can get **Food Channel (Domino's)**, along with the **Get Console ID** homebrew app for easy console ID registration on our [Discord server](https://discord.gg/wiilink), for ***free***!

>(**Food Channel (Domino's)** is only available in the US and Canada!)

Compatible with Windows (**10** and **11**), macOS, and Linux!

### Debug
In order to troubleshoot any issues, you can use the `--debug` flag while running the patcher to have extended logs.
```
> <patcher executable> --debug
```

## Compiling

Clone or download the repository. You can compile the project using the provided build scripts:

| Operating System | Build Script |
| --- | --- |
| Windows | `build.ps1` |
| Linux/macOS | `build.sh` |

By default, running these scripts with no arguments will compile the project for all supported platforms. If you want to build for a specific platform, append `-b` or `--build`, followed by the platform identifier:

| Platform Identifier | Description |
| --- | --- |
| `win-x64` | Windows (64-bit) |
| `osx-x64` | macOS (Intel, 64-bit) |
| `osx-arm64` | macOS (Apple Silicon) |
| `linux-x64` | Linux (64-bit) |
| `linux-arm64` | Linux (ARM, 64-bit) |

---

For example, to build only for Windows, you would run `./build.ps1 -b <platform>` on Windows, or `./build.sh -b <platform>` on Linux/macOS, replacing `<platform>` with the desired platform identifier.

---

If you want to mark the build as a nightly build, append `-n` or `--nightly` to the build command, along with a version number, using `-v` or `--version`.

For example, to build a nightly version for Windows, you would run `./build.ps1 -n -v <version>` on Windows, or `./build.sh -n -v <version>` on Linux/macOS, replacing `<version>` with the desired version number (e.g. `v100` for version 1.0.0).

---

## Screenshot
![C# Version](https://i.imgur.com/DlH8c0V.png)
