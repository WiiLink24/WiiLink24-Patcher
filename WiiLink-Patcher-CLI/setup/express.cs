using System.Text;
using Spectre.Console;

public class express
{
    // Configure region for all WiiConnect24 services (Express Install)
    public static void WC24Setup()
    {
        while (true)
        {
            menu.PrintHeader();

            // Express Install Header Text
            string EIHeader = main.patcherLang == main.PatcherLanguage.en
                ? "Express Install"
                : $"{main.localizedText?["ExpressInstall"]?["Header"]}";
            AnsiConsole.MarkupLine($"[bold springgreen2_1]{EIHeader}[/]\n");

            // Welcome the user to the Express Install of WiiLink
            string welcome = main.patcherLang == main.PatcherLanguage.en
                ? "[bold]Welcome to the Express Install of [springgreen2_1]WiiLink[/]![/]"
                : $"{main.localizedText?["ExpressInstall"]?["WC24Setup"]?["welcome"]}";
            AnsiConsole.MarkupLine($"{welcome}\n");

            // Step 1 Text
            string stepNum = main.patcherLang == main.PatcherLanguage.en
                ? "Step 1"
                : $"{main.localizedText?["ExpressInstall"]?["WC24Setup"]?["stepNum"]}";
            string stepTitle = main.patcherLang == main.PatcherLanguage.en
                ? "Choose region for WiiConnect24 services"
                : $"{main.localizedText?["ExpressInstall"]?["WC24Setup"]?["stepTitle"]}";

            AnsiConsole.MarkupLine($"[bold]{stepNum}: {stepTitle}[/]\n");

            // Instructions Text
            string instructions = main.patcherLang == main.PatcherLanguage.en
                ? "For the WiiConnect24 services, which region would you like to install?"
                : $"{main.localizedText?["ExpressInstall"]?["WC24Setup"]?["instructions"]}";
            AnsiConsole.MarkupLine($"{instructions}\n");

            // User Choices
            string northAmerica = main.patcherLang == main.PatcherLanguage.en
                ? "North America (NTSC-U)"
                : $"{main.localizedText?["ExpressInstall"]?["WC24Setup"]?["northAmerica"]}";
            string pal = main.patcherLang == main.PatcherLanguage.en
                ? "Europe (PAL)"
                : $"{main.localizedText?["ExpressInstall"]?["WC24Setup"]?["pal"]}";
            string japan = main.patcherLang == main.PatcherLanguage.en
                ? "Japan (NTSC-J)"
                : $"{main.localizedText?["ExpressInstall"]?["WC24Setup"]?["japan"]}";
            string goBackToMainMenu = main.patcherLang == main.PatcherLanguage.en
                ? "Go Back to Main Menu"
                : $"{main.localizedText?["goBackToMainMenu"]}";

            AnsiConsole.MarkupLine($"1. {northAmerica}");
            AnsiConsole.MarkupLine($"2. {pal}");
            AnsiConsole.MarkupLine($"3. {japan}\n");

            AnsiConsole.MarkupLine($"4. {goBackToMainMenu}\n");

            int choice = menu.UserChoose("1234");
            switch (choice)
            {
                case 1: // USA
                    main.wc24_reg = main.Region.USA;
                    WiiLinkRegionalChannelsSetup();
                    break;
                case 2: // PAL
                    main.wc24_reg = main.Region.PAL;
                    WiiLinkRegionalChannelsSetup();
                    break;
                case 3: // Japan
                    main.wc24_reg = main.Region.Japan;
                    WiiLinkRegionalChannelsSetup();
                    break;
                case 4: // Go back to main menu
                    menu.MainMenu();
                    break;
                default:
                    break;
            }
        }
    }

