using System.Runtime.InteropServices;
using Spectre.Console;

public class SdClass
{
    // SD card setup
    public static void SDSetup(MainClass.SetupType setupType)
    {
        while (true)
        {
            MenuClass.PrintHeader();

            // Change step number depending on if WiiConnect24 is being installed or not
            string stepNum = setupType switch
            {
                MainClass.SetupType.express => MainClass.patcherLang == "en-US"
                    ? !MainClass.installRegionalChannels ? "Step 3" : "Step 4"
                    : $"{MainClass.localizedText?["SDSetup"]?["ifExpress"]?[MainClass.installRegionalChannels ? "ifWC24" : "ifNoWC24"]?["stepNum"]}",
                _ => MainClass.patcherLang == "en-US"
                    ? "Step 4"
                    : $"{MainClass.localizedText?["SDSetup"]?["ifCustom"]?["stepNum"]}"
            };

            // Change header depending on the setup type
            string installType = setupType switch
            {
                MainClass.SetupType.express => MainClass.patcherLang == "en-US"
                    ? "Express Install"
                    : $"{MainClass.localizedText?["ExpressInstall"]?["Header"]}",
                MainClass.SetupType.custom => MainClass.patcherLang == "en-US"
                    ? "Custom Install"
                    : $"{MainClass.localizedText?["CustomSetup"]?["Header"]}",
                MainClass.SetupType.extras => MainClass.patcherLang == "en-US"
                    ? "Install Extras"
                    : $"{MainClass.localizedText?["InstallExtras"]?["Header"]}",
                _ => throw new NotImplementedException()
            };

            // Step title
            string stepTitle = MainClass.patcherLang == "en-US"
                ? "Insert SD Card / USB Drive (if applicable)"
                : $"{MainClass.localizedText?["SDSetup"]?["stepTitle"]}";

            // After passing this step text
            string afterPassingThisStep = MainClass.patcherLang == "en-US"
                ? "After passing this step, any user interaction won't be needed, so sit back and relax!"
                : $"{MainClass.localizedText?["SDSetup"]?["afterPassingThisStep"]}";

            // Download to SD card text
            string downloadToSD = MainClass.patcherLang == "en-US"
                ? "You can download everything directly to your Wii SD Card / USB Drive if you insert it before starting the patching\nprocess. Otherwise, everything will be saved in the same folder as this patcher on your computer."
                : $"{MainClass.localizedText?["SDSetup"]?["downloadToSD"]}";



            // SD card detected text
            string sdDetected = MainClass.patcherLang == "en-US"
                ? MainClass.sdcard != null ? $"SD card detected: [bold springgreen2_1]{MainClass.sdcard}[/]" : ""
                : MainClass.sdcard != null ? $"{MainClass.localizedText?["SDSetup"]?["sdDetected"]}: [bold springgreen2_1]{MainClass.sdcard}[/]" : "";

            // Go Back to Main Menu Text
            string goBackToMainMenu = MainClass.patcherLang == "en-US"
                ? "Go Back to Main Menu"
                : $"{MainClass.localizedText?["goBackToMainMenu"]}";

            AnsiConsole.MarkupLine($"[bold springgreen2_1]{installType}[/]\n");

            AnsiConsole.MarkupLine($"[bold]{stepNum}: {stepTitle}[/]\n");

            Console.WriteLine($"{afterPassingThisStep}\n");

            Console.WriteLine($"{downloadToSD}\n");

            if (MainClass.platformType == MainClass.Platform.vWii && setupType == MainClass.SetupType.express)
            {
                string eulaChannel = MainClass.patcherLang == "en-US"
                ? "[bold]NOTE:[/] For [bold deepskyblue1]vWii[/] users, The EULA channel will also be included."
                : $"{MainClass.localizedText?["ExpressInstall"]?["SDSetup"]?["eulaChannel"]}";
                AnsiConsole.MarkupLine($"{eulaChannel}\n");
            }

            // User Choices
            string startOption = MainClass.patcherLang == "en-US"
                ? MainClass.sdcard != null ? "Start [bold]with[/] SD Card / USB Drive" : "Start [bold]without[/] SD Card / USB Drive"
                : MainClass.sdcard != null ? $"{MainClass.localizedText?["SDSetup"]?["start_withSD"]}" : $"{MainClass.localizedText?["SDSetup"]?["start_noSD"]}";
            string startWithoutSDOption = MainClass.patcherLang == "en-US"
                ? "Start [bold]without[/] SD Card / USB Drive"
                : $"{MainClass.localizedText?["SDSetup"]?["start_noSD"]}";
            string manualDetection = MainClass.patcherLang == "en-US"
                ? "Manually Select SD Card / USB Drive Path\n"
                : $"{MainClass.localizedText?["SDSetup"]?["manualDetection"]}\n";

            AnsiConsole.MarkupLine($"1. {startOption}");
            AnsiConsole.MarkupLine($"2. {(MainClass.sdcard != null ? startWithoutSDOption : manualDetection)}");
            AnsiConsole.MarkupLine($"3. {(MainClass.sdcard != null ? manualDetection : goBackToMainMenu)}");

            if (MainClass.sdcard != null)
            {
                AnsiConsole.MarkupLine($"4. {goBackToMainMenu}\n");

                AnsiConsole.MarkupLine($"{sdDetected}");
            }

            AnsiConsole.MarkupLine("");
            int choice = MainClass.sdcard != null ? MenuClass.UserChoose("1234") : MenuClass.UserChoose("123");

            switch (choice)
            {
                case 1: // Check if WAD folder exists before starting patching process
                    MenuClass.WADFolderCheck(setupType);
                    break;
                case 2: // Start patching process without SD card or Manually select SD card
                    if (MainClass.sdcard != null)
                    {
                        MainClass.sdcard = null;
                        MenuClass.WADFolderCheck(setupType);
                    }
                    else
                    {
                        SDCardSelect();
                    }
                    break;
                case 3: // Manually select SD card or Go back to main menu
                    if (MainClass.sdcard != null)
                    {
                        SDCardSelect();
                    }
                    else
                    {
                        // Clear all lists (just in case it's Custom Setup)
                        MainClass.wiiLinkChannels_selection.Clear();
                        MainClass.wiiConnect24Channels_selection.Clear();
                        MainClass.extraChannels_selection.Clear();
                        MainClass.combinedChannels_selection.Clear();
                        MenuClass.MainMenu();
                    }
                    break;
                case 4: // Go back to main menu
                    if (MainClass.sdcard != null)
                    {
                        // Clear all lists (just in case it's Custom Setup)
                        MainClass.wiiLinkChannels_selection.Clear();
                        MainClass.wiiConnect24Channels_selection.Clear();
                        MainClass.extraChannels_selection.Clear();
                        MainClass.combinedChannels_selection.Clear();
                        MenuClass.MainMenu();
                    }
                    break;
                default:
                    break;
            }
        }
    }

