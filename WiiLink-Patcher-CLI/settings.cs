using Spectre.Console;

public class settings
{
    public static void SettingsMenu()
    {
        while (true)
        {
            menu.PrintHeader();
            menu.PrintNotice();

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

            int choice = menu.UserChoose("123");
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
                    menu.MainMenu();
                    break;
                case 3 when !main.inCompatabilityMode:
                    menu.MainMenu();
                    break;
                default:
                    break;
            }
        }
    }

    // Credits function
    public static void CreditsScreen()
    {
        menu.PrintHeader();

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
        creditGrid.AddRow(new Text("Isla", new Style(Color.SpringGreen2_1, null, Decoration.Bold)).RightJustified(), new Text(harryDesc, new Style(null, null, Decoration.Bold)));
        creditGrid.AddRow(new Text("Luna", new Style(Color.SpringGreen2_1, null, Decoration.Bold)).RightJustified(), new Text(lunaDesc, new Style(null, null, Decoration.Bold)));

        if (main.patcherLang != main.PatcherLanguage.en)
            creditGrid.AddRow(new Text($"{main.localizedText?["Credits"]?["translatorName"]}", new Style(Color.SpringGreen2_1, null, Decoration.Bold)).RightJustified(), new Text($"{main.localizedText?["Credits"]?["translatorDesc"]}", new Style(null, null, Decoration.Bold)));

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


}