    // Ask user if they want to install WiiConnect24 services (Express Install)
    static void WiiLinkRegionalChannelsSetup()
    {
        while (true)
        {
            menu.PrintHeader();

            // Express Install Header Text
            string EIHeader = main.patcherLang == main.PatcherLanguage.en
                ? "Express Install"
                : $"{main.localizedText?["ExpressInstall"]?["Header"]}";
            AnsiConsole.MarkupLine($"[bold springgreen2_1]{EIHeader}[/]\n");

            // Would you like to install WiiLink's regional channel services Text
            string wouldYouLike = main.patcherLang == main.PatcherLanguage.en
                ? "Would you like to install [bold][springgreen2_1]WiiLink[/]'s regional channel services[/]?"
                : $"{main.localizedText?["ExpressInstall"]?["WiiLinkSetup"]?["wouldYouLike"]}";
            AnsiConsole.MarkupLine($"{wouldYouLike}\n");

            // Services that would be installed Text
            string toBeInstalled = main.patcherLang == main.PatcherLanguage.en
                ? "Services that would be installed:"
                : $"{main.localizedText?["ExpressInstall"]?["WiiLinkSetup"]?["toBeInstalled"]}";
            AnsiConsole.MarkupLine($"{toBeInstalled}\n");

            // Channel Names
            string wiiRoom = main.patcherLang == main.PatcherLanguage.en
                ? "Wii Room [bold](Wii no Ma)[/]"
                : $"{main.localizedText?["ExpressInstall"]?["WiiLinkSetup"]?["WiiRoom"]}";
            string photoPrints = main.patcherLang == main.PatcherLanguage.en
                ? "Photo Prints Channel [bold](Digicam Print Channel)[/]"
                : $"{main.localizedText?["ExpressInstall"]?["WiiLinkSetup"]?["PhotoPrints"]}";
            string foodChannel = main.patcherLang == main.PatcherLanguage.en
                ? "Food Channel [bold](Demae Channel)[/]"
                : $"{main.localizedText?["ExpressInstall"]?["WiiLinkSetup"]?["FoodChannel"]}";
            string kirbyTV = main.patcherLang == main.PatcherLanguage.en
                ? "Kirby TV Channel"
                : $"{main.localizedText?["ExpressInstall"]?["WiiLinkSetup"]?["KirbyTV"]}";

            AnsiConsole.MarkupLine($"  ● {wiiRoom}");
            AnsiConsole.MarkupLine($"  ● {photoPrints}");
            AnsiConsole.MarkupLine($"  ● {foodChannel}");
            AnsiConsole.MarkupLine($"  ● {kirbyTV}\n");

            // Yes or No Text
            string yes = main.patcherLang == main.PatcherLanguage.en
                ? "Yes"
                : $"{main.localizedText?["yes"]}";
            string no = main.patcherLang == main.PatcherLanguage.en
                ? "No"
                : $"{main.localizedText?["no"]}";

            Console.WriteLine($"1. {yes}");
            Console.WriteLine($"2. {no}\n");

            // Go Back to Main Menu Text
            string goBackToMainMenu = main.patcherLang == main.PatcherLanguage.en
                ? "Go Back to Main Menu"
                : $"{main.localizedText?["goBackToMainMenu"]}";
            Console.WriteLine($"3. {goBackToMainMenu}\n");

            int choice = menu.UserChoose("123");
            switch (choice)
            {
                case 1:
                    main.installRegionalChannels = true;
                    WiiLinkChannels_LangSetup();
                    break;
                case 2:
                    main.installRegionalChannels = false;
                    ChoosePlatform();
                    break;
                case 3: // Go back to main menu
                    menu.MainMenu();
                    break;
                default:
                    break;
            }
        }
    }

