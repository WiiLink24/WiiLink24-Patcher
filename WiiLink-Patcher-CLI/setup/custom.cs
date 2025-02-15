using System.Text;
using Spectre.Console;

public class custom
{
    // Custom Install (Part 1 - Select WiiLink channels)
    public static void CustomInstall_WiiLinkChannels_Setup()
    {
        main.task = "Custom Install (Part 1 - Select WiiLink channels)";

        // Define a dictionary to map the Japanese channel names to easy-to-read format
        var wiiLinkChannelMap = new Dictionary<string, string>()
        {
            { "Wii Room [bold](English)[/]", "wiiroom_en" },
            { "Wii Room [bold](Spanish)[/]", "wiiroom_es"},
            { "Wii Room [bold](French)[/]","wiiroom_fr" },
            { "Wii Room [bold](Deutsch)[/]", "wiiroom_de" },
            { "Wii Room [bold](Italiano)[/]", "wiiroom_it" },
            { "Wii Room [bold](Nederlands)[/]", "wiiroom_du" },
            { "Wii Room [bold](Português)[/]", "wiiroom_ptbr" },
            { "Wii Room [bold](Русский)[/]", "wiiroom_ru" },
            { "Wii no Ma [bold](Japanese)[/]", "wiinoma_jp" },
            { "Photo Prints Channel [bold](English)[/]", "digicam_en" },
            { "Digicam Print Channel [bold](Japanese)[/]", "digicam_jp" },
            { "Food Channel [bold](Standard) (USA) [[English]][/]", "food_us" },
            { "Food Channel [bold](Standard) (Europe) [[English]][/]", "food_eu" },
            { "Demae Channel [bold](Standard) [[Japanese]][/]", "demae_jp" },
            { "Food Channel [bold](Dominos) [[English]][/]", "food_dominos" },
            { "Kirby TV Channel", "kirbytv" }
        };

        // Define a dictionary to map the WC24 channel names to easy-to-read format
        var wc24ChannelMap = new Dictionary<string, string>()
        {
            { "Nintendo Channel [bold](USA)[/]", "nc_us" },
            { "Nintendo Channel [bold](Europe)[/]", "nc_eu" },
            { "Minna no Nintendo Channel [bold](Japan)[/]", "mnnc_jp" },
            { "Forecast Channel [bold](USA)[/]", "forecast_us" },
            { "Forecast Channel [bold](Europe)[/]", "forecast_eu" },
            { "Forecast Channel [bold](Japan)[/]", "forecast_jp" },
            { "News Channel [bold](USA)[/]", "news_us" },
            { "News Channel [bold](Europe)[/]", "news_eu" },
            { "News Channel [bold](Japan)[/]", "news_jp" },
            { "Everybody Votes Channel [bold](USA)[/]", "evc_us" },
            { "Everybody Votes Channel [bold](Europe)[/]", "evc_eu" },
            { "Everybody Votes Channel [bold](Japan)[/]", "evc_jp" },
            { "Check Mii Out Channel [bold](USA)[/]", "cmoc_us" },
            { "Mii Contest Channel [bold](Europe)[/]", "cmoc_eu" },
            { "Mii Contest Channel [bold](Japan)[/]", "cmoc_jp" }
        };

        // Merge the two dictionaries into one
        var channelMap = wc24ChannelMap.Concat(wiiLinkChannelMap).ToDictionary(x => x.Key, x => x.Value);

        // Not selected and Selected strings
            string notSelected = main.patcherLang == main.PatcherLanguage.en
                ? "Not selected"
                : $"{main.localizedText?["CustomSetup"]?["notSelected"]}";
            string selectedText = main.patcherLang == main.PatcherLanguage.en
                ? "Selected"
                : $"{main.localizedText?["CustomSetup"]?["selected"]}";

        // Initialize selection list to "Not selected" using LINQ
        if (main.combinedChannels_selection.Count == 0) // Only do this
            
            main.combinedChannels_selection = channelMap.Values.Select(_ => $"[grey]{notSelected}[/]").ToList();

        // Page setup
        const int ITEMS_PER_PAGE = 9;
        int currentPage = 1;

        while (true)
        {
            menu.PrintHeader();

            // Print title
            string customInstall = main.patcherLang == main.PatcherLanguage.en
                ? "Custom Install"
                : $"{main.localizedText?["CustomSetup"]?["Header"]}";
            AnsiConsole.MarkupLine($"[bold springgreen2_1]{customInstall}[/]\n");

            // Print step number and title
            string stepNum = main.patcherLang == main.PatcherLanguage.en
                ? "Step 1"
                : $"{main.localizedText?["CustomSetup"]?["wiiLinkChannels_Setup"]?["stepNum"]}";
            string stepTitle = main.patcherLang == main.PatcherLanguage.en
                ? "Select WiiConnect24 / Regional channel(s) to install"
                : $"{main.localizedText?["CustomSetup"]?["wiiLinkChannels_Setup"]?["stepTitle"]}";
            AnsiConsole.MarkupLine($"[bold]{stepNum}:[/] {stepTitle}\n");

            // Display WiiLink channel selection menu
            string selectWiiLinkChns = main.patcherLang == main.PatcherLanguage.en
                ? "Select WiiConnect24 / Regional channel(s) to install:"
                : $"{main.localizedText?["CustomSetup"]?["wiiLinkChannels_Setup"]?["selectWiiLinkChns"]}";
            AnsiConsole.MarkupLine($"[bold]{selectWiiLinkChns}[/]\n");
            var grid = new Grid();

            // Add channels to grid
            grid.AddColumn();
            grid.AddColumn();

            // Calculate the start and end indices for the items on the current page
            (int start, int end) = menu.GetPageIndices(currentPage, channelMap.Count, ITEMS_PER_PAGE);

            // Display list of channels
            for (int i = start; i < end; i++)
            {
                KeyValuePair<string, string> channel = channelMap.ElementAt(i);
                grid.AddRow($"[bold][[{i - start + 1}]][/] {channel.Key}", main.combinedChannels_selection[i]);

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
                string pageNum = main.patcherLang == main.PatcherLanguage.en
                    ? $"Page {currentPage} of {totalPages}"
                    : $"{main.localizedText?["CustomSetup"]?["pageNum"]}"
                        .Replace("{currentPage}", currentPage.ToString())
                        .Replace("{totalPages}", totalPages.ToString());
                AnsiConsole.Markup($"[bold]{pageNum}[/] ");

                // If the current page is less than total pages, display a bold white '>?' for next page navigation
                // Otherwise, display a space '  '
                AnsiConsole.Markup(currentPage < totalPages ? "[bold white]>>[/]" : "  ");

                // Print instructions
                //AnsiConsole.MarkupLine(" [grey](Press [bold white]<-[/] or [bold white]->[/] to navigate pages)[/]\n");
                string pageInstructions = main.patcherLang == main.PatcherLanguage.en
                    ? "(Press [bold white]<-[/] or [bold white]->[/] to navigate pages)"
                    : $"{main.localizedText?["CustomSetup"]?["pageInstructions"]}";
                AnsiConsole.MarkupLine($" [grey]{pageInstructions}[/]\n");
            }

            // Print regular instructions
            string regInstructions = main.patcherLang == main.PatcherLanguage.en
                ? "< Press [bold white]a number[/] to select/deselect a channel, [bold white]ENTER[/] to continue, [bold white]Backspace[/] to go back, [bold white]ESC[/] to go back to exit setup >"
                : $"{main.localizedText?["CustomSetup"]?["regInstructions"]}";
            AnsiConsole.MarkupLine($"[grey]{regInstructions}[/]\n");

            // Generate the choice string dynamically
            string choices = string.Join("", Enumerable.Range(1, ITEMS_PER_PAGE).Select(n => n.ToString()));
            int choice = menu.UserChoose(choices);

            // Handle page navigation
            if (choice == -99 && currentPage > 1) // Left arrow
            {
                currentPage--;
            }
            else if (choice == 99 && currentPage < totalPages) // Right arrow
            {
                currentPage++;
            }

            // Handle user input
            switch (choice)
            {
                case -1: // Escape
                case -2: // Backspace
                    // Clear selection list
                    main.wiiLinkChannels_selection.Clear();
                    main.wiiConnect24Channels_selection.Clear();
                    main.combinedChannels_selection.Clear();
                    channelMap.Clear();
                    menu.MainMenu();
                    break;
                case 0: // Enter
                    // Save selected channels to global variable if any are selected, divide them into WiiLink and WC24 channels
                    foreach (string channel in channelMap.Values.Where(main.combinedChannels_selection.Contains))
                    {
                        if (wiiLinkChannelMap.ContainsValue(channel) && !main.wiiLinkChannels_selection.Contains(channel))
                            main.wiiLinkChannels_selection.Add(channel);
                        else if (wc24ChannelMap.ContainsValue(channel) && !main.wiiConnect24Channels_selection.Contains(channel))
                            main.wiiConnect24Channels_selection.Add(channel);
                    }
                    // If selection is empty, display error message
                    if (!channelMap.Values.Any(main.combinedChannels_selection.Contains))
                    {
                        //AnsiConsole.MarkupLine("\n[bold red]ERROR:[/] You must select at least one channel to proceed!");
                        string mustSelectOneChannel = main.patcherLang == main.PatcherLanguage.en
                            ? "[bold red]ERROR:[/] You must select at least one channel to proceed!"
                            : $"{main.localizedText?["CustomSetup"]?["mustSelectOneChannel"]}";
                        AnsiConsole.MarkupLine($"\n{mustSelectOneChannel}");
                        Thread.Sleep(3000);
                        continue;
                    }

                    // Go to next step
                    CustomInstall_ConsolePlatform_Setup();
                    break;
                default:
                    if (choice >= 1 && choice <= Math.Min(ITEMS_PER_PAGE, channelMap.Count - start))
                    {
                        int index = start + choice - 1;
                        string channelName = channelMap.Values.ElementAt(index);
                        if (main.combinedChannels_selection.Contains(channelName))
                        {
                            main.combinedChannels_selection = main.combinedChannels_selection.Where(val => val != channelName).ToList();
                            main.combinedChannels_selection[index] = $"[grey]{notSelected}[/]";
                        }
                        else
                        {
                            main.combinedChannels_selection = main.combinedChannels_selection.Append(channelName).ToList();
                            main.combinedChannels_selection[index] = $"[bold springgreen2_1]{selectedText}[/]";
                        }
                    }
                    break;
            }
        }
    }

