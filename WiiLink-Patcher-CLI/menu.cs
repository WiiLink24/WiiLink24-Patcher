using System.Diagnostics;
using System.Text;
using System.Runtime.InteropServices;
using Spectre.Console;
using libWiiSharp;
using System.Net;
using Newtonsoft.Json.Linq;
using System.IO.Compression;

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
                .Replace("{buildDate}", main.buildDate)
                .Replace("{buildTime}", main.buildTime);
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
            ? "Github repository"
            : $"{main.localizedText?["Credits"]?["githubRepo"]}";

        var linksGrid = new Grid().AddColumn().AddColumn();

        linksGrid.AddRow($"[bold springgreen2_1]{wiilinkSite}[/]:", "[link]https://wiilink24.com[/]");
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

    // Configure Wii Room Channel (choosing language) [Express Install]
    static void WiiRoomConfiguration()
    {
        while (true)
        {
            PrintHeader();

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

            // User Choices
            string english = main.patcherLang == main.PatcherLanguage.en
                ? "English"
                : $"{main.localizedText?["ExpressInstall"]?["WiiRoomConfiguration"]?["english"]}";
            string spanish = main.patcherLang == main.PatcherLanguage.en
                ? "Español"
                : $"{main.localizedText?["ExpressInstall"]?["WiiRoomConfiguration"]?["spanish"]}";
            string french = main.patcherLang == main.PatcherLanguage.en
                ? "Français"
                : $"{main.localizedText?["ExpressInstall"]?["WiiRoomConfiguration"]?["french"]}";
            string german = main.patcherLang == main.PatcherLanguage.en
                ? "Deutsch"
                : $"{main.localizedText?["ExpressInstall"]?["WiiRoomConfiguration"]?["german"]}";
            string italian = main.patcherLang == main.PatcherLanguage.en
                ? "Italiano"
                : $"{main.localizedText?["ExpressInstall"]?["WiiRoomConfiguration"]?["italian"]}";
            string dutch = main.patcherLang == main.PatcherLanguage.en
                ? "Nederlands"
                : $"{main.localizedText?["ExpressInstall"]?["WiiRoomConfiguration"]?["dutch"]}";
            string portuguese = main.patcherLang == main.PatcherLanguage.en
                ? "Português (Brasil)"
                : $"{main.localizedText?["ExpressInstall"]?["WiiRoomConfiguration"]?["portuguese"]}";
            string russian = main.patcherLang == main.PatcherLanguage.en
                ? "Русский"
                : $"{main.localizedText?["ExpressInstall"]?["WiiRoomConfiguration"]?["russian"]}";
            string goBackToMainMenu = main.patcherLang == main.PatcherLanguage.en
                ? "Go Back to Main Menu"
                : $"{main.localizedText?["ExpressInstall"]?["WiiRoomConfiguration"]?["goBackToMainMenu"]}";

            AnsiConsole.MarkupLine($"1. {english}");
            AnsiConsole.MarkupLine($"2. {spanish}");
            AnsiConsole.MarkupLine($"3. {french}");
            AnsiConsole.MarkupLine($"4. {german}");
            AnsiConsole.MarkupLine($"5. {italian}");
            AnsiConsole.MarkupLine($"6. {dutch}");
            AnsiConsole.MarkupLine($"7. {portuguese}");
            AnsiConsole.MarkupLine($"8. {russian}\n");

            AnsiConsole.MarkupLine($"9. {goBackToMainMenu}\n");

            int choice = UserChoose("123456789");
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
                    MainMenu();
                    break;
                default:
                    break;
            }
        }
    }

    // If Russian is chosen, show a special message for additional instructions
    static void RussianNoticeForWiiRoom()
    {
        PrintHeader();

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
            PrintHeader();

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
                : $"{main.localizedText?["ExpressInstall"]?["DemaeConfiguration"]?["goBackToMainMenu"]}";

            AnsiConsole.MarkupLine($"1. {demaeStandard}");
            AnsiConsole.MarkupLine($"2. {demaeDominos}\n");

            AnsiConsole.MarkupLine($"3. {goBackToMainMenu}\n");

            int choice = UserChoose("123");
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
                    MainMenu();
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
            PrintHeader();

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
                : $"{main.localizedText?["ExpressInstall"]?["WiiLinkSetup"]?["yes"]}";
            string no = main.patcherLang == main.PatcherLanguage.en
                ? "No"
                : $"{main.localizedText?["ExpressInstall"]?["WiiLinkSetup"]?["no"]}";

            Console.WriteLine($"1. {yes}");
            Console.WriteLine($"2. {no}\n");

            // Go Back to Main Menu Text
            string goBackToMainMenu = main.patcherLang == main.PatcherLanguage.en
                ? "Go Back to Main Menu"
                : $"{main.localizedText?["ExpressInstall"]?["WiiLinkSetup"]?["goBackToMainMenu"]}";
            Console.WriteLine($"3. {goBackToMainMenu}\n");

            int choice = UserChoose("123");
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
                    MainMenu();
                    break;
                default:
                    break;
            }
        }
    }

    // Configure region for all WiiConnect24 services (Express Install)
    static void WC24Setup()
    {
        while (true)
        {
            PrintHeader();

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
                : $"{main.localizedText?["ExpressInstall"]?["WC24Setup"]?["goBackToMainMenu"]}";

            AnsiConsole.MarkupLine($"1. {northAmerica}");
            AnsiConsole.MarkupLine($"2. {pal}");
            AnsiConsole.MarkupLine($"3. {japan}\n");

            AnsiConsole.MarkupLine($"4. {goBackToMainMenu}\n");

            int choice = UserChoose("1234");
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
                    MainMenu();
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
            PrintHeader();

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
                ? "Wii [bold][/]"
                : $"{main.localizedText?["ExpressInstall"]?["ChoosePlatform"]?["wii"]}";
            string vWii = main.patcherLang == main.PatcherLanguage.en
                ? "vWii [bold](Wii U)[/]"
                : $"{main.localizedText?["ExpressInstall"]?["ChoosePlatform"]?["vWii"]}";
            string Dolphin = main.patcherLang == main.PatcherLanguage.en
                ? "Dolphin Emulator[bold][/]"
                : $"{main.localizedText?["ExpressInstall"]?["ChoosePlatform"]?["dolphin"]}";
            string goBackToMainMenu = main.patcherLang == main.PatcherLanguage.en
                ? "Go Back to Main Menu"
                : $"{main.localizedText?["ExpressInstall"]?["ChoosePlatform"]?["goBackToMainMenu"]}";

            AnsiConsole.MarkupLine($"1. {wii}");
            AnsiConsole.MarkupLine($"2. {vWii}");
            AnsiConsole.MarkupLine($"3. {Dolphin}\n");

            AnsiConsole.MarkupLine($"4. {goBackToMainMenu}\n");

            int choice = UserChoose("1234");
            switch (choice)
            {
                case 1:
                    main.platformType = main.Platform.Wii;
                    SDSetup();
                    break;
                case 2:
                    main.platformType = main.Platform.vWii;
                    SDSetup();
                    break;
                case 3:
                    main.platformType = main.Platform.Dolphin;
                    main.sdcard = null;
                    WADFolderCheck(false);
                    break;
                case 4: // Go back to main menu
                    MainMenu();
                    break;
                default:
                    break;
            }
        }
    }


    // SD card setup
    static void SDSetup(bool isCustomSetup = false)
    {
        while (true)
        {
            PrintHeader();

            // Change step number depending on if WiiConnect24 is being installed or not
            string stepNum = main.patcherLang == main.PatcherLanguage.en
                ? isCustomSetup ? "Step 4" : (!main.installRegionalChannels ? "Step 3" : "Step 4")
                : $"{main.localizedText?["SDSetup"]?[isCustomSetup ? "ifCustom" : "ifExpress"]?[main.installRegionalChannels ? "ifWC24" : "ifNoWC24"]?["stepNum"]}";

            // Change header depending on if it's Express Install or Custom Install
            string installType = main.patcherLang == main.PatcherLanguage.en
                ? isCustomSetup ? "Custom Install" : "Express Install"
                : isCustomSetup ? $"{main.localizedText?["ExpressInstall"]?["Header"]}" : $"{main.localizedText?["CustomInstall"]?["Header"]}";

            // Step title
            string stepTitle = main.patcherLang == main.PatcherLanguage.en
                ? "Insert SD Card / USB Drive (if applicable)"
                : $"{main.localizedText?["SDSetup"]?["stepTitle"]}";

            // After passing this step text
            string afterPassingThisStep = main.patcherLang == main.PatcherLanguage.en
                ? "After passing this step, any user interaction won't be needed, so sit back and relax!"
                : $"{main.localizedText?["SDSetup"]?["afterPassingThisStep"]}";

            // Download to SD card text
            string downloadToSD = main.patcherLang == main.PatcherLanguage.en
                ? "You can download everything directly to your Wii SD Card / USB Drive if you insert it before starting the patching\nprocess. Otherwise, everything will be saved in the same folder as this patcher on your computer."
                : $"{main.localizedText?["SDSetup"]?["downloadToSD"]}";



            // SD card detected text
            string sdDetected = main.patcherLang == main.PatcherLanguage.en
                ? main.sdcard != null ? $"SD card detected: [bold springgreen2_1]{main.sdcard}[/]" : ""
                : main.sdcard != null ? $"{main.localizedText?["SDSetup"]?["sdDetected"]}: [bold springgreen2_1]{main.sdcard}[/]" : "";

            // Go Back to Main Menu Text
            string goBackToMainMenu = main.patcherLang == main.PatcherLanguage.en
                ? "Go Back to Main Menu"
                : $"{main.localizedText?["ExpressInstall"]?["SDSetup"]?["goBackToMainMenu"]}";

            AnsiConsole.MarkupLine($"[bold springgreen2_1]{installType}[/]\n");

            AnsiConsole.MarkupLine($"[bold]{stepNum}: {stepTitle}[/]\n");

            Console.WriteLine($"{afterPassingThisStep}\n");

            Console.WriteLine($"{downloadToSD}\n");

            if (main.platformType == main.Platform.vWii && !isCustomSetup)
            {
                string eulaChannel = main.patcherLang == main.PatcherLanguage.en
                ? "[bold]NOTE:[/] For [bold deepskyblue1]vWii[/] users, The EULA channel will also be included."
                : $"{main.localizedText?["ExpressInstall"]?["SDSetup"]?["eulaChannel"]}";
                AnsiConsole.MarkupLine($"{eulaChannel}\n");
            }

            // User Choices
            string startOption = main.patcherLang == main.PatcherLanguage.en
                ? main.sdcard != null ? "Start [bold]with[/] SD Card / USB Drive" : "Start [bold]without[/] SD Card / USB Drive"
                : main.sdcard != null ? $"{main.localizedText?["SDSetup"]?["start_withSD"]}" : $"{main.localizedText?["SDSetup"]?["start_noSD"]}";
            string startWithoutSDOption = main.patcherLang == main.PatcherLanguage.en
                ? "Start [bold]without[/] SD Card / USB Drive"
                : $"{main.localizedText?["SDSetup"]?["start_noSD"]}";
            string manualDetection = main.patcherLang == main.PatcherLanguage.en
                ? "Manually Select SD Card / USB Drive Path\n"
                : $"{main.localizedText?["SDSetup"]?["manualDetection"]}";

            AnsiConsole.MarkupLine($"1. {startOption}");
            AnsiConsole.MarkupLine($"2. {(main.sdcard != null ? startWithoutSDOption : manualDetection)}");
            AnsiConsole.MarkupLine($"3. {(main.sdcard != null ? manualDetection : goBackToMainMenu)}");

            if (main.sdcard != null)
            {
                AnsiConsole.MarkupLine($"4. {goBackToMainMenu}\n");

                AnsiConsole.MarkupLine($"{sdDetected}");
            }

            AnsiConsole.MarkupLine("");
            int choice = main.sdcard != null ? UserChoose("1234") : UserChoose("123");

            switch (choice)
            {
                case 1: // Check if WAD folder exists before starting patching process
                    WADFolderCheck(isCustomSetup);
                    break;
                case 2: // Start patching process without SD card or Manually select SD card
                    if (main.sdcard != null)
                    {
                        main.sdcard = null;
                        WADFolderCheck(isCustomSetup);
                    }
                    else
                    {
                        sd.SDCardSelect();
                    }
                    break;
                case 3: // Manually select SD card or Go back to main menu
                    if (main.sdcard != null)
                    {
                        sd.SDCardSelect();
                    }
                    else
                    {
                        MainMenu();
                    }
                    break;
                case 4: // Go back to main menu
                    if (main.sdcard != null)
                    {
                        MainMenu();
                    }
                    break;
                default:
                    break;
            }
        }
    }


    // WAD folder check (Express Install)
    static void WADFolderCheck(bool isCustomSetup = false)
    {
        // Start patching process if WAD folder doesn't exist
        if (!Directory.Exists("WAD"))
        {
            if (!isCustomSetup)
                PatchingProgress_Express();
            else
                PatchingProgress_Custom();
        }
        else
        {
            while (true)
            {
                PrintHeader();

                // Change header depending on if it's Express Install or Custom Install
                string installType = main.patcherLang == main.PatcherLanguage.en
                    ? isCustomSetup ? "Custom Install" : "Express Install"
                    : isCustomSetup ? $"{main.localizedText?["ExpressInstall"]?["Header"]}" : $"{main.localizedText?["CustomInstall"]?["Header"]}";
                string stepNum = main.patcherLang == main.PatcherLanguage.en
                    ? isCustomSetup ? "Step 5" : (!main.installRegionalChannels ? "Step 4" : "Step 5")
                    : isCustomSetup ? $"{main.localizedText?["WADFolderCheck"]?["ifCustom"]?["stepNum"]}" : $"{main.localizedText?["WADFolderCheck"]?["ifExpress"]?[main.installRegionalChannels ? "ifWC24" : "ifNoWC24"]?["stepNum"]}";

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
                    : $"{main.localizedText?["WADFolderCheck"]?["goBackToMainMenu"]}";

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
                                : $"{main.localizedText?["WADFolderCheck"]?["pressAnyKey"]}";
                            AnsiConsole.MarkupLine($"{pressAnyKey}\n");

                            WADFolderCheck();
                        }

                        if (!isCustomSetup)
                            PatchingProgress_Express();
                        else
                            PatchingProgress_Custom();
                        break;
                    case 2: // Keep WAD folder
                        if (!isCustomSetup)
                            PatchingProgress_Express();
                        else
                            PatchingProgress_Custom();
                        break;
                    case 3: // Go back to main menu
                        MainMenu();
                        break;
                    default:
                        break;
                }
            }
        }
    }


    // Patching progress function (Express Install)
    static void PatchingProgress_Express()
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
        string demae_title = main.patcherLang == main.PatcherLanguage.en // Demae Channel
            ? main.lang == main.Language.English
                ? $"Food Channel [bold](English)[/] [bold][[{demaeVerTxt}]][/]"
                : $"Demae Channel [bold](Japanese)[/] [bold][[{demaeVerTxt}]][/]"
            : $"{main.localizedText?["ChannelNames"]?[$"{main.lang}]?[${(main.lang == main.Language.English ? "food" : "demae")}"]} [bold]({main.lang})[/] [bold][[{demaeVerTxt}]][/]";

        string wiiroom_title = main.patcherLang == main.PatcherLanguage.en // Wii no Ma
            ? main.wiiRoomLang != main.Language.Japan
                ? $"Wii Room [bold]({main.wiiRoomLang})[/]"
                : "Wii no Ma [bold](Japanese)[/]"
            : $"{main.localizedText?["ChannelNames"]?[$"{main.wiiRoomLang}"]?[$"{(main.wiiRoomLang != main.Language.Japan ? "wiiRoom" : "wiiNoMa")}"]} [bold]({main.wiiRoomLang})[/]";

        string digicam_title = main.patcherLang == main.PatcherLanguage.en // Digicam Print Channel
            ? main.lang == main.Language.English
                ? "Photo Prints Channel [bold](English)[/]"
                : "Digicam Print Channel [bold](Japanese)[/]"
            : $"{main.localizedText?["ChannelNames"]?[$"{main.lang}"]?[$"{(main.lang == main.Language.English ? "photoPrints" : "digicam")}"]} [bold]({main.lang})[/]";

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
            PrintHeader();

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
        Finished();
    }

    // Patching progress function (Custom Install)
    static void PatchingProgress_Custom()
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
            PrintHeader();

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

            if (main.extraChannels_selection.Count > 0)
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
        Finished();
    }

    static void Finished()
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
                        : $"{main.localizedText?["Finished"]?["pleaseProceedWii"]}";
                    AnsiConsole.MarkupLine($"{pleaseProceed}\n");
                }
                else if (main.platformType == main.Platform.vWii)
                {
                    string pleaseProceed = main.patcherLang == main.PatcherLanguage.en
                        ? "Please proceed with the tutorial that you can find on [bold springgreen2_1 link]https://wiilink.ca/guide/vwii/#section-iii---installing-wads-and-patching-wii-mail[/]"
                        : $"{main.localizedText?["Finished"]?["pleaseProceedvWii"]}";
                    AnsiConsole.MarkupLine($"{pleaseProceed}\n");
                }
                else
                {
                    string pleaseProceed = main.patcherLang == main.PatcherLanguage.en
                        ? "Please proceed with the tutorial that you can find on [bold springgreen2_1 link]https://wiilink.ca/guide/dolphin/#section-ii---installing-wads[/]"
                        : $"{main.localizedText?["Finished"]?["pleaseProceedDolphin"]}";
                    AnsiConsole.MarkupLine($"{pleaseProceed}\n");
                }
            }
            else
            {
                if (main.platformType == main.Platform.Dolphin)
                {
                    string installWad = main.patcherLang == main.PatcherLanguage.en
                        ? "Please proceed with installing the WADs through the Dolphin interface (Tools > Install WAD...)"
                        : $"{main.localizedText?["Finished"]?["installWadDolphin"]}";
                    AnsiConsole.MarkupLine($"{installWad}\n");
                }
                else
                {
                    string installWad = main.patcherLang == main.PatcherLanguage.en
                        ? "Please proceed with the tutorial that you can find on [bold springgreen2_1 link]https://wii.hacks.guide/yawmme[/]"
                        : $"{main.localizedText?["Finished"]?["installWadYawmme"]}";
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
                : $"{main.localizedText?["Finished"]?["goBackToMainMenu"]}";
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
                    WC24Setup();
                    break;
                case 2: // Start Custom Install
                    CustomInstall_WiiLinkChannels_Setup();
                    break;
                case 3: // Start Extras Setup
                    systemChannelRestorer_Setup();
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

    // Install Choose (Express Install)
    static void WiiLinkChannels_LangSetup()
    {
        while (true)
        {
            PrintHeader();

            // Express Install Header Text
            string EIHeader = main.patcherLang == main.PatcherLanguage.en
                ? "Express Install"
                : $"{main.localizedText?["ExpressInstall"]?["Header"]}";
            AnsiConsole.MarkupLine($"[bold springgreen2_1]{EIHeader}[/]\n");

            // Step 2 Text
            string step1Message = main.patcherLang == main.PatcherLanguage.en
                ? "Step 2: Choose WiiLink's regional channels language"
                : $"{main.localizedText?["ExpressInstall"]?["WiiLinkChannels_LangSetup"]?["step1Message"]}";
            AnsiConsole.MarkupLine($"[bold]{step1Message}[/]\n");

            // Instructions Text
            string instructions = main.patcherLang == main.PatcherLanguage.en
                ? "For [bold]Wii Room[/], [bold]Photo Prints Channel[/], and [bold]Food Channel[/], which language would you like to select?"
                : $"{main.localizedText?["ExpressInstall"]?["WiiLinkChannels_LangSetup"]?["instructions"]}";
            AnsiConsole.MarkupLine($"{instructions}\n");

            // User Choices
            string englishTranslation = main.patcherLang == main.PatcherLanguage.en
                ? "Translated (eg. English, French, etc.)"
                : $"{main.localizedText?["ExpressInstall"]?["WiiLinkChannels_LangSetup"]?["englishOption"]}";
            string japanese = main.patcherLang == main.PatcherLanguage.en
                ? "Japanese"
                : $"{main.localizedText?["ExpressInstall"]?["WiiLinkChannels_LangSetup"]?["japaneseOption"]}";
            string goBackToMainMenu = main.patcherLang == main.PatcherLanguage.en
                ? "Go Back to Main Menu"
                : $"{main.localizedText?["ExpressInstall"]?["WiiLinkChannels_LangSetup"]?["goBackToMainMenu"]}";

            AnsiConsole.MarkupLine($"1. {englishTranslation}");
            AnsiConsole.MarkupLine($"2. {japanese}\n");

            AnsiConsole.MarkupLine($"3. {goBackToMainMenu}\n");

            int choice = UserChoose("123");
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
                    MainMenu(); // Go back to main menu
                    return;
                default:
                    break;
            }
        }
    }

    // Page selection function
    public static (int, int) GetPageIndices(int currentPage, int totalItems, int itemsPerPage)
    {
        int start = (currentPage - 1) * itemsPerPage;
        int end = Math.Min(start + itemsPerPage, totalItems);
        return (start, end);
    }

    // Custom Install (Part 1 - Select WiiLink channels)
    static void CustomInstall_WiiLinkChannels_Setup()
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

        // Initialize selection list to "Not selected" using LINQ
        if (main.combinedChannels_selection.Count == 0) // Only do this
            main.combinedChannels_selection = channelMap.Values.Select(_ => "[grey]Not selected[/]").ToList();

        // Page setup
        const int ITEMS_PER_PAGE = 9;
        int currentPage = 1;

        while (true)
        {
            PrintHeader();

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
            (int start, int end) = GetPageIndices(currentPage, channelMap.Count, ITEMS_PER_PAGE);

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
            int choice = UserChoose(choices);

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
            string notSelected = main.patcherLang == main.PatcherLanguage.en
                ? "Not selected"
                : $"{main.localizedText?["CustomSetup"]?["notSelected"]}";
            string selectedText = main.patcherLang == main.PatcherLanguage.en
                ? "Selected"
                : $"{main.localizedText?["CustomSetup"]?["selected"]}";

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
                    MainMenu();
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
            PrintHeader();

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

            int choice = UserChoose("123");

            // Use a switch statement to handle user's SPD version selection
            switch (choice)
            {
                case -1: // Escape
                    main.combinedChannels_selection.Clear();
                    main.wiiLinkChannels_selection.Clear();
                    main.wiiConnect24Channels_selection.Clear();
                    MainMenu();
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
            PrintHeader();

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
                : $"{main.localizedText?["CustomSetup"]?["summaryScreen"]?["confirmation"]?["yes"]}";
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

            var choice = UserChoose("123");

            // Handle user confirmation choice
            switch (choice)
            {
                case 1: // Yes
                    if (main.platformType_custom != main.Platform.Dolphin)
                    {
                        SDSetup(isCustomSetup: true);
                        break;
                    }
                    else
                    {
                        main.sdcard = null;
                        WADFolderCheck(true);
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
                    MainMenu();
                    break;
                default:
                    break;
            }
        }
    }

    // Install Extras (Part 1 - System Channel Restorer)
    static void systemChannelRestorer_Setup()
    {
        main.task = "Install Extras (Part 1 - System Channel Restorer)";
        while (true)
        {
            PrintHeader();

            // Print title
            string installExtras = main.patcherLang == main.PatcherLanguage.en
                ? "Install Extras"
                : $"{main.localizedText?["InstallExtras"]?["Header"]}";
            AnsiConsole.MarkupLine($"[bold springgreen2_1]{installExtras}[/]\n");

            // Print step number and title
            string stepNum = main.patcherLang == main.PatcherLanguage.en
                ? "Step 1"
                : $"{main.localizedText?["CustomSetup"]?["wiiLinkChannels_Setup"]?["stepNum"]}";
            string stepTitle = main.patcherLang == main.PatcherLanguage.en
                ? "System Channel Restorer"
                : $"{main.localizedText?["CustomSetup"]?["ConsolePlatform_Setup"]?["stepTitle"]}";
            AnsiConsole.MarkupLine($"[bold]{stepNum}:[/] {stepTitle}\n");

            // Display console platform selection menu
            string AskSystemChannelRestorer = main.patcherLang == main.PatcherLanguage.en
                ? "Would you like to download System Channel Restorer?"
                : $"{main.localizedText?["ExtrasInstall"]?["systemChannelRestorer_Setup"]?["systemChannelRestorer"]}";
            AnsiConsole.MarkupLine($"[bold]{AskSystemChannelRestorer}[/]\n");

            // Display console platform selection menu
            string systemChannelRestorerInfo = main.patcherLang == main.PatcherLanguage.en
                ? "System Channel Restorer is a homebrew application that allows for proper installation of Photo Channel 1.1 directly to your console."
                : $"{main.localizedText?["ExtrasInstall"]?["systemChannelRestorer_Setup"]?["systemChannelRestorerInfo"]}";
            AnsiConsole.MarkupLine($"[grey]{systemChannelRestorerInfo}[/]");

            // Display console platform selection menu
            string moreSystemChannelRestorerInfo = main.patcherLang == main.PatcherLanguage.en
                ? "Use of System Channel Restorer requires an internet connection on your console, and is more difficult to use on Dolphin than offline WADs."
                : $"{main.localizedText?["ExtrasInstall"]?["systemChannelRestorer_Setup"]?["moreSystemChannelRestorerInfo"]}";
            AnsiConsole.MarkupLine($"[grey]{moreSystemChannelRestorerInfo}[/]\n");

            // Print Console Platform options
            string useSystemChannelRestorer = main.patcherLang == main.PatcherLanguage.en
                ? "[bold]System Channel Restorer[/]"
                : $"{main.localizedText?["ExtrasInstall"]?["systemChannelRestorer_Setup"]?["onWii"]}";
            string offlineWADs = main.patcherLang == main.PatcherLanguage.en
                ? "[bold]Offline WADs[/]"
                : $"{main.localizedText?["ExtrasInstall"]?["systemChannelRestorer_Setup"]?["offlineWADs"]}";
            AnsiConsole.MarkupLine($"[bold]1.[/] {useSystemChannelRestorer}");
            AnsiConsole.MarkupLine($"[bold]2.[/] {offlineWADs}\n");

            // Print instructions
            string platformInstructions = main.patcherLang == main.PatcherLanguage.en
                ? "< Press [bold white]a number[/] to make your selection, [bold white]Backspace[/] to go back, [bold white]ESC[/] to go back to exit setup >"
                : $"{main.localizedText?["CustomSetup"]?["systemChannelRestorer_Setup"]?["selectionInstructions"]}";
            AnsiConsole.MarkupLine($"[grey]{platformInstructions}[/]\n");

            int choice = UserChoose("12");

            // Use a switch statement to handle user's SPD version selection
            switch (choice)
            {
                case -1: // Escape
                case -2: // Backspace
                    MainMenu();
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
        main.task = "Install Extras (Part 2 - Select Extra channels)";

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
        if (main.combinedChannels_selection.Count == 0) // Only do this
            main.combinedChannels_selection = channelMap.Values.Select(_ => "[grey]Not selected[/]").ToList();

        if (systemChannelRestorer == true)
        {
            main.combinedChannels_selection = main.combinedChannels_selection.Append("scr").ToList();
            main.extraChannels_selection.Add("scr");
        }

        // Page setup
        const int ITEMS_PER_PAGE = 9;
        int currentPage = 1;

        while (true)
        {
            PrintHeader();

            // Print title
            string installExtras = main.patcherLang == main.PatcherLanguage.en
                ? "Install Extras"
                : $"{main.localizedText?["InstallExtras"]?["Header"]}";
            AnsiConsole.MarkupLine($"[bold springgreen2_1]{installExtras}[/]\n");

            // Print step number and title
            string stepNum = main.patcherLang == main.PatcherLanguage.en
                ? "Step 2"
                : $"{main.localizedText?["CustomSetup"]?["wiiLinkChannels_Setup"]?["stepNum"]}";
            string stepTitle = main.patcherLang == main.PatcherLanguage.en
                ? "Select extra channel(s) to install"
                : $"{main.localizedText?["installExtras"]?["extraChannels_Setup"]?["stepTitle"]}";
            AnsiConsole.MarkupLine($"[bold]{stepNum}:[/] {stepTitle}\n");

            // Display extra channel selection menu
            string selectExtraChns = main.patcherLang == main.PatcherLanguage.en
                ? "Select extra channel(s) to install:"
                : $"{main.localizedText?["CustomSetup"]?["extraChannels_Setup"]?["selectExtraChns"]}";
            AnsiConsole.MarkupLine($"[bold]{selectExtraChns}[/]\n");
            var grid = new Grid();

            // Add channels to grid
            grid.AddColumn();
            grid.AddColumn();

            // Calculate the start and end indices for the items on the current page
            (int start, int end) = GetPageIndices(currentPage, channelMap.Count, ITEMS_PER_PAGE);

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
            int choice = UserChoose(choices);

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
            string notSelected = main.patcherLang == main.PatcherLanguage.en
                ? "Not selected"
                : $"{main.localizedText?["CustomSetup"]?["notSelected"]}";
            string selectedText = main.patcherLang == main.PatcherLanguage.en
                ? "Selected"
                : $"{main.localizedText?["CustomSetup"]?["selected"]}";

            // Handle user input
            switch (choice)
            {
                case -1: // Escape
                    // Clear selection list
                    main.combinedChannels_selection.Clear();
                    main.extraChannels_selection.Clear();
                    channelMap.Clear();
                    MainMenu();
                    break;
                case -2: // Backspace
                    // Clear selection list
                    main.combinedChannels_selection.Clear();
                    main.extraChannels_selection.Clear();
                    channelMap.Clear();
                    systemChannelRestorer_Setup();
                    break;
                case 0: // Enter
                    // Save selected channels to global variable if any are selected, divide them into WiiLink and WC24 channels
                    foreach (string channel in channelMap.Values.Where(main.combinedChannels_selection.Contains))
                    {
                        if (extraChannelMap.ContainsValue(channel) && !main.extraChannels_selection.Contains(channel))
                            main.extraChannels_selection.Add(channel);
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
                    ExtraChannels_ConsolePlatform_Setup();
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

    // Install Extras (Part 3 - Select Console Platform)
    static void ExtraChannels_ConsolePlatform_Setup()
    {
        main.task = "Install Extras (Part 3 - Select Console Platform)";
        while (true)
        {
            PrintHeader();

            // Print title
            string installExtras = main.patcherLang == main.PatcherLanguage.en
                ? "Install Extras"
                : $"{main.localizedText?["InstallExtras"]?["Header"]}";
            AnsiConsole.MarkupLine($"[bold springgreen2_1]{installExtras}[/]\n");

            // Print step number and title
            string stepNum = main.patcherLang == main.PatcherLanguage.en
                ? "Step 3"
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

            int choice = UserChoose("123");

            // Use a switch statement to handle user's SPD version selection
            switch (choice)
            {
                case -1: // Escape
                    main.combinedChannels_selection.Clear();
                    main.extraChannels_selection.Clear();
                    MainMenu();
                    break;
                case -2: // Backspace
                    systemChannelRestorer_Setup();
                    break;
                case 1:
                    main.platformType_custom = main.Platform.Wii;
                    main.platformType = main.Platform.Wii;
                    ExtraChannels_SummaryScreen(showSPD: true);
                    break;
                case 2:
                    main.platformType_custom = main.Platform.vWii;
                    main.platformType = main.Platform.vWii;
                    ExtraChannels_SummaryScreen(showSPD: true);
                    break;
                case 3:
                    main.platformType_custom = main.Platform.Dolphin;
                    main.platformType = main.Platform.Dolphin;
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
        main.task = "Install Extras (Part 4 - Show summary of selected channels to be installed)";

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
        foreach (string channel in main.combinedChannels_selection)
        {
            if (extraChannelMap.TryGetValue(channel, out string? modifiedChannel))
                selectedExtraChannels.Add(modifiedChannel);
        }

        while (true)
        {
            PrintHeader();

            // Print title
            string installExtras = main.patcherLang == main.PatcherLanguage.en
                ? "Install Extras"
                : $"{main.localizedText?["InstallExtras"]?["Header"]}";
            string summaryHeader = main.patcherLang == main.PatcherLanguage.en
                ? "Summary of selected channels to be installed:"
                : $"{main.localizedText?["CustomSetup"]?["summaryScreen"]?["summaryHeader"]}";
            AnsiConsole.MarkupLine($"[bold springgreen2_1]{installExtras}[/]\n");
            AnsiConsole.MarkupLine($"[bold]{summaryHeader}[/]\n");

            // Display summary of selected channels in two columns using a grid
            var grid = new Grid();
            grid.AddColumn();
            grid.AddColumn();

            // Grid header text
            string extraChannels = main.patcherLang == main.PatcherLanguage.en
                ? "Extra Channels:"
                : $"{main.localizedText?["CustomSetup"]?["summaryScreen"]?["extraChannels"]}";
            string consoleVersion = main.patcherLang == main.PatcherLanguage.en
                ? "Console Platform:"
                : $"{main.localizedText?["CustomSetup"]?["summaryScreen"]?["ConsoleVersion"]}";

            grid.AddRow($"[bold springgreen2_1]{extraChannels}[/]", $"[bold]{consoleVersion}[/]");

            if (main.platformType_custom == main.Platform.Wii)
                grid.AddRow(string.Join("\n", selectedExtraChannels), "● [bold]Wii[/]");
            else if (main.platformType_custom == main.Platform.vWii)
                grid.AddRow(string.Join("\n", selectedExtraChannels), "● [bold]vWii (Wii U)[/]");
            else
                grid.AddRow(string.Join("\n", selectedExtraChannels), "● [bold]Dolphin Emulator[/]");

            AnsiConsole.Write(grid);

            // Print instructions
            string prompt = main.patcherLang == main.PatcherLanguage.en
                ? "Are you sure you want to install these selected channels?"
                : $"{main.localizedText?["CustomSetup"]?["summaryScreen"]?["confirmation"]?["prompt"]}";

            // User confirmation strings
            string yes = main.patcherLang == main.PatcherLanguage.en
                ? "Yes"
                : $"{main.localizedText?["CustomSetup"]?["summaryScreen"]?["confirmation"]?["yes"]}";
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

            var choice = UserChoose("123");

            // Handle user confirmation choice
            switch (choice)
            {
                case 1: // Yes
                    if (main.platformType_custom != main.Platform.Dolphin)
                    {
                        SDSetup(isCustomSetup: true);
                        break;
                    }
                    else
                    {
                        main.sdcard = null;
                        WADFolderCheck(true);
                        break;
                    }
                case 2: // No, start over
                    main.combinedChannels_selection.Clear();
                    main.extraChannels_selection.Clear();
                    systemChannelRestorer_Setup();
                    break;
                case 3: // No, go back to main menu
                    main.combinedChannels_selection.Clear();
                    main.extraChannels_selection.Clear();
                    MainMenu();
                    break;
                default:
                    break;
            }
        }
    }

    static void SettingsMenu()
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
                    LanguageMenu();
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

    // Language Menu function, supports English, Spanish, French, and Japanese
    static void LanguageMenu()
    {
        while (true)
        {
            PrintHeader();
            PrintNotice();

            // Dictionary for language codes and their names
            var languages = new Dictionary<main.PatcherLanguage, string>
            {
                {main.PatcherLanguage.en, "English"},
                // Add other language codes here
            };

            // Choose a Language text
            string chooseALanguage = main.patcherLang == main.PatcherLanguage.en
                ? "Choose a Language"
                : $"{main.localizedText?["LanguageSettings"]?["chooseALanguage"]}";
            AnsiConsole.MarkupLine($"[bold springgreen2_1]{chooseALanguage}[/]\n");

            // More languages coming soon text
            AnsiConsole.MarkupLine($"[bold springgreen2_1]More languages coming soon![/]\n");

            // Display languages
            StringBuilder choices = new();
            for (int i = 1; i <= languages.Count; i++)
            {
                AnsiConsole.MarkupLine($"[bold]{i}.[/] {languages.ElementAt(i - 1).Value}");
                choices.Append(i);
            }
            choices.Append(languages.Count + 1); // So user can go back to Settings Menu

            // Go back to Settings Menu text
            string goBack = main.patcherLang == main.PatcherLanguage.en
                ? "Go back to Settings Menu"
                : $"{main.localizedText?["LanguageSettings"]?["goBack"]}";
            AnsiConsole.MarkupLine($"\n[bold]{languages.Count + 1}.[/] {goBack}\n");

            int choice = UserChoose(choices.ToString());

            // Check if choice is valid
            if (choice < 1 || choice > languages.Count + 1)
                continue; // Restart LanguageMenu

            // Map choice to language code
            if (choice <= languages.Count)
            {
                var selectedLanguage = languages.ElementAt(choice - 1);
                var langCode = selectedLanguage.Key;

                // Set programLang to chosen language code
                main.patcherLang = langCode;

                // Since English is hardcoded, there's no language pack for it
                if (main.patcherLang == main.PatcherLanguage.en)
                {
                    SettingsMenu();
                    break;
                }

                // Download language pack
                main.DownloadLanguagePack(langCode.ToString());

                // Set localizedText to use the language pack
                main.localizedText = JObject.Parse(File.ReadAllText(Path.Join(main.tempDir, "LanguagePack", $"LocalizedText.{langCode}.json")));

                SettingsMenu();
            }
            else if (choice == languages.Count + 1)
            {
                SettingsMenu();
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