    // Install Choose (Express Install)
    static void WiiLinkChannels_LangSetup()
    {
        while (true)
        {
            menu.PrintHeader();

            // Express Install Header Text
            string EIHeader = main.patcherLang == main.PatcherLanguage.en
                ? "Express Install"
                : $"{main.localizedText?["ExpressInstall"]?["Header"]}";
            AnsiConsole.MarkupLine($"[bold springgreen2_1]{EIHeader}[/]\n");

            // Step 2 Text
            string step2Message = main.patcherLang == main.PatcherLanguage.en
                ? "Step 2: Choose WiiLink's regional channels language"
                : $"{main.localizedText?["ExpressInstall"]?["WiiLinkChannels_LangSetup"]?["step2Message"]}";
            AnsiConsole.MarkupLine($"[bold]{step2Message}[/]\n");

            // Instructions Text
            string instructions = main.patcherLang == main.PatcherLanguage.en
                ? "For [bold]Wii Room[/], [bold]Photo Prints Channel[/], and [bold]Food Channel[/], which language would you like to select?"
                : $"{main.localizedText?["ExpressInstall"]?["WiiLinkChannels_LangSetup"]?["instructions"]}";
            AnsiConsole.MarkupLine($"{instructions}\n");

            // User Choices
            string translated = main.patcherLang == main.PatcherLanguage.en
                ? "Translated (eg. English, French, etc.)"
                : $"{main.localizedText?["ExpressInstall"]?["WiiLinkChannels_LangSetup"]?["translatedOption"]}";
            string japanese = main.patcherLang == main.PatcherLanguage.en
                ? "Japanese"
                : $"{main.localizedText?["ExpressInstall"]?["WiiLinkChannels_LangSetup"]?["japaneseOption"]}";
            string goBackToMainMenu = main.patcherLang == main.PatcherLanguage.en
                ? "Go Back to Main Menu"
                : $"{main.localizedText?["goBackToMainMenu"]}";

            AnsiConsole.MarkupLine($"1. {translated}");
            AnsiConsole.MarkupLine($"2. {japanese}\n");

            AnsiConsole.MarkupLine($"3. {goBackToMainMenu}\n");

            int choice = menu.UserChoose("123");
            switch (choice)
            {
                case 1:
                    main.lang = main.Language.English;
                    WiiRoomConfiguration();
                    break;
                case 2:
                    main.lang = main.Language.Japan;
                    main.wiiRoomLang = main.Language.Japan;
                    main.demaeVersion = main.DemaeVersion.Standard;
                    ChoosePlatform();
                    break;
                case 3:
                    menu.MainMenu(); // Go back to main menu
                    return;
                default:
                    break;
            }
        }
    }

    // Configure Wii Room Channel (choosing language) [Express Install]
    static void WiiRoomConfiguration()
    {
        while (true)
        {
            menu.PrintHeader();

            // Express Install Header Text
            string EIHeader = main.patcherLang == main.PatcherLanguage.en
                ? "Express Install"
                : $"{main.localizedText?["ExpressInstall"]?["Header"]}";
            AnsiConsole.MarkupLine($"[bold springgreen2_1]{EIHeader}[/]\n");

            // Step 2A Text
            string stepNumber = main.patcherLang == main.PatcherLanguage.en
                ? "Step 2A"
                : $"{main.localizedText?["ExpressInstall"]?["WiiRoomConfiguration"]?["stepNum"]}";
            string step1aTitle = main.patcherLang == main.PatcherLanguage.en
                ? "Choose Wii Room language"
                : $"{main.localizedText?["ExpressInstall"]?["WiiRoomConfiguration"]?["stepTitle"]}";
            AnsiConsole.MarkupLine($"[bold]{stepNumber}: {step1aTitle}[/]\n");

            // Instructions Text
            string instructions = main.patcherLang == main.PatcherLanguage.en
                ? "For [bold]Wii Room[/], which language would you like to select?"
                : $"{main.localizedText?["ExpressInstall"]?["WiiRoomConfiguration"]?["instructions"]}";
            AnsiConsole.MarkupLine($"{instructions}\n");

            string goBackToMainMenu = main.patcherLang == main.PatcherLanguage.en
                ? "Go Back to Main Menu"
                : $"{main.localizedText?["goBackToMainMenu"]}";

            AnsiConsole.MarkupLine($"1. English");
            AnsiConsole.MarkupLine($"2. Español");
            AnsiConsole.MarkupLine($"3. Français");
            AnsiConsole.MarkupLine($"4. Deutsch");
            AnsiConsole.MarkupLine($"5. Italiano");
            AnsiConsole.MarkupLine($"6. Nederlands");
            AnsiConsole.MarkupLine($"7. Português (Brasil)");
            AnsiConsole.MarkupLine($"8. Русский\n");

            AnsiConsole.MarkupLine($"9. {goBackToMainMenu}\n");

            int choice = menu.UserChoose("123456789");
            switch (choice)
            {
                case 1:
                    main.wiiRoomLang = main.Language.English;
                    DemaeConfiguration();
                    break;
                case 2:
                    main.wiiRoomLang = main.Language.Spanish;
                    DemaeConfiguration();
                    break;
                case 3:
                    main.wiiRoomLang = main.Language.French;
                    DemaeConfiguration();
                    break;
                case 4:
                    main.wiiRoomLang = main.Language.German;
                    DemaeConfiguration();
                    break;
                case 5:
                    main.wiiRoomLang = main.Language.Italian;
                    DemaeConfiguration();
                    break;
                case 6:
                    main.wiiRoomLang = main.Language.Dutch;
                    DemaeConfiguration();
                    break;
                case 7:
                    main.wiiRoomLang = main.Language.Portuguese;
                    DemaeConfiguration();
                    break;
                case 8:
                    main.wiiRoomLang = main.Language.Russian;
                    RussianNoticeForWiiRoom();
                    break;
                case 9: // Go back to main menu
                    menu.MainMenu();
                    break;
                default:
                    break;
            }
        }
    }

