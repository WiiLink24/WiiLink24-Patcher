using Spectre.Console;

public class SettingsClass
{
    public static void SettingsMenu()
    {
        while (true)
        {
            MenuClass.PrintHeader();
            MenuClass.PrintNotice();

            // Settings text
            string settings = MainClass.patcherLang == MainClass.PatcherLanguage.en
                ? "Settings"
                : $"{MainClass.localizedText?["SettingsMenu"]?["settings"]}";
            AnsiConsole.MarkupLine($"[bold springgreen2_1]{settings}[/]\n");

            if (!MainClass.inCompatabilityMode)
            {
                // User choices
                string changeLanguage = MainClass.patcherLang == MainClass.PatcherLanguage.en
                    ? "Change Language"
                    : $"{MainClass.localizedText?["SettingsMenu"]?["changeLanguage"]}";
                string credits = MainClass.patcherLang == MainClass.PatcherLanguage.en
                    ? "Credits"
                    : $"{MainClass.localizedText?["SettingsMenu"]?["credits"]}";
                string goBack = MainClass.patcherLang == MainClass.PatcherLanguage.en
                    ? "Go back to Main Menu"
                    : $"{MainClass.localizedText?["SettingsMenu"]?["goBack"]}";

                AnsiConsole.MarkupLine($"1. {changeLanguage}");
                AnsiConsole.MarkupLine($"2. {credits}\n");

                AnsiConsole.MarkupLine($"3. {goBack}\n");
            }
            else
            {
                Console.WriteLine("1. Credits\n");

                Console.WriteLine("2. Go back to Main Menu\n");
            }

            int choice = MenuClass.UserChoose("123");
            switch (choice)
            {
                case 1 when !MainClass.inCompatabilityMode:
                    LanguageClass.LanguageMenu(false);
                    break;
                case 1 when MainClass.inCompatabilityMode:
                    CreditsScreen();
                    break;
                case 2 when !MainClass.inCompatabilityMode:
                    CreditsScreen();
                    break;
                case 2 when MainClass.inCompatabilityMode:
                    MenuClass.MainMenu();
                    break;
                case 3 when !MainClass.inCompatabilityMode:
                    MenuClass.MainMenu();
                    break;
                default:
                    break;
            }
        }
    }

