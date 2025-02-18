using System.Text;
using Spectre.Console;
using Newtonsoft.Json.Linq;

public class language
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

            if (languages.ContainsKey(main.sysLang))
            {
                string detectedLang = languages[main.sysLang];

                menu.PrintHeader();
                menu.PrintNotice();

                AnsiConsole.MarkupLine($"[bold springgreen2_1]Detected Language:[/] {detectedLang}\n");
                AnsiConsole.MarkupLine("Press [bold]C[/] to change language, or any other key to continue with this language.\n");

                string chooseText = main.patcherLang == main.PatcherLanguage.en
                    ? "Choose: "
                    : $"{main.localizedText?["UserChoose"]} ";
                AnsiConsole.Markup(chooseText);
                
                ConsoleKeyInfo keyPressed = Console.ReadKey(intercept: true);
                if (keyPressed.Key == ConsoleKey.C)
                {
                    LanguageMenu(true);
                }
                else
                {
                    if (main.sysLang != "en")
                        DownloadLanguagePack(main.sysLang);
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
            menu.PrintHeader();
            menu.PrintNotice();

            // Dictionary for language codes and their names
            var languages = new Dictionary<main.PatcherLanguage, string>
            {
                {main.PatcherLanguage.en, "English"},
                {main.PatcherLanguage.it, "Italiano"},
                {main.PatcherLanguage.nl, "Dutch"},
                {main.PatcherLanguage.hu, "Magyar"}
                // Add other language codes here
            };

            // Choose a Language text
            string chooseALanguage = main.patcherLang == main.PatcherLanguage.en
                ? "Choose a Language"
                : $"{main.localizedText?["LanguageSettings"]?["chooseALanguage"]}";
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
                string goBack = main.patcherLang == main.PatcherLanguage.en
                    ? "Go back to Settings Menu"
                    : $"{main.localizedText?["LanguageSettings"]?["goBack"]}";
                AnsiConsole.MarkupLine($"[bold]{languages.Count + 1}.[/] {goBack}\n");
            }

            int choice = menu.UserChoose(choices.ToString());

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
                    if (startup)
                    {
                        break;
                    }
                    else
                    {
                        settings.SettingsMenu();
                        break;
                    }
                }

                // Download language pack
                DownloadLanguagePack(langCode.ToString());

                // Set localizedText to use the language pack
                main.localizedText = JObject.Parse(File.ReadAllText(Path.Join(main.tempDir, "LanguagePack", $"LocalizedText.{langCode}.json")));

                if (startup)
                    break;
                else
                    settings.SettingsMenu();
            }
            else if (choice == languages.Count + 1 && !startup)
            {
                settings.SettingsMenu();
            }
        }
    }

    public static void DownloadLanguagePack(string languageCode)
    {
        string URL = $"{main.wiiLinkPatcherUrl}/lang";

        AnsiConsole.MarkupLine($"\n[bold springgreen2_1]Checking for Language Pack updates ({languageCode})[/]");

        // Create LanguagePack folder if it doesn't exist
        var languagePackDir = Path.Join(main.tempDir, "LanguagePack");
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
            HttpResponseMessage response = main.httpClient.Send(request);
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
            patch.DownloadFile(languageFileUrl, languageFilePath, $"Language Pack ({languageCode})");
        }
        else
        {
            AnsiConsole.MarkupLine($"[bold springgreen2_1]Language Pack ({languageCode}) is up to date[/]");
            Thread.Sleep(500);
        }
    }
}