    // If Russian is chosen, show a special message for additional instructions
    static void RussianNoticeForWiiRoom()
    {
        menu.PrintHeader();

        AnsiConsole.MarkupLine("[bold yellow]NOTICE FOR RUSSIAN USERS[/]\n");
        AnsiConsole.MarkupLine("You have selected Russian translation in the installation options.\n");
        AnsiConsole.MarkupLine("Proper functionality is not guaranteed for systems without the Russian Wii menu.\n");
        AnsiConsole.MarkupLine("Read the installation guide here:");
        AnsiConsole.MarkupLine("[bold link springgreen2_1]https://wii.zazios.ru/rus_menu[/]\n");
        AnsiConsole.MarkupLine("[italic](The guide is only available in Russian for now)[/]\n");

        AnsiConsole.MarkupLine("[bold]Press any key to continue...[/]");
        Console.ReadKey();

        DemaeConfiguration();
    }

    // Configure Demae Channel (if English was selected) [Express Install]
    static void DemaeConfiguration()
    {
        while (true)
        {
            menu.PrintHeader();

            // Express Install Header Text
            string EIHeader = main.patcherLang == main.PatcherLanguage.en
                ? "Express Install"
                : $"{main.localizedText?["ExpressInstall"]?["Header"]}";
            AnsiConsole.MarkupLine($"[bold springgreen2_1]{EIHeader}[/]\n");

            // Step 2B Text
            string stepNumber = main.patcherLang == main.PatcherLanguage.en
                ? "Step 2B"
                : $"{main.localizedText?["ExpressInstall"]?["DemaeConfiguration"]?["stepNum"]}";
            string step1bTitle = main.patcherLang == main.PatcherLanguage.en
                ? "Choose Food Channel version"
                : $"{main.localizedText?["ExpressInstall"]?["DemaeConfiguration"]?["stepTitle"]}";
            AnsiConsole.MarkupLine($"[bold]{stepNumber}: {step1bTitle}[/]\n");

            // Instructions Text
            string instructions = main.patcherLang == main.PatcherLanguage.en
                ? "For [bold]Food Channel[/], which version would you like to install?"
                : $"{main.localizedText?["ExpressInstall"]?["DemaeConfiguration"]?["instructions"]}";
            AnsiConsole.MarkupLine($"{instructions}\n");

            // User Choices
            string demaeStandard = main.patcherLang == main.PatcherLanguage.en
                ? "Standard [bold](Fake Ordering)[/]"
                : $"{main.localizedText?["ExpressInstall"]?["DemaeConfiguration"]?["demaeStandard"]}";
            string demaeDominos = main.patcherLang == main.PatcherLanguage.en
                ? "Domino's [bold](US and Canada only)[/]"
                : $"{main.localizedText?["ExpressInstall"]?["DemaeConfiguration"]?["demaeDominos"]}";
            string goBackToMainMenu = main.patcherLang == main.PatcherLanguage.en
                ? "Go Back to Main Menu"
                : $"{main.localizedText?["goBackToMainMenu"]}";

            AnsiConsole.MarkupLine($"1. {demaeStandard}");
            AnsiConsole.MarkupLine($"2. {demaeDominos}\n");

            AnsiConsole.MarkupLine($"3. {goBackToMainMenu}\n");

            int choice = menu.UserChoose("123");
            switch (choice)
            {
                case 1:
                    main.demaeVersion = main.DemaeVersion.Standard;
                    ChoosePlatform();
                    break;
                case 2:
                    main.demaeVersion = main.DemaeVersion.Dominos;
                    ChoosePlatform();
                    break;
                case 3: // Go back to main menu
                    menu.MainMenu();
                    break;
                default:
                    break;
            }
        }
    }

