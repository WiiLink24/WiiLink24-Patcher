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
            string settings = MainClass.patcherLang == "en-US"
                ? "Settings"
                : $"{MainClass.localizedText?["SettingsMenu"]?["settings"]}";
            AnsiConsole.MarkupLine($"[bold springgreen2_1]{settings}[/]\n");

            if (!MainClass.inCompatabilityMode)
            {
                // User choices
                string changeLanguage = MainClass.patcherLang == "en-US"
                    ? "Change Language"
                    : $"{MainClass.localizedText?["SettingsMenu"]?["changeLanguage"]}";
                string credits = MainClass.patcherLang == "en-US"
                    ? "Credits"
                    : $"{MainClass.localizedText?["SettingsMenu"]?["credits"]}";
                string goBack = MainClass.patcherLang == "en-US"
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
        string buildInfo = MainClass.patcherLang == "en-US"
            ? $"This build was compiled on [bold springgreen2_1]{MainClass.buildDate}[/] at [bold springgreen2_1]{MainClass.buildTime}[/]."
            : $"{MainClass.localizedText?["Credits"]?["buildInfo"]}"
                .Replace("{main.buildDate}", MainClass.buildDate)
                .Replace("{main.buildTime}", MainClass.buildTime);
        AnsiConsole.MarkupLine($"{buildInfo}\n");

        // Credits table
        var creditTable = new Table().Border(MainClass.inCompatabilityMode ? TableBorder.None : TableBorder.DoubleEdge);

        // Credits header
        string credits = MainClass.patcherLang == "en-US"
            ? "Credits"
            : $"{MainClass.localizedText?["Credits"]?["credits"]}";
        creditTable.AddColumn(new TableColumn($"[bold springgreen2_1]{credits}[/]").Centered());

        // Credits grid
        var creditGrid = new Grid().AddColumn().AddColumn();

        // Credits text
        string sketchDesc = MainClass.patcherLang == "en-US"
            ? "WiiLink Lead"
            : $"{MainClass.localizedText?["Credits"]?["sketchDesc"]}";
        string pablosDesc = MainClass.patcherLang == "en-US"
            ? "WiiLink Patcher Developer"
            : $"{MainClass.localizedText?["Credits"]?["pablosDesc"]}";
        string harryDesc = MainClass.patcherLang == "en-US"
            ? "WiiLink Patcher Developer"
            : $"{MainClass.localizedText?["Credits"]?["harryDesc"]}";
        string lunaDesc = MainClass.patcherLang == "en-US"
            ? "Lead Translator"
            : $"{MainClass.localizedText?["Credits"]?["lunaDesc"]}";
        string leathlWiiDatabase = MainClass.patcherLang == "en-US"
            ? "leathl and WiiDatabase"
            : $"{MainClass.localizedText?["Credits"]?["leathlWiiDatabase"]}";
        string leathlWiiDatabaseDesc = MainClass.patcherLang == "en-US"
            ? "libWiiSharp developers"
            : $"{MainClass.localizedText?["Credits"]?["leathlWiiDatabaseDesc"]}";

        creditGrid.AddRow(new Text("Sketch", new Style(Color.SpringGreen2_1, null, Decoration.Bold)).RightJustified(), new Text(sketchDesc, new Style(null, null, Decoration.Bold)));
        creditGrid.AddRow(new Text("PablosCorner", new Style(Color.SpringGreen2_1, null, Decoration.Bold)).RightJustified(), new Text(pablosDesc, new Style(null, null, Decoration.Bold)));
        creditGrid.AddRow(new Text("Harry", new Style(Color.SpringGreen2_1, null, Decoration.Bold)).RightJustified(), new Text(harryDesc, new Style(null, null, Decoration.Bold)));
        creditGrid.AddRow(new Text("Luna", new Style(Color.SpringGreen2_1, null, Decoration.Bold)).RightJustified(), new Text(lunaDesc, new Style(null, null, Decoration.Bold)));

        if (MainClass.patcherLang != "en-US")
            creditGrid.AddRow(new Text($"{MainClass.localizedText?["Credits"]?["translatorName"]}", new Style(Color.SpringGreen2_1, null, Decoration.Bold)).RightJustified(), new Text($"{MainClass.localizedText?["Credits"]?["translatorDesc"]}", new Style(null, null, Decoration.Bold)));

        creditGrid.AddRow(new Text(leathlWiiDatabase, new Style(Color.SpringGreen2_1, null, Decoration.Bold)).RightJustified(), new Text(leathlWiiDatabaseDesc, new Style(null, null, Decoration.Bold)));
        creditGrid.AddRow(new Text("SnowflakePowered", new Style(Color.SpringGreen2_1, null, Decoration.Bold)).RightJustified(), new Text("VCDiff", new Style(null, null, Decoration.Bold)));

        // Add the grid to the table
        creditTable.AddRow(creditGrid).Centered();
        AnsiConsole.Write(creditTable);

        // Special thanks grid
        string specialThanksTo = MainClass.patcherLang == "en-US"
            ? "Special thanks to:"
            : $"{MainClass.localizedText?["Credits"]?["specialThanksTo"]}";
        AnsiConsole.MarkupLine($"\n[bold springgreen2_1]{specialThanksTo}[/]\n");

        var specialThanksGrid = new Grid().AddColumn().AddColumn();

        // Special thanks text
        string theshadoweeveeRole = MainClass.patcherLang == "en-US"
            ? "- Pointing me in the right direction with implementing libWiiSharp!"
            : $"{MainClass.localizedText?["Credits"]?["theshadoweeveeRole"]}";
        string ourTesters = MainClass.patcherLang == "en-US"
            ? "Our Testers"
            : $"{MainClass.localizedText?["Credits"]?["ourTesters"]}";
        string ourTestersRole = MainClass.patcherLang == "en-US"
            ? "- For testing the patcher and reporting bugs/anomalies!"
            : $"{MainClass.localizedText?["Credits"]?["ourTestersRole"]}";
        string you = MainClass.patcherLang == "en-US"
            ? "You!"
            : $"{MainClass.localizedText?["Credits"]?["you"]}";
        string youRole = MainClass.patcherLang == "en-US"
            ? "- For your continued support of WiiLink!"
            : $"{MainClass.localizedText?["Credits"]?["youRole"]}";

        specialThanksGrid.AddRow($"  ● [bold springgreen2_1]TheShadowEevee[/]", theshadoweeveeRole);
        specialThanksGrid.AddRow($"  ● [bold springgreen2_1]{ourTesters}[/]", ourTestersRole);
        specialThanksGrid.AddRow($"  ● [bold springgreen2_1]{you}[/]", youRole);

        AnsiConsole.Write(specialThanksGrid);
        AnsiConsole.MarkupLine("");

        // Links grid
        string wiilinkSite = MainClass.patcherLang == "en-US"
            ? "WiiLink website"
            : $"{MainClass.localizedText?["Credits"]?["wiilinkSite"]}";
        string githubRepo = MainClass.patcherLang == "en-US"
            ? "GitHub repository"
            : $"{MainClass.localizedText?["Credits"]?["githubRepo"]}";

        var linksGrid = new Grid().AddColumn().AddColumn();

        linksGrid.AddRow($"[bold springgreen2_1]{wiilinkSite}[/]:", "[link]https://wiilink.ca[/]");
        linksGrid.AddRow($"[bold springgreen2_1]{githubRepo}[/]:", "[link]https://github.com/WiiLink24/WiiLink24-Patcher[/]");

        AnsiConsole.Write(linksGrid);
        AnsiConsole.MarkupLine("");

        // Press any key to go back to settings
        string pressAnyKey = MainClass.patcherLang == "en-US"
            ? "Press any key to go back to settings..."
            : $"{MainClass.localizedText?["Credits"]?["pressAnyKey"]}";
        AnsiConsole.Markup($"[bold]{pressAnyKey}[/]");
        Console.ReadKey();
    }


}