    // Custom Install (Part 2 - Select Console Platform)
    static void CustomInstall_ConsolePlatform_Setup()
    {
        main.task = "Custom Install (Part 2 - Select Console Platform)";
        while (true)
        {
            menu.PrintHeader();

            // Print title
            string customInstall = main.patcherLang == main.PatcherLanguage.en
                ? "Custom Install"
                : $"{main.localizedText?["CustomSetup"]?["Header"]}";
            AnsiConsole.MarkupLine($"[bold springgreen2_1]{customInstall}[/]\n");

            // Print step number and title
            string stepNum = main.patcherLang == main.PatcherLanguage.en
                ? "Step 2"
                : $"{main.localizedText?["CustomSetup"]?["ConsolePlatform_Setup"]?["stepNum"]}";
            string stepTitle = main.patcherLang == main.PatcherLanguage.en
                ? "Select Console Platform"
                : $"{main.localizedText?["CustomSetup"]?["ConsolePlatform_Setup"]?["stepTitle"]}";
            AnsiConsole.MarkupLine($"[bold]{stepNum}:[/] {stepTitle}\n");

            // Display console platform selection menu
            string selectConsolePlatform = main.patcherLang == main.PatcherLanguage.en
                ? "Which console platform are you installing these channels on?"
                : $"{main.localizedText?["CustomSetup"]?["ConsolePlatform_Setup"]?["selectConsolePlatform"]}";
            AnsiConsole.MarkupLine($"[bold]{selectConsolePlatform}[/]\n");

            // Print Console Platform options
            string onWii = main.patcherLang == main.PatcherLanguage.en
                ? "[bold]Wii[/]"
                : $"{main.localizedText?["CustomSetup"]?["ConsolePlatform_Setup"]?["onWii"]}";
            string onvWii = main.patcherLang == main.PatcherLanguage.en
                ? "[bold]vWii (Wii U)[/]"
                : $"{main.localizedText?["CustomSetup"]?["ConsolePlatform_Setup"]?["onvWii"]}";
            string onDolphin = main.patcherLang == main.PatcherLanguage.en
                ? "[bold]Dolphin Emulator[/]"
                : $"{main.localizedText?["CustomSetup"]?["ConsolePlatform_Setup"]?["onDolphin"]}";
            AnsiConsole.MarkupLine($"[bold]1.[/] {onWii}");
            AnsiConsole.MarkupLine($"[bold]2.[/] {onvWii}");
            AnsiConsole.MarkupLine($"[bold]3.[/] {onDolphin}\n");

            // Print instructions
            string platformInstructions = main.patcherLang == main.PatcherLanguage.en
                ? "< Press [bold white]a number[/] to select platform, [bold white]Backspace[/] to go back, [bold white]ESC[/] to go back to exit setup >"
                : $"{main.localizedText?["CustomSetup"]?["ConsolePlatform_Setup"]?["platformInstructions"]}";
            AnsiConsole.MarkupLine($"[grey]{platformInstructions}[/]\n");

            int choice = menu.UserChoose("123");

            // Use a switch statement to handle user's SPD version selection
            switch (choice)
            {
                case -1: // Escape
                    main.combinedChannels_selection.Clear();
                    main.wiiLinkChannels_selection.Clear();
                    main.wiiConnect24Channels_selection.Clear();
                    menu.MainMenu();
                    break;
                case -2: // Backspace
                    CustomInstall_WiiLinkChannels_Setup();
                    break;
                case 1:
                    main.platformType_custom = main.Platform.Wii;
                    main.platformType = main.Platform.Wii;
                    CustomInstall_SummaryScreen(showSPD: true);
                    break;
                case 2:
                    main.platformType_custom = main.Platform.vWii;
                    main.platformType = main.Platform.vWii;
                    CustomInstall_SummaryScreen(showSPD: true);
                    break;
                case 3:
                    main.platformType_custom = main.Platform.Dolphin;
                    main.platformType = main.Platform.Dolphin;
                    CustomInstall_SummaryScreen(showSPD: true);
                    break;
                default:
                    break;
            }
        }
    }