    // Choose console platformType (Wii [Dolphin Emulator] or vWii [Wii U]) [Express Install]
    static void ChoosePlatform()
    {
        while (true)
        {
            menu.PrintHeader();

            // Express Install Header Text
            string EIHeader = main.patcherLang == main.PatcherLanguage.en
                ? "Express Install"
                : $"{main.localizedText?["ExpressInstall"]?["Header"]}";
            AnsiConsole.MarkupLine($"[bold springgreen2_1]{EIHeader}[/]\n");

            // Change step number depending on if WiiConnect24 is being installed or not
            string stepNum = main.patcherLang == main.PatcherLanguage.en
                ? !main.installRegionalChannels ? "Step 2" : "Step 3"
                : $"{main.localizedText?["ExpressInstall"]?["ChoosePlatform"]?[!main.installRegionalChannels ? "ifNoWC24" : "ifWC24"]?["stepNum"]}";
            string stepTitle = main.patcherLang == main.PatcherLanguage.en
                ? "Choose console platform"
                : $"{main.localizedText?["ExpressInstall"]?["ChoosePlatform"]?["stepTitle"]}";

            AnsiConsole.MarkupLine($"[bold]{stepNum}: {stepTitle}[/]\n");

            // Instructions Text
            string instructions = main.patcherLang == main.PatcherLanguage.en
                ? "Which Wii version are you installing to?"
                : $"{main.localizedText?["ExpressInstall"]?["ChoosePlatform"]?["instructions"]}";
            AnsiConsole.MarkupLine($"{instructions}\n");

            // User Choices
            string wii = main.patcherLang == main.PatcherLanguage.en
                ? "Wii"
                : $"{main.localizedText?["ExpressInstall"]?["ChoosePlatform"]?["wii"]}";
            string vWii = main.patcherLang == main.PatcherLanguage.en
                ? "vWii [bold](Wii U)[/]"
                : $"{main.localizedText?["ExpressInstall"]?["ChoosePlatform"]?["vWii"]}";
            string Dolphin = main.patcherLang == main.PatcherLanguage.en
                ? "Dolphin Emulator"
                : $"{main.localizedText?["ExpressInstall"]?["ChoosePlatform"]?["dolphin"]}";
            string goBackToMainMenu = main.patcherLang == main.PatcherLanguage.en
                ? "Go Back to Main Menu"
                : $"{main.localizedText?["goBackToMainMenu"]}";

            AnsiConsole.MarkupLine($"1. {wii}");
            AnsiConsole.MarkupLine($"2. {vWii}");
            AnsiConsole.MarkupLine($"3. {Dolphin}\n");

            AnsiConsole.MarkupLine($"4. {goBackToMainMenu}\n");

            int choice = menu.UserChoose("1234");
            switch (choice)
            {
                case 1:
                    main.platformType = main.Platform.Wii;
                    sd.SDSetup(main.SetupType.express);
                    break;
                case 2:
                    main.platformType = main.Platform.vWii;
                    sd.SDSetup(main.SetupType.express);
                    break;
                case 3:
                    main.platformType = main.Platform.Dolphin;
                    main.sdcard = null;
                    menu.WADFolderCheck(main.SetupType.express);
                    break;
                case 4: // Go back to main menu
                    menu.MainMenu();
                    break;
                default:
                    break;
            }
        }
    }