    // Finish SD Copy
    public static void FinishSDCopy()
    {
        // Copying files to SD card and user is not running the patcher on the removable drive
        MainClass.task = "Copying files to SD card";

        if (MainClass.sdcard != null && MainClass.curDir != MainClass.sdcard)
        {
            // Copying files to SD card
            string copyingFiles = MainClass.patcherLang == "en-US"
                ? "Copying files to SD card, which may take a while."
                : $"{MainClass.localizedText?["FinishSDCopy"]?["copyingFiles"]}";
            AnsiConsole.MarkupLine($" [bold][[*]] {copyingFiles}[/]");

            try
            {
                // Copy apps and WAD folder to SD card
                MainClass.CopyFolder(Path.Join("WiiLink", "apps"), Path.Join(MainClass.sdcard, "apps"));
                MainClass.CopyFolder(Path.Join("WiiLink", "WAD"), Path.Join(MainClass.sdcard, "WAD"));
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
                string pressAnyKey_error = MainClass.patcherLang == "en-US"
                    ? "Press any key to try again..."
                    : $"{MainClass.localizedText?["FinishSDCopy"]?["pressAnyKey_error"]}";
                AnsiConsole.MarkupLine($"[bold red]ERROR:[/] {exceptionMessage}\n{pressAnyKey_error}");
                Console.ReadKey();
                FinishSDCopy();
            }

            // Delete the WiiLink folder if it exists
            if (Directory.Exists("WiiLink"))
                Directory.Delete("WiiLink", true);
        }

        // Finished patching
        MainClass.patchingProgress_express["finishing"] = "done";
    }

