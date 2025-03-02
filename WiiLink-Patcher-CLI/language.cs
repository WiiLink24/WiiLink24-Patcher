using Spectre.Console;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

public class LanguageClass
{
    // Automatically set language function, uses system locale and prompts user to ensure correct detection
    public static void AutoSetLang()
    {
            int langMatches = 0;
            string detectedLang = "";
            string detectedLangName = "";

            // Dictionary for language codes and their names
            Dictionary<string, string> languages = JsonConvert.DeserializeObject<Dictionary<string, string>>(MainClass.languageList) ?? throw new InvalidOperationException();

            MenuClass.PrintHeader();
            MenuClass.PrintNotice();

            if (languages.ContainsKey(MainClass.sysLang))
            {
                langMatches = 1;
                detectedLang = MainClass.sysLang;
                detectedLangName = languages[MainClass.sysLang];
            }
            else
            {
                foreach (string langCode in languages.Keys)
                {
                    if (langCode.StartsWith(MainClass.shortLang))
                    {
                        langMatches += 1;
                        detectedLang = langCode;
                        detectedLangName = languages[langCode];
                    }
                }
            }

            if (langMatches == 1)
            {
                AnsiConsole.MarkupLine($"[bold springgreen2_1]Detected Language:[/] {detectedLangName}\n");
                AnsiConsole.MarkupLine("Press [bold]C[/] to change language, or any other key to continue with this language.\n");

                AnsiConsole.Markup("Choose: ");
                
                ConsoleKeyInfo keyPressed = Console.ReadKey(intercept: true);
                if (keyPressed.Key == ConsoleKey.C)
                {
                    LanguageMenu(true);
                }
                else
                {
                    if (detectedLang != "en-US")
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
        // Page setup
        const int ITEMS_PER_PAGE = 9;
        int currentPage = 1;

        while (true)
        {
            MenuClass.PrintHeader();
            MenuClass.PrintNotice();

            // Dictionary for language codes and their names
            Dictionary<string, string> languages = JsonConvert.DeserializeObject<Dictionary<string, string>>(MainClass.languageList) ?? throw new InvalidOperationException();

            // Choose a Language text
            string chooseALanguage = MainClass.patcherLang == "en-US"
                ? "Choose a Language"
                : $"{MainClass.localizedText?["LanguageSettings"]?["chooseALanguage"]}";
            AnsiConsole.MarkupLine($"[bold springgreen2_1]{chooseALanguage}[/]\n");

            var grid = new Grid();

            grid.AddColumn();

            // Calculate the start and end indices for the items on the current page
            (int start, int end) = MenuClass.GetPageIndices(currentPage, languages.Count, ITEMS_PER_PAGE);

            // Display list of channels
            for (int i = start; i < end; i++)
            {
                KeyValuePair<string, string> language = languages.ElementAt(i);
                grid.AddRow($"[bold][[{i - start + 1}]][/] {language.Value}");

                // Add blank rows if there are less than nine pages
                if (i == end - 1 && end - start < 9)
                {
                    int numBlankRows = 9 - (end - start);
                    for (int j = 0; j < numBlankRows; j++)
                    {
                        grid.AddRow("");
                    }
                }
            }

            AnsiConsole.Write(grid);
            Console.WriteLine();

            // Page navigation
            double totalPages = Math.Ceiling((double)languages.Count / ITEMS_PER_PAGE);

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
                AnsiConsole.MarkupLine($" [grey](Press [bold white]<-[/] or [bold white]->[/] to navigate pages)[/]\n");
            }

            // Print regular instructions
            string regInstructions = "";
            if (startup)
                regInstructions = "< Press [bold white]a number[/] to select your language >";
            else
            {
                regInstructions = "< Press [bold white]a number[/] to select your language or [bold white]ESC[/] to return to settings >";
            }
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
            
            if (choice == -1 && !startup) // Escape
            {
                break;
            }

            // Map choice to language code
            if (choice >= 1 && choice <= Math.Min(ITEMS_PER_PAGE, languages.Count - start))
            {
                var selectedLanguage = languages.ElementAt(start + choice - 1);
                var langCode = selectedLanguage.Key;

                // Since English is hardcoded, there's no language pack for it
                if (langCode == "en-US")
                {
                    MainClass.patcherLang = "en-US";
                    break;
                }

                // Download language pack
                DownloadLanguagePack(langCode);

                break;
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

        if (File.Exists(languageFilePath))
        {
            // Set programLang to chosen language code
            MainClass.patcherLang = languageCode;


            // Set localizedText to use the language pack
            MainClass.localizedText = JObject.Parse(File.ReadAllText(Path.Join(MainClass.tempDir, "LanguagePack", $"LocalizedText.{languageCode}.json")));
        }
    }

    // Download list of languages from a server, allows new languages to be introduced without updating patcher
    public static string DownloadLanguageList()
    {
        string URL = $"{MainClass.wiiLinkPatcherUrl}/lang/LanguageDictionary.json";

        try
        {
            // Send a GET request to the specified URL.
            var response = MainClass.httpClient.GetAsync(URL).Result;
            if (response.IsSuccessStatusCode)
            {
                // If the response is successful, return the language list.
                return MainClass.httpClient.GetStringAsync($"{URL}").Result;
            }
            else
            {
                // If the response is not successful, display an error message.
                int statusCode = (int)response.StatusCode;
                AnsiConsole.MarkupLine($"[bold red]ERROR:[/] Failed to download [bold]language list[/] from [bold]{URL}[/]: Status code {statusCode}");
                AnsiConsole.MarkupLine("Press any key to try again...");
                Console.ReadKey(true);
                return DownloadLanguageList();
            }
        }
        // Timeout exception
        catch (TaskCanceledException)
        {
            AnsiConsole.MarkupLine($"[bold red]ERROR:[/] Failed to download [bold]language list[/] from [bold]{URL}[/]: Request timed out (1 minute)");
            AnsiConsole.MarkupLine("Press any key to try again...");
            Console.ReadKey(true);
            return DownloadLanguageList();
        }
        catch (HttpRequestException e)
        {
            AnsiConsole.MarkupLine($"[bold red]ERROR:[/] {e.Message}");
            AnsiConsole.MarkupLine("Press any key to try again...");
            Console.ReadKey(true);
            return DownloadLanguageList();
        }
    }
}
