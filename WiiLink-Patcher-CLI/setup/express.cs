using System.Text;
using Spectre.Console;

public class ExpressClass
{
    // Configure region for all WiiConnect24 services (Express Install)
    public static void WC24Setup()
    {
        while (true)
        {
            MenuClass.PrintHeader();

            // Express Install Header Text
            string EIHeader = MainClass.patcherLang == "en-US"
                ? "Express Install"
                : $"{MainClass.localizedText?["ExpressInstall"]?["Header"]}";
            AnsiConsole.MarkupLine($"[bold springgreen2_1]{EIHeader}[/]\n");

            // Welcome the user to the Express Install of WiiLink
            string welcome = MainClass.patcherLang == "en-US"
                ? "[bold]Welcome to the Express Install of [springgreen2_1]WiiLink[/]![/]"
                : $"{MainClass.localizedText?["ExpressInstall"]?["WC24Setup"]?["welcome"]}";
            AnsiConsole.MarkupLine($"{welcome}\n");

            // Step 1 Text
            string stepNum = MainClass.patcherLang == "en-US"
                ? "Step 1"
                : $"{MainClass.localizedText?["ExpressInstall"]?["WC24Setup"]?["stepNum"]}";
            string stepTitle = MainClass.patcherLang == "en-US"
                ? "Choose region for WiiConnect24 services"
                : $"{MainClass.localizedText?["ExpressInstall"]?["WC24Setup"]?["stepTitle"]}";

            AnsiConsole.MarkupLine($"[bold]{stepNum}: {stepTitle}[/]\n");

            // Instructions Text
            string instructions = MainClass.patcherLang == "en-US"
                ? "For the WiiConnect24 services, which region would you like to install?"
                : $"{MainClass.localizedText?["ExpressInstall"]?["WC24Setup"]?["instructions"]}";
            AnsiConsole.MarkupLine($"{instructions}\n");

            // User Choices
            string northAmerica = MainClass.patcherLang == "en-US"
                ? "North America (NTSC-U)"
                : $"{MainClass.localizedText?["ExpressInstall"]?["WC24Setup"]?["northAmerica"]}";
            string pal = MainClass.patcherLang == "en-US"
                ? "Europe (PAL)"
                : $"{MainClass.localizedText?["ExpressInstall"]?["WC24Setup"]?["pal"]}";
            string japan = MainClass.patcherLang == "en-US"
                ? "Japan (NTSC-J)"
                : $"{MainClass.localizedText?["ExpressInstall"]?["WC24Setup"]?["japan"]}";
            string goBackToMainMenu = MainClass.patcherLang == "en-US"
                ? "Go Back to Main Menu"
                : $"{MainClass.localizedText?["goBackToMainMenu"]}";

            AnsiConsole.MarkupLine($"1. {northAmerica}");
            AnsiConsole.MarkupLine($"2. {pal}");
            AnsiConsole.MarkupLine($"3. {japan}\n");

            AnsiConsole.MarkupLine($"4. {goBackToMainMenu}\n");

            int choice = MenuClass.UserChoose("1234");
            switch (choice)
            {
                case 1: // USA
                    MainClass.wc24_reg = MainClass.Region.USA;
                    WiiLinkRegionalChannelsSetup();
                    break;
                case 2: // PAL
                    MainClass.wc24_reg = MainClass.Region.PAL;
                    WiiLinkRegionalChannelsSetup();
                    break;
                case 3: // Japan
                    MainClass.wc24_reg = MainClass.Region.Japan;
                    WiiLinkRegionalChannelsSetup();
                    break;
                case 4: // Go back to main menu
                    MenuClass.MainMenu();
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
            MenuClass.PrintHeader();

            // Express Install Header Text
            string EIHeader = MainClass.patcherLang == "en-US"
                ? "Express Install"
                : $"{MainClass.localizedText?["ExpressInstall"]?["Header"]}";
            AnsiConsole.MarkupLine($"[bold springgreen2_1]{EIHeader}[/]\n");

            // Would you like to install WiiLink's regional channel services Text
            string wouldYouLike = MainClass.patcherLang == "en-US"
                ? "Would you like to install [bold][springgreen2_1]WiiLink[/]'s regional channel services[/]?"
                : $"{MainClass.localizedText?["ExpressInstall"]?["WiiLinkSetup"]?["wouldYouLike"]}";
            AnsiConsole.MarkupLine($"{wouldYouLike}\n");

            // Services that would be installed Text
            string toBeInstalled = MainClass.patcherLang == "en-US"
                ? "Services that would be installed:"
                : $"{MainClass.localizedText?["ExpressInstall"]?["WiiLinkSetup"]?["toBeInstalled"]}";
            AnsiConsole.MarkupLine($"{toBeInstalled}\n");

            // Channel Names
            string wiiRoom = MainClass.patcherLang == "en-US"
                ? "Wii Room [bold](Wii no Ma)[/]"
                : $"{MainClass.localizedText?["ExpressInstall"]?["WiiLinkSetup"]?["WiiRoom"]}";
            string photoPrints = MainClass.patcherLang == "en-US"
                ? "Photo Prints Channel [bold](Digicam Print Channel)[/]"
                : $"{MainClass.localizedText?["ExpressInstall"]?["WiiLinkSetup"]?["PhotoPrints"]}";
            string foodChannel = MainClass.patcherLang == "en-US"
                ? "Food Channel [bold](Demae Channel)[/]"
                : $"{MainClass.localizedText?["ExpressInstall"]?["WiiLinkSetup"]?["FoodChannel"]}";
            string kirbyTV = MainClass.patcherLang == "en-US"
                ? "Kirby TV Channel"
                : $"{MainClass.localizedText?["ExpressInstall"]?["WiiLinkSetup"]?["KirbyTV"]}";

            AnsiConsole.MarkupLine($"  ● {wiiRoom}");
            AnsiConsole.MarkupLine($"  ● {photoPrints}");
            AnsiConsole.MarkupLine($"  ● {foodChannel}");
            AnsiConsole.MarkupLine($"  ● {kirbyTV}\n");

            // Yes or No Text
            string yes = MainClass.patcherLang == "en-US"
                ? "Yes"
                : $"{MainClass.localizedText?["yes"]}";
            string no = MainClass.patcherLang == "en-US"
                ? "No"
                : $"{MainClass.localizedText?["no"]}";

            Console.WriteLine($"1. {yes}");
            Console.WriteLine($"2. {no}\n");

            // Go Back to Main Menu Text
            string goBackToMainMenu = MainClass.patcherLang == "en-US"
                ? "Go Back to Main Menu"
                : $"{MainClass.localizedText?["goBackToMainMenu"]}";
            Console.WriteLine($"3. {goBackToMainMenu}\n");

            int choice = MenuClass.UserChoose("123");
            switch (choice)
            {
                case 1:
                    MainClass.installRegionalChannels = true;
                    WiiLinkChannels_LangSetup();
                    break;
                case 2:
                    MainClass.installRegionalChannels = false;
                    ChoosePlatform();
                    break;
                case 3: // Go back to main menu
                    MenuClass.MainMenu();
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
            MenuClass.PrintHeader();

            // Express Install Header Text
            string EIHeader = MainClass.patcherLang == "en-US"
                ? "Express Install"
                : $"{MainClass.localizedText?["ExpressInstall"]?["Header"]}";
            AnsiConsole.MarkupLine($"[bold springgreen2_1]{EIHeader}[/]\n");

            // Step 2 Text
            string step2Message = MainClass.patcherLang == "en-US"
                ? "Step 2: Choose WiiLink's regional channels language"
                : $"{MainClass.localizedText?["ExpressInstall"]?["WiiLinkChannels_LangSetup"]?["step2Message"]}";
            AnsiConsole.MarkupLine($"[bold]{step2Message}[/]\n");

            // Instructions Text
            string instructions = MainClass.patcherLang == "en-US"
                ? "For [bold]Wii Room[/], [bold]Photo Prints Channel[/], and [bold]Food Channel[/], which language would you like to select?"
                : $"{MainClass.localizedText?["ExpressInstall"]?["WiiLinkChannels_LangSetup"]?["instructions"]}";
            AnsiConsole.MarkupLine($"{instructions}\n");

            // User Choices
            string translated = MainClass.patcherLang == "en-US"
                ? "Translated (eg. English, French, etc.)"
                : $"{MainClass.localizedText?["ExpressInstall"]?["WiiLinkChannels_LangSetup"]?["translatedOption"]}";
            string japanese = MainClass.patcherLang == "en-US"
                ? "Japanese"
                : $"{MainClass.localizedText?["ExpressInstall"]?["WiiLinkChannels_LangSetup"]?["japaneseOption"]}";
            string goBackToMainMenu = MainClass.patcherLang == "en-US"
                ? "Go Back to Main Menu"
                : $"{MainClass.localizedText?["goBackToMainMenu"]}";

            AnsiConsole.MarkupLine($"1. {translated}");
            AnsiConsole.MarkupLine($"2. {japanese}\n");

            AnsiConsole.MarkupLine($"3. {goBackToMainMenu}\n");

            int choice = MenuClass.UserChoose("123");
            switch (choice)
            {
                case 1:
                    MainClass.lang = MainClass.Language.English;
                    WiiRoomConfiguration();
                    break;
                case 2:
                    MainClass.lang = MainClass.Language.Japan;
                    MainClass.wiiRoomLang = MainClass.Language.Japan;
                    MainClass.demaeVersion = MainClass.DemaeVersion.Standard;
                    ChoosePlatform();
                    break;
                case 3:
                    MenuClass.MainMenu(); // Go back to main menu
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
            MenuClass.PrintHeader();

            // Express Install Header Text
            string EIHeader = MainClass.patcherLang == "en-US"
                ? "Express Install"
                : $"{MainClass.localizedText?["ExpressInstall"]?["Header"]}";
            AnsiConsole.MarkupLine($"[bold springgreen2_1]{EIHeader}[/]\n");

            // Step 2A Text
            string stepNumber = MainClass.patcherLang == "en-US"
                ? "Step 2A"
                : $"{MainClass.localizedText?["ExpressInstall"]?["WiiRoomConfiguration"]?["stepNum"]}";
            string step1aTitle = MainClass.patcherLang == "en-US"
                ? "Choose Wii Room language"
                : $"{MainClass.localizedText?["ExpressInstall"]?["WiiRoomConfiguration"]?["stepTitle"]}";
            AnsiConsole.MarkupLine($"[bold]{stepNumber}: {step1aTitle}[/]\n");

            // Instructions Text
            string instructions = MainClass.patcherLang == "en-US"
                ? "For [bold]Wii Room[/], which language would you like to select?"
                : $"{MainClass.localizedText?["ExpressInstall"]?["WiiRoomConfiguration"]?["instructions"]}";
            AnsiConsole.MarkupLine($"{instructions}\n");

            string goBackToMainMenu = MainClass.patcherLang == "en-US"
                ? "Go Back to Main Menu"
                : $"{MainClass.localizedText?["goBackToMainMenu"]}";

            AnsiConsole.MarkupLine($"1. English");
            AnsiConsole.MarkupLine($"2. Español");
            AnsiConsole.MarkupLine($"3. Français");
            AnsiConsole.MarkupLine($"4. Deutsch");
            AnsiConsole.MarkupLine($"5. Italiano");
            AnsiConsole.MarkupLine($"6. Nederlands");
            AnsiConsole.MarkupLine($"7. Português (Brasil)");
            AnsiConsole.MarkupLine($"8. Русский\n");

            AnsiConsole.MarkupLine($"9. {goBackToMainMenu}\n");

            int choice = MenuClass.UserChoose("123456789");
            switch (choice)
            {
                case 1:
                    MainClass.wiiRoomLang = MainClass.Language.English;
                    DemaeConfiguration();
                    break;
                case 2:
                    MainClass.wiiRoomLang = MainClass.Language.Spanish;
                    DemaeConfiguration();
                    break;
                case 3:
                    MainClass.wiiRoomLang = MainClass.Language.French;
                    DemaeConfiguration();
                    break;
                case 4:
                    MainClass.wiiRoomLang = MainClass.Language.German;
                    DemaeConfiguration();
                    break;
                case 5:
                    MainClass.wiiRoomLang = MainClass.Language.Italian;
                    DemaeConfiguration();
                    break;
                case 6:
                    MainClass.wiiRoomLang = MainClass.Language.Dutch;
                    DemaeConfiguration();
                    break;
                case 7:
                    MainClass.wiiRoomLang = MainClass.Language.Portuguese;
                    DemaeConfiguration();
                    break;
                case 8:
                    MainClass.wiiRoomLang = MainClass.Language.Russian;
                    RussianNoticeForWiiRoom();
                    break;
                case 9: // Go back to main menu
                    MenuClass.MainMenu();
                    break;
                default:
                    break;
            }
        }
    }

    // If Russian is chosen, show a special message for additional instructions
    static void RussianNoticeForWiiRoom()
    {
        MenuClass.PrintHeader();

        AnsiConsole.MarkupLine("[bold yellow]NOTICE FOR RUSSIAN USERS[/]\n");
        AnsiConsole.MarkupLine("You have selected Russian translation in the installation options.\n");
        AnsiConsole.MarkupLine("Proper functionality is not guaranteed for systems without the Russian Wii Menu.\n");
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
            MenuClass.PrintHeader();

            // Express Install Header Text
            string EIHeader = MainClass.patcherLang == "en-US"
                ? "Express Install"
                : $"{MainClass.localizedText?["ExpressInstall"]?["Header"]}";
            AnsiConsole.MarkupLine($"[bold springgreen2_1]{EIHeader}[/]\n");

            // Step 2B Text
            string stepNumber = MainClass.patcherLang == "en-US"
                ? "Step 2B"
                : $"{MainClass.localizedText?["ExpressInstall"]?["DemaeConfiguration"]?["stepNum"]}";
            string step1bTitle = MainClass.patcherLang == "en-US"
                ? "Choose Food Channel version"
                : $"{MainClass.localizedText?["ExpressInstall"]?["DemaeConfiguration"]?["stepTitle"]}";
            AnsiConsole.MarkupLine($"[bold]{stepNumber}: {step1bTitle}[/]\n");

            // Instructions Text
            string instructions = MainClass.patcherLang == "en-US"
                ? "For [bold]Food Channel[/], which version would you like to install?"
                : $"{MainClass.localizedText?["ExpressInstall"]?["DemaeConfiguration"]?["instructions"]}";
            AnsiConsole.MarkupLine($"{instructions}\n");

            // User Choices
            string demaeStandard = MainClass.patcherLang == "en-US"
                ? "Standard [bold](Fake Ordering)[/]"
                : $"{MainClass.localizedText?["ExpressInstall"]?["DemaeConfiguration"]?["demaeStandard"]}";
            string demaeDominos = MainClass.patcherLang == "en-US"
                ? "Domino's [bold](US and Canada only)[/]"
                : $"{MainClass.localizedText?["ExpressInstall"]?["DemaeConfiguration"]?["demaeDominos"]}";
            string goBackToMainMenu = MainClass.patcherLang == "en-US"
                ? "Go Back to Main Menu"
                : $"{MainClass.localizedText?["goBackToMainMenu"]}";

            AnsiConsole.MarkupLine($"1. {demaeStandard}");
            AnsiConsole.MarkupLine($"2. {demaeDominos}\n");

            AnsiConsole.MarkupLine($"3. {goBackToMainMenu}\n");

            int choice = MenuClass.UserChoose("123");
            switch (choice)
            {
                case 1:
                    MainClass.demaeVersion = MainClass.DemaeVersion.Standard;
                    ChoosePlatform();
                    break;
                case 2:
                    MainClass.demaeVersion = MainClass.DemaeVersion.Dominos;
                    ChoosePlatform();
                    break;
                case 3: // Go back to main menu
                    MenuClass.MainMenu();
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
            MenuClass.PrintHeader();

            // Express Install Header Text
            string EIHeader = MainClass.patcherLang == "en-US"
                ? "Express Install"
                : $"{MainClass.localizedText?["ExpressInstall"]?["Header"]}";
            AnsiConsole.MarkupLine($"[bold springgreen2_1]{EIHeader}[/]\n");

            // Change step number depending on if WiiConnect24 is being installed or not
            string stepNum = MainClass.patcherLang == "en-US"
                ? !MainClass.installRegionalChannels ? "Step 2" : "Step 3"
                : $"{MainClass.localizedText?["ExpressInstall"]?["ChoosePlatform"]?[!MainClass.installRegionalChannels ? "ifNoWC24" : "ifWC24"]?["stepNum"]}";
            string stepTitle = MainClass.patcherLang == "en-US"
                ? "Choose console platform"
                : $"{MainClass.localizedText?["ExpressInstall"]?["ChoosePlatform"]?["stepTitle"]}";

            AnsiConsole.MarkupLine($"[bold]{stepNum}: {stepTitle}[/]\n");

            // Instructions Text
            string instructions = MainClass.patcherLang == "en-US"
                ? "Which Wii version are you installing to?"
                : $"{MainClass.localizedText?["ExpressInstall"]?["ChoosePlatform"]?["instructions"]}";
            AnsiConsole.MarkupLine($"{instructions}\n");

            // User Choices
            string wii = MainClass.patcherLang == "en-US"
                ? "Wii"
                : $"{MainClass.localizedText?["ExpressInstall"]?["ChoosePlatform"]?["wii"]}";
            string vWii = MainClass.patcherLang == "en-US"
                ? "vWii [bold](Wii U)[/]"
                : $"{MainClass.localizedText?["ExpressInstall"]?["ChoosePlatform"]?["vWii"]}";
            string Dolphin = MainClass.patcherLang == "en-US"
                ? "Dolphin Emulator"
                : $"{MainClass.localizedText?["ExpressInstall"]?["ChoosePlatform"]?["dolphin"]}";
            string goBackToMainMenu = MainClass.patcherLang == "en-US"
                ? "Go Back to Main Menu"
                : $"{MainClass.localizedText?["goBackToMainMenu"]}";

            AnsiConsole.MarkupLine($"1. {wii}");
            AnsiConsole.MarkupLine($"2. {vWii}");
            AnsiConsole.MarkupLine($"3. {Dolphin}\n");

            AnsiConsole.MarkupLine($"4. {goBackToMainMenu}\n");

            int choice = MenuClass.UserChoose("1234");
            switch (choice)
            {
                case 1:
                    MainClass.platformType = MainClass.Platform.Wii;
                    SdClass.SDSetup(MainClass.SetupType.express);
                    break;
                case 2:
                    MainClass.platformType = MainClass.Platform.vWii;
                    SdClass.SDSetup(MainClass.SetupType.express);
                    break;
                case 3:
                    MainClass.platformType = MainClass.Platform.Dolphin;
                    MainClass.sdcard = null;
                    MenuClass.WADFolderCheck(MainClass.SetupType.express);
                    break;
                case 4: // Go back to main menu
                    MenuClass.MainMenu();
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
        if (!Directory.Exists(MainClass.tempDir))
            Directory.CreateDirectory(MainClass.tempDir);

        // Demae version text to be used in the patching progress for Demae Channel (eg. "Food Channel (English) [Standard]")
        string demaeVerTxt = MainClass.demaeVersion == MainClass.DemaeVersion.Standard
            ? "Standard"
            : "Domino's";

        // Define WiiLink channels titles
        string demae_title = MainClass.lang == MainClass.Language.English
                ? $"Food Channel [bold](English)[/] [bold][[{demaeVerTxt}]][/]"
                : $"Demae Channel [bold](Japanese)[/] [bold][[{demaeVerTxt}]][/]";

        string wiiroom_title = MainClass.wiiRoomLang != MainClass.Language.Japan
                ? $"Wii Room [bold]({MainClass.wiiRoomLang})[/]"
                : "Wii no Ma [bold](Japanese)[/]";

        string digicam_title = MainClass.lang == MainClass.Language.English
                ? "Photo Prints Channel [bold](English)[/]"
                : "Digicam Print Channel [bold](Japanese)[/]";

        string kirbytv_title = "Kirby TV Channel"; // Kirby TV Channel

        // Define the channelMessages dictionary with WC24 channel titles
        string internationalOrJapanese = (MainClass.wc24_reg == MainClass.Region.USA || MainClass.wc24_reg == MainClass.Region.PAL) ? "International" : "Japanese";
        string NCTitle, forecastTitle, newsTitle, evcTitle, cmocTitle;

        if (MainClass.patcherLang == "en-US")
        {
            NCTitle = $"{(MainClass.wc24_reg == MainClass.Region.USA || MainClass.wc24_reg == MainClass.Region.PAL ? "Nintendo Channel" : "Minna no Nintendo Channel")} [bold]({MainClass.wc24_reg})[/]";
            forecastTitle = $"Forecast Channel [bold]({MainClass.wc24_reg})[/]";
            newsTitle = $"News Channel [bold]({MainClass.wc24_reg})[/]";
            evcTitle = $"Everybody Votes Channel [bold]({MainClass.wc24_reg})[/]";
            cmocTitle = $"{(MainClass.wc24_reg == MainClass.Region.USA ? "Check Mii Out Channel" : "Mii Contest Channel")} [bold]({MainClass.wc24_reg})[/]";
        }
        else
        {
            NCTitle = $"{MainClass.localizedText?["ChannelNames"]?[internationalOrJapanese]?["nintendoChn"]} [bold]({MainClass.wc24_reg})[/]";
            forecastTitle = $"{MainClass.localizedText?["ChannelNames"]?["International"]?["forecastChn"]} [bold]({MainClass.wc24_reg})[/]";
            newsTitle = $"{MainClass.localizedText?["ChannelNames"]?["International"]?["newsChn"]} [bold]({MainClass.wc24_reg})[/]";
            evcTitle = $"{MainClass.localizedText?["ChannelNames"]?["International"]?["everybodyVotes"]} [bold]({MainClass.wc24_reg})[/]";
            cmocTitle = $"{MainClass.localizedText?["ChannelNames"]?["International"]?["cmoc"]} [bold]({MainClass.wc24_reg})[/]";
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
        if (MainClass.installRegionalChannels)
        {
            channelMessages.Add("wiiroom", wiiroom_title);
            channelMessages.Add("digicam", digicam_title);
            channelMessages.Add("demae", demae_title);
            channelMessages.Add("kirbytv", kirbytv_title);
        }

        // Setup patching process list
        var patching_functions = new List<Action>
        {
            PatchClass.DownloadAllPatches,
            () => PatchClass.NC_Patch(MainClass.wc24_reg),
            () => PatchClass.Forecast_Patch(MainClass.wc24_reg),
            () => PatchClass.News_Patch(MainClass.wc24_reg),
            () => PatchClass.EVC_Patch(MainClass.wc24_reg),
            () => PatchClass.CheckMiiOut_Patch(MainClass.wc24_reg)
        };

        // Add other patching functions if applicable
        if (MainClass.installRegionalChannels)
        {
            patching_functions.Add(() => PatchClass.WiiRoom_Patch(MainClass.wiiRoomLang));
            patching_functions.Add(() => PatchClass.Digicam_Patch(MainClass.lang));
            patching_functions.Add(() => PatchClass.Demae_Patch(MainClass.lang, MainClass.demaeVersion, MainClass.wc24_reg));
            patching_functions.Add(PatchClass.KirbyTV_Patch);
        }

        patching_functions.Add(SdClass.FinishSDCopy);

        //// Set up patching progress dictionary ////
        // Flush dictionary and downloading patches
        MainClass.patchingProgress_express.Clear();
        MainClass.patchingProgress_express.Add("downloading", "in_progress");

        // Patching WiiConnect24 channels
        foreach (string channel in new string[] { "nc", "forecast", "news", "evc", "cmoc" })
            MainClass.patchingProgress_express.Add(channel, "not_started");

        if (MainClass.installRegionalChannels)
        {
            // Patching Regional Channels
            foreach (string channel in new string[] { "wiiroom", "digicam", "demae", "kirbytv" })
                MainClass.patchingProgress_express.Add(channel, "not_started");
        }

        // Finishing up
        MainClass.patchingProgress_express.Add("finishing", "not_started");

        // While the patching process is not finished
        while (MainClass.patchingProgress_express["finishing"] != "done")
        {
            MenuClass.PrintHeader();

            // Progress bar and completion display
            string patching = MainClass.patcherLang == "en-US"
                ? "Patching... this can take some time depending on the processing speed (CPU) of your computer."
                : $"{MainClass.localizedText?["PatchingProgress"]?["patching"]}";
            string progress = MainClass.patcherLang == "en-US"
                ? "Progress"
                : $"{MainClass.localizedText?["PatchingProgress"]?["progress"]}";
            AnsiConsole.MarkupLine($"[bold][[*]] {patching}[/]\n");
            AnsiConsole.Markup($"    {progress}: ");

            //// Progress bar and completion display ////
            // Calculate percentage based on how many channels are completed
            int percentage = (int)((float)MainClass.patchingProgress_express.Where(x => x.Value == "done").Count() / (float)MainClass.patchingProgress_express.Count * 100.0f);

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
            string percentComplete = MainClass.patcherLang == "en-US"
                ? "completed"
                : $"{MainClass.localizedText?["PatchingProgress"]?["percentComplete"]}";
            string pleaseWait = MainClass.patcherLang == "en-US"
                ? "Please wait while the patching process is in progress..."
                : $"{MainClass.localizedText?["PatchingProgress"]?["pleaseWait"]}";
            AnsiConsole.Markup($" [bold]{percentage}%[/] {percentComplete}\n\n");
            AnsiConsole.MarkupLine($"{pleaseWait}\n");

            //// Display progress for each channel ////

            // Pre-Patching Section: Downloading files
            string prePatching = MainClass.patcherLang == "en-US"
                ? "Pre-Patching"
                : $"{MainClass.localizedText?["PatchingProgress"]?["prePatching"]}";
            string downloadingFiles = MainClass.patcherLang == "en-US"
                ? "Downloading files..."
                : $"{MainClass.localizedText?["PatchingProgress"]?["downloadingFiles"]}";
            AnsiConsole.MarkupLine($"[bold]{prePatching}:[/]");
            switch (MainClass.patchingProgress_express["downloading"])
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
            string patchingWiiConnect24Channels = MainClass.patcherLang == "en-US"
                ? "Patching WiiConnect24 Channels"
                : $"{MainClass.localizedText?["PatchingProgress"]?["patchingWiiConnect24Channels"]}";

            AnsiConsole.MarkupLine($"\n[bold]{patchingWiiConnect24Channels}:[/]");
            foreach (string channel in new string[] { "nc", "forecast", "news", "evc", "cmoc" })
            {
                switch (MainClass.patchingProgress_express[channel])
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
            if (MainClass.installRegionalChannels)
            {
                string patchingWiiLinkChannels = MainClass.patcherLang == "en-US"
                    ? "Patching Regional Channels"
                    : $"{MainClass.localizedText?["PatchingProgress"]?["patchingWiiLinkChannels"]}";
                AnsiConsole.MarkupLine($"\n[bold]{patchingWiiLinkChannels}:[/]");
                foreach (string channel in new string[] { "wiiroom", "digicam", "demae", "kirbytv" })
                {
                    switch (MainClass.patchingProgress_express[channel])
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
            string postPatching = MainClass.patcherLang == "en-US"
                ? "Post-Patching"
                : $"{MainClass.localizedText?["PatchingProgress"]?["postPatching"]}";
            string finishingUp = MainClass.patcherLang == "en-US"
                ? "Finishing up..."
                : $"{MainClass.localizedText?["PatchingProgress"]?["finishingUp"]}";
            AnsiConsole.MarkupLine($"\n[bold]{postPatching}:[/]");
            switch (MainClass.patchingProgress_express["finishing"])
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
        MenuClass.Finished();
    }
}