    // Patching progress function (Express Install)
    public static void PatchingProgress_Express()
    {
        int counter_done = 0;
        int percent = 0;

        // Make sure the temp folder exists
        if (!Directory.Exists(main.tempDir))
            Directory.CreateDirectory(main.tempDir);

        // Demae version text to be used in the patching progress for Demae Channel (eg. "Food Channel (English) [Standard]")
        string demaeVerTxt = main.demaeVersion == main.DemaeVersion.Standard
            ? "Standard"
            : "Domino's";

        // Define WiiLink channels titles
        string demae_title = main.lang == main.Language.English
                ? $"Food Channel [bold](English)[/] [bold][[{demaeVerTxt}]][/]"
                : $"Demae Channel [bold](Japanese)[/] [bold][[{demaeVerTxt}]][/]";

        string wiiroom_title = main.wiiRoomLang != main.Language.Japan
                ? $"Wii Room [bold]({main.wiiRoomLang})[/]"
                : "Wii no Ma [bold](Japanese)[/]";

        string digicam_title = main.lang == main.Language.English
                ? "Photo Prints Channel [bold](English)[/]"
                : "Digicam Print Channel [bold](Japanese)[/]";

        string kirbytv_title = "Kirby TV Channel"; // Kirby TV Channel

        // Define the channelMessages dictionary with WC24 channel titles
        string internationalOrJapanese = (main.wc24_reg == main.Region.USA || main.wc24_reg == main.Region.PAL) ? "International" : "Japanese";
        string NCTitle, forecastTitle, newsTitle, evcTitle, cmocTitle;

        if (main.patcherLang == main.PatcherLanguage.en)
        {
            NCTitle = $"{(main.wc24_reg == main.Region.USA || main.wc24_reg == main.Region.PAL ? "Nintendo Channel" : "Minna no Nintendo Channel")} [bold]({main.wc24_reg})[/]";
            forecastTitle = $"Forecast Channel [bold]({main.wc24_reg})[/]";
            newsTitle = $"News Channel [bold]({main.wc24_reg})[/]";
            evcTitle = $"Everybody Votes Channel [bold]({main.wc24_reg})[/]";
            cmocTitle = $"{(main.wc24_reg == main.Region.USA ? "Check Mii Out Channel" : "Mii Contest Channel")} [bold]({main.wc24_reg})[/]";
        }
        else
        {
            NCTitle = $"{main.localizedText?["ChannelNames"]?[internationalOrJapanese]?["nintendoChn"]} [bold]({main.wc24_reg})[/]";
            forecastTitle = $"{main.localizedText?["ChannelNames"]?["International"]?["forecastChn"]} [bold]({main.wc24_reg})[/]";
            newsTitle = $"{main.localizedText?["ChannelNames"]?["International"]?["newsChn"]} [bold]({main.wc24_reg})[/]";
            evcTitle = $"{main.localizedText?["ChannelNames"]?["International"]?["everybodyVotes"]} [bold]({main.wc24_reg})[/]";
            cmocTitle = $"{main.localizedText?["ChannelNames"]?["International"]?["cmoc"]} [bold]({main.wc24_reg})[/]";
        }

        var channelMessages = new Dictionary<string, string>
        {
            { "nc", NCTitle },
            { "forecast", forecastTitle },
            { "news", newsTitle },
            { "evc", evcTitle },
            { "cmoc", cmocTitle }
        };

        // Add other channel titles to the channelMessages dictionary (if applicable)
        if (main.installRegionalChannels)
        {
            channelMessages.Add("wiiroom", wiiroom_title);
            channelMessages.Add("digicam", digicam_title);
            channelMessages.Add("demae", demae_title);
            channelMessages.Add("kirbytv", kirbytv_title);
        }

        // Setup patching process list
        var patching_functions = new List<Action>
        {
            patch.DownloadAllPatches,
            () => patch.NC_Patch(main.wc24_reg),
            () => patch.Forecast_Patch(main.wc24_reg),
            () => patch.News_Patch(main.wc24_reg),
            () => patch.EVC_Patch(main.wc24_reg),
            () => patch.CheckMiiOut_Patch(main.wc24_reg)
        };

        // Add other patching functions if applicable
        if (main.installRegionalChannels)
        {
            patching_functions.Add(() => patch.WiiRoom_Patch(main.wiiRoomLang));
            patching_functions.Add(() => patch.Digicam_Patch(main.lang));
            patching_functions.Add(() => patch.Demae_Patch(main.lang, main.demaeVersion, main.wc24_reg));
            patching_functions.Add(patch.KirbyTV_Patch);
        }

        patching_functions.Add(sd.FinishSDCopy);

        //// Set up patching progress dictionary ////
        // Flush dictionary and downloading patches
        main.patchingProgress_express.Clear();
        main.patchingProgress_express.Add("downloading", "in_progress");

        // Patching WiiConnect24 channels
        foreach (string channel in new string[] { "nc", "forecast", "news", "evc", "cmoc" })
            main.patchingProgress_express.Add(channel, "not_started");

        if (main.installRegionalChannels)
        {
            // Patching Regional Channels
            foreach (string channel in new string[] { "wiiroom", "digicam", "demae", "kirbytv" })
                main.patchingProgress_express.Add(channel, "not_started");
        }

        // Finishing up
        main.patchingProgress_express.Add("finishing", "not_started");

        // While the patching process is not finished
        while (main.patchingProgress_express["finishing"] != "done")
        {
            menu.PrintHeader();

            // Progress bar and completion display
            string patching = main.patcherLang == main.PatcherLanguage.en
                ? "Patching... this can take some time depending on the processing speed (CPU) of your computer."
                : $"{main.localizedText?["PatchingProgress"]?["patching"]}";
            string progress = main.patcherLang == main.PatcherLanguage.en
                ? "Progress"
                : $"{main.localizedText?["PatchingProgress"]?["progress"]}";
            AnsiConsole.MarkupLine($"[bold][[*]] {patching}[/]\n");
            AnsiConsole.Markup($"    {progress}: ");

            //// Progress bar and completion display ////
            // Calculate percentage based on how many channels are completed
            int percentage = (int)((float)main.patchingProgress_express.Where(x => x.Value == "done").Count() / (float)main.patchingProgress_express.Count * 100.0f);

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
            switch (main.patchingProgress_express["downloading"])
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

            // Patching Section: Patching WiiConnect24 channels (if applicable)
            string patchingWiiConnect24Channels = main.patcherLang == main.PatcherLanguage.en
                ? "Patching WiiConnect24 Channels"
                : $"{main.localizedText?["PatchingProgress"]?["patchingWiiConnect24Channels"]}";

            AnsiConsole.MarkupLine($"\n[bold]{patchingWiiConnect24Channels}:[/]");
            foreach (string channel in new string[] { "nc", "forecast", "news", "evc", "cmoc" })
            {
                switch (main.patchingProgress_express[channel])
                {
                    case "not_started":
                        AnsiConsole.MarkupLine($"○ {channelMessages[channel]}");
                        break;
                    case "in_progress":
                        AnsiConsole.MarkupLine($"[slowblink yellow]●[/] {channelMessages[channel]}");
                        break;
                    case "done":
                        AnsiConsole.MarkupLine($"[bold springgreen2_1]●[/] {channelMessages[channel]}");
                        break;
                }
            }

            // Patching Section: Patching Regional Channels
            if (main.installRegionalChannels)
            {
                string patchingWiiLinkChannels = main.patcherLang == main.PatcherLanguage.en
                    ? "Patching Regional Channels"
                    : $"{main.localizedText?["PatchingProgress"]?["patchingWiiLinkChannels"]}";
                AnsiConsole.MarkupLine($"\n[bold]{patchingWiiLinkChannels}:[/]");
                foreach (string channel in new string[] { "wiiroom", "digicam", "demae", "kirbytv" })
                {
                    switch (main.patchingProgress_express[channel])
                    {
                        case "not_started":
                            AnsiConsole.MarkupLine($"○ {channelMessages[channel]}");
                            break;
                        case "in_progress":
                            AnsiConsole.MarkupLine($"[slowblink yellow]●[/] {channelMessages[channel]}");
                            break;
                        case "done":
                            AnsiConsole.MarkupLine($"[bold springgreen2_1]●[/] {channelMessages[channel]}");
                            break;
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
            switch (main.patchingProgress_express["finishing"])
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

            patching_functions[percent]();

            // Increment percent
            percent++;
        }

        // After all the channels are patched, we're done!
        Thread.Sleep(2000);
        menu.Finished();
    }
}