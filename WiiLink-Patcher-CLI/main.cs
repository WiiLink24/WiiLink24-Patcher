using System.Text;
using System.Runtime.InteropServices;
using Spectre.Console;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Globalization;

// Author: PablosCorner and WiiLink Team
// Project: WiiLink Patcher (CLI Version)
// Description: WiiLink Patcher (CLI Version) is a command-line interface to patch and revive the exclusive Japanese Wii Channels that were shut down, along with the international WiiConnect24 Channels.

public class MainClass
{
    //// Build Info ////
    public static readonly string version = "v3.0.0 RC1";
    public static readonly string copyrightYear = DateTime.Now.Year.ToString();
    public static readonly DateTime buildDateTime = new DateTime(2025, 3, 26, 15, 30, 00); // Year, Month, Day, Hour, Minute, Second
    public static readonly string buildDate = buildDateTime.ToLongDateString();
    public static readonly string buildTime = buildDateTime.ToShortTimeString();
    public static string? sdcard = SdClass.DetectRemovableDrive;
    public static readonly string wiiLinkPatcherUrl = "https://patcher.wiilink24.com";
    ////////////////////

    //// Setup Info ////
    // Express Install variables
    public static Language lang;
    public static DemaeVersion demaeVersion;
    public static Language wiiRoomLang;
    public static bool installRegionalChannels = false;
    public static Region wc24_reg;
    public static Platform platformType;
    public static Dictionary<string, string> patchingProgress_express = [];

    // Custom Install variables
    public static List<string> wiiLinkChannels_selection = [];
    public static List<string> wiiConnect24Channels_selection = [];
    public static List<string> extraChannels_selection = [];

    public static List<string> combinedChannels_selection = [];
    public static Platform platformType_custom;
    public static bool inCompatabilityMode = false;
    public static Dictionary<string, string> patchingProgress_custom = [];

    // Misc. variables
    public static string task = "";
    public static string curCmd = "";
    public static readonly string curDir = Directory.GetCurrentDirectory();
    public static readonly string tempDir = Path.Join(Path.GetTempPath(), "WiiLink_Patcher");
    public static bool DEBUG_MODE = false;
    public static string patcherLang = "en-US";
    public static string languageList = "";
    public static JObject? localizedText = null;

    // Enums
    public enum Region : int { USA, PAL, Japan }
    public enum Language : int { English, Japan, Russian, Catalan, Portuguese, French, Italian, German, Dutch, Spanish }
    public enum DemaeVersion : int { Standard, Dominos }
    public enum Platform : int { Wii, vWii, Dolphin }
    public enum SetupType : int { express, custom, extras }

    // Get current console window size
    public static int console_width = 0;
    public static int console_height = 0;

    // Get system language
    public static readonly string sysLang = CultureInfo.InstalledUICulture.Name;
    public static readonly string shortLang = CultureInfo.InstalledUICulture.TwoLetterISOLanguageName;

    // HttpClient
    public static readonly HttpClient httpClient = new() { Timeout = TimeSpan.FromMinutes(1) };
    ////////////////////

    public static (bool, int, string) CheckServer(string serverURL)
    {
        // Define the URL to the connection test file and the expected response
        string url = $"{serverURL}/connectiontest.txt";
        string expectedResponse = "If the patcher can read this, the connection test succeeds.";

        MenuClass.PrintHeader();

        // Display server status check message
        string checkingServerStatus = patcherLang == "en-US"
            ? "Checking server status..."
            : $"{localizedText?["CheckServerStatus"]?["checking"]}";
        AnsiConsole.MarkupLine($"{checkingServerStatus}\n");

        try
        {
            // Create a new HttpClient instance and download the content of the connection test file
            using var client = new HttpClient();
            string responseText = client.GetStringAsync(url).Result;

            // Check if the response matches the expected response
            if (responseText.Trim() == expectedResponse)
            {
                // If successful, display success message and return
                string success = patcherLang == "en-US"
                    ? "Successfully connected to server!"
                    : $"{localizedText?["CheckServerStatus"]?["success"]}";
                AnsiConsole.MarkupLine($"[bold springgreen2_1]{success}[/]\n");
                // Wait for 1 second before returning
                Thread.Sleep(1000);

                // If it matches, return success with status code 200
                return (true, 200, "Successfully connected to server!");
            }
            else
            {
                // If it doesn't match, return an error with status code 400
                return (false, 400, "Unexpected response from server!");
            }
        }
        // Handle specific WebException errors
        catch (WebException ex)
        {
            // If the exception has an associated HTTP response, return the status code and description
            if (ex.Response is HttpWebResponse response)
            {
                return (false, (int)response.StatusCode, $"Error: {(int)response.StatusCode} - {response.StatusDescription}");
            }
            else
            {
                // If the exception doesn't have an associated HTTP response, return the exception message with status code 500
                return (false, 500, $"Request exception: {ex.Message}");
            }
        }
        // Handle any other types of exceptions
        catch (Exception ex)
        {
            // Return the exception message with status code 500
            return (false, 500, $"Unexpected exception: {ex.Message}");
        }
    }

