using Spectre.Console;

public class ExtrasClass
{
    // Install Extras (Part 1 - System Channel Restorer)
    public static void systemChannelRestorer_Setup()
    {
        MainClass.task = "Install Extras (Part 1 - System Channel Restorer)";
        while (true)
        {
            MenuClass.PrintHeader();

            // Print title
            string installExtras = MainClass.patcherLang == "en-US"
                ? "Install Extras"
                : $"{MainClass.localizedText?["InstallExtras"]?["Header"]}";
            AnsiConsole.MarkupLine($"[bold springgreen2_1]{installExtras}[/]\n");

            // Print step number and title
            string stepNum = MainClass.patcherLang == "en-US"
                ? "Step 1"
                : $"{MainClass.localizedText?["InstallExtras"]?["systemChannelRestorer_Setup"]?["stepNum"]}";
            string stepTitle = MainClass.patcherLang == "en-US"
                ? "System Channel Restorer"
                : $"{MainClass.localizedText?["InstallExtras"]?["systemChannelRestorer_Setup"]?["stepTitle"]}";
            AnsiConsole.MarkupLine($"[bold]{stepNum}:[/] {stepTitle}\n");

            // Display console platform selection menu
            string AskSystemChannelRestorer = MainClass.patcherLang == "en-US"
                ? "Would you like to download System Channel Restorer?"
                : $"{MainClass.localizedText?["InstallExtras"]?["systemChannelRestorer_Setup"]?["systemChannelRestorer"]}";
            AnsiConsole.MarkupLine($"[bold]{AskSystemChannelRestorer}[/]\n");

            // Display console platform selection menu
            string systemChannelRestorerInfo = MainClass.patcherLang == "en-US"
                ? "System Channel Restorer is a homebrew application that allows for proper installation of Photo Channel 1.1 directly to your console."
                : $"{MainClass.localizedText?["InstallExtras"]?["systemChannelRestorer_Setup"]?["systemChannelRestorerInfo"]}";
            AnsiConsole.MarkupLine($"[grey]{systemChannelRestorerInfo}[/]");

            // Display console platform selection menu
            string moreSystemChannelRestorerInfo = MainClass.patcherLang == "en-US"
                ? "Use of System Channel Restorer requires an internet connection on your console, and is more difficult to use on Dolphin than offline WADs."
                : $"{MainClass.localizedText?["InstallExtras"]?["systemChannelRestorer_Setup"]?["moreSystemChannelRestorerInfo"]}";
            AnsiConsole.MarkupLine($"[grey]{moreSystemChannelRestorerInfo}[/]\n");

            // Print Console Platform options
            string useSystemChannelRestorer = MainClass.patcherLang == "en-US"
                ? "[bold]System Channel Restorer[/]"
                : $"{MainClass.localizedText?["InstallExtras"]?["systemChannelRestorer_Setup"]?["getSCR"]}";
            string offlineWADs = MainClass.patcherLang == "en-US"
                ? "[bold]Offline WADs[/]"
                : $"{MainClass.localizedText?["InstallExtras"]?["systemChannelRestorer_Setup"]?["offlineWADs"]}";
            AnsiConsole.MarkupLine($"[bold]1.[/] {useSystemChannelRestorer}");
            AnsiConsole.MarkupLine($"[bold]2.[/] {offlineWADs}\n");

            // Print instructions
            string platformInstructions = MainClass.patcherLang == "en-US"
                ? "< Press [bold white]a number[/] to make your selection, [bold white]Backspace[/] to go back, [bold white]ESC[/] to go back to exit setup >"
                : $"{MainClass.localizedText?["InstallExtras"]?["systemChannelRestorer_Setup"]?["selectionInstructions"]}";
            AnsiConsole.MarkupLine($"[grey]{platformInstructions}[/]\n");

            int choice = MenuClass.UserChoose("12");

            // Use a switch statement to handle user's SPD version selection
            switch (choice)
            {
                case -1: // Escape
                case -2: // Backspace
                    MenuClass.MainMenu();
                    break;
                case 1:
                    ExtraChannels_Setup(true);
                    break;
                case 2:
                    ExtraChannels_Setup(false);
                    break;
                default:
                    break;
            }
        }
    }


