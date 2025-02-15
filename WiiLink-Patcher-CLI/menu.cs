using System.Diagnostics;
using System.Text;
using System.Runtime.InteropServices;
using Spectre.Console;
using Newtonsoft.Json.Linq;

public class menu
{
    public static void PrintHeader()
    {
        Console.Clear();

        string headerText = main.patcherLang == main.PatcherLanguage.en
            ? $"[springgreen2_1]WiiLink[/] Patcher {main.version} - (c) {main.copyrightYear} WiiLink Team"
            : $"{main.localizedText?["Header"]}"
                .Replace("{version}", main.version)
                .Replace("{year}", main.copyrightYear);

        AnsiConsole.MarkupLine($"{(main.inCompatabilityMode
            ? headerText
            : $"[bold]{headerText}[/]")}");

        char borderChar = '=';
        string borderLine = new(borderChar, Console.WindowWidth);

        AnsiConsole.MarkupLine($"{(main.inCompatabilityMode
            ? borderLine
            : $"[bold]{borderLine}[/]")}\n");
    }

    public static void PrintNotice()
    {
        string title = main.patcherLang == main.PatcherLanguage.en
            ? "Notice"
            : $"{main.localizedText?["Notice"]?["noticeTitle"]}";
        string text = main.patcherLang == main.PatcherLanguage.en
            ? "If you have any issues with the patcher or services offered by WiiLink, please report them on our [springgreen2_1 link=https://discord.gg/wiilink]Discord Server[/]!"
            : $"{main.localizedText?["Notice"]?["noticeMsg"]}";

        var panel = new Panel($"[bold]{text}[/]")
        {
            Header = new PanelHeader($"[bold springgreen2_1] {title} [/]", Justify.Center),
            Border = main.inCompatabilityMode ? BoxBorder.Ascii : BoxBorder.Heavy,
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
        string chooseText = main.patcherLang == main.PatcherLanguage.en
                ? "Choose: "
                : $"{main.localizedText?["UserChoose"]} ";
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

    // Credits function
    public static void CreditsScreen()
    {
        PrintHeader();

        // Build info
        string buildInfo = main.patcherLang == main.PatcherLanguage.en
            ? $"This build was compiled on [bold springgreen2_1]{main.buildDate}[/] at [bold springgreen2_1]{main.buildTime}[/]."
            : $"{main.localizedText?["Credits"]?["buildInfo"]}"
                .Replace("{main.buildDate}", main.buildDate)
                .Replace("{main.buildTime}", main.buildTime);
        AnsiConsole.MarkupLine($"{buildInfo}\n");

        // Credits table
        var creditTable = new Table().Border(main.inCompatabilityMode ? TableBorder.None : TableBorder.DoubleEdge);

        // Credits header
        string credits = main.patcherLang == main.PatcherLanguage.en
            ? "Credits"
            : $"{main.localizedText?["Credits"]?["credits"]}";
        creditTable.AddColumn(new TableColumn($"[bold springgreen2_1]{credits}[/]").Centered());

        // Credits grid
        var creditGrid = new Grid().AddColumn().AddColumn();

        // Credits text
        string sketchDesc = main.patcherLang == main.PatcherLanguage.en
            ? "WiiLink Lead"
            : $"{main.localizedText?["Credits"]?["sketchDesc"]}";
        string pablosDesc = main.patcherLang == main.PatcherLanguage.en
            ? "WiiLink Patcher Developer"
            : $"{main.localizedText?["Credits"]?["pablosDesc"]}";
        string harryDesc = main.patcherLang == main.PatcherLanguage.en
            ? "WiiLink Patcher Developer"
            : $"{main.localizedText?["Credits"]?["harryDesc"]}";
        string lunaDesc = main.patcherLang == main.PatcherLanguage.en
            ? "Lead Translator"
            : $"{main.localizedText?["Credits"]?["lunaDesc"]}";
        string leathlWiiDatabase = main.patcherLang == main.PatcherLanguage.en
            ? "leathl and WiiDatabase"
            : $"{main.localizedText?["Credits"]?["leathlWiiDatabase"]}";
        string leathlWiiDatabaseDesc = main.patcherLang == main.PatcherLanguage.en
            ? "libWiiSharp developers"
            : $"{main.localizedText?["Credits"]?["leathlWiiDatabaseDesc"]}";

        creditGrid.AddRow(new Text("Sketch", new Style(Color.SpringGreen2_1, null, Decoration.Bold)).RightJustified(), new Text(sketchDesc, new Style(null, null, Decoration.Bold)));
        creditGrid.AddRow(new Text("PablosCorner", new Style(Color.SpringGreen2_1, null, Decoration.Bold)).RightJustified(), new Text(pablosDesc, new Style(null, null, Decoration.Bold)));
        creditGrid.AddRow(new Text("AyeItsHarry", new Style(Color.SpringGreen2_1, null, Decoration.Bold)).RightJustified(), new Text(harryDesc, new Style(null, null, Decoration.Bold)));
        creditGrid.AddRow(new Text("Luna", new Style(Color.SpringGreen2_1, null, Decoration.Bold)).RightJustified(), new Text(lunaDesc, new Style(null, null, Decoration.Bold)));
        creditGrid.AddRow(new Text(leathlWiiDatabase, new Style(Color.SpringGreen2_1, null, Decoration.Bold)).RightJustified(), new Text(leathlWiiDatabaseDesc, new Style(null, null, Decoration.Bold)));
        creditGrid.AddRow(new Text("SnowflakePowered", new Style(Color.SpringGreen2_1, null, Decoration.Bold)).RightJustified(), new Text("VCDiff", new Style(null, null, Decoration.Bold)));

        if (main.patcherLang != main.PatcherLanguage.en)
            creditGrid.AddRow(new Text($"{main.localizedText?["Credits"]?["translatorName"]}", new Style(Color.SpringGreen2_1, null, Decoration.Bold)).RightJustified(), new Text($"{main.localizedText?["Credits"]?["translatorDesc"]}", new Style(null, null, Decoration.Bold)));

        // Add the grid to the table
        creditTable.AddRow(creditGrid).Centered();
        AnsiConsole.Write(creditTable);

        // Special thanks grid
        string specialThanksTo = main.patcherLang == main.PatcherLanguage.en
            ? "Special thanks to:"
            : $"{main.localizedText?["Credits"]?["specialThanksTo"]}";
        AnsiConsole.MarkupLine($"\n[bold springgreen2_1]{specialThanksTo}[/]\n");

        var specialThanksGrid = new Grid().AddColumn().AddColumn();

        // Special thanks text
        string theshadoweeveeRole = main.patcherLang == main.PatcherLanguage.en
            ? "- Pointing me in the right direction with implementing libWiiSharp!"
            : $"{main.localizedText?["Credits"]?["theshadoweeveeRole"]}";
        string ourTesters = main.patcherLang == main.PatcherLanguage.en
            ? "Our Testers"
            : $"{main.localizedText?["Credits"]?["ourTesters"]}";
        string ourTestersRole = main.patcherLang == main.PatcherLanguage.en
            ? "- For testing the patcher and reporting bugs/anomalies!"
            : $"{main.localizedText?["Credits"]?["ourTestersRole"]}";
        string you = main.patcherLang == main.PatcherLanguage.en
            ? "You!"
            : $"{main.localizedText?["Credits"]?["you"]}";
        string youRole = main.patcherLang == main.PatcherLanguage.en
            ? "- For your continued support of WiiLink!"
            : $"{main.localizedText?["Credits"]?["youRole"]}";

        specialThanksGrid.AddRow($"  ● [bold springgreen2_1]TheShadowEevee[/]", theshadoweeveeRole);
        specialThanksGrid.AddRow($"  ● [bold springgreen2_1]{ourTesters}[/]", ourTestersRole);
        specialThanksGrid.AddRow($"  ● [bold springgreen2_1]{you}[/]", youRole);

        AnsiConsole.Write(specialThanksGrid);
        AnsiConsole.MarkupLine("");

        // Links grid
        string wiilinkSite = main.patcherLang == main.PatcherLanguage.en
            ? "WiiLink website"
            : $"{main.localizedText?["Credits"]?["wiilinkSite"]}";
        string githubRepo = main.patcherLang == main.PatcherLanguage.en
            ? "GitHub repository"
            : $"{main.localizedText?["Credits"]?["githubRepo"]}";

        var linksGrid = new Grid().AddColumn().AddColumn();

        linksGrid.AddRow($"[bold springgreen2_1]{wiilinkSite}[/]:", "[link]https://wiilink.ca[/]");
        linksGrid.AddRow($"[bold springgreen2_1]{githubRepo}[/]:", "[link]https://github.com/WiiLink24/WiiLink24-Patcher[/]");

        AnsiConsole.Write(linksGrid);
        AnsiConsole.MarkupLine("");

        // Press any key to go back to settings
        string pressAnyKey = main.patcherLang == main.PatcherLanguage.en
            ? "Press any key to go back to settings..."
            : $"{main.localizedText?["Credits"]?["pressAnyKey"]}";
        AnsiConsole.Markup($"[bold]{pressAnyKey}[/]");
        Console.ReadKey();
    }

    // WAD folder check (Express Install)
    public static void WADFolderCheck(main.SetupType setupType)
    {
        // Start patching process if WAD folder doesn't exist
        if (!Directory.Exists("WAD"))
        {
            if (setupType == main.SetupType.express)
                express.PatchingProgress_Express();
            else
                custom.PatchingProgress_Custom();
        }
        else
        {
            while (true)
            {
                PrintHeader();

                // Change header depending on if it's Express Install or Custom Install
                string installType = setupType switch
                {
                    main.SetupType.express => main.patcherLang == main.PatcherLanguage.en
                        ? "Express Install"
                        : $"{main.localizedText?["ExpressInstall"]?["Header"]}",
                    main.SetupType.custom => main.patcherLang == main.PatcherLanguage.en
                        ? "Custom Install"
                        : $"{main.localizedText?["CustomSetup"]?["Header"]}",
                    main.SetupType.extras => main.patcherLang == main.PatcherLanguage.en
                        ? "Install Extras"
                        : $"{main.localizedText?["InstallExtras"]?["Header"]}",
                    _ => throw new NotImplementedException()
                };

                string stepNum = setupType switch
                {
                    main.SetupType.express => main.patcherLang == main.PatcherLanguage.en
                        ? !main.installRegionalChannels ? "Step 4" : "Step 5"
                        : $"{main.localizedText?["WADFolderCheck"]?["ifExpress"]?[main.installRegionalChannels ? "ifWC24" : "ifNoWC24"]?["stepNum"]}",
                    _ => main.patcherLang == main.PatcherLanguage.en
                        ? "Step 5"
                        : $"{main.localizedText?["WADFolderCheck"]?["ifCustom"]?["stepNum"]}"
                };

                AnsiConsole.MarkupLine($"[bold springgreen2_1]{installType}[/]\n");

                // Step title
                string stepTitle = main.patcherLang == main.PatcherLanguage.en
                    ? "WAD folder detected"
                    : $"{main.localizedText?["WADFolderCheck"]?["stepTitle"]}";

                AnsiConsole.MarkupLine($"[bold]{stepNum}: {stepTitle}[/]\n");

                // WAD folder detected text
                string wadFolderDetected = main.patcherLang == main.PatcherLanguage.en
                    ? "A [bold]WAD[/] folder has been detected in the current directory. This folder is used to store the WAD files that are downloaded during the patching process. If you choose to delete this folder, it will be recreated when you start the patching process again."
                    : $"{main.localizedText?["WADFolderCheck"]?["wadFolderDetected"]}";

                AnsiConsole.MarkupLine($"{wadFolderDetected}\n");

                // User Choices
                string deleteWADFolder = main.patcherLang == main.PatcherLanguage.en
                    ? "Delete WAD folder"
                    : $"{main.localizedText?["WADFolderCheck"]?["deleteWADFolder"]}";
                string keepWADFolder = main.patcherLang == main.PatcherLanguage.en
                    ? "Keep WAD folder"
                    : $"{main.localizedText?["WADFolderCheck"]?["keepWADFolder"]}";
                string goBackToMainMenu = main.patcherLang == main.PatcherLanguage.en
                    ? "Go Back to Main Menu"
                    : $"{main.localizedText?["goBackToMainMenu"]}";

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
                            string pressAnyKey = main.patcherLang == main.PatcherLanguage.en
                                ? "Press any key to try again..."
                                : $"{main.localizedText?["pressAnyKey"]}";
                            AnsiConsole.MarkupLine($"{pressAnyKey}\n");

                            WADFolderCheck(setupType);
                        }

                        if (setupType == main.SetupType.express)
                            express.PatchingProgress_Express();
                        else
                            custom.PatchingProgress_Custom();
                        break;
                    case 2: // Keep WAD folder
                        if (setupType == main.SetupType.express)
                            express.PatchingProgress_Express();
                        else
                            custom.PatchingProgress_Custom();
                        break;
                    case 3: // Go back to main menu
                        // Clear all lists (just in case it's Custom Setup)
                        main.wiiLinkChannels_selection.Clear();
                        main.wiiConnect24Channels_selection.Clear();
                        main.extraChannels_selection.Clear();
                        main.combinedChannels_selection.Clear();
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
        main.sdcard = sd.DetectRemovableDrive;

        while (true)
        {
            PrintHeader();
            // Patching Completed text
            string completed = main.patcherLang == main.PatcherLanguage.en
                ? "Patching Completed!"
                : $"{main.localizedText?["Finished"]?["completed"]}";
            AnsiConsole.MarkupLine($"[bold slowblink springgreen2_1]{completed}[/]\n");

            if (main.sdcard != null)
            {
                // Every file is in its place text
                string everyFileInPlace = main.patcherLang == main.PatcherLanguage.en
                    ? "Every file is in its place on your SD Card / USB Drive!"
                    : $"{main.localizedText?["Finished"]?["withSD/USB"]?["everyFileInPlace"]}";
                AnsiConsole.MarkupLine($"{everyFileInPlace}\n");
            }
            else
            {
                if (main.platformType != main.Platform.Dolphin)
                {
                    // Please connect text
                    string connectDrive = main.patcherLang == main.PatcherLanguage.en
                        ? "Please connect your Wii SD Card / USB Drive and copy the [u]WAD[/] and [u]apps[/] folders to the root (main folder) of your SD Card / USB Drive."
                        : $"{main.localizedText?["Finished"]?["withoutSD/USB"]?["connectDrive"]}";
                    AnsiConsole.MarkupLine($"{connectDrive}\n");

                    // Open the folder text
                    string canFindFolders = main.patcherLang == main.PatcherLanguage.en
                        ? "You can find these folders in the [u]{curDir}[/] folder of your computer."
                        : $"{main.localizedText?["Finished"]?["canFindFolders"]}";
                    canFindFolders = canFindFolders.Replace("{curDir}", main.curDir);
                    AnsiConsole.MarkupLine($"{canFindFolders}\n");
                }
            }

            // Please proceed text
            if (main.extraChannels_selection.Count == 0)
            {
                if (main.platformType == main.Platform.Wii)
                {
                    string pleaseProceed = main.patcherLang == main.PatcherLanguage.en
                        ? "Please proceed with the tutorial that you can find on [bold springgreen2_1 link]https://wiilink.ca/guide/wii/#section-ii---installing-wads-and-patching-wii-mail[/]"
                        : $"{main.localizedText?["Finished"]?["pleaseProceed"]?["Wii"]}";
                    AnsiConsole.MarkupLine($"{pleaseProceed}\n");
                }
                else if (main.platformType == main.Platform.vWii)
                {
                    string pleaseProceed = main.patcherLang == main.PatcherLanguage.en
                        ? "Please proceed with the tutorial that you can find on [bold springgreen2_1 link]https://wiilink.ca/guide/vwii/#section-iii---installing-wads-and-patching-wii-mail[/]"
                        : $"{main.localizedText?["Finished"]?["pleaseProceed"]?["vWii"]}";
                    AnsiConsole.MarkupLine($"{pleaseProceed}\n");
                }
                else
                {
                    string pleaseProceed = main.patcherLang == main.PatcherLanguage.en
                        ? "Please proceed with the tutorial that you can find on [bold springgreen2_1 link]https://wiilink.ca/guide/dolphin/#section-ii---installing-wads[/]"
                        : $"{main.localizedText?["Finished"]?["pleaseProceed"]?["Dolphin"]}";
                    AnsiConsole.MarkupLine($"{pleaseProceed}\n");
                }
            }
            else
            {
                if (main.platformType == main.Platform.Dolphin)
                {
                    string installWad = main.patcherLang == main.PatcherLanguage.en
                        ? "Please proceed with installing the WADs through the Dolphin interface (Tools > Install WAD...)"
                        : $"{main.localizedText?["Finished"]?["installWad"]?["Dolphin"]}";
                    AnsiConsole.MarkupLine($"{installWad}\n");
                }
                else
                {
                    string installWad = main.patcherLang == main.PatcherLanguage.en
                        ? "Please proceed with the tutorial that you can find on [bold springgreen2_1 link]https://wii.hacks.guide/yawmme[/]"
                        : $"{main.localizedText?["Finished"]?["installWad"]?["yawmME"]}";
                    AnsiConsole.MarkupLine($"{installWad}\n");
                }
            }

            // What would you like to do now text
            string whatWouldYouLikeToDo = main.patcherLang == main.PatcherLanguage.en
                ? "What would you like to do now?"
                : $"{main.localizedText?["Finished"]?["whatWouldYouLikeToDo"]}";
            AnsiConsole.MarkupLine($"{whatWouldYouLikeToDo}\n");

            // User choices
            string openFolder = main.patcherLang == main.PatcherLanguage.en
                ? main.sdcard != null ? "Open the SD Card / USB Drive folder" : "Open the folder"
                : main.sdcard != null ? $"{main.localizedText?["Finished"]?["withSD/USB"]?["openFolder"]}" : $"{main.localizedText?["Finished"]?["withoutSD/USB"]?["openFolder"]}";
            string goBackToMainMenu = main.patcherLang == main.PatcherLanguage.en
                ? "Go back to the main menu"
                : $"{main.localizedText?["goBackToMainMenu"]}";
            string exitProgram = main.patcherLang == main.PatcherLanguage.en
                ? "Exit the program"
                : $"{main.localizedText?["Finished"]?["exitProgram"]}";

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
                            ArgumentList = { main.sdcard ?? main.curDir },
                            UseShellExecute = false,
                        };
                        Process.Start(psi);
                    }
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    {
                        var psi = new ProcessStartInfo
                        {
                            FileName = "xdg-open",
                            ArgumentList = { main.sdcard ?? main.curDir },
                            UseShellExecute = false,
                        };
                        Process.Start(psi);
                    }
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    {
                        var psi = new ProcessStartInfo
                        {
                            FileName = "open",
                            ArgumentList = { main.sdcard ?? main.curDir },
                            UseShellExecute = false,
                        };
                        Process.Start(psi);
                    }
                    break;
                case 2:
                    // Clear all lists (just in case it's Custom Setup)
                    main.wiiLinkChannels_selection.Clear();
                    main.wiiConnect24Channels_selection.Clear();
                    main.extraChannels_selection.Clear();
                    main.combinedChannels_selection.Clear();
                    // Check to see if removable drive is still connected
                    main.sdcard = sd.DetectRemovableDrive;
                    MainMenu();
                    break;
                case 3:
                    Console.Clear();
                    main.ExitApp();
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
        string tempPath = Path.Join(main.tempDir);
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
            string welcomeMessage = main.patcherLang == main.PatcherLanguage.en
                ? "Welcome to the [springgreen2_1]WiiLink[/] Patcher!"
                : $"{main.localizedText?["MainMenu"]?["welcomeMessage"]}";

            // Express Install text
            string startExpressSetup = main.patcherLang == main.PatcherLanguage.en
                ? "Start Express Install Setup [bold springgreen2_1](Recommended)[/]"
                : $"{main.localizedText?["MainMenu"]?["startExpressSetup"]}";

            // Custom Install text
            string startCustomSetup = main.patcherLang == main.PatcherLanguage.en
                ? "Start Custom Install Setup [bold](Advanced)[/]"
                : $"{main.localizedText?["MainMenu"]?["startCustomSetup"]}";

            // Install Extras text
            string startExtrasSetup = main.patcherLang == main.PatcherLanguage.en
                ? "Install Extra Channels [bold grey](Optional)[/]"
                : $"{main.localizedText?["MainMenu"]?["startExtrasSetup"]}";


            // Settings text
            string settings = main.patcherLang == main.PatcherLanguage.en
                ? "Settings"
                : $"{main.localizedText?["MainMenu"]?["settings"]}";

            // Visit the GitHub repository text
            string visitGitHub = main.patcherLang == main.PatcherLanguage.en
                ? "Visit the GitHub Repository"
                : $"{main.localizedText?["MainMenu"]?["visitGitHub"]}";

            // Visit the WiiLink website text
            string visitWiiLink = main.patcherLang == main.PatcherLanguage.en
                ? "Visit the WiiLink Website"
                : $"{main.localizedText?["MainMenu"]?["visitWiiLink"]}";

            // Exit Patcher text
            string exitPatcher = main.patcherLang == main.PatcherLanguage.en
                ? "Exit Patcher"
                : $"{main.localizedText?["MainMenu"]?["exitPatcher"]}";

            // Print all the text
            AnsiConsole.MarkupLine($"[bold]{welcomeMessage}[/]\n");

            AnsiConsole.MarkupLine($"1. {startExpressSetup}");
            AnsiConsole.MarkupLine($"2. {startCustomSetup}");
            AnsiConsole.MarkupLine($"3. {startExtrasSetup}");
            AnsiConsole.MarkupLine($"4. {settings}\n");

            AnsiConsole.MarkupLine($"5. {visitGitHub}");

            AnsiConsole.MarkupLine($"6. {visitWiiLink}\n");

            AnsiConsole.MarkupLine($"7. {exitPatcher}\n");

            // Detect SD Card / USB Drive text
            string SDDetectedOrNot = main.sdcard != null
                ? $"[bold springgreen2_1]{(main.patcherLang == main.PatcherLanguage.en
                    ? "Detected SD Card / USB Drive:"
                    : main.localizedText?["MainMenu"]?["sdCardDetected"])}[/] {main.sdcard}"
                : $"[bold red]{(main.patcherLang == main.PatcherLanguage.en
                    ? "Could not detect your SD Card / USB Drive!"
                    : main.localizedText?["MainMenu"]?["noSDCard"])}[/]";
            AnsiConsole.MarkupLine(SDDetectedOrNot);

            // Automatically detect SD Card / USB Drive text
            string automaticDetection = main.patcherLang == main.PatcherLanguage.en
                ? "R. Automatically detect SD Card / USB Drive"
                : $"{main.localizedText?["MainMenu"]?["automaticDetection"]}";
            AnsiConsole.MarkupLine(automaticDetection);

            // Manually select SD Card / USB Drive text
            string manualDetection = main.patcherLang == main.PatcherLanguage.en
                ? "M. Manually select SD Card / USB Drive path\n"
                : $"{main.localizedText?["MainMenu"]?["manualDetection"]}\n";
            AnsiConsole.MarkupLine(manualDetection);

            // User chooses an option
            int choice = UserChoose("1234567RrMm");
            switch (choice)
            {
                case 1: // Start Express Install
                    express.WC24Setup();
                    break;
                case 2: // Start Custom Install
                    custom.CustomInstall_WiiLinkChannels_Setup();
                    break;
                case 3: // Start Extras Setup
                    extras.systemChannelRestorer_Setup();
                    break;
                case 4: // Settings                
                    SettingsMenu();
                    break;
                case 5: // Visit GitHub
                    VisitWebsite("https://github.com/WiiLink24/WiiLink24-Patcher");
                    break;
                case 6: // Visit WiiLink website
                    VisitWebsite("https://wiilink.ca");
                    break;
                case 7: // Clear console and Exit app
                    Console.Clear();
                    main.ExitApp();
                    break;
                case 8: // Automatically detect SD Card path (R/r)
                case 9:
                    main.sdcard = sd.DetectRemovableDrive;
                    break;
                case 10: // Manually select SD Card path (M/m)
                case 11:
                    sd.SDCardSelect();
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
        string connectionFailed = main.patcherLang == main.PatcherLanguage.en
            ? "Connection to server failed!"
            : $"{main.localizedText?["ServerDown"]?["connectionFailed"]}";
        AnsiConsole.MarkupLine($"[bold blink red]{connectionFailed}[/]\n");

        // Check internet connection text
        string checkInternet = main.patcherLang == main.PatcherLanguage.en
            ? "Connection to the server failed. Please check your internet connection and try again."
            : $"{main.localizedText?["ServerDown"]?["checkInternet"]}";
        string serverOrInternet = main.patcherLang == main.PatcherLanguage.en
            ? "It seems that either the server is down or your internet connection is not working."
            : $"{main.localizedText?["ServerDown"]?["serverOrInternet"]}";
        string reportIssue = main.patcherLang == main.PatcherLanguage.en
            ? "If you are sure that your internet connection is working, please join our [link=https://discord.gg/wiilink bold springgreen2_1]Discord Server[/] and report this issue."
            : $"{main.localizedText?["ServerDown"]?["reportIssue"]}";
        AnsiConsole.MarkupLine($"{checkInternet}\n");
        AnsiConsole.MarkupLine($"{serverOrInternet}\n");
        AnsiConsole.MarkupLine($"{reportIssue}\n");

        // Status code text
        string statusCodeText = main.patcherLang == main.PatcherLanguage.en
            ? "Status code:"
            : $"{main.localizedText?["ServerDown"]?["statusCode"]}";
        string messageText = main.patcherLang == main.PatcherLanguage.en
            ? "Message:"
            : $"{main.localizedText?["ServerDown"]?["message"]}";
        string exitMessage = main.patcherLang == main.PatcherLanguage.en
            ? "Press any key to exit..."
            : $"{main.localizedText?["ServerDown"]?["exitMessage"]}";
        AnsiConsole.MarkupLine($"{statusCodeText} {statusCode}");
        AnsiConsole.MarkupLine($"{messageText} {errorMsg}\n");

        AnsiConsole.MarkupLine($"[bold yellow]{exitMessage}[/]");

        Console.ReadKey();
        main.ExitApp();
    }

    // Page selection function
    public static (int, int) GetPageIndices(int currentPage, int totalItems, int itemsPerPage)
    {
        int start = (currentPage - 1) * itemsPerPage;
        int end = Math.Min(start + itemsPerPage, totalItems);
        return (start, end);
    }

    public static void SettingsMenu()
    {
        while (true)
        {
            PrintHeader();
            PrintNotice();

            // Settings text
            string settings = main.patcherLang == main.PatcherLanguage.en
                ? "Settings"
                : $"{main.localizedText?["SettingsMenu"]?["settings"]}";
            AnsiConsole.MarkupLine($"[bold springgreen2_1]{settings}[/]\n");

            if (!main.inCompatabilityMode)
            {
                // User choices
                string changeLanguage = main.patcherLang == main.PatcherLanguage.en
                    ? "Change Language"
                    : $"{main.localizedText?["SettingsMenu"]?["changeLanguage"]}";
                string credits = main.patcherLang == main.PatcherLanguage.en
                    ? "Credits"
                    : $"{main.localizedText?["SettingsMenu"]?["credits"]}";
                string goBack = main.patcherLang == main.PatcherLanguage.en
                    ? "Go back to Main Menu"
                    : $"{main.localizedText?["SettingsMenu"]?["goBack"]}";

                AnsiConsole.MarkupLine($"1. {changeLanguage}");
                AnsiConsole.MarkupLine($"2. {credits}\n");

                AnsiConsole.MarkupLine($"3. {goBack}\n");
            }
            else
            {
                Console.WriteLine("1. Credits\n");

                Console.WriteLine("2. Go back to Main Menu\n");
            }

            int choice = UserChoose("123");
            switch (choice)
            {
                case 1 when !main.inCompatabilityMode:
                    language.LanguageMenu(false);
                    break;
                case 1 when main.inCompatabilityMode:
                    CreditsScreen();
                    break;
                case 2 when !main.inCompatabilityMode:
                    CreditsScreen();
                    break;
                case 2 when main.inCompatabilityMode:
                    MainMenu();
                    break;
                case 3 when !main.inCompatabilityMode:
                    MainMenu();
                    break;
                default:
                    break;
            }
        }
    }

    public static void ErrorScreen(int exitCode, string msg = "")
    {
        // Clear regional and WiiConnect24 channel selections if they exist
        main.wiiLinkChannels_selection.Clear();
        main.wiiConnect24Channels_selection.Clear();
        main.extraChannels_selection.Clear();

        PrintHeader();

        // An error has occurred text
        string errorOccurred = main.patcherLang == main.PatcherLanguage.en
            ? "An error has occurred."
            : $"{main.localizedText?["ErrorScreen"]?["title"]}";
        AnsiConsole.MarkupLine($"[bold red]{errorOccurred}[/]\n");

        // Error details text
        string errorDetails = main.patcherLang == main.PatcherLanguage.en
            ? "ERROR DETAILS:"
            : $"{main.localizedText?["ErrorScreen"]?["details"]}";
        string taskText = main.patcherLang == main.PatcherLanguage.en
            ? "Task: "
            : $"{main.localizedText?["ErrorScreen"]?["task"]}";
        string commandText = main.patcherLang == main.PatcherLanguage.en
            ? "Command:"
            : $"{main.localizedText?["ErrorScreen"]?["command"]}";
        string messageText = main.patcherLang == main.PatcherLanguage.en
            ? "Message:"
            : $"{main.localizedText?["ErrorScreen"]?["message"]}";
        string exitCodeText = main.patcherLang == main.PatcherLanguage.en
            ? "Exit code:"
            : $"{main.localizedText?["ErrorScreen"]?["exitCode"]}";

        AnsiConsole.MarkupLine($"{errorDetails}\n");
        AnsiConsole.MarkupLine($" * {taskText} {main.task}");
        AnsiConsole.MarkupLine(msg == null ? $" * {commandText} {main.curCmd}" : $" * {messageText} {msg}");
        AnsiConsole.MarkupLine($" * {exitCodeText} {exitCode}\n");

        // Please open an issue text
        string openAnIssue = main.patcherLang == main.PatcherLanguage.en
            ? "Please open an issue on our GitHub page ([link bold springgreen2_1]https://github.com/WiiLink24/WiiLink24-Patcher/issues[/]) and describe the\nerror you encountered. Please include the error details above in your issue."
            : $"{main.localizedText?["ErrorScreen"]?["githubIssue"]}";
        AnsiConsole.MarkupLine($"{openAnIssue}\n");

        // Press any key to go back to the main menu text
        string pressAnyKey = main.patcherLang == main.PatcherLanguage.en
            ? "Press any key to go back to the main menu..."
            : $"{main.localizedText?["ErrorScreen"]?["pressAnyKey"]}";
        AnsiConsole.MarkupLine($"[bold]{pressAnyKey}[/]");
        Console.ReadKey();

        // Go back to the main menu
        MainMenu();
    }
}