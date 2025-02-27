using System.Text;
using Spectre.Console;
using Newtonsoft.Json.Linq;

public class LanguageClass
{
    // Automatically set language function, uses system locale and prompts user to ensure correct detection
    public static void AutoSetLang()
    {
            // Dictionary for language codes and their names
            var languages = new Dictionary<string, string>
            {
                {"en", "English"},
                {"it", "Italiano"},
                {"nl", "Dutch"},
                {"hu", "Magyar"}
                // Add other language codes here
            };

            if (languages.ContainsKey(MainClass.sysLang))
            {
                string detectedLang = languages[MainClass.sysLang];

                MenuClass.PrintHeader();
                MenuClass.PrintNotice();

                AnsiConsole.MarkupLine($"[bold springgreen2_1]Detected Language:[/] {detectedLang}\n");
                AnsiConsole.MarkupLine("Press [bold]C[/] to change language, or any other key to continue with this language.\n");

                string chooseText = MainClass.patcherLang == MainClass.PatcherLanguage.en
                    ? "Choose: "
                    : $"{MainClass.localizedText?["UserChoose"]} ";
                AnsiConsole.Markup(chooseText);
                
                ConsoleKeyInfo keyPressed = Console.ReadKey(intercept: true);
                if (keyPressed.Key == ConsoleKey.C)
                {
                    LanguageMenu(true);
                }
                else
                {
                    if (MainClass.sysLang != "en")
                        DownloadLanguagePack(MainClass.sysLang);
                }
            }
            else
            {
                LanguageMenu(true);
            }
    }

    // Language Menu function, supports English, Spanish, French, and Japanese
    public static void LanguageMenu(bool startup)
    {
        while (true)
        {
            MenuClass.PrintHeader();
            MenuClass.PrintNotice();

            // Dictionary for language codes and their names
            var languages = new Dictionary<MainClass.PatcherLanguage, string>
            {
                {MainClass.PatcherLanguage.en, "English"},
                {MainClass.PatcherLanguage.it, "Italiano"},
                {MainClass.PatcherLanguage.nl, "Dutch"},
                {MainClass.PatcherLanguage.hu, "Magyar"}
                // Add other language codes here
            };

            // Choose a Language text
            string chooseALanguage = MainClass.patcherLang == MainClass.PatcherLanguage.en
                ? "Choose a Language"
                : $"{MainClass.localizedText?["LanguageSettings"]?["chooseALanguage"]}";
            AnsiConsole.MarkupLine($"[bold springgreen2_1]{chooseALanguage}[/]\n");

            // Display languages
            StringBuilder choices = new();
            for (int i = 1; i <= languages.Count; i++)
            {
                AnsiConsole.MarkupLine($"[bold]{i}.[/] {languages.ElementAt(i - 1).Value}");
                choices.Append(i);
            }

            AnsiConsole.Markup("\n");

            if (!startup)
            {
                choices.Append(languages.Count + 1); // So user can go back to Settings Menu

                // Go back to Settings Menu text
                string goBack = MainClass.patcherLang == MainClass.PatcherLanguage.en
                    ? "Go back to Settings Menu"
                    : $"{MainClass.localizedText?["LanguageSettings"]?["goBack"]}";
                AnsiConsole.MarkupLine($"[bold]{languages.Count + 1}.[/] {goBack}\n");
            }

            int choice = MenuClass.UserChoose(choices.ToString());

            // Check if choice is valid
            if (choice < 1 || choice > languages.Count + 1)
                continue; // Restart LanguageMenu

            // Map choice to language code
            if (choice <= languages.Count)
            {
                var selectedLanguage = languages.ElementAt(choice - 1);
                var langCode = selectedLanguage.Key;

                // Set programLang to chosen language code
                MainClass.patcherLang = langCode;

                // Since English is hardcoded, there's no language pack for it
                if (MainClass.patcherLang == MainClass.PatcherLanguage.en)
                {
                    if (startup)
                    {
                        break;
                    }
                    else
                    {
                        SettingsClass.SettingsMenu();
                        break;
                    }
                }

                // Download language pack
                DownloadLanguagePack(langCode.ToString());

                // Set localizedText to use the language pack
                MainClass.localizedText = JObject.Parse(File.ReadAllText(Path.Join(MainClass.tempDir, "LanguagePack", $"LocalizedText.{langCode}.json")));

                if (startup)
                    break;
                else
                    SettingsClass.SettingsMenu();
            }
            else if (choice == languages.Count + 1 && !startup)
            {
                SettingsClass.SettingsMenu();
            }
        }
    }

    public static void DownloadLanguagePack(string languageCode)
    {
        string URL = $"{MainClass.wiiLinkPatcherUrl}/lang";

        AnsiConsole.MarkupLine($"\n[bold springgreen2_1]Checking for Language Pack updates ({languageCode})[/]");

        // Create LanguagePack folder if it doesn't exist
        var languagePackDir = Path.Join(MainClass.tempDir, "LanguagePack");
        if (!Directory.Exists(languagePackDir))
            Directory.CreateDirectory(languagePackDir);

        // Prepare language file paths
        var languageFileUrl = $"{URL}/LocalizedText.{languageCode}.json";
        var languageFilePath = Path.Join(languagePackDir, $"LocalizedText.{languageCode}.json");

        bool shouldDownload = false;

        // Check if local file exists
        if (File.Exists(languageFilePath))
        {
            // Get last modified date of local file
            DateTime localFileModifiedDate = File.GetLastWriteTime(languageFilePath);

            // Get last modified date of server file
            HttpRequestMessage request = new(HttpMethod.Head, languageFileUrl);
            HttpResponseMessage response = MainClass.httpClient.Send(request);
            DateTime serverFileModifiedDate = response.Content.Headers.LastModified.GetValueOrDefault().DateTime;

            // If server file is newer, download it
            if (serverFileModifiedDate > localFileModifiedDate)
            {
                shouldDownload = true;
            }
        }
        else
        {
            // If local file doesn't exist, download it
            shouldDownload = true;
        }

        if (shouldDownload)
        {
            AnsiConsole.MarkupLine($"[bold springgreen2_1]Downloading Language Pack ({languageCode})[/]");
            PatchClass.DownloadFile(languageFileUrl, languageFilePath, $"Language Pack ({languageCode})");
        }
        else
        {
            AnsiConsole.MarkupLine($"[bold springgreen2_1]Language Pack ({languageCode}) is up to date[/]");
            Thread.Sleep(500);
        }
    }
}