    // Custom Install (Part 3 - Show summary of selected channels to be installed)
    static void CustomInstall_SummaryScreen(bool showSPD = false)
    {
        main.task = "Custom Install (Part 3 - Show summary of selected channels to be installed)";
        // Convert WiiLink channel names to proper names
        var wiiLinkChannelMap = new Dictionary<string, string>()
        {
            { "wiiroom_en", "● Wii Room [bold](English)[/]" },
            { "wiiroom_es", "● Wii Room [bold](Spanish)[/]"},
            { "wiiroom_fr", "● Wii Room [bold](French)[/]" },
            { "wiiroom_de", "● Wii Room [bold](Deutsch)[/]" },
            { "wiiroom_it", "● Wii Room [bold](Italiano)[/]" },
            { "wiiroom_du", "● Wii Room [bold](Nederlands)[/]" },
            { "wiiroom_ptbr", "● Wii Room [bold](Português)[/]" },
            { "wiiroom_ru", "● Wii Room [bold](Русский)[/]" },
            { "wiinoma_jp", "● Wii no Ma [bold](Japanese)[/]" },
            { "digicam_en", "● Photo Prints Channel [bold](English)[/]" },
            { "digicam_jp", "● Digicam Print Channel [bold](Japanese)[/]" },
            { "food_us", "● Food Channel [bold](Standard) (USA) [[English]][/]" },
            { "food_eu", "● Food Channel [bold](Standard) (Europe) [[English]][/]" },
            { "demae_jp", "● Demae Channel [bold](Standard) [[Japanese]][/]" },
            { "food_dominos", "● Food Channel [bold](Dominos) [[English]][/]" },
            { "kirbytv", "● Kirby TV Channel" }
        };

        var selectedRegionalChannels = new List<string>();
        if (main.wiiLinkChannels_selection.Count > 0)
        {
            foreach (string channel in main.combinedChannels_selection)
            {
                if (wiiLinkChannelMap.TryGetValue(channel, out string? modifiedChannel))
                    selectedRegionalChannels.Add(modifiedChannel);
            }
        }
        else
        {
            selectedRegionalChannels.Add("● [grey]N/A[/]");
        }

        // Convert WiiConnect24 channel names to proper names
        var wiiConnect24ChannelMap = new Dictionary<string, string>()
        {
            { "nc_us", "● Nintendo Channel [bold](USA)[/]" },
            { "nc_eu", "● Nintendo Channel [bold](Europe)[/]" },
            { "mnnc_jp", "● Minna no Nintendo Channel [bold](Japan)[/]" },
            { "forecast_us", "● Forecast Channel [bold](USA)[/]" },
            { "forecast_eu", "● Forecast Channel [bold](Europe)[/]"},
            { "forecast_jp", "● Forecast Channel [bold](Japan)[/]"},
            { "news_us", "● News Channel [bold](USA)[/]" },
            { "news_eu", "● News Channel [bold](Europe)[/]" },
            { "news_jp", "● News Channel [bold](Japan)[/]" },
            { "evc_us", "● Everybody Votes Channel [bold](USA)[/]" },
            { "evc_eu", "● Everybody Votes Channel [bold](Europe)[/]" },
            { "evc_jp", "● Everybody Votes Channel [bold](Japan)[/]" },
            { "cmoc_us", "● Check Mii Out Channel [bold](USA)[/]" },
            { "cmoc_eu", "● Mii Contest Channel [bold](Europe)[/]" },
            { "cmoc_jp", "● Mii Contest Channel [bold](Japan)[/]" }
        };

        var selectedWiiConnect24Channels = new List<string>();
        foreach (string channel in main.combinedChannels_selection)
        {
            if (wiiConnect24ChannelMap.TryGetValue(channel, out string? modifiedChannel))
                selectedWiiConnect24Channels.Add(modifiedChannel);
        }

        if (!selectedRegionalChannels.Any())
            selectedRegionalChannels.Add("● [grey]N/A[/]");
        if (!selectedWiiConnect24Channels.Any())
            selectedWiiConnect24Channels.Add("● [grey]N/A[/]");

        while (true)
        {
            menu.PrintHeader();

            // Print title
            string customInstall = main.patcherLang == main.PatcherLanguage.en
                ? "Custom Install"
                : $"{main.localizedText?["CustomSetup"]?["Header"]}";
            string summaryHeader = main.patcherLang == main.PatcherLanguage.en
                ? "Summary of selected channels to be installed:"
                : $"{main.localizedText?["CustomSetup"]?["summaryScreen"]?["summaryHeader"]}";
            AnsiConsole.MarkupLine($"[bold springgreen2_1]{customInstall}[/]\n");
            AnsiConsole.MarkupLine($"[bold]{summaryHeader}[/]\n");

            // Display summary of selected channels in two columns using a grid
            var grid = new Grid();
            grid.AddColumn();
            grid.AddColumn();

            // Grid header text
            string regionalChannels = main.patcherLang == main.PatcherLanguage.en
                ? "Regional channels:"
                : $"{main.localizedText?["CustomSetup"]?["summaryScreen"]?["wiiLinkChannels"]}";
            string wiiConnect24Channels = main.patcherLang == main.PatcherLanguage.en
                ? "WiiConnect24 Channels:"
                : $"{main.localizedText?["CustomSetup"]?["summaryScreen"]?["wiiConnect24Channels"]}";
            string consoleVersion = main.patcherLang == main.PatcherLanguage.en
                ? "Console Platform:"
                : $"{main.localizedText?["CustomSetup"]?["summaryScreen"]?["ConsoleVersion"]}";

            grid.AddColumn();

            grid.AddRow($"[bold deepskyblue1]{wiiConnect24Channels}[/]", $"[bold springgreen2_1]{regionalChannels}[/]", $"[bold]{consoleVersion}[/]");

            if (main.platformType_custom == main.Platform.Wii)
                grid.AddRow(string.Join("\n", selectedWiiConnect24Channels), string.Join("\n", selectedRegionalChannels), "● [bold]Wii[/]");
            else if (main.platformType_custom == main.Platform.vWii)
                grid.AddRow(string.Join("\n", selectedWiiConnect24Channels), string.Join("\n", selectedRegionalChannels), "● [bold]vWii (Wii U)[/]");
            else
                grid.AddRow(string.Join("\n", selectedWiiConnect24Channels), string.Join("\n", selectedRegionalChannels), "● [bold]Dolphin Emulator[/]");

            AnsiConsole.Write(grid);

            // If user chose vWii as their platform, notify that the EULA channel will be included
            if (main.platformType_custom == main.Platform.vWii && main.wiiConnect24Channels_selection.Any())
            {
                string eulaChannel = main.patcherLang == main.PatcherLanguage.en
                    ? "[bold]NOTE:[/] For [bold deepskyblue1]vWii[/] users, The EULA channel will be included."
                    : $"{main.localizedText?["CustomSetup"]?["summaryScreen"]?["eulaChannel"]}";
                AnsiConsole.MarkupLine($"\n{eulaChannel}");
            }

            // If user chose Russian Wii Room, provide extra instructions
            if (main.combinedChannels_selection.Contains("wiiroom_ru"))
            {
                AnsiConsole.MarkupLine("\n[bold yellow]NOTICE FOR RUSSIAN USERS[/]\n");
                AnsiConsole.MarkupLine("Proper functionality is not guaranteed for systems without the Russian Wii menu.\n");
                AnsiConsole.MarkupLine("Read the installation guide here (Russian only for now):");
                AnsiConsole.MarkupLine("[bold link springgreen2_1]https://wii.zazios.ru/rus_menu[/]");
            }

            if (main.combinedChannels_selection.Contains("food_dominos"))
            {
                string internetNotice = main.patcherLang == main.PatcherLanguage.en
                    ? "[bold]NOTE:[/] For [bold]Food Channel (Dominos)[/] users, the Internet Channel will be included to allow you to track your order."
                    : $"{main.localizedText?["CustomSetup"]?["summaryScreen"]?["internetNotice"]}";
                AnsiConsole.MarkupLine($"\n{internetNotice}");
            }

            // Print instructions
            string prompt = main.patcherLang == main.PatcherLanguage.en
                ? "Are you sure you want to install these selected channels?"
                : $"{main.localizedText?["CustomSetup"]?["summaryScreen"]?["confirmation"]?["prompt"]}";

            // User confirmation strings
            string yes = main.patcherLang == main.PatcherLanguage.en
                ? "Yes"
                : $"{main.localizedText?["yes"]}";
            string noStartOver = main.patcherLang == main.PatcherLanguage.en
                ? "No, start over"
                : $"{main.localizedText?["CustomSetup"]?["summaryScreen"]?["confirmation"]?["noStartOver"]}";
            string noGoBackToMainMenu = main.patcherLang == main.PatcherLanguage.en
                ? "No, go back to Main Menu"
                : $"{main.localizedText?["CustomSetup"]?["summaryScreen"]?["confirmation"]?["noGoBackToMainMenu"]}";

            AnsiConsole.MarkupLine($"\n[bold]{prompt}[/]\n");

            AnsiConsole.MarkupLine($"1. {yes}");
            AnsiConsole.MarkupLine($"2. {noStartOver}\n");

            AnsiConsole.MarkupLine($"3. {noGoBackToMainMenu}\n");

            var choice = menu.UserChoose("123");

            // Handle user confirmation choice
            switch (choice)
            {
                case 1: // Yes
                    if (main.platformType_custom != main.Platform.Dolphin)
                    {
                        sd.SDSetup(main.SetupType.custom);
                        break;
                    }
                    else
                    {
                        main.sdcard = null;
                        menu.WADFolderCheck(main.SetupType.custom);
                        break;
                    }
                case 2: // No, start over
                    main.combinedChannels_selection.Clear();
                    main.wiiLinkChannels_selection.Clear();
                    main.wiiConnect24Channels_selection.Clear();
                    CustomInstall_WiiLinkChannels_Setup();
                    break;
                case 3: // No, go back to main menu
                    main.combinedChannels_selection.Clear();
                    main.wiiLinkChannels_selection.Clear();
                    main.wiiConnect24Channels_selection.Clear();
                    menu.MainMenu();
                    break;
                default:
                    break;
            }
        }
    }

