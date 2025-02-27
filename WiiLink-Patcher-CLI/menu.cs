using System.Diagnostics;
using System.Runtime.InteropServices;
using Spectre.Console;

public class MenuClass
{
    public static void PrintHeader()
    {
        Console.Clear();

        string headerText = MainClass.patcherLang == MainClass.PatcherLanguage.en
            ? $"[springgreen2_1]WiiLink[/] Patcher {MainClass.version} - (c) {MainClass.copyrightYear} WiiLink Team"
            : $"{MainClass.localizedText?["Header"]}"
                .Replace("{version}", MainClass.version)
                .Replace("{year}", MainClass.copyrightYear);

        AnsiConsole.MarkupLine($"{(MainClass.inCompatabilityMode
            ? headerText
            : $"[bold]{headerText}[/]")}");

        char borderChar = '=';
        string borderLine = new(borderChar, Console.WindowWidth);

        AnsiConsole.MarkupLine($"{(MainClass.inCompatabilityMode
            ? borderLine
            : $"[bold]{borderLine}[/]")}\n");
    }

    public static void PrintNotice()
    {
        string title = MainClass.patcherLang == MainClass.PatcherLanguage.en
            ? "Notice"
            : $"{MainClass.localizedText?["Notice"]?["noticeTitle"]}";
        string text = MainClass.patcherLang == MainClass.PatcherLanguage.en
            ? "If you have any issues with the patcher or services offered by WiiLink, please report them on our [springgreen2_1 link=https://discord.gg/wiilink]Discord Server[/]!"
            : $"{MainClass.localizedText?["Notice"]?["noticeMsg"]}";

        var panel = new Panel($"[bold]{text}[/]")
        {
            Header = new PanelHeader($"[bold springgreen2_1] {title} [/]", Justify.Center),
            Border = MainClass.inCompatabilityMode ? BoxBorder.Ascii : BoxBorder.Heavy,
            BorderStyle = new Style(Color.SpringGreen2_1),
            Expand = true,
        };

        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// This method allows the user to make a selection from a list of choices.
    /// </summary>
    /// <param name="choices">A string containing the valid choices for the user.</param>
    /// <returns>
    /// Returns an integer representing the user's choice. 
    /// If the user presses a special key (like Escape, Enter, Backspace, Arrow keys), 
    /// it returns a specific negative integer associated with that key.
    /// If the user enters a valid choice, it returns the index of the choice in the string (1-indexed).
    /// If the user enters an invalid choice, it returns -24.
    /// </returns>
    public static int UserChoose(string choices)
    {
        const int ESCAPE = -1;
        const int ENTER = 0;
        const int BACKSPACE = -2;
        const int LEFT_ARROW = -99;
        const int RIGHT_ARROW = 99;
        const int UP_ARROW = -98;
        const int DOWN_ARROW = 98;
        const int INVALID_INPUT = -24;

        ConsoleKeyInfo keyPressed;
        string chooseText = MainClass.patcherLang == MainClass.PatcherLanguage.en
                ? "Choose: "
                : $"{MainClass.localizedText?["UserChoose"]} ";
        AnsiConsole.Markup(chooseText);

        do
        {
            keyPressed = Console.ReadKey(intercept: true);

            switch (keyPressed.Key)
            {
                case ConsoleKey.Escape:
                    return ESCAPE;
                case ConsoleKey.Enter:
                    Console.WriteLine();
                    return ENTER;
                case ConsoleKey.Backspace:
                    return BACKSPACE;
                case ConsoleKey.LeftArrow:
                    return LEFT_ARROW;
                case ConsoleKey.RightArrow:
                    return RIGHT_ARROW;
                case ConsoleKey.UpArrow:
                    return UP_ARROW;
                case ConsoleKey.DownArrow:
                    return DOWN_ARROW;
                default:
                    if (choices.Contains(keyPressed.KeyChar))
                    {
                        Console.WriteLine();
                        return choices.IndexOf(keyPressed.KeyChar) + 1;
                    }
                    else
                        return INVALID_INPUT;
            }
        } while (true);
    }

    // WAD folder check (Express Install)
    public static void WADFolderCheck(MainClass.SetupType setupType)
    {
        // Start patching process if WAD folder doesn't exist
        if (!Directory.Exists("WAD"))
        {
            if (setupType == MainClass.SetupType.express)
                ExpressClass.PatchingProgress_Express();
            else
                CustomClass.PatchingProgress_Custom();
        }
        else
        {
            while (true)
            {
                PrintHeader();

                // Change header depending on if it's Express Install or Custom Install
                string installType = setupType switch
                {
                    MainClass.SetupType.express => MainClass.patcherLang == MainClass.PatcherLanguage.en
                        ? "Express Install"
                        : $"{MainClass.localizedText?["ExpressInstall"]?["Header"]}",
                    MainClass.SetupType.custom => MainClass.patcherLang == MainClass.PatcherLanguage.en
                        ? "Custom Install"
                        : $"{MainClass.localizedText?["CustomSetup"]?["Header"]}",
                    MainClass.SetupType.extras => MainClass.patcherLang == MainClass.PatcherLanguage.en
                        ? "Install Extras"
                        : $"{MainClass.localizedText?["InstallExtras"]?["Header"]}",
                    _ => throw new NotImplementedException()
                };

                string stepNum = setupType switch
                {
                    MainClass.SetupType.express => MainClass.patcherLang == MainClass.PatcherLanguage.en
                        ? !MainClass.installRegionalChannels ? "Step 4" : "Step 5"
                        : $"{MainClass.localizedText?["WADFolderCheck"]?["ifExpress"]?[MainClass.installRegionalChannels ? "ifWC24" : "ifNoWC24"]?["stepNum"]}",
                    _ => MainClass.patcherLang == MainClass.PatcherLanguage.en
                        ? "Step 5"
                        : $"{MainClass.localizedText?["WADFolderCheck"]?["ifCustom"]?["stepNum"]}"
                };

                AnsiConsole.MarkupLine($"[bold springgreen2_1]{installType}[/]\n");

                // Step title
                string stepTitle = MainClass.patcherLang == MainClass.PatcherLanguage.en
                    ? "WAD folder detected"
                    : $"{MainClass.localizedText?["WADFolderCheck"]?["stepTitle"]}";

                AnsiConsole.MarkupLine($"[bold]{stepNum}: {stepTitle}[/]\n");

                // WAD folder detected text
                string wadFolderDetected = MainClass.patcherLang == MainClass.PatcherLanguage.en
                    ? "A [bold]WAD[/] folder has been detected in the current directory. This folder is used to store the WAD files that are downloaded during the patching process. If you choose to delete this folder, it will be recreated when you start the patching process again."
                    : $"{MainClass.localizedText?["WADFolderCheck"]?["wadFolderDetected"]}";

                AnsiConsole.MarkupLine($"{wadFolderDetected}\n");

                // User Choices
                string deleteWADFolder = MainClass.patcherLang == MainClass.PatcherLanguage.en
                    ? "Delete WAD folder"
                    : $"{MainClass.localizedText?["WADFolderCheck"]?["deleteWADFolder"]}";
                string keepWADFolder = MainClass.patcherLang == MainClass.PatcherLanguage.en
                    ? "Keep WAD folder"
                    : $"{MainClass.localizedText?["WADFolderCheck"]?["keepWADFolder"]}";
                string goBackToMainMenu = MainClass.patcherLang == MainClass.PatcherLanguage.en
                    ? "Go Back to Main Menu"
                    : $"{MainClass.localizedText?["goBackToMainMenu"]}";

                AnsiConsole.MarkupLine($"1. {deleteWADFolder}");
                AnsiConsole.MarkupLine($"2. {keepWADFolder}\n");

                AnsiConsole.MarkupLine($"3. {goBackToMainMenu}\n");

                int choice = UserChoose("123");
                switch (choice)
                {
                    case 1: // Delete WAD folder in a try catch block
                        try
                        {
                            Directory.Delete("WAD", true);
                        }
                        catch (Exception e)
                        {
                            AnsiConsole.MarkupLine($"[bold red]ERROR:[/] {e.Message}\n");

                            // Press any key to try again
                            string pressAnyKey = MainClass.patcherLang == MainClass.PatcherLanguage.en
                                ? "Press any key to try again..."
                                : $"{MainClass.localizedText?["pressAnyKey"]}";
                            AnsiConsole.MarkupLine($"{pressAnyKey}\n");

                            WADFolderCheck(setupType);
                        }

                        if (setupType == MainClass.SetupType.express)
                            ExpressClass.PatchingProgress_Express();
                        else
                            CustomClass.PatchingProgress_Custom();
                        break;
                    case 2: // Keep WAD folder
                        if (setupType == MainClass.SetupType.express)
                            ExpressClass.PatchingProgress_Express();
                        else
                            CustomClass.PatchingProgress_Custom();
                        break;
                    case 3: // Go back to main menu
                        // Clear all lists (just in case it's Custom Setup)
                        MainClass.wiiLinkChannels_selection.Clear();
                        MainClass.wiiConnect24Channels_selection.Clear();
                        MainClass.extraChannels_selection.Clear();
                        MainClass.combinedChannels_selection.Clear();
                        MainMenu();
                        break;
                    default:
                        break;
                }
            }
        }
    }
    public static void Finished()
    {
        // Detect SD card (in case the user chose to not copy files to SD card)
        MainClass.sdcard = SdClass.DetectRemovableDrive;

        while (true)
        {
            PrintHeader();
            // Patching Completed text
            string completed = MainClass.patcherLang == MainClass.PatcherLanguage.en
                ? "Patching Completed!"
                : $"{MainClass.localizedText?["Finished"]?["completed"]}";
            AnsiConsole.MarkupLine($"[bold slowblink springgreen2_1]{completed}[/]\n");

            if (MainClass.sdcard != null)
            {
                // Every file is in its place text
                string everyFileInPlace = MainClass.patcherLang == MainClass.PatcherLanguage.en
                    ? "Every file is in its place on your SD Card / USB Drive!"
                    : $"{MainClass.localizedText?["Finished"]?["withSD/USB"]?["everyFileInPlace"]}";
                AnsiConsole.MarkupLine($"{everyFileInPlace}\n");
            }
            else
            {
                if (MainClass.platformType != MainClass.Platform.Dolphin)
                {
                    // Please connect text
                    string connectDrive = MainClass.patcherLang == MainClass.PatcherLanguage.en
                        ? "Please connect your Wii SD Card / USB Drive and copy the [u]WAD[/] and [u]apps[/] folders to the root (main folder) of your SD Card / USB Drive."
                        : $"{MainClass.localizedText?["Finished"]?["withoutSD/USB"]?["connectDrive"]}";
                    AnsiConsole.MarkupLine($"{connectDrive}\n");

                    // Open the folder text
                    string canFindFolders = MainClass.patcherLang == MainClass.PatcherLanguage.en
                        ? "You can find these folders in the [u]{curDir}[/] folder of your computer."
                        : $"{MainClass.localizedText?["Finished"]?["canFindFolders"]}";
                    canFindFolders = canFindFolders.Replace("{curDir}", MainClass.curDir);
                    AnsiConsole.MarkupLine($"{canFindFolders}\n");
                }
            }

            // Please proceed text
            if (MainClass.extraChannels_selection.Count == 0)
            {
                if (MainClass.platformType == MainClass.Platform.Wii)
                {
                    string pleaseProceed = MainClass.patcherLang == MainClass.PatcherLanguage.en
                        ? "Please proceed with the tutorial that you can find on [bold springgreen2_1 link]https://wiilink.ca/guide/wii/#section-ii---installing-wads-and-patching-wii-mail[/]"
                        : $"{MainClass.localizedText?["Finished"]?["pleaseProceed"]?["Wii"]}";
                    AnsiConsole.MarkupLine($"{pleaseProceed}\n");
                }
                else if (MainClass.platformType == MainClass.Platform.vWii)
                {
                    string pleaseProceed = MainClass.patcherLang == MainClass.PatcherLanguage.en
                        ? "Please proceed with the tutorial that you can find on [bold springgreen2_1 link]https://wiilink.ca/guide/vwii/#section-iii---installing-wads-and-patching-wii-mail[/]"
                        : $"{MainClass.localizedText?["Finished"]?["pleaseProceed"]?["vWii"]}";
                    AnsiConsole.MarkupLine($"{pleaseProceed}\n");
                }
                else
                {
                    string pleaseProceed = MainClass.patcherLang == MainClass.PatcherLanguage.en
                        ? "Please proceed with the tutorial that you can find on [bold springgreen2_1 link]https://wiilink.ca/guide/dolphin/#section-ii---installing-wads[/]"
                        : $"{MainClass.localizedText?["Finished"]?["pleaseProceed"]?["Dolphin"]}";
                    AnsiConsole.MarkupLine($"{pleaseProceed}\n");
                }
            }
            else
            {
                if (MainClass.platformType == MainClass.Platform.Dolphin)
                {
                    string installWad = MainClass.patcherLang == MainClass.PatcherLanguage.en
                        ? "Please proceed with installing the WADs through the Dolphin interface (Tools > Install WAD...)"
                        : $"{MainClass.localizedText?["Finished"]?["installWad"]?["Dolphin"]}";
                    AnsiConsole.MarkupLine($"{installWad}\n");
                }
                else
                {
                    string installWad = MainClass.patcherLang == MainClass.PatcherLanguage.en
                        ? "Please proceed with the tutorial that you can find on [bold springgreen2_1 link]https://wii.hacks.guide/yawmme[/]"
                        : $"{MainClass.localizedText?["Finished"]?["installWad"]?["yawmME"]}";
                    AnsiConsole.MarkupLine($"{installWad}\n");
                }
            }

            // What would you like to do now text
            string whatWouldYouLikeToDo = MainClass.patcherLang == MainClass.PatcherLanguage.en
                ? "What would you like to do now?"
                : $"{MainClass.localizedText?["Finished"]?["whatWouldYouLikeToDo"]}";
            AnsiConsole.MarkupLine($"{whatWouldYouLikeToDo}\n");

            // User choices
            string openFolder = MainClass.patcherLang == MainClass.PatcherLanguage.en
                ? MainClass.sdcard != null ? "Open the SD Card / USB Drive folder" : "Open the folder"
                : MainClass.sdcard != null ? $"{MainClass.localizedText?["Finished"]?["withSD/USB"]?["openFolder"]}" : $"{MainClass.localizedText?["Finished"]?["withoutSD/USB"]?["openFolder"]}";
            string goBackToMainMenu = MainClass.patcherLang == MainClass.PatcherLanguage.en
                ? "Go back to the main menu"
                : $"{MainClass.localizedText?["goBackToMainMenu"]}";
            string exitProgram = MainClass.patcherLang == MainClass.PatcherLanguage.en
                ? "Exit the program"
                : $"{MainClass.localizedText?["Finished"]?["exitProgram"]}";

            AnsiConsole.MarkupLine($"1. {openFolder}");
            AnsiConsole.MarkupLine($"2. {goBackToMainMenu}");
            AnsiConsole.MarkupLine($"3. {exitProgram}\n");

            int choice = UserChoose("123");
            switch (choice)
            {
                case 1:
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        var psi = new ProcessStartInfo
                        {
                            FileName = "explorer.exe",
                            ArgumentList = { MainClass.sdcard ?? MainClass.curDir },
                            UseShellExecute = false,
                        };
                        Process.Start(psi);
                    }
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    {
                        var psi = new ProcessStartInfo
                        {
                            FileName = "xdg-open",
                            ArgumentList = { MainClass.sdcard ?? MainClass.curDir },
                            UseShellExecute = false,
                        };
                        Process.Start(psi);
                    }
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    {
                        var psi = new ProcessStartInfo
                        {
                            FileName = "open",
                            ArgumentList = { MainClass.sdcard ?? MainClass.curDir },
                            UseShellExecute = false,
                        };
                        Process.Start(psi);
                    }
                    break;
                case 2:
                    // Clear all lists (just in case it's Custom Setup)
                    MainClass.wiiLinkChannels_selection.Clear();
                    MainClass.wiiConnect24Channels_selection.Clear();
                    MainClass.extraChannels_selection.Clear();
                    MainClass.combinedChannels_selection.Clear();
                    // Check to see if removable drive is still connected
                    MainClass.sdcard = SdClass.DetectRemovableDrive;
                    MainMenu();
                    break;
                case 3:
                    Console.Clear();
                    MainClass.ExitApp();
                    break;
                default:
                    break;
            }
        }
    }

     // Main Menu function
    public static void MainMenu()
    {
        // Delete specific folders in temp folder
        string tempPath = Path.Join(MainClass.tempDir);
        if (Directory.Exists(tempPath))
        {
            string[] foldersToDelete = ["Patches", "Unpack", "Unpack_Patched"];
            foreach (string folder in foldersToDelete)
            {
                string folderPath = Path.Join(tempPath, folder);
                if (Directory.Exists(folderPath))
                    Directory.Delete(folderPath, true);
            }
        }

        while (true)
        {
            // Display header and notice
            PrintHeader();
            PrintNotice();

            // Main Menu text
            string welcomeMessage = MainClass.patcherLang == MainClass.PatcherLanguage.en
                ? "Welcome to the [springgreen2_1]WiiLink[/] Patcher!"
                : $"{MainClass.localizedText?["MainMenu"]?["welcomeMessage"]}";

            // Express Install text
            string startExpressSetup = MainClass.patcherLang == MainClass.PatcherLanguage.en
                ? "Start Express Install Setup [bold springgreen2_1](Recommended)[/]"
                : $"{MainClass.localizedText?["MainMenu"]?["startExpressSetup"]}";

            // Custom Install text
            string startCustomSetup = MainClass.patcherLang == MainClass.PatcherLanguage.en
                ? "Start Custom Install Setup [bold](Advanced)[/]"
                : $"{MainClass.localizedText?["MainMenu"]?["startCustomSetup"]}";

            // Install Extras text
            string startExtrasSetup = MainClass.patcherLang == MainClass.PatcherLanguage.en
                ? "Install Extra Channels [bold grey](Optional)[/]"
                : $"{MainClass.localizedText?["MainMenu"]?["startExtrasSetup"]}";


            // Settings text
            string settingsText = MainClass.patcherLang == MainClass.PatcherLanguage.en
                ? "Settings"
                : $"{MainClass.localizedText?["MainMenu"]?["settings"]}";

            // Visit the GitHub repository text
            string visitGitHub = MainClass.patcherLang == MainClass.PatcherLanguage.en
                ? "Visit the GitHub Repository"
                : $"{MainClass.localizedText?["MainMenu"]?["visitGitHub"]}";

            // Visit the WiiLink website text
            string visitWiiLink = MainClass.patcherLang == MainClass.PatcherLanguage.en
                ? "Visit the WiiLink Website"
                : $"{MainClass.localizedText?["MainMenu"]?["visitWiiLink"]}";

            // Exit Patcher text
            string exitPatcher = MainClass.patcherLang == MainClass.PatcherLanguage.en
                ? "Exit Patcher"
                : $"{MainClass.localizedText?["MainMenu"]?["exitPatcher"]}";

            // Print all the text
            AnsiConsole.MarkupLine($"[bold]{welcomeMessage}[/]\n");

            AnsiConsole.MarkupLine($"1. {startExpressSetup}");
            AnsiConsole.MarkupLine($"2. {startCustomSetup}");
            AnsiConsole.MarkupLine($"3. {startExtrasSetup}");
            AnsiConsole.MarkupLine($"4. {settingsText}\n");

            AnsiConsole.MarkupLine($"5. {visitGitHub}");

            AnsiConsole.MarkupLine($"6. {visitWiiLink}\n");

            AnsiConsole.MarkupLine($"7. {exitPatcher}\n");

            // Detect SD Card / USB Drive text
            string SDDetectedOrNot = MainClass.sdcard != null
                ? $"[bold springgreen2_1]{(MainClass.patcherLang == MainClass.PatcherLanguage.en
                    ? "Detected SD Card / USB Drive:"
                    : MainClass.localizedText?["MainMenu"]?["sdCardDetected"])}[/] {MainClass.sdcard}"
                : $"[bold red]{(MainClass.patcherLang == MainClass.PatcherLanguage.en
                    ? "Could not detect your SD Card / USB Drive!"
                    : MainClass.localizedText?["MainMenu"]?["noSDCard"])}[/]";
            AnsiConsole.MarkupLine(SDDetectedOrNot);

            // Automatically detect SD Card / USB Drive text
            string automaticDetection = MainClass.patcherLang == MainClass.PatcherLanguage.en
                ? "R. Automatically detect SD Card / USB Drive"
                : $"{MainClass.localizedText?["MainMenu"]?["automaticDetection"]}";
            AnsiConsole.MarkupLine(automaticDetection);

            // Manually select SD Card / USB Drive text
            string manualDetection = MainClass.patcherLang == MainClass.PatcherLanguage.en
                ? "M. Manually select SD Card / USB Drive path\n"
                : $"{MainClass.localizedText?["MainMenu"]?["manualDetection"]}\n";
            AnsiConsole.MarkupLine(manualDetection);

            // User chooses an option
            int choice = UserChoose("1234567RrMm");
            switch (choice)
            {
                case 1: // Start Express Install
                    ExpressClass.WC24Setup();
                    break;
                case 2: // Start Custom Install
                    CustomClass.CustomInstall_WiiLinkChannels_Setup();
                    break;
                case 3: // Start Extras Setup
                    ExtrasClass.systemChannelRestorer_Setup();
                    break;
                case 4: // Settings                
                    SettingsClass.SettingsMenu();
                    break;
                case 5: // Visit GitHub
                    VisitWebsite("https://github.com/WiiLink24/WiiLink24-Patcher");
                    break;
                case 6: // Visit WiiLink website
                    VisitWebsite("https://wiilink.ca");
                    break;
                case 7: // Clear console and Exit app
                    Console.Clear();
                    MainClass.ExitApp();
                    break;
                case 8: // Automatically detect SD Card path (R/r)
                case 9:
                    MainClass.sdcard = SdClass.DetectRemovableDrive;
                    break;
                case 10: // Manually select SD Card path (M/m)
                case 11:
                    SdClass.SDCardSelect();
                    break;
                default:
                    break;
            }
        }
    }

    static void VisitWebsite(string url)
    {
        try
        {
            // Determine the operating system
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Windows
                var psi = new ProcessStartInfo
                {
                    FileName = "cmd",
                    Arguments = $"/c start {url}",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                };
                Process.Start(psi);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                // Linux
                var psi = new ProcessStartInfo
                {
                    FileName = "xdg-open",
                    Arguments = url,
                    UseShellExecute = false,
                };
                Process.Start(psi);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                // macOS
                var psi = new ProcessStartInfo
                {
                    FileName = "open",
                    Arguments = url,
                    UseShellExecute = false,
                };
                Process.Start(psi);
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"\n[bold red]ERROR:[/] {ex.Message}");
        }
    }

    public static void ConnectionFailed(int statusCode, string errorMsg)
    {
        PrintHeader();

        // Connection to server failed text
        string connectionFailed = MainClass.patcherLang == MainClass.PatcherLanguage.en
            ? "Connection to server failed!"
            : $"{MainClass.localizedText?["ServerDown"]?["connectionFailed"]}";
        AnsiConsole.MarkupLine($"[bold blink red]{connectionFailed}[/]\n");

        // Check internet connection text
        string checkInternet = MainClass.patcherLang == MainClass.PatcherLanguage.en
            ? "Connection to the server failed. Please check your internet connection and try again."
            : $"{MainClass.localizedText?["ServerDown"]?["checkInternet"]}";
        string serverOrInternet = MainClass.patcherLang == MainClass.PatcherLanguage.en
            ? "It seems that either the server is down or your internet connection is not working."
            : $"{MainClass.localizedText?["ServerDown"]?["serverOrInternet"]}";
        string reportIssue = MainClass.patcherLang == MainClass.PatcherLanguage.en
            ? "If you are sure that your internet connection is working, please join our [link=https://discord.gg/wiilink bold springgreen2_1]Discord Server[/] and report this issue."
            : $"{MainClass.localizedText?["ServerDown"]?["reportIssue"]}";
        AnsiConsole.MarkupLine($"{checkInternet}\n");
        AnsiConsole.MarkupLine($"{serverOrInternet}\n");
        AnsiConsole.MarkupLine($"{reportIssue}\n");

        // Status code text
        string statusCodeText = MainClass.patcherLang == MainClass.PatcherLanguage.en
            ? "Status code:"
            : $"{MainClass.localizedText?["ServerDown"]?["statusCode"]}";
        string messageText = MainClass.patcherLang == MainClass.PatcherLanguage.en
            ? "Message:"
            : $"{MainClass.localizedText?["ServerDown"]?["message"]}";
        string exitMessage = MainClass.patcherLang == MainClass.PatcherLanguage.en
            ? "Press any key to exit..."
            : $"{MainClass.localizedText?["ServerDown"]?["exitMessage"]}";
        AnsiConsole.MarkupLine($"{statusCodeText} {statusCode}");
        AnsiConsole.MarkupLine($"{messageText} {errorMsg}\n");

        AnsiConsole.MarkupLine($"[bold yellow]{exitMessage}[/]");

        Console.ReadKey();
        MainClass.ExitApp();
    }

    // Page selection function
    public static (int, int) GetPageIndices(int currentPage, int totalItems, int itemsPerPage)
    {
        int start = (currentPage - 1) * itemsPerPage;
        int end = Math.Min(start + itemsPerPage, totalItems);
        return (start, end);
    }

    public static void ErrorScreen(int exitCode, string msg = "")
    {
        // Clear regional and WiiConnect24 channel selections if they exist
        MainClass.wiiLinkChannels_selection.Clear();
        MainClass.wiiConnect24Channels_selection.Clear();
        MainClass.extraChannels_selection.Clear();

        PrintHeader();

        // An error has occurred text
        string errorOccurred = MainClass.patcherLang == MainClass.PatcherLanguage.en
            ? "An error has occurred."
            : $"{MainClass.localizedText?["ErrorScreen"]?["title"]}";
        AnsiConsole.MarkupLine($"[bold red]{errorOccurred}[/]\n");

        // Error details text
        string errorDetails = MainClass.patcherLang == MainClass.PatcherLanguage.en
            ? "ERROR DETAILS:"
            : $"{MainClass.localizedText?["ErrorScreen"]?["details"]}";
        string taskText = MainClass.patcherLang == MainClass.PatcherLanguage.en
            ? "Task: "
            : $"{MainClass.localizedText?["ErrorScreen"]?["task"]}";
        string commandText = MainClass.patcherLang == MainClass.PatcherLanguage.en
            ? "Command:"
            : $"{MainClass.localizedText?["ErrorScreen"]?["command"]}";
        string messageText = MainClass.patcherLang == MainClass.PatcherLanguage.en
            ? "Message:"
            : $"{MainClass.localizedText?["ErrorScreen"]?["message"]}";
        string exitCodeText = MainClass.patcherLang == MainClass.PatcherLanguage.en
            ? "Exit code:"
            : $"{MainClass.localizedText?["ErrorScreen"]?["exitCode"]}";

        AnsiConsole.MarkupLine($"{errorDetails}\n");
        AnsiConsole.MarkupLine($" * {taskText} {MainClass.task}");
        AnsiConsole.MarkupLine(msg == null ? $" * {commandText} {MainClass.curCmd}" : $" * {messageText} {msg}");
        AnsiConsole.MarkupLine($" * {exitCodeText} {exitCode}\n");

        // Please open an issue text
        string openAnIssue = MainClass.patcherLang == MainClass.PatcherLanguage.en
            ? "Please open an issue on our GitHub page ([link bold springgreen2_1]https://github.com/WiiLink24/WiiLink24-Patcher/issues[/]) and describe the\nerror you encountered. Please include the error details above in your issue."
            : $"{MainClass.localizedText?["ErrorScreen"]?["githubIssue"]}";
        AnsiConsole.MarkupLine($"{openAnIssue}\n");

        // Press any key to go back to the main menu text
        string pressAnyKey = MainClass.patcherLang == MainClass.PatcherLanguage.en
            ? "Press any key to go back to the main menu..."
            : $"{MainClass.localizedText?["ErrorScreen"]?["pressAnyKey"]}";
        AnsiConsole.MarkupLine($"[bold]{pressAnyKey}[/]");
        Console.ReadKey();

        // Go back to the main menu
        MainMenu();
    }
}