    // Install Extras (Part 2 - Select Extra channels)
    static void ExtraChannels_Setup(bool systemChannelRestorer)
    {
        MainClass.task = "Install Extras (Part 2 - Select Extra channels)";

        // Define a dictionary to map the extra channel names to easy-to-read format

        var extraChannelMap = new Dictionary<string, string>()
        {
            { "Wii Speak Channel [bold](USA)[/]", "ws_us" },
            { "Wii Speak Channel [bold](Europe)[/]", "ws_eu" },
            { "Wii Speak Channel [bold](Japan)[/]", "ws_jp" },
            { "Today and Tomorrow Channel [bold](Europe)[/]", "tatc_eu" },
            { "Today and Tomorrow Channel [bold](Japan)[/]", "tatc_jp" }
        };

        if (systemChannelRestorer == false)
        {
            extraChannelMap.Add("Photo Channel 1.1", "pc");
            extraChannelMap.Add("Internet Channel [bold](USA)[/]", "ic_us");
            extraChannelMap.Add("Internet Channel [bold](Europe)[/]", "ic_eu");
            extraChannelMap.Add("Internet Channel [bold](Japan)[/]", "ic_jp");
        }
        

        // Create channel map dictionary
        var channelMap = extraChannelMap.ToDictionary(x => x.Key, x => x.Value);

        // Initialize selection list to "Not selected" using LINQ
        if (MainClass.combinedChannels_selection.Count == 0) // Only do this
            MainClass.combinedChannels_selection = channelMap.Values.Select(_ => "[grey]Not selected[/]").ToList();

        if (systemChannelRestorer == true)
        {
            MainClass.combinedChannels_selection = MainClass.combinedChannels_selection.Append("scr").ToList();
            MainClass.extraChannels_selection.Add("scr");
        }

        // Page setup
        const int ITEMS_PER_PAGE = 9;
        int currentPage = 1;

        while (true)
        {
            MenuClass.PrintHeader();

            // Print title
            string installExtras = MainClass.patcherLang == "en-US"
                ? "Install Extras"
                : $"{MainClass.localizedText?["InstallExtras"]?["Header"]}";
            AnsiConsole.MarkupLine($"[bold springgreen2_1]{installExtras}[/]\n");

            // Print step number and title
            string stepNum = MainClass.patcherLang == "en-US"
                ? "Step 2"
                : $"{MainClass.localizedText?["InstallExtras"]?["ExtraChannels_Setup"]?["stepNum"]}";
            string stepTitle = MainClass.patcherLang == "en-US"
                ? "Select extra channel(s) to install"
                : $"{MainClass.localizedText?["InstallExtras"]?["ExtraChannels_Setup"]?["stepTitle"]}";
            AnsiConsole.MarkupLine($"[bold]{stepNum}:[/] {stepTitle}\n");

            // Display extra channel selection menu
            string selectExtraChns = MainClass.patcherLang == "en-US"
                ? "Select extra channel(s) to install:"
                : $"{MainClass.localizedText?["InstallExtras"]?["ExtraChannels_Setup"]?["selectExtraChns"]}";
            AnsiConsole.MarkupLine($"[bold]{selectExtraChns}[/]\n");
            var grid = new Grid();

            // Add channels to grid
            grid.AddColumn();
            grid.AddColumn();

            // Calculate the start and end indices for the items on the current page
            (int start, int end) = MenuClass.GetPageIndices(currentPage, channelMap.Count, ITEMS_PER_PAGE);

            // Display list of channels
            for (int i = start; i < end; i++)
            {
                KeyValuePair<string, string> channel = channelMap.ElementAt(i);
                grid.AddRow($"[bold][[{i - start + 1}]][/] {channel.Key}", MainClass.combinedChannels_selection[i]);

                // Add blank rows if there are less than nine pages
                if (i == end - 1 && end - start < 9)
                {
                    int numBlankRows = 9 - (end - start);
                    for (int j = 0; j < numBlankRows; j++)
                    {
                        grid.AddRow("", "");
                    }
                }
            }

            AnsiConsole.Write(grid);
            Console.WriteLine();

            // Page navigation
            double totalPages = Math.Ceiling((double)channelMap.Count / ITEMS_PER_PAGE);

            // Only display page navigation and number if there's more than one page
            if (totalPages > 1)
            {
                // If the current page is greater than 1, display a bold white '<<' for previous page navigation
                // Otherwise, display two lines '||'
                AnsiConsole.Markup(currentPage > 1 ? "[bold white]<<[/] " : "   ");

                // Print page number
                string pageNum = MainClass.patcherLang == "en-US"
                    ? $"Page {currentPage} of {totalPages}"
                    : $"{MainClass.localizedText?["CustomSetup"]?["pageNum"]}"
                        .Replace("{currentPage}", currentPage.ToString())
                        .Replace("{totalPages}", totalPages.ToString());
                AnsiConsole.Markup($"[bold]{pageNum}[/] ");

                // If the current page is less than total pages, display a bold white '>?' for next page navigation
                // Otherwise, display a space '  '
                AnsiConsole.Markup(currentPage < totalPages ? "[bold white]>>[/]" : "  ");

                // Print instructions
                //AnsiConsole.MarkupLine(" [grey](Press [bold white]<-[/] or [bold white]->[/] to navigate pages)[/]\n");
                string pageInstructions = MainClass.patcherLang == "en-US"
                    ? "(Press [bold white]<-[/] or [bold white]->[/] to navigate pages)"
                    : $"{MainClass.localizedText?["InstallExtras"]?["pageInstructions"]}";
                AnsiConsole.MarkupLine($" [grey]{pageInstructions}[/]\n");
            }

            // Print regular instructions
            string regInstructions = MainClass.patcherLang == "en-US"
                ? "< Press [bold white]a number[/] to select/deselect a channel, [bold white]ENTER[/] to continue, [bold white]Backspace[/] to go back, [bold white]ESC[/] to go back to exit setup >"
                : $"{MainClass.localizedText?["InstallExtras"]?["regInstructions"]}";
            AnsiConsole.MarkupLine($"[grey]{regInstructions}[/]\n");

            // Generate the choice string dynamically
            string choices = string.Join("", Enumerable.Range(1, ITEMS_PER_PAGE).Select(n => n.ToString()));
            int choice = MenuClass.UserChoose(choices);

            // Handle page navigation
            if (choice == -99 && currentPage > 1) // Left arrow
            {
                currentPage--;
            }
            else if (choice == 99 && currentPage < totalPages) // Right arrow
            {
                currentPage++;
            }

            // Not selected and Selected strings
            string notSelected = MainClass.patcherLang == "en-US"
                ? "Not selected"
                : $"{MainClass.localizedText?["InstallExtras"]?["notSelected"]}";
            string selectedText = MainClass.patcherLang == "en-US"
                ? "Selected"
                : $"{MainClass.localizedText?["InstallExtras"]?["selected"]}";

            // Handle user input
            switch (choice)
            {
                case -1: // Escape
                    // Clear selection list
                    MainClass.combinedChannels_selection.Clear();
                    MainClass.extraChannels_selection.Clear();
                    channelMap.Clear();
                    MenuClass.MainMenu();
                    break;
                case -2: // Backspace
                    // Clear selection list
                    MainClass.combinedChannels_selection.Clear();
                    MainClass.extraChannels_selection.Clear();
                    channelMap.Clear();
                    systemChannelRestorer_Setup();
                    break;
                case 0: // Enter
                    // Save selected channels to global variable if any are selected, divide them into WiiLink and WC24 channels
                    foreach (string channel in channelMap.Values.Where(MainClass.combinedChannels_selection.Contains))
                    {
                        if (extraChannelMap.ContainsValue(channel) && !MainClass.extraChannels_selection.Contains(channel))
                            MainClass.extraChannels_selection.Add(channel);
                    }
                    // If selection is empty, display error message
                    if (!channelMap.Values.Any(MainClass.combinedChannels_selection.Contains))
                    {
                        //AnsiConsole.MarkupLine("\n[bold red]ERROR:[/] You must select at least one channel to proceed!");
                        string mustSelectOneChannel = MainClass.patcherLang == "en-US"
                            ? "[bold red]ERROR:[/] You must select at least one channel to proceed!"
                            : $"{MainClass.localizedText?["InstallExtras"]?["mustSelectOneChannel"]}";
                        AnsiConsole.MarkupLine($"\n{mustSelectOneChannel}");
                        Thread.Sleep(3000);
                        continue;
                    }

                    // Go to next step
                    ExtraChannels_ConsolePlatform_Setup();
                    break;
                default:
                    if (choice >= 1 && choice <= Math.Min(ITEMS_PER_PAGE, channelMap.Count - start))
                    {
                        int index = start + choice - 1;
                        string channelName = channelMap.Values.ElementAt(index);
                        if (MainClass.combinedChannels_selection.Contains(channelName))
                        {
                            MainClass.combinedChannels_selection = MainClass.combinedChannels_selection.Where(val => val != channelName).ToList();
                            MainClass.combinedChannels_selection[index] = $"[grey]{notSelected}[/]";
                        }
                        else
                        {
                            MainClass.combinedChannels_selection = MainClass.combinedChannels_selection.Append(channelName).ToList();
                            MainClass.combinedChannels_selection[index] = $"[bold springgreen2_1]{selectedText}[/]";
                        }
                    }
                    break;
            }
        }
    }