    // Patching progress function (Custom Install)
    public static void PatchingProgress_Custom()
    {
        main.task = "Patching channels...";
        int counter_done = 0;
        int partCompleted = 0;

        // List of channels to patch
        List<string> channelsToPatch = [.. main.wiiConnect24Channels_selection, .. main.wiiLinkChannels_selection, .. main.extraChannels_selection];


        // Set up patching progress dictionary
        main.patchingProgress_custom.Clear(); // Flush dictionary
        main.patchingProgress_custom.Add("downloading", "in_progress"); // Downloading patches
        foreach (string channel in channelsToPatch) // Patching channels
            main.patchingProgress_custom.Add(channel, "not_started");
        main.patchingProgress_custom.Add("finishing", "not_started"); // Finishing up

        // Give each WiiLink channel a proper name
        var channelMap = new Dictionary<string, string>()
        {
            { "wiiroom_en", "Wii Room [bold](English)[/]" },
            { "wiinoma_jp", "Wii no Ma [bold](Japanese)[/]" },
            { "wiiroom_es", "Wii Room [bold](Spanish)[/]" },
            { "wiiroom_fr", "Wii Room [bold](French)[/]" },
            { "wiiroom_de", "Wii Room [bold](German)[/]" },
            { "wiiroom_it", "Wii Room [bold](Italian)[/]" },
            { "wiiroom_du", "Wii Room [bold](Dutch)[/]" },
            { "wiiroom_ptbr", "Wii Room [bold](Portuguese)[/]" },
            { "wiiroom_ru", "Wii Room [bold](Russian)[/]" },
            { "digicam_en", "Photo Prints Channel [bold](English)[/]" },
            { "digicam_jp", "Digicam Print Channel [bold](Japanese)[/]" },
            { "food_us", "Food Channel [bold](Standard) (USA) [[English]][/]" },
            { "food_eu", "Food Channel [bold](Standard) (Europe) [[English]][/]" },
            { "demae_jp", "Demae Channel [bold](Standard) [[Japanese]][/]" },
            { "food_dominos", "Food Channel [bold](Dominos) [[English]][/]" },
            { "nc_us", "Nintendo Channel [bold](USA)[/]" },
            { "nc_eu", "Nintendo Channel [bold](Europe)[/]" },
            { "mnnc_jp", "Minna no Nintendo Channel [bold](Japan)[/]" },
            { "forecast_us", "Forecast Channel [bold](USA)[/]" },
            { "forecast_eu", "Forecast Channel [bold](Europe)[/]" },
            { "forecast_jp", "Forecast Channel [bold](Japan)[/]" },
            { "news_us", "News Channel [bold](USA)[/]" },
            { "news_eu", "News Channel [bold](Europe)[/]" },
            { "news_jp", "News Channel [bold](Japan)[/]"},
            { "evc_us", "Everybody Votes Channel [bold](USA)[/]" },
            { "evc_eu", "Everybody Votes Channel [bold](Europe)[/]" },
            { "evc_jp", "Everybody Votes Channel [bold](Japan)[/]" },
            { "cmoc_us", "Check Mii Out Channel [bold](USA)[/]" },
            { "cmoc_eu", "Mii Contest Channel [bold](Europe)[/]" },
            { "cmoc_jp", "Mii Contest Channel [bold](Japan)[/]" },
            { "kirbytv", "Kirby TV Channel" },
            { "ws_us", "Wii Speak Channel [bold](USA)[/]" },
            { "ws_eu", "Wii Speak Channel [bold](Europe)[/]" },
            { "ws_jp", "Wii Speak Channel [bold](Japan)[/]" },
            { "tatc_eu", "Today and Tomorrow Channel [bold](Europe)[/]" },
            { "tatc_jp", "Today and Tomorrow Channel [bold](Japan)[/]" },
            { "pc", "Photo Channel 1.1" },
            { "ic_us", "Internet Channel [bold](USA)[/]" },
            { "ic_eu", "Internet Channel [bold](Europe)[/]" },
            { "ic_jp", "Internet Channel [bold](Japan)[/]" },
            { "scr", "System Channel Restorer" }
        };

        // Setup patching process arrays based on the selected channels
        var channelPatchingFunctions = new Dictionary<string, Action>()
        {
            { "wiiroom_en", () => patch.WiiRoom_Patch(main.Language.English) },
            { "wiiroom_es", () => patch.WiiRoom_Patch(main.Language.Spanish) },
            { "wiinoma_jp", () => patch.WiiRoom_Patch(main.Language.Japan) },
            { "wiiroom_fr", () => patch.WiiRoom_Patch(main.Language.French) },
            { "wiiroom_de", () => patch.WiiRoom_Patch(main.Language.German) },
            { "wiiroom_it", () => patch.WiiRoom_Patch(main.Language.Italian) },
            { "wiiroom_du", () => patch.WiiRoom_Patch(main.Language.Dutch) },
            { "wiiroom_ptbr", () => patch.WiiRoom_Patch(main.Language.Portuguese) },
            { "wiiroom_ru", () => patch.WiiRoom_Patch(main.Language.Russian) },
            { "digicam_en", () => patch.Digicam_Patch(main.Language.English) },
            { "digicam_jp", () => patch.Digicam_Patch(main.Language.Japan) },
            { "food_us", () => patch.Demae_Patch(main.Language.English, main.DemaeVersion.Standard, main.Region.USA) },
            { "food_eu", () => patch.Demae_Patch(main.Language.English, main.DemaeVersion.Standard, main.Region.PAL) },
            { "demae_jp", () => patch.Demae_Patch(main.Language.Japan, main.DemaeVersion.Standard, main.Region.Japan) },
            { "food_dominos", () => patch.Demae_Patch(main.Language.English, main.DemaeVersion.Dominos, main.Region.USA) },
            { "kirbytv", patch.KirbyTV_Patch },
            { "nc_us", () => patch.NC_Patch(main.Region.USA) },
            { "nc_eu", () => patch.NC_Patch(main.Region.PAL) },
            { "mnnc_jp", () => patch.NC_Patch(main.Region.Japan) },
            { "forecast_us", () => patch.Forecast_Patch(main.Region.USA) },
            { "forecast_eu", () => patch.Forecast_Patch(main.Region.PAL) },
            { "forecast_jp", () => patch.Forecast_Patch(main.Region.Japan) },
            { "news_us", () => patch.News_Patch(main.Region.USA) },
            { "news_eu", () => patch.News_Patch(main.Region.PAL) },
            { "news_jp", () => patch.News_Patch(main.Region.Japan) },
            { "evc_us", () => patch.EVC_Patch(main.Region.USA) },
            { "evc_eu", () => patch.EVC_Patch(main.Region.PAL) },
            { "evc_jp", () => patch.EVC_Patch(main.Region.Japan) },
            { "cmoc_us", () => patch.CheckMiiOut_Patch(main.Region.USA) },
            { "cmoc_eu", () => patch.CheckMiiOut_Patch(main.Region.PAL) },
            { "cmoc_jp", () => patch.CheckMiiOut_Patch(main.Region.Japan) },
            { "ws_us", () => patch.WiiSpeak_Patch(main.Region.USA) },
            { "ws_eu", () => patch.WiiSpeak_Patch(main.Region.PAL) },
            { "ws_jp", () => patch.WiiSpeak_Patch(main.Region.Japan) },
            { "tatc_eu", () => patch.TodayTomorrow_Download(main.Region.PAL) },
            { "tatc_jp", () => patch.TodayTomorrow_Download(main.Region.Japan) },
            { "pc", () => patch.PhotoChannel_Download() },
            { "ic_us", () => patch.DownloadWC24Channel("ic", "Internet Channel", 1024, main.Region.USA, "0001000148414445") },
            { "ic_eu", () => patch.DownloadWC24Channel("ic", "Internet Channel", 1024, main.Region.PAL, "0001000148414450") },
            { "ic_jp", () => patch.DownloadWC24Channel("ic", "Internet Channel", 1024, main.Region.Japan, "000100014841444a") },
            { "scr", () => patch.DownloadOSCApp("system-channel-restorer") }
        };

        // Create a list of patching functions to execute
        var selectedPatchingFunctions = new List<Action>
        {
            // Add the patching functions to the list
            () => patch.DownloadCustomPatches(channelsToPatch)
        };

        foreach (string selectedChannel in channelsToPatch)
            selectedPatchingFunctions.Add(channelPatchingFunctions[selectedChannel]);

        selectedPatchingFunctions.Add(sd.FinishSDCopy);

        // Start patching
        int totalChannels = channelsToPatch.Count;
        while (main.patchingProgress_custom["finishing"] != "done")
        {
            menu.PrintHeader();

            // Progress text
            string patching = main.patcherLang == main.PatcherLanguage.en
                ? "Patching... this can take some time depending on the processing speed (CPU) of your computer."
                : $"{main.localizedText?["PatchingProgress"]?["patching"]}";
            string progress = main.patcherLang == main.PatcherLanguage.en
                ? "Progress"
                : $"{main.localizedText?["PatchingProgress"]?["progress"]}";
            AnsiConsole.MarkupLine($"[bold][[*]] {patching}[/]\n");
            AnsiConsole.Markup($"    {progress}: ");

            //// Progress bar and completion display
            // Calculate percentage based on how many channels are selected
            int percentage = (int)((float)partCompleted / (float)(totalChannels + 2) * 100);

            // Calculate progress bar
            counter_done = (int)((float)percentage / 10.0f);

            // Display progress bar
            StringBuilder progressBar = new("[[");
            for (int i = 0; i < counter_done; i++) // Add completed blocks
                progressBar.Append("[bold springgreen2_1]■[/]");
            for (int i = counter_done; i < 10; i++) // Add empty blocks
                progressBar.Append(" ");
            progressBar.Append("]]");

            AnsiConsole.Markup(progressBar.ToString());

            // Display percentage
            string percentComplete = main.patcherLang == main.PatcherLanguage.en
                ? "completed"
                : $"{main.localizedText?["PatchingProgress"]?["percentComplete"]}";
            string pleaseWait = main.patcherLang == main.PatcherLanguage.en
                ? "Please wait while the patching process is in progress..."
                : $"{main.localizedText?["PatchingProgress"]?["pleaseWait"]}";
            AnsiConsole.Markup($" [bold]{percentage}%[/] {percentComplete}\n\n");
            AnsiConsole.MarkupLine($"{pleaseWait}\n");

            //// Display progress for each channel ////

            // Pre-Patching Section: Downloading files
            string prePatching = main.patcherLang == main.PatcherLanguage.en
                ? "Pre-Patching"
                : $"{main.localizedText?["PatchingProgress"]?["prePatching"]}";
            string downloadingFiles = main.patcherLang == main.PatcherLanguage.en
                ? "Downloading files..."
                : $"{main.localizedText?["PatchingProgress"]?["downloadingFiles"]}";
            AnsiConsole.MarkupLine($"[bold]{prePatching}:[/]");
            switch (main.patchingProgress_custom["downloading"])
            {
                case "not_started":
                    AnsiConsole.MarkupLine($"○ {downloadingFiles}");
                    break;
                case "in_progress":
                    AnsiConsole.MarkupLine($"[slowblink yellow]●[/] {downloadingFiles}");
                    break;
                case "done":
                    AnsiConsole.MarkupLine($"[bold springgreen2_1]●[/] {downloadingFiles}");
                    break;
            }

            // Patching Section: Patching WiiConnect24 channels
            if (main.wiiConnect24Channels_selection.Count > 0)
            {
                //AnsiConsole.MarkupLine("\n[bold]Patching WiiConnect24 Channels:[/]");
                string patchingWC24Channels = main.patcherLang == main.PatcherLanguage.en
                    ? "Patching WiiConnect24 Channels"
                    : $"{main.localizedText?["PatchingProgress"]?["patchingWC24Channels"]}";
                AnsiConsole.MarkupLine($"\n[bold]{patchingWC24Channels}:[/]");
                foreach (string wiiConnect24Channel in channelsToPatch)
                {
                    List<string> wiiConnect24Channels = ["nc_us", "nc_eu", "mnnc_jp", "forecast_us", "forecast_eu", "forecast_jp", "news_us", "news_eu", "news_jp", "evc_us", "evc_eu", "evc_jp", "cmoc_us", "cmoc_eu", "cmoc_jp"];
                    if (wiiConnect24Channels.Contains(wiiConnect24Channel))
                    {
                        switch (main.patchingProgress_custom[wiiConnect24Channel])
                        {
                            case "not_started":
                                AnsiConsole.MarkupLine($"○ {channelMap[wiiConnect24Channel]}");
                                break;
                            case "in_progress":
                                AnsiConsole.MarkupLine($"[slowblink yellow]●[/] {channelMap[wiiConnect24Channel]}");
                                break;
                            case "done":
                                AnsiConsole.MarkupLine($"[bold springgreen2_1]●[/] {channelMap[wiiConnect24Channel]}");
                                break;
                        }
                    }
                }
            }

            // Patching Section: Patching Regional Channels
            if (main.wiiLinkChannels_selection.Count > 0)
            {
                string patchingWiiLinkChannels = main.patcherLang == main.PatcherLanguage.en
                    ? "Patching Regional Channels"
                    : $"{main.localizedText?["PatchingProgress"]?["patchingWiiLinkChannels"]}";
                AnsiConsole.MarkupLine($"\n[bold]{patchingWiiLinkChannels}:[/]");
                foreach (string jpnChannel in channelsToPatch)
                {
                    List<string> jpnChannels = ["wiiroom_en", "wiiroom_es", "wiiroom_fr", "wiinoma_jp", "wiiroom_de", "wiiroom_it", "wiiroom_du", "wiiroom_ptbr", "wiiroom_ru", "digicam_en", "digicam_jp", "food_us", "food_eu", "demae_jp", "food_dominos", "kirbytv"];
                    if (jpnChannels.Contains(jpnChannel))
                    {
                        switch (main.patchingProgress_custom[jpnChannel])
                        {
                            case "not_started":
                                AnsiConsole.MarkupLine($"○ {channelMap[jpnChannel]}");
                                break;
                            case "in_progress":
                                AnsiConsole.MarkupLine($"[slowblink yellow]●[/] {channelMap[jpnChannel]}");
                                break;
                            case "done":
                                AnsiConsole.MarkupLine($"[bold springgreen2_1]●[/] {channelMap[jpnChannel]}");
                                break;
                        }
                    }
                }
            }

            else if (main.extraChannels_selection.Count > 0)
            {
                string patchingExtraChannels = main.patcherLang == main.PatcherLanguage.en
                    ? "Downloading Extra Channels"
                    : $"{main.localizedText?["PatchingProgress"]?["patchingExtraChannels"]}";
                AnsiConsole.MarkupLine($"\n[bold]{patchingExtraChannels}:[/]");
                foreach (string extraChannel in channelsToPatch)
                {
                    List<string> extraChannels = ["ws_us", "ws_eu", "ws_jp", "tatc_eu", "tatc_jp", "pc", "ic_us", "ic_eu", "ic_jp"];
                    if (extraChannels.Contains(extraChannel))
                    {
                        switch (main.patchingProgress_custom[extraChannel])
                        {
                            case "not_started":
                                AnsiConsole.MarkupLine($"○ {channelMap[extraChannel]}");
                                break;
                            case "in_progress":
                                AnsiConsole.MarkupLine($"[slowblink yellow]●[/] {channelMap[extraChannel]}");
                                break;
                            case "done":
                                AnsiConsole.MarkupLine($"[bold springgreen2_1]●[/] {channelMap[extraChannel]}");
                                break;
                        }
                    }
                }
            }

            // Post-Patching Section: Finishing up
            string postPatching = main.patcherLang == main.PatcherLanguage.en
                ? "Post-Patching"
                : $"{main.localizedText?["PatchingProgress"]?["postPatching"]}";
            string finishingUp = main.patcherLang == main.PatcherLanguage.en
                ? "Finishing up..."
                : $"{main.localizedText?["PatchingProgress"]?["finishingUp"]}";
            AnsiConsole.MarkupLine($"\n[bold]{postPatching}:[/]");
            switch (main.patchingProgress_custom["finishing"])
            {
                case "not_started":
                    AnsiConsole.MarkupLine($"○ {finishingUp}");
                    break;
                case "in_progress":
                    AnsiConsole.MarkupLine($"[slowblink yellow]●[/] {finishingUp}");
                    break;
                case "done":
                    AnsiConsole.MarkupLine($"[bold springgreen2_1]●[/] {finishingUp}");
                    break;
            }

            // Execute the next patching function
            selectedPatchingFunctions[partCompleted]();

            // Increment the percentage
            main.task = "Moving to next patch";
            partCompleted++;

            switch (partCompleted)
            {
                case 1:
                    // If we're on the first channel, mark downloading as done and the first channel as in progress
                    main.patchingProgress_custom["downloading"] = "done";
                    main.patchingProgress_custom[channelsToPatch[0]] = "in_progress";
                    break;
                case int n when n > 1 && n < totalChannels + 1:
                    // If we're on a channel that's not the first or last, mark the previous channel as done and the current channel as in progress
                    main.patchingProgress_custom[channelsToPatch[partCompleted - 2]] = "done";
                    main.patchingProgress_custom[channelsToPatch[partCompleted - 1]] = "in_progress";
                    break;
                case int n when n == totalChannels + 1:
                    // If we're on the last channel, mark the previous channel as done and finishing as in progress
                    main.patchingProgress_custom[channelsToPatch[partCompleted - 2]] = "done";
                    main.patchingProgress_custom["finishing"] = "in_progress";
                    break;
                case int n when n == totalChannels + 2:
                    // If we're done patching, mark finishing as done
                    main.patchingProgress_custom["finishing"] = "done";
                    break;
            }
        }

        // We're finally done patching!
        Thread.Sleep(2000);
        menu.Finished();
    }
}