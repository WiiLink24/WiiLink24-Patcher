using System.Diagnostics;
using System.Text;
using System.Runtime.InteropServices;
using Spectre.Console;
using libWiiSharp;
using System.Net;
using Newtonsoft.Json.Linq;
using System.IO.Compression;

public class sd
{
    // Finish SD Copy
    public static void FinishSDCopy()
    {
        // Copying files to SD card and user is not running the patcher on the removable drive
        main.task = "Copying files to SD card";

        if (main.sdcard != null && main.curDir != main.sdcard)
        {
            // Copying files to SD card
            string copyingFiles = main.patcherLang == main.PatcherLanguage.en
                ? "Copying files to SD card, which may take a while."
                : $"{main.localizedText?["FinishSDCopy"]?["copyingFiles"]}";
            AnsiConsole.MarkupLine($" [bold][[*]] {copyingFiles}[/]");

            try
            {
                // Copy apps and WAD folder to SD card
                main.CopyFolder("apps", Path.Join(main.sdcard, "apps"));
                main.CopyFolder("WAD", Path.Join(main.sdcard, "WAD"));
            }
            catch (Exception e)
            {
                // Format exception message to prevent a crash when reading square brackets
                string exceptionMessage = e.Message;
                if (exceptionMessage.Contains("["))
                    exceptionMessage = exceptionMessage.Replace("[", "[[");
                if (exceptionMessage.Contains("]"))
                    exceptionMessage = exceptionMessage.Replace("]", "]]");
                
                // Error message
                string pressAnyKey_error = main.patcherLang == main.PatcherLanguage.en
                    ? "Press any key to try again..."
                    : $"{main.localizedText?["FinishSDCopy"]?["pressAnyKey_error"]}";
                AnsiConsole.MarkupLine($"[bold red]ERROR:[/] {exceptionMessage}\n{pressAnyKey_error}");
                Console.ReadKey();
                FinishSDCopy();
            }

            // Delete the WAD and apps folder if they exist
            if (Directory.Exists("WAD"))
                Directory.Delete("WAD", true);
            if (Directory.Exists("apps"))
                Directory.Delete("apps", true);
        }

        // Delete WiiLink_Patcher folder
        if (Directory.Exists("WiiLink_Patcher"))
            Directory.Delete("WiiLink_Patcher", true);

        // Finished patching
        main.patchingProgress_express["finishing"] = "done";
    }