    // Install Extras (Part 3 - Select Console Platform)
    static void ExtraChannels_ConsolePlatform_Setup()
    {
        MainClass.task = "Install Extras (Part 3 - Select Console Platform)";
        while (true)
        {
            MenuClass.PrintHeader();

            // Print title
            string installExtras = MainClass.patcherLang == "en-US"
                ? "Install Extras"
                : $"{MainClass.localizedText?["InstallExtras"]?["Header"]}";
            AnsiConsole.MarkupLine($"[bold springgreen2_1]{installExtras}[/]\n");

            // Print step number and title
            string stepNum = MainClass.patcherLang == "en-US"
                ? "Step 3"
                : $"{MainClass.localizedText?["CustomSetup"]?["ConsolePlatform_Setup"]?["stepNum"]}";
            string stepTitle = MainClass.patcherLang == "en-US"
                ? "Select Console Platform"
                : $"{MainClass.localizedText?["CustomSetup"]?["ConsolePlatform_Setup"]?["stepTitle"]}";
            AnsiConsole.MarkupLine($"[bold]{stepNum}:[/] {stepTitle}\n");

            // Display console platform selection menu
            string selectConsolePlatform = MainClass.patcherLang == "en-US"
                ? "Which console platform are you installing these channels on?"
                : $"{MainClass.localizedText?["CustomSetup"]?["ConsolePlatform_Setup"]?["selectConsolePlatform"]}";
            AnsiConsole.MarkupLine($"[bold]{selectConsolePlatform}[/]\n");

            // Print Console Platform options
            string onWii = MainClass.patcherLang == "en-US"
                ? "[bold]Wii[/]"
                : $"{MainClass.localizedText?["CustomSetup"]?["ConsolePlatform_Setup"]?["onWii"]}";
            string onvWii = MainClass.patcherLang == "en-US"
                ? "[bold]vWii (Wii U)[/]"
                : $"{MainClass.localizedText?["CustomSetup"]?["ConsolePlatform_Setup"]?["onvWii"]}";
            string onDolphin = MainClass.patcherLang == "en-US"
                ? "[bold]Dolphin Emulator[/]"
                : $"{MainClass.localizedText?["CustomSetup"]?["ConsolePlatform_Setup"]?["onDolphin"]}";
            AnsiConsole.MarkupLine($"[bold]1.[/] {onWii}");
            AnsiConsole.MarkupLine($"[bold]2.[/] {onvWii}");
            AnsiConsole.MarkupLine($"[bold]3.[/] {onDolphin}\n");

            // Print instructions
            string platformInstructions = MainClass.patcherLang == "en-US"
                ? "< Press [bold white]a number[/] to select platform, [bold white]Backspace[/] to go back, [bold white]ESC[/] to go back to exit setup >"
                : $"{MainClass.localizedText?["CustomSetup"]?["ConsolePlatform_Setup"]?["platformInstructions"]}";
            AnsiConsole.MarkupLine($"[grey]{platformInstructions}[/]\n");

            int choice = MenuClass.UserChoose("123");

            // Use a switch statement to handle user's SPD version selection
            switch (choice)
            {
                case -1: // Escape
                    MainClass.combinedChannels_selection.Clear();
                    MainClass.extraChannels_selection.Clear();
                    MenuClass.MainMenu();
                    break;
                case -2: // Backspace
                    systemChannelRestorer_Setup();
                    break;
                case 1:
                    MainClass.platformType_custom = MainClass.Platform.Wii;
                    MainClass.platformType = MainClass.Platform.Wii;
                    ExtraChannels_SummaryScreen(showSPD: true);
                    break;
                case 2:
                    MainClass.platformType_custom = MainClass.Platform.vWii;
                    MainClass.platformType = MainClass.Platform.vWii;
                    ExtraChannels_SummaryScreen(showSPD: true);
                    break;
                case 3:
                    MainClass.platformType_custom = MainClass.Platform.Dolphin;
                    MainClass.platformType = MainClass.Platform.Dolphin;
                    ExtraChannels_SummaryScreen(showSPD: true);
                    break;
                default:
                    break;
            }
        }
    }


    // Install Extras (Part 4 - Show summary of selected channels to be installed)
    static void ExtraChannels_SummaryScreen(bool showSPD = false)
    {
        MainClass.task = "Install Extras (Part 4 - Show summary of selected channels to be installed)";

        // Convert extra channel names to proper names
        var extraChannelMap = new Dictionary<string, string>()
        {
            { "ws_us", "● Wii Speak Channel [bold](USA)[/]" },
            { "ws_eu", "● Wii Speak Channel [bold](Europe)[/]" },
            { "ws_jp", "● Wii Speak Channel [bold](Japan)[/]" },
            { "tatc_eu", "● Today and Tomorrow Channel [bold](Europe)[/]" },
            { "tatc_jp", "● Today and Tomorrow Channel [bold](Japan)[/]" },
            { "pc", "● Photo Channel 1.1" },
            { "ic_us", "● Internet Channel [bold](USA)[/]" },
            { "ic_eu", "● Internet Channel [bold](Europe)[/]" },
            { "ic_jp", "● Internet Channel [bold](Japan)[/]" },
            { "scr", "● System Channel Restorer" }
        };

        var selectedExtraChannels = new List<string>();
        foreach (string channel in MainClass.combinedChannels_selection)
        {
            if (extraChannelMap.TryGetValue(channel, out string? modifiedChannel))
                selectedExtraChannels.Add(modifiedChannel);
        }

        while (true)
        {
            MenuClass.PrintHeader();

            // Print title
            string installExtras = MainClass.patcherLang == "en-US"
                ? "Install Extras"
                : $"{MainClass.localizedText?["InstallExtras"]?["Header"]}";
            string summaryHeader = MainClass.patcherLang == "en-US"
                ? "Summary of selected channels to be installed:"
                : $"{MainClass.localizedText?["InstallExtras"]?["summaryScreen"]?["summaryHeader"]}";
            AnsiConsole.MarkupLine($"[bold springgreen2_1]{installExtras}[/]\n");
            AnsiConsole.MarkupLine($"[bold]{summaryHeader}[/]\n");

            // Display summary of selected channels in two columns using a grid
            var grid = new Grid();
            grid.AddColumn();
            grid.AddColumn();

            // Grid header text
            string extraChannels = MainClass.patcherLang == "en-US"
                ? "Extra Channels:"
                : $"{MainClass.localizedText?["InstallExtras"]?["summaryScreen"]?["extraChannels"]}";
            string consoleVersion = MainClass.patcherLang == "en-US"
                ? "Console Platform:"
                : $"{MainClass.localizedText?["InstallExtras"]?["summaryScreen"]?["ConsoleVersion"]}";

            grid.AddRow($"[bold springgreen2_1]{extraChannels}[/]", $"[bold]{consoleVersion}[/]");

            if (MainClass.platformType_custom == MainClass.Platform.Wii)
                grid.AddRow(string.Join("\n", selectedExtraChannels), "● [bold]Wii[/]");
            else if (MainClass.platformType_custom == MainClass.Platform.vWii)
                grid.AddRow(string.Join("\n", selectedExtraChannels), "● [bold]vWii (Wii U)[/]");
            else
                grid.AddRow(string.Join("\n", selectedExtraChannels), "● [bold]Dolphin Emulator[/]");

            AnsiConsole.Write(grid);

            // Print instructions
            string prompt = MainClass.patcherLang == "en-US"
                ? "Are you sure you want to install these selected channels?"
                : $"{MainClass.localizedText?["InstallExtras"]?["summaryScreen"]?["confirmation"]?["prompt"]}";

            // User confirmation strings
            string yes = MainClass.patcherLang == "en-US"
                ? "Yes"
                : $"{MainClass.localizedText?["yes"]}";
            string noStartOver = MainClass.patcherLang == "en-US"
                ? "No, start over"
                : $"{MainClass.localizedText?["InstallExtras"]?["summaryScreen"]?["confirmation"]?["noStartOver"]}";
            string noGoBackToMainMenu = MainClass.patcherLang == "en-US"
                ? "No, go back to Main Menu"
                : $"{MainClass.localizedText?["InstallExtras"]?["summaryScreen"]?["confirmation"]?["noGoBackToMainMenu"]}";

            AnsiConsole.MarkupLine($"\n[bold]{prompt}[/]\n");

            AnsiConsole.MarkupLine($"1. {yes}");
            AnsiConsole.MarkupLine($"2. {noStartOver}\n");

            AnsiConsole.MarkupLine($"3. {noGoBackToMainMenu}\n");

            var choice = MenuClass.UserChoose("123");

            // Handle user confirmation choice
            switch (choice)
            {
                case 1: // Yes
                    if (MainClass.platformType_custom != MainClass.Platform.Dolphin)
                    {
                        SdClass.SDSetup(MainClass.SetupType.extras);
                        break;
                    }
                    else
                    {
                        MainClass.sdcard = null;
                        MenuClass.WADFolderCheck(MainClass.SetupType.extras);
                        break;
                    }
                case 2: // No, start over
                    MainClass.combinedChannels_selection.Clear();
                    MainClass.extraChannels_selection.Clear();
                    systemChannelRestorer_Setup();
                    break;
                case 3: // No, go back to main menu
                    MainClass.combinedChannels_selection.Clear();
                    MainClass.extraChannels_selection.Clear();
                    MenuClass.MainMenu();
                    break;
                default:
                    break;
            }
        }
    }

}