    public static async Task CheckForUpdates(string currentVersion)
    {
        MenuClass.PrintHeader();

        // Check for updates text
        string checkingForUpdates = patcherLang == "en-US"
            ? "Checking for updates..."
            : $"{localizedText?["CheckForUpdates"]?["checking"]}";
        AnsiConsole.MarkupLine($"{checkingForUpdates}\n");

        // URL of the text file containing the latest version number
        string updateUrl = "https://raw.githubusercontent.com/PablosCorner/wiilink-patcher-version/main/version.txt";

        // Download the latest version number from the server
        string updateInfo;
        try
        {
            updateInfo = await httpClient.GetStringAsync(updateUrl);
        }
        catch (HttpRequestException ex)
        {
            // Error retrieving update information text
            string errorRetrievingUpdateInfo = patcherLang == "en-US"
                ? $"Error retrieving update information: [bold red]{ex.Message}[/]"
                : $"{localizedText?["CheckForUpdates"]?["errorChecking"]}"
                    .Replace("{ex.Message}", ex.Message);
            string skippingUpdateCheck = patcherLang == "en-US"
                ? "Skipping update check..."
                : $"{localizedText?["CheckForUpdates"]?["skippingCheck"]}";

            AnsiConsole.MarkupLine($"{errorRetrievingUpdateInfo}\n");

            AnsiConsole.MarkupLine($"{skippingUpdateCheck}\n");
            Thread.Sleep(5000);
            return;
        }
        // Timeout error
        catch (TaskCanceledException ex)
        {
            AnsiConsole.MarkupLine($"[bold red]Error:[/] {ex.Message}");
            AnsiConsole.MarkupLine($"[bold yellow]Skipping update check...[/]");
            Thread.Sleep(5000);
            return;
        }

        // Get the latest version number from the text file
        string latestVersion = updateInfo.Split('\n')[0].Trim();

        // Remove any . from the version number (v2.0.2-1 -> v202-1)
        string versionWithoutDots = latestVersion.Replace(".", "");

        // Map operating system names to executable names
        var executables = new Dictionary<string, string>
        {
            { "Windows", $"WiiLinkPatcher_Windows_{versionWithoutDots}.exe" },
            { "Linux", RuntimeInformation.ProcessArchitecture == Architecture.Arm64
                                ? $"WiiLinkPatcher_Linux-ARM64_{versionWithoutDots}"
                                : $"WiiLinkPatcher_Linux-x64_{versionWithoutDots}" },
            { "OSX", RuntimeInformation.ProcessArchitecture == Architecture.Arm64
                                ? $"WiiLinkPatcher_macOS-ARM64_{versionWithoutDots}"
                                : $"WiiLinkPatcher_macOS-x64_{versionWithoutDots}" }
        };

        // Get the download URL for the latest version
        string downloadUrl = $"https://github.com/WiiLink24/WiiLink24-Patcher/releases/download/{latestVersion}/";

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && executables.ContainsKey("Windows"))
            downloadUrl += executables["Windows"];
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && executables.ContainsKey("Linux"))
            downloadUrl += executables["Linux"];
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) && executables.ContainsKey("OSX"))
            downloadUrl += executables["OSX"];

        // Check if the latest version is newer than the current version
        if (latestVersion != currentVersion)
        {
            do
            {
                // Print header
                MenuClass.PrintHeader();

                // Prompt user to download the latest version
                string updateAvailable = patcherLang == "en-US"
                    ? "A new version is available! Would you like to download it now?"
                    : $"{localizedText?["CheckForUpdates"]?["updateAvailable"]}";
                string currentVersionText = patcherLang == "en-US"
                    ? "Current version:"
                    : $"{localizedText?["CheckForUpdates"]?["currentVersion"]}";
                string latestVersionText = patcherLang == "en-US"
                    ? "Latest version:"
                    : $"{localizedText?["CheckForUpdates"]?["latestVersion"]}";

                AnsiConsole.MarkupLine($"{updateAvailable}\n");

                AnsiConsole.MarkupLine($"{currentVersionText} {currentVersion}");
                AnsiConsole.MarkupLine($"{latestVersionText} [bold springgreen2_1]{latestVersion}[/]\n");

                // Show changelog via Github link
                string changelogLink = patcherLang == "en-US"
                    ? "Changelog:"
                    : $"{localizedText?["CheckForUpdates"]?["changelogLink"]}";
                AnsiConsole.MarkupLine($"[bold]{changelogLink}[/] [link springgreen2_1]https://github.com/WiiLink24/WiiLink24-Patcher/releases/tag/{latestVersion}[/]\n");

                // Yes/No text
                string yes = patcherLang == "en-US"
                    ? "Yes"
                    : $"{localizedText?["yes"]}";
                string no = patcherLang == "en-US"
                    ? "No"
                    : $"{localizedText?["no"]}";

                AnsiConsole.MarkupLine($"1. {yes}");
                AnsiConsole.MarkupLine($"2. {no}\n");

                // Get user's choice
                int choice = MenuClass.UserChoose("12");

                switch (choice)
                {
                    case 1: // Download the latest version
                        // Determine the operating system name
                        string? osName = RuntimeInformation
                            .IsOSPlatform(OSPlatform.Windows) ? "Windows" :
                            RuntimeInformation
                            .IsOSPlatform(OSPlatform.OSX) ? "macOS" :
                            RuntimeInformation
                            .IsOSPlatform(OSPlatform.Linux) ? "Linux" : "Unknown";

                        // Log message
                        string downloadingFrom = patcherLang == "en-US"
                            ? $"Downloading [springgreen2_1]{latestVersion}[/] for [springgreen2_1]{osName}[/]..."
                            : $"{localizedText?["CheckForUpdates"]?["downloadingFrom"]}"
                                .Replace("{latestVersion}", latestVersion)
                                .Replace("{osName}", osName);
                        AnsiConsole.MarkupLine($"\n[bold]{downloadingFrom}[/]");
                        Console.Out.Flush();

                        // Download the latest version and save it to a file
                        HttpResponseMessage response;
                        response = await httpClient.GetAsync(downloadUrl);
                        if (!response.IsSuccessStatusCode) // Ideally shouldn't happen if version.txt is set up correctly
                        {
                            // Download failed text
                            string downloadFailed = patcherLang == "en-US"
                                ? $"An error occurred while downloading the latest version:[/] {response.StatusCode}"
                                : $"{localizedText?["CheckForUpdates"]?["downloadFailed"]}"
                                    .Replace("{response.StatusCode}", response.StatusCode.ToString());
                            string pressAnyKey = patcherLang == "en-US"
                                ? "Press any key to exit..."
                                : $"{localizedText?["CheckForUpdates"]?["pressAnyKey"]}";
                            AnsiConsole.MarkupLine($"\n[red]{downloadFailed}[/]");
                            AnsiConsole.MarkupLine($"[red]{pressAnyKey}[/]");
                            Console.ReadKey();
                            ExitApp();
                            return;
                        }

                        // Save the downloaded file to disk
                        byte[] content = await response.Content.ReadAsByteArrayAsync();
                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && executables.ContainsKey("Windows"))
                            File.WriteAllBytes(executables["Windows"], content);
                        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && executables.ContainsKey("Linux"))
                            File.WriteAllBytes(executables["Linux"], content);
                        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) && executables.ContainsKey("OSX"))
                            File.WriteAllBytes(executables["OSX"], content);

                        // Tell user that program will exit in 5 seconds with a countdown in a loop
                        for (int i = 5; i > 0; i--)
                        {
                            MenuClass.PrintHeader();
                            // Download complete text
                            string downloadComplete = patcherLang == "en-US"
                                ? $"[bold springgreen2_1]Download complete![/] Exiting in [bold springgreen2_1]{i}[/] seconds..."
                                : $"{localizedText?["CheckForUpdates"]?["downloadComplete"]}"
                                    .Replace("{i}", i.ToString());
                            AnsiConsole.MarkupLine($"\n{downloadComplete}");
                            Thread.Sleep(1000);
                        }

                        // Exit the program
                        ExitApp();
                        return;
                    case 2: // Don't download the latest version
                        return;
                    default:
                        break;
                }
            } while (true);
        }
        else // On the latest version
        {
            // Log message
            string onLatestVersion = patcherLang == "en-US"
                ? "You are running the latest version!"
                : $"{localizedText?["CheckForUpdates"]?["onLatestVersion"]}";
            AnsiConsole.MarkupLine($"[bold springgreen2_1]{onLatestVersion}[/]");
            Thread.Sleep(1000);
        }
    }

    public static void CopyFolder(string sourcePath, string destinationPath)
    {
        DirectoryInfo source = new(sourcePath);
        DirectoryInfo destination = new(destinationPath);

        // If the destination folder doesn't exist, create it.
        if (!destination.Exists)
            destination.Create();

        // Copy each file to the destination folder.
        foreach (var file in source.GetFiles())
            file.CopyTo(Path.Combine(destination.FullName, file.Name), true);

        // Recursively copy each subdirectory to the destination folder.
        foreach (var subfolder in source.GetDirectories())
            CopyFolder(subfolder.FullName, Path.Combine(destination.FullName, subfolder.Name));
    }

    public static void WinCompatWarning()
    {
        MenuClass.PrintHeader();

        AnsiConsole.MarkupLine("[bold red]WARNING:[/] Older version of Windows detected!\n");

        AnsiConsole.MarkupLine("You are running the WiiLink Patcher on an older version of Windows.");
        AnsiConsole.MarkupLine("While the patcher may work, it is not guaranteed to work on this version of Windows.\n");

        AnsiConsole.MarkupLine("If you encounter any issues while running the patcher, we will most likely not be able to help you.\n");

        AnsiConsole.MarkupLine("Please consider upgrading to Windows 10 or above before continuing, or use the macOS/Linux patcher instead.\n");

        AnsiConsole.MarkupLine("Otherwise, for the best visual experience, set your console font to [springgreen2_1]Consolas[/] with a size of [springgreen2_1]16[/].");
        AnsiConsole.MarkupLine("Also, set your console window and buffer size to [springgreen2_1]120x30[/].\n");

        AnsiConsole.MarkupLine("\n[bold yellow]Press ESC to quit, or any other key to proceed at your own risk...[/]");

        ConsoleKeyInfo keyInfo = Console.ReadKey();
        if (keyInfo.Key == ConsoleKey.Escape)
            ExitApp();

        inCompatabilityMode = true;
    }

    // Exit console app
    public static void ExitApp()
    {
        // Restore original console size if not Windows
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            Console.Write($"\u001b[8;{console_height};{console_width}t");

        // Clear console
        Console.Clear();

        Environment.Exit(0);
    }

    static async Task Main(string[] args)
    {
        // Check for debugging flag
        bool debugArgExists = Array.Exists(args, element => element.ToLower() == "--debug");

        // Set DEBUG_MODE
        DEBUG_MODE = debugArgExists;

        // Set console encoding to UTF-8
        Console.OutputEncoding = Encoding.UTF8;

        // Cache current console size to console_width and console_height
        console_width = Console.WindowWidth;
        console_height = Console.WindowHeight;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Console.Title = $"WiiLink Patcher {version}";
            if (DEBUG_MODE) Console.Title += $" [DEBUG MODE]";
            if (version.Contains("Nightly") || version.Contains("RC")) Console.Title += $" (Test Build)";
        }

        // Set console window size to 120x30 on macOS and Linux and on Windows, check for Windows version
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Console.Write("\u001b[8;30;120t");
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            if (console_width < 100 || console_height < 25)
                Console.Write("\u001b[8;30;120t");
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            if (Environment.OSVersion.Version.Major < 10)
                WinCompatWarning();
        }

        // Check if the server is up
        // If the server is down, show the status code and error message
        var result = CheckServer(wiiLinkPatcherUrl);
        if (!result.Item1)
            MenuClass.ConnectionFailed(result.Item2, result.Item3);

        // Initialise language list
        languageList = LanguageClass.DownloadLanguageList();

        // Attempt to automatically set language, showing a prompt to ensure it was correctly detected
        LanguageClass.AutoSetLang();

        // Check latest version if not on a nightly build or release candidate
        if (!version.Contains("Nightly") && !version.Contains("RC"))
            await CheckForUpdates(version);

        // Go to the main menu
        MenuClass.MainMenu();
    }
}