    // Manually select your SD card path
    public static void SDCardSelect()
    {
        while (true)
        {
            menu.PrintHeader();

            // Manual SD card selection header
            string header = main.patcherLang == main.PatcherLanguage.en
                ? "Manually Select SD Card / USB Drive Path"
                : $"{main.localizedText?["SDCardSelect"]?["header"]}";
            AnsiConsole.MarkupLine($"[bold springgreen2_1]{header}[/]\n");

            string inputMessage = "";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                inputMessage = main.patcherLang == main.PatcherLanguage.en
                    ? "Please enter the drive letter of your SD card/USB drive (e.g. E)"
                    : $"{main.localizedText?["SDCardSelect"]?["inputMessage"]?["windows"]}";
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                inputMessage = main.patcherLang == main.PatcherLanguage.en
                    ? "Please enter the mount name of your SD card/USB drive (e.g. /media/username/Wii)"
                    : $"{main.localizedText?["SDCardSelect"]?["inputMessage"]?["linux"]}";
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                inputMessage = main.patcherLang == main.PatcherLanguage.en
                    ? "Please enter the volume name of your SD card/USB drive (e.g. /Volumes/Wii)"
                    : $"{main.localizedText?["SDCardSelect"]?["inputMessage"]?["osx"]}";
            AnsiConsole.MarkupLine($"{inputMessage}");

            // Type EXIT to go back to previous menu
            string exitMessage = main.patcherLang == main.PatcherLanguage.en
                ? "(Type [bold]EXIT[/] to go back to the previous menu)"
                : $"{main.localizedText?["SDCardSelect"]?["exitMessage"]}";
            AnsiConsole.MarkupLine($"{exitMessage}\n");

            // New SD card/USB drive text
            string newSDCardMessage = "";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                newSDCardMessage = main.patcherLang == main.PatcherLanguage.en
                    ? "New SD card/USB drive:"
                    : $"{main.localizedText?["SDCardSelect"]?["newSDCardMessage"]?["windows"]}";
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                newSDCardMessage = main.patcherLang == main.PatcherLanguage.en
                    ? "New SD card/USB drive volume:"
                    : $"{main.localizedText?["SDCardSelect"]?["newSDCardMessage"]?["linux"]}";
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                newSDCardMessage = main.patcherLang == main.PatcherLanguage.en
                    ? "New SD card/USB drive volume:"
                    : $"{main.localizedText?["SDCardSelect"]?["newSDCardMessage"]?["osx"]}";
            AnsiConsole.Markup($"{newSDCardMessage} ");

            // Get user input, if user presses ESC (without needing to press ENTER), go back to previous menu
            string? sdcard_new = Console.ReadLine();
            string? inputUpper = sdcard_new?.ToUpper();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                sdcard_new = inputUpper;

            // Restart SDCardSelect if user input is empty
            if (inputUpper == "")
                SDCardSelect();
            else if (inputUpper == "EXIT")
                return;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Error if drive letter is more than 1 character
                if (sdcard_new?.Length > 1)
                {
                    // Driver letter must be 1 character text
                    string driveLetterError = main.patcherLang == main.PatcherLanguage.en
                        ? "Drive letter must be 1 character!"
                        : $"{main.localizedText?["SDCardSelect"]?["driveLetterError"]}";
                    AnsiConsole.MarkupLine($"[bold red]{driveLetterError}[/]");
                    Thread.Sleep(2000);
                    continue;
                }
            }

            // Format SD card path depending on OS
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                sdcard_new += ":\\";
            else
            {
                // If / is already at the end of the path, remove it
                if (sdcard_new?.EndsWith("/") == true)
                    sdcard_new = sdcard_new.Remove(sdcard_new.Length - 1);
            }

            // Prevent user from selecting boot drive
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                if (sdcard_new == "/")
                {
                    // You cannot select your boot drive text
                    string bootDriveError = main.patcherLang == main.PatcherLanguage.en
                        ? "You cannot select your boot drive!"
                        : $"{main.localizedText?["SDCardSelect"]?["bootDriveError"]}";
                    AnsiConsole.MarkupLine($"[bold red]{bootDriveError}[/]");
                    Thread.Sleep(2000);
                    continue;
                }
            }
            else if (Path.GetPathRoot(sdcard_new) == Path.GetPathRoot(Path.GetPathRoot(Environment.SystemDirectory)))
            {
                // You cannot select your boot drive text
                string bootDriveError = main.patcherLang == main.PatcherLanguage.en
                    ? "You cannot select your boot drive!"
                    : $"{main.localizedText?["SDCardSelect"]?["bootDriveError"]}";
                AnsiConsole.MarkupLine($"[bold red]{bootDriveError}[/]");
                Thread.Sleep(2000);
                continue;
            }

            // On Windows, don't allow the user to pick a drive that's not removable
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (sdcard_new == null)
                    continue;