    // Credits function
    public static void CreditsScreen()
    {
        MenuClass.PrintHeader();

        // Build info
        string buildInfo = MainClass.patcherLang == MainClass.PatcherLanguage.en
            ? $"This build was compiled on [bold springgreen2_1]{MainClass.buildDate}[/] at [bold springgreen2_1]{MainClass.buildTime}[/]."
            : $"{MainClass.localizedText?["Credits"]?["buildInfo"]}"
                .Replace("{MainClass.buildDate}", MainClass.buildDate)
                .Replace("{MainClass.buildTime}", MainClass.buildTime);
        AnsiConsole.MarkupLine($"{buildInfo}\n");

        // Credits table
        var creditTable = new Table().Border(MainClass.inCompatabilityMode ? TableBorder.None : TableBorder.DoubleEdge);

        // Credits header
        string credits = MainClass.patcherLang == MainClass.PatcherLanguage.en
            ? "Credits"
            : $"{MainClass.localizedText?["Credits"]?["credits"]}";
        creditTable.AddColumn(new TableColumn($"[bold springgreen2_1]{credits}[/]").Centered());

        // Credits grid
        var creditGrid = new Grid().AddColumn().AddColumn();

        // Credits text
        string sketchDesc = MainClass.patcherLang == MainClass.PatcherLanguage.en
            ? "WiiLink Lead"
            : $"{MainClass.localizedText?["Credits"]?["sketchDesc"]}";
        string pablosDesc = MainClass.patcherLang == MainClass.PatcherLanguage.en
            ? "WiiLink Patcher Developer"
            : $"{MainClass.localizedText?["Credits"]?["pablosDesc"]}";
        string harryDesc = MainClass.patcherLang == MainClass.PatcherLanguage.en
            ? "WiiLink Patcher Developer"
            : $"{MainClass.localizedText?["Credits"]?["harryDesc"]}";
        string lunaDesc = MainClass.patcherLang == MainClass.PatcherLanguage.en
            ? "Lead Translator"
            : $"{MainClass.localizedText?["Credits"]?["lunaDesc"]}";
        string leathlWiiDatabase = MainClass.patcherLang == MainClass.PatcherLanguage.en
            ? "leathl and WiiDatabase"
            : $"{MainClass.localizedText?["Credits"]?["leathlWiiDatabase"]}";
        string leathlWiiDatabaseDesc = MainClass.patcherLang == MainClass.PatcherLanguage.en
            ? "libWiiSharp developers"
            : $"{MainClass.localizedText?["Credits"]?["leathlWiiDatabaseDesc"]}";

        creditGrid.AddRow(new Text("Sketch", new Style(Color.SpringGreen2_1, null, Decoration.Bold)).RightJustified(), new Text(sketchDesc, new Style(null, null, Decoration.Bold)));
        creditGrid.AddRow(new Text("PablosCorner", new Style(Color.SpringGreen2_1, null, Decoration.Bold)).RightJustified(), new Text(pablosDesc, new Style(null, null, Decoration.Bold)));
        creditGrid.AddRow(new Text("Isla", new Style(Color.SpringGreen2_1, null, Decoration.Bold)).RightJustified(), new Text(harryDesc, new Style(null, null, Decoration.Bold)));
        creditGrid.AddRow(new Text("Luna", new Style(Color.SpringGreen2_1, null, Decoration.Bold)).RightJustified(), new Text(lunaDesc, new Style(null, null, Decoration.Bold)));

        if (MainClass.patcherLang != MainClass.PatcherLanguage.en)
            creditGrid.AddRow(new Text($"{MainClass.localizedText?["Credits"]?["translatorName"]}", new Style(Color.SpringGreen2_1, null, Decoration.Bold)).RightJustified(), new Text($"{MainClass.localizedText?["Credits"]?["translatorDesc"]}", new Style(null, null, Decoration.Bold)));

        creditGrid.AddRow(new Text(leathlWiiDatabase, new Style(Color.SpringGreen2_1, null, Decoration.Bold)).RightJustified(), new Text(leathlWiiDatabaseDesc, new Style(null, null, Decoration.Bold)));
        creditGrid.AddRow(new Text("SnowflakePowered", new Style(Color.SpringGreen2_1, null, Decoration.Bold)).RightJustified(), new Text("VCDiff", new Style(null, null, Decoration.Bold)));

        // Add the grid to the table
        creditTable.AddRow(creditGrid).Centered();
        AnsiConsole.Write(creditTable);

        // Special thanks grid
        string specialThanksTo = MainClass.patcherLang == MainClass.PatcherLanguage.en
            ? "Special thanks to:"
            : $"{MainClass.localizedText?["Credits"]?["specialThanksTo"]}";
        AnsiConsole.MarkupLine($"\n[bold springgreen2_1]{specialThanksTo}[/]\n");

        var specialThanksGrid = new Grid().AddColumn().AddColumn();

        // Special thanks text
        string theshadoweeveeRole = MainClass.patcherLang == MainClass.PatcherLanguage.en
            ? "- Pointing me in the right direction with implementing libWiiSharp!"
            : $"{MainClass.localizedText?["Credits"]?["theshadoweeveeRole"]}";
        string ourTesters = MainClass.patcherLang == MainClass.PatcherLanguage.en
            ? "Our Testers"
            : $"{MainClass.localizedText?["Credits"]?["ourTesters"]}";
        string ourTestersRole = MainClass.patcherLang == MainClass.PatcherLanguage.en
            ? "- For testing the patcher and reporting bugs/anomalies!"
            : $"{MainClass.localizedText?["Credits"]?["ourTestersRole"]}";
        string you = MainClass.patcherLang == MainClass.PatcherLanguage.en
            ? "You!"
            : $"{MainClass.localizedText?["Credits"]?["you"]}";
        string youRole = MainClass.patcherLang == MainClass.PatcherLanguage.en
            ? "- For your continued support of WiiLink!"
            : $"{MainClass.localizedText?["Credits"]?["youRole"]}";

        specialThanksGrid.AddRow($"  ● [bold springgreen2_1]TheShadowEevee[/]", theshadoweeveeRole);
        specialThanksGrid.AddRow($"  ● [bold springgreen2_1]{ourTesters}[/]", ourTestersRole);
        specialThanksGrid.AddRow($"  ● [bold springgreen2_1]{you}[/]", youRole);

        AnsiConsole.Write(specialThanksGrid);
        AnsiConsole.MarkupLine("");

        // Links grid
        string wiilinkSite = MainClass.patcherLang == MainClass.PatcherLanguage.en
            ? "WiiLink website"
            : $"{MainClass.localizedText?["Credits"]?["wiilinkSite"]}";
        string githubRepo = MainClass.patcherLang == MainClass.PatcherLanguage.en
            ? "GitHub repository"
            : $"{MainClass.localizedText?["Credits"]?["githubRepo"]}";

        var linksGrid = new Grid().AddColumn().AddColumn();

        linksGrid.AddRow($"[bold springgreen2_1]{wiilinkSite}[/]:", "[link]https://wiilink.ca[/]");
        linksGrid.AddRow($"[bold springgreen2_1]{githubRepo}[/]:", "[link]https://github.com/WiiLink24/WiiLink24-Patcher[/]");

        AnsiConsole.Write(linksGrid);
        AnsiConsole.MarkupLine("");

        // Press any key to go back to settings
        string pressAnyKey = MainClass.patcherLang == MainClass.PatcherLanguage.en
            ? "Press any key to go back to settings..."
            : $"{MainClass.localizedText?["Credits"]?["pressAnyKey"]}";
        AnsiConsole.Markup($"[bold]{pressAnyKey}[/]");
        Console.ReadKey();
    }


}