    // Manually select your SD card path
    public static void SDCardSelect()
    {
        while (true)
        {
            MenuClass.PrintHeader();

            // Manual SD card selection header
            string header = MainClass.patcherLang == "en-US"
                ? "Manually Select SD Card / USB Drive Path"
                : $"{MainClass.localizedText?["SDCardSelect"]?["header"]}";
            AnsiConsole.MarkupLine($"[bold springgreen2_1]{header}[/]\n");

            string inputMessage = "";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                inputMessage = MainClass.patcherLang == "en-US"
                    ? "Please enter the drive letter of your SD card/USB drive (e.g. E)"
                    : $"{MainClass.localizedText?["SDCardSelect"]?["inputMessage"]?["windows"]}";
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                inputMessage = MainClass.patcherLang == "en-US"
                    ? "Please enter the mount name of your SD card/USB drive (e.g. /media/username/Wii)"
                    : $"{MainClass.localizedText?["SDCardSelect"]?["inputMessage"]?["linux"]}";
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                inputMessage = MainClass.patcherLang == "en-US"
                    ? "Please enter the volume name of your SD card/USB drive (e.g. /Volumes/Wii)"
                    : $"{MainClass.localizedText?["SDCardSelect"]?["inputMessage"]?["osx"]}";
            AnsiConsole.MarkupLine($"{inputMessage}");

            // Type EXIT to go back to previous menu
            string exitMessage = MainClass.patcherLang == "en-US"
                ? "(Type [bold]EXIT[/] to go back to the previous menu)"
                : $"{MainClass.localizedText?["SDCardSelect"]?["exitMessage"]}";
            AnsiConsole.MarkupLine($"{exitMessage}\n");

            // New SD card/USB drive text
            string newSDCardMessage = "";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                newSDCardMessage = MainClass.patcherLang == "en-US"
                    ? "New SD card/USB drive:"
                    : $"{MainClass.localizedText?["SDCardSelect"]?["newSDCardMessage"]?["windows"]}";
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                newSDCardMessage = MainClass.patcherLang == "en-US"
                    ? "New SD card/USB drive volume:"
                    : $"{MainClass.localizedText?["SDCardSelect"]?["newSDCardMessage"]?["linux"]}";
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                newSDCardMessage = MainClass.patcherLang == "en-US"
                    ? "New SD card/USB drive volume:"
                    : $"{MainClass.localizedText?["SDCardSelect"]?["newSDCardMessage"]?["osx"]}";
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
                    string driveLetterError = MainClass.patcherLang == "en-US"
                        ? "Drive letter must be 1 character!"
                        : $"{MainClass.localizedText?["SDCardSelect"]?["driveLetterError"]}";
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
                    string bootDriveError = MainClass.patcherLang == "en-US"
                        ? "You cannot select your boot drive!"
                        : $"{MainClass.localizedText?["SDCardSelect"]?["bootDriveError"]}";
                    AnsiConsole.MarkupLine($"[bold red]{bootDriveError}[/]");
                    Thread.Sleep(2000);
                    continue;
                }
            }
            else if (Path.GetPathRoot(sdcard_new) == Path.GetPathRoot(Path.GetPathRoot(Environment.SystemDirectory)))
            {
                // You cannot select your boot drive text
                string bootDriveError = MainClass.patcherLang == "en-US"
                    ? "You cannot select your boot drive!"
                    : $"{MainClass.localizedText?["SDCardSelect"]?["bootDriveError"]}";
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
                    string driveNotRemovableError = MainClass.patcherLang == "en-US"
                        ? "Drive selected is not a removable drive! Please select a removable drive (e.g. SD card or USB drive)."
                        : $"{MainClass.localizedText?["SDCardSelect"]?["driveNotRemovableError"]}";
                    AnsiConsole.MarkupLine($"[bold red]{driveNotRemovableError}[/]");
                    Thread.Sleep(5000);
                    continue;
                }
            }

            // Check if new SD card path is the same as the old one
            if (sdcard_new == MainClass.sdcard)
            {
                // You have already selected this SD card/USB drive text
                string alreadySelectedError = MainClass.patcherLang == "en-US"
                    ? "You have already selected this SD card/USB drive!"
                    : $"{MainClass.localizedText?["SDCardSelect"]?["alreadySelectedError"]}";
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
                    driveNotExistError = MainClass.patcherLang == "en-US"
                        ? "Drive does not exist!"
                        : $"{MainClass.localizedText?["SDCardSelect"]?["driveNotExistError"]?["windows"]}";
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    driveNotExistError = MainClass.patcherLang == "en-US"
                        ? "Volume does not exist!"
                        : $"{MainClass.localizedText?["SDCardSelect"]?["driveNotExistError"]?["linux"]}";
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    driveNotExistError = MainClass.patcherLang == "en-US"
                        ? "Volume does not exist!"
                        : $"{MainClass.localizedText?["SDCardSelect"]?["driveNotExistError"]?["osx"]}";
                AnsiConsole.MarkupLine($"[bold red]{driveNotExistError}[/]");

                Thread.Sleep(2000);
                continue;
            }

            // Check if SD card has /apps folder (using PathCombine)
            if (Directory.Exists(Path.Join(sdcard_new, "apps")))
            {
                // SD card is valid
                MainClass.sdcard = sdcard_new;
                break;
            }
            else
            {
                // SD card is invalid text
                string noAppsFolderError_message = MainClass.patcherLang == "en-US"
                    ? "Drive detected, but no /apps folder found!"
                    : $"{MainClass.localizedText?["SDCardSelect"]?["noAppsFolderError"]?["message"]}";
                string noAppsFolderError_instructions = MainClass.patcherLang == "en-US"
                    ? "Please create it first and then try again."
                    : $"{MainClass.localizedText?["SDCardSelect"]?["noAppsFolderError"]?["instructions"]}";
                AnsiConsole.MarkupLine($"[bold]{noAppsFolderError_message}[/]");
                AnsiConsole.MarkupLine($"{noAppsFolderError_instructions}\n");

                // Press any key to continue text
                string pressAnyKey = MainClass.patcherLang == "en-US"
                    ? "Press any key to continue..."
                    : $"{MainClass.localizedText?["SDCardSelect"]?["pressAnyKey"]}";
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