                DriveInfo driveInfo = new(sdcard_new);
                if (!driveInfo.IsReady || driveInfo.DriveType != DriveType.Removable)
                {
                    // Drive is not removable text
                    string driveNotRemovableError = main.patcherLang == main.PatcherLanguage.en
                        ? "Drive selected is not a removable drive! Please select a removable drive (e.g. SD card or USB drive)."
                        : $"{main.localizedText?["SDCardSelect"]?["driveNotRemovableError"]}";
                    AnsiConsole.MarkupLine($"[bold red]{driveNotRemovableError}[/]");
                    Thread.Sleep(5000);
                    continue;
                }
            }

            // Check if new SD card path is the same as the old one
            if (sdcard_new == main.sdcard)
            {
                // You have already selected this SD card/USB drive text
                string alreadySelectedError = main.patcherLang == main.PatcherLanguage.en
                    ? "You have already selected this SD card/USB drive!"
                    : $"{main.localizedText?["SDCardSelect"]?["alreadySelectedError"]}";
                AnsiConsole.MarkupLine($"[bold red]{alreadySelectedError}[/]");
                Thread.Sleep(2000);
                continue;
            }

            // Check if drive/volume exists
            if (!Directory.Exists(sdcard_new))
            {
                // Drive does not exist text
                string driveNotExistError = "";
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    driveNotExistError = main.patcherLang == main.PatcherLanguage.en
                        ? "Drive does not exist!"
                        : $"{main.localizedText?["SDCardSelect"]?["driveNotExistError"]?["windows"]}";
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    driveNotExistError = main.patcherLang == main.PatcherLanguage.en
                        ? "Volume does not exist!"
                        : $"{main.localizedText?["SDCardSelect"]?["driveNotExistError"]?["linux"]}";
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    driveNotExistError = main.patcherLang == main.PatcherLanguage.en
                        ? "Volume does not exist!"
                        : $"{main.localizedText?["SDCardSelect"]?["driveNotExistError"]?["osx"]}";
                AnsiConsole.MarkupLine($"[bold red]{driveNotExistError}[/]");

                Thread.Sleep(2000);
                continue;
            }

            // Check if SD card has /apps folder (using PathCombine)
            if (Directory.Exists(Path.Join(sdcard_new, "apps")))
            {
                // SD card is valid
                main.sdcard = sdcard_new;
                break;
            }
            else
            {
                // SD card is invalid text
                string noAppsFolderError_message = main.patcherLang == main.PatcherLanguage.en
                    ? "Drive detected, but no /apps folder found!"
                    : $"{main.localizedText?["SDCardSelect"]?["noAppsFolderError"]?["message"]}";
                string noAppsFolderError_instructions = main.patcherLang == main.PatcherLanguage.en
                    ? "Please create it first and then try again."
                    : $"{main.localizedText?["SDCardSelect"]?["noAppsFolderError"]?["instructions"]}";
                AnsiConsole.MarkupLine($"[bold]{noAppsFolderError_message}[/]");
                AnsiConsole.MarkupLine($"{noAppsFolderError_instructions}\n");

                // Press any key to continue text
                string pressAnyKey = main.patcherLang == main.PatcherLanguage.en
                    ? "Press any key to continue..."
                    : $"{main.localizedText?["SDCardSelect"]?["pressAnyKey"]}";
                AnsiConsole.MarkupLine($"{pressAnyKey}");
                Console.ReadKey();
            }
        }
    }

    /// <summary>
    /// Gets the base path of the removable drive where the "apps" directory exists.
    /// </summary>
    /// <returns>The base path of the removable drive, or null if no removable drive with the "apps" directory is found.</returns>
    public static string? DetectRemovableDrive
    {
        get
        {
            var basePaths = new List<string>();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                basePaths = DriveInfo.GetDrives()
                    .Where(drive => drive.DriveType == DriveType.Removable && drive.IsReady)
                    .Select(drive => drive.Name)
                    .ToList();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                basePaths.Add("/Volumes");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                var mediaPath = $"/media/{Environment.UserName}";
                if (Directory.Exists("/media") && Directory.Exists(mediaPath)) basePaths.Add(mediaPath);

                var runMediaPath = $"/run/media/{Environment.UserName}";
                if (Directory.Exists("/run/media") && Directory.Exists(runMediaPath)) basePaths.Add(runMediaPath);
            }

            foreach (var basePath in basePaths)
            {
                try
                {
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        // Check if the apps directory exists on the root of the removable drive
                        if (Directory.Exists(Path.Join(basePath, "apps")))
                        {
                            return basePath;
                        }
                    }
                    else
                    {
                        var subDirectories = Directory.EnumerateDirectories(basePath);
                        foreach (var subDirectory in subDirectories)
                        {
                            // Check if the apps directory exists on the root of the removable drive
                            if (Directory.Exists(Path.Join(subDirectory, "apps")))
                            {
                                return subDirectory;
                            }
                        }
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    // If the user doesn't have permission to access the directory, skip it.
                    continue;
                }
            }

            return null;
        }
    }

}