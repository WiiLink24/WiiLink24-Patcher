using System.Diagnostics;
using System.Text;
using System.Runtime.InteropServices;
using Spectre.Console;
using libWiiSharp;
using System.Net;
using Newtonsoft.Json.Linq;
using System.IO.Compression;

// Author: PablosCorner and WiiLink Team
// Project: WiiLink Patcher (CLI Version)
// Description: WiiLink Patcher (CLI Version) is a command-line interface to patch and revive the exclusive Japanese Wii Channels that were shut down, along with the international WiiConnect24 Channels.

class WiiLink_Patcher
{
    //// Build Info ////
    static readonly string version = "v2.0.4";
    static readonly string copyrightYear = DateTime.Now.Year.ToString();
    static readonly string buildDate = "October 11th, 2024";
    static readonly string buildTime = "3:24 PM";
    static string? sdcard = DetectRemovableDrive;
    static readonly string wiiLinkPatcherUrl = "https://patcher.wiilink24.com";
    ////////////////////

    //// Setup Info ////
    // Express Install variables
    static Language lang;
    static DemaeVersion demaeVersion;
    static Language wiiRoomLang;
    static bool installRegionalChannels = false;
    static Region wc24_reg;
    static Platform platformType;
    static Dictionary<string, string> patchingProgress_express = [];

    // Custom Install variables
    static List<string> wiiLinkChannels_selection = [];
    static List<string> wiiConnect24Channels_selection = [];
    static List<string> combinedChannels_selection = [];
    static Platform platformType_custom;
    static bool inCompatabilityMode = false;
    static Dictionary<string, string> patchingProgress_custom = [];

    // Misc. variables
    static string task = "";
    static string curCmd = "";
    static readonly string curDir = Directory.GetCurrentDirectory();
    static readonly string tempDir = Path.Join(Path.GetTempPath(), "WiiLink_Patcher");
    static bool DEBUG_MODE = false;
    static PatcherLanguage patcherLang = PatcherLanguage.en;
    static JObject? localizedText = null;

    // Enums
    enum Region : int { USA, PAL, Japan }
    enum Language : int { English, Japan, Russian, Catalan, Portuguese, French, Italian, German, Dutch, Spanish }
    enum PatcherLanguage : int { en }
    enum DemaeVersion : int { Standard, Dominos }
    enum Platform : int { Wii, vWii, Dolphin }

    // Get current console window size
    static int console_width = 0;
    static int console_height = 0;

    // HttpClient
    static readonly HttpClient httpClient = new() { Timeout = TimeSpan.FromSeconds(30) };
    ////////////////////

    static void PrintHeader()
    {
        Console.Clear();

        string headerText = patcherLang == PatcherLanguage.en
            ? $"[springgreen2_1]WiiLink[/] Patcher {version} - (c) {copyrightYear} WiiLink Team"
            : $"{localizedText?["Header"]}"
                .Replace("{version}", version)
                .Replace("{year}", copyrightYear);

        AnsiConsole.MarkupLine($"{(inCompatabilityMode
            ? headerText
            : $"[bold]{headerText}[/]")}");

        char borderChar = '=';
        string borderLine = new(borderChar, Console.WindowWidth);

        AnsiConsole.MarkupLine($"{(inCompatabilityMode
            ? borderLine
            : $"[bold]{borderLine}[/]")}\n");
    }

    static void PrintNotice()
    {
        string title = patcherLang == PatcherLanguage.en
            ? "Notice"
            : $"{localizedText?["Notice"]?["noticeTitle"]}";
        string text = patcherLang == PatcherLanguage.en
            ? "If you have any issues with the patcher or services offered by WiiLink, please report them on our [springgreen2_1 link=https://discord.gg/wiilink]Discord Server[/]!"
            : $"{localizedText?["Notice"]?["noticeMsg"]}";

        var panel = new Panel($"[bold]{text}[/]")
        {
            Header = new PanelHeader($"[bold springgreen2_1] {title} [/]", Justify.Center),
            Border = inCompatabilityMode ? BoxBorder.Ascii : BoxBorder.Heavy,
            BorderStyle = new Style(Color.SpringGreen2_1),
            Expand = true,
        };

        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();
    }

    /// <summary>
    /// Gets the base path of the removable drive where the "apps" directory exists.
    /// </summary>
    /// <returns>The base path of the removable drive, or null if no removable drive with the "apps" directory is found.</returns>
    static string? DetectRemovableDrive
    {
        get
        {
            var basePaths = new List<string>();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                basePaths = DriveInfo.GetDrives()
                    .Where(drive => drive.DriveType == DriveType.Removable && drive.IsReady)
                    .Select(drive => drive.Name)
                    .ToList();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                basePaths.Add("/Volumes");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                var mediaPath = $"/media/{Environment.UserName}";
                if (Directory.Exists("/media") && Directory.Exists(mediaPath)) basePaths.Add(mediaPath);

                var runMediaPath = $"/run/media/{Environment.UserName}";
                if (Directory.Exists("/run/media") && Directory.Exists(runMediaPath)) basePaths.Add(runMediaPath);
            }

            foreach (var basePath in basePaths)
            {
                try
                {
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        // Check if the apps directory exists on the root of the removable drive
                        if (Directory.Exists(Path.Join(basePath, "apps")))
                        {
                            return basePath;
                        }
                    }
                    else
                    {
                        var subDirectories = Directory.EnumerateDirectories(basePath);
                        foreach (var subDirectory in subDirectories)
                        {
                            // Check if the apps directory exists on the root of the removable drive
                            if (Directory.Exists(Path.Join(subDirectory, "apps")))
                            {
                                return subDirectory;
                            }
                        }
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    // If the user doesn't have permission to access the directory, skip it.
                    continue;
                }
            }

            return null;
        }
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
    static int UserChoose(string choices)
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
        string chooseText = patcherLang == PatcherLanguage.en
                ? "Choose: "
                : $"{localizedText?["UserChoose"]} ";
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
    static void CreditsScreen()
    {
        PrintHeader();

        // Build info
        string buildInfo = patcherLang == PatcherLanguage.en
            ? $"This build was compiled on [bold springgreen2_1]{buildDate}[/] at [bold springgreen2_1]{buildTime}[/]."
            : $"{localizedText?["Credits"]?["buildInfo"]}"
                .Replace("{buildDate}", buildDate)
                .Replace("{buildTime}", buildTime);
        AnsiConsole.MarkupLine($"{buildInfo}\n");

        // Credits table
        var creditTable = new Table().Border(inCompatabilityMode ? TableBorder.None : TableBorder.DoubleEdge);

        // Credits header
        string credits = patcherLang == PatcherLanguage.en
            ? "Credits"
            : $"{localizedText?["Credits"]?["credits"]}";
        creditTable.AddColumn(new TableColumn($"[bold springgreen2_1]{credits}[/]").Centered());

        // Credits grid
        var creditGrid = new Grid().AddColumn().AddColumn();

        // Credits text
        string sketchDesc = patcherLang == PatcherLanguage.en
            ? "WiiLink Lead"
            : $"{localizedText?["Credits"]?["sketchDesc"]}";
        string pablosDesc = patcherLang == PatcherLanguage.en
            ? "WiiLink Patcher Developer"
            : $"{localizedText?["Credits"]?["pablosDesc"]}";
        string lunaDesc = patcherLang == PatcherLanguage.en
            ? "Lead Translator"
            : $"{localizedText?["Credits"]?["lunaDesc"]}";
        string leathlWiiDatabase = patcherLang == PatcherLanguage.en
            ? "leathl and WiiDatabase"
            : $"{localizedText?["Credits"]?["leathlWiiDatabase"]}";
        string leathlWiiDatabaseDesc = patcherLang == PatcherLanguage.en
            ? "libWiiSharp developers"
            : $"{localizedText?["Credits"]?["leathlWiiDatabaseDesc"]}";

        creditGrid.AddRow(new Text("Sketch", new Style(Color.SpringGreen2_1, null, Decoration.Bold)).RightJustified(), new Text(sketchDesc, new Style(null, null, Decoration.Bold)));
        creditGrid.AddRow(new Text("PablosCorner", new Style(Color.SpringGreen2_1, null, Decoration.Bold)).RightJustified(), new Text(pablosDesc, new Style(null, null, Decoration.Bold)));
        creditGrid.AddRow(new Text("Luna", new Style(Color.SpringGreen2_1, null, Decoration.Bold)).RightJustified(), new Text(lunaDesc, new Style(null, null, Decoration.Bold)));
        creditGrid.AddRow(new Text(leathlWiiDatabase, new Style(Color.SpringGreen2_1, null, Decoration.Bold)).RightJustified(), new Text(leathlWiiDatabaseDesc, new Style(null, null, Decoration.Bold)));
        creditGrid.AddRow(new Text("SnowflakePowered", new Style(Color.SpringGreen2_1, null, Decoration.Bold)).RightJustified(), new Text("VCDiff", new Style(null, null, Decoration.Bold)));

        // Add the grid to the table
        creditTable.AddRow(creditGrid).Centered();
        AnsiConsole.Write(creditTable);

        // Special thanks grid
        string specialThanksTo = patcherLang == PatcherLanguage.en
            ? "Special thanks to:"
            : $"{localizedText?["Credits"]?["specialThanksTo"]}";
        AnsiConsole.MarkupLine($"\n[bold springgreen2_1]{specialThanksTo}[/]\n");

        var specialThanksGrid = new Grid().AddColumn().AddColumn();

        // Special thanks text
        string theshadoweeveeRole = patcherLang == PatcherLanguage.en
            ? "- Pointing me in the right direction with implementing libWiiSharp!"
            : $"{localizedText?["Credits"]?["theshadoweeveeRole"]}";
        string ourTesters = patcherLang == PatcherLanguage.en
            ? "Our Testers"
            : $"{localizedText?["Credits"]?["ourTesters"]}";
        string ourTestersRole = patcherLang == PatcherLanguage.en
            ? "- For testing the patcher and reporting bugs/anomalies!"
            : $"{localizedText?["Credits"]?["ourTestersRole"]}";
        string you = patcherLang == PatcherLanguage.en
            ? "You!"
            : $"{localizedText?["Credits"]?["you"]}";
        string youRole = patcherLang == PatcherLanguage.en
            ? "- For your continued support of WiiLink!"
            : $"{localizedText?["Credits"]?["youRole"]}";

        specialThanksGrid.AddRow($"  ● [bold springgreen2_1]TheShadowEevee[/]", theshadoweeveeRole);
        specialThanksGrid.AddRow($"  ● [bold springgreen2_1]{ourTesters}[/]", ourTestersRole);
        specialThanksGrid.AddRow($"  ● [bold springgreen2_1]{you}[/]", youRole);

        AnsiConsole.Write(specialThanksGrid);
        AnsiConsole.MarkupLine("");

        // Links grid
        string wiilinkSite = patcherLang == PatcherLanguage.en
            ? "WiiLink website"
            : $"{localizedText?["Credits"]?["wiilinkSite"]}";
        string githubRepo = patcherLang == PatcherLanguage.en
            ? "Github repository"
            : $"{localizedText?["Credits"]?["githubRepo"]}";

        var linksGrid = new Grid().AddColumn().AddColumn();

        linksGrid.AddRow($"[bold springgreen2_1]{wiilinkSite}[/]:", "[link]https://wiilink24.com[/]");
        linksGrid.AddRow($"[bold springgreen2_1]{githubRepo}[/]:", "[link]https://github.com/WiiLink24/WiiLink24-Patcher[/]");

        AnsiConsole.Write(linksGrid);
        AnsiConsole.MarkupLine("");

        // Press any key to go back to settings
        string pressAnyKey = patcherLang == PatcherLanguage.en
            ? "Press any key to go back to settings..."
            : $"{localizedText?["Credits"]?["pressAnyKey"]}";
        AnsiConsole.Markup($"[bold]{pressAnyKey}[/]");
        Console.ReadKey();
    }

    /// <summary>
    /// Downloads an app from the Open Shop Channel
    /// </summary>
    /// <param name="appName"></param>
    static public void DownloadOSCApp(string appName)
    {
        task = $"Downloading {appName}";
        string appPath = Path.Join("apps", appName);

        if (!Directory.Exists(appPath))
            Directory.CreateDirectory(appPath);

        DownloadFile($"https://hbb1.oscwii.org/unzipped_apps/{appName}/apps/{appName}/boot.dol", Path.Join(appPath, "boot.dol"), appName);
        DownloadFile($"https://hbb1.oscwii.org/unzipped_apps/{appName}/apps/{appName}/meta.xml", Path.Join(appPath, "meta.xml"), appName);
        DownloadFile($"https://hbb1.oscwii.org/api/v3/contents/{appName}/icon.png", Path.Join(appPath, "icon.png"), appName);
    }

    /// <summary>
    /// Downloads AnyGlobe Changer from OSC or GitHub, depending on platform, as the latest OSC release doesn't work with Dolphin.
    /// </summary>
    static public void DownloadAGC()
    {
        if (platformType != Platform.Dolphin) {
            DownloadOSCApp("AnyGlobe_Changer");
        }
        else if (!Directory.Exists("./apps/AnyGlobe Changer")) {
            task = $"Downloading AnyGlobe_Changer";
            string appPath = Path.Join(tempDir, "AGC");
            Directory.CreateDirectory(appPath);
            DownloadFile($"https://github.com/fishguy6564/AnyGlobe-Changer/releases/download/1.0/AnyGlobe.Changer.zip", Path.Join(appPath, "AGC.zip"), "AnyGlobe_Changer");
            ZipFile.ExtractToDirectory(Path.Join(appPath, "AGC.zip"), "./");
            Directory.Delete(appPath, true);
        }
    }

    /// <summary>
    /// Downloads a file from the specified URL to the specified destination with the specified name.
    /// </summary>
    /// <param name="URL">The URL to download the file from.</param>
    /// <param name="dest">The destination to save the file to.</param>
    /// <param name="name">The name of the file.</param>
    /// <param name="noError">Optional parameter. If true, the function will return instead of throwing an error.</param>
    static void DownloadFile(string URL, string dest, string name, bool noError = false)
    {
        task = $"Downloading {name}";
        curCmd = $"DownloadFile({URL}, {dest}, {name})";
        if (DEBUG_MODE)
            AnsiConsole.MarkupLine($"[springgreen2_1]Downloading [bold]{name}[/] from [bold]{URL}[/] to [bold]{dest}[/][/]...");

        try
        {
            // Send a GET request to the specified URL.
            var response = httpClient.GetAsync(URL).Result;
            if (response.IsSuccessStatusCode)
            {
                // If the response is successful, create a new file at the specified destination and save the response stream to it.
                using var stream = response.Content.ReadAsStream();
                using var fileStream = File.Create(dest);
                stream.CopyTo(fileStream);
            }
            else if (!noError)
            {
                // If the response is not successful, display an error message.
                int statusCode = (int)response.StatusCode;
                ErrorScreen(statusCode, $"Failed to download [bold]{name}[/] from [bold]{URL}[/] to [bold]{dest}[/]");
            }
        }
        catch (Exception e)
        {
            if (!noError)
            {
                AnsiConsole.MarkupLine($"[bold red]ERROR:[/] {e.Message}");
                // If the exception is a WebException, display the status code of the response.
                if (e is WebException we && we.Response is HttpWebResponse response)
                {
                    int statusCode = (int)response.StatusCode;
                    AnsiConsole.MarkupLine($"Status code: {statusCode}");
                }
                AnsiConsole.MarkupLine("Press any key to try again...");
                Console.ReadKey(true);
            }
        }
    }

    /// <summary>
    /// Downloads channel contents from NUS.
    /// </summary>
    /// <param name="titleID"></param>
    /// <param name="outputDir"></param>
    /// <param name="appVer"></param>
    /// <param name="isWC24"></param>
    /// <returns></returns>
    static string DownloadNUS(string titleID, string outputDir, string? appVer = null, bool isWC24 = false)
    {
        task = $"Downloading {titleID}";

        // Create a new NusClient instance to handle the download.
        var nus = new NusClient();

        // Create a list of store types to download.
        var store = new List<StoreType> { isWC24 ? StoreType.DecryptedContent : StoreType.WAD };

        // Check that the title ID is the correct length.
        if (titleID.Length != 16)
        {
            ErrorScreen(16, "Title ID must be 16 characters long");
            return "";
        }

        try
        {
            // If the appVer parameter is not specified, download the latest version of the title's TMD to determine the latest version.
            if (appVer == null)
            {
                TMD tmd = nus.DownloadTMD(titleID, "");
                appVer = tmd.TitleVersion.ToString();
            }

            // Download the title with the specified title ID, version, and store types to the specified output directory.
            nus.DownloadTitle(titleID, appVer, outputDir, store.ToArray());

            // Return the version of the title that was downloaded.
            return appVer;
        }
        catch (Exception e)
        {
            ErrorScreen(e.HResult, e.Message);
            return "";
        }
    }


    static void UnpackWAD(string wadFilePath, string outputDir)
    {
        task = $"Unpacking WAD";
        WAD wad = new();

        try
        {
            wad.LoadFile(wadFilePath);
            wad.Unpack(outputDir);
        }
        catch (Exception e)
        {
            ErrorScreen(e.HResult, e.Message);
        }
    }

    static void PackWAD(string unpackPath, string outputWADDir)
    {
        task = $"Packing WAD";
        WAD wad = new();

        try
        {
            wad.CreateNew(unpackPath);
            wad.Save(outputWADDir);
        }
        catch (Exception e)
        {
            ErrorScreen(e.HResult, e.Message);
        }
    }

    static void DownloadPatch(string folderName, string patchInput, string patchOutput, string channelName)
    {
        string patchUrl = $"{wiiLinkPatcherUrl}/{folderName.ToLower()}/{patchInput}";
        string patchDestinationPath = Path.Join(tempDir, "Patches", folderName, patchOutput);

        if (DEBUG_MODE)
        {
            AnsiConsole.MarkupLine($"[bold yellow]URL:[/] {patchUrl}");
            AnsiConsole.MarkupLine($"[bold yellow]Destination:[/] {patchDestinationPath}");
            AnsiConsole.MarkupLine("------- Press any key to continue -------");
            Console.ReadKey(true);
        }

        // If tempDir/Patches/{folderName} doesn't exist, make it
        if (!Directory.Exists(Path.Join(tempDir, "Patches", folderName)))
            Directory.CreateDirectory(Path.Join(tempDir, "Patches", folderName));

        DownloadFile(patchUrl, patchDestinationPath, channelName);
    }

    static void ApplyPatch(FileStream original, FileStream patch, FileStream output)
    {
        try
        {
            // Create a new VCDiff decoder with the original, patch, and output files.
            using var decoder = new VCDiff.Decoders.VcDecoder(original, patch, output);
            decoder.Decode(out _);  // Decode the patch and write the result to the output file.
        }
        catch (Exception e)
        {
            ErrorScreen(e.HResult, e.Message);
        }
        finally
        {
            // Close all file streams.
            original.Close();
            patch.Close();
            output.Close();
        }
    }

    static void DownloadSPD(Platform platformType)
    {
        // Create WAD folder in current directory if it doesn't exist
        if (!Directory.Exists(Path.Join("WAD")))
            Directory.CreateDirectory(Path.Join("WAD"));

        string spdUrl = $"{wiiLinkPatcherUrl}/spd/WiiLink_SPD.wad";
        string spdDestinationPath = Path.Join("WAD", $"WiiLink Address Settings.wad");

        DownloadFile(spdUrl, spdDestinationPath, "SPD");
    }


    // Patches the Japanese-exclusive channels
    static void PatchRegionalChannel(string channelName, string channelTitle, string titleID, List<KeyValuePair<string, string>> patchFilesDict, string? appVer = null, Language? lang = null)
    {
        // Set up folder paths and file names
        string titleFolder = Path.Join(tempDir, "Unpack");
        string tempFolder = Path.Join(tempDir, "Unpack_Patched");
        string patchFolder = Path.Join(tempDir, "Patches", channelName);
        string outputChannel = lang == null ? Path.Join("WAD", $"{channelTitle}.wad") : Path.Join("WAD", $"{channelTitle} [{lang}] (WiiLink).wad");
        string urlSubdir = channelName.ToLower();

        // Create unpack and unpack-patched folders
        Directory.CreateDirectory(titleFolder);
        Directory.CreateDirectory(tempFolder);

        // Download and extract the Wii channel files
        task = $"Downloading and extracting files for {channelTitle}";
        appVer = DownloadNUS(titleID, titleFolder, appVer);
        string outputWad = Path.Join(titleFolder, $"{titleID}v{appVer}.wad");
        UnpackWAD(outputWad, titleFolder);

        // Download the patched TMD file and rename it to title_id.tmd
        task = $"Downloading patched TMD file for {channelTitle}";
        DownloadFile($"{wiiLinkPatcherUrl}/{urlSubdir}/{channelName}.tmd", Path.Join(titleFolder, $"{titleID}.tmd"), channelTitle);

        //// Apply delta patches to the app files ////
        task = $"Applying delta patches for {channelTitle}";

        bool translated = lang == Language.English || lang == Language.Russian || lang == Language.Catalan || lang == Language.Portuguese || lang == Language.French || lang == Language.Italian || lang == Language.German || lang == Language.Dutch;

        // Apply the patches
        foreach (var patch in patchFilesDict)
        {
            // Check if the patch value is not null
            if (patch.Value != null)
            {
                // Determine if the patch should be applied based on specific conditions
                bool applyPatch = true;
                if (patch.Key == patchFilesDict[0].Key)
                {
                    // Condition for the first delta patch
                    applyPatch = translated || channelName == "Dominos";
                }
                else if (patch.Key == patchFilesDict[2].Key)
                {
                    // Condition for the third delta patch
                    applyPatch = translated || channelName == "Dominos" || channelName == "WiinoMa";
                }
                // Apply the patch if conditions are met
                if (applyPatch)
                {
                    ApplyPatch(File.OpenRead(Path.Join(titleFolder, $"{patch.Value}.app")), File.OpenRead(Path.Join(patchFolder, $"{patch.Key}.delta")), File.OpenWrite(Path.Join(tempFolder, $"{patch.Value}.app")));
                }
            }
        }

        // Copy patched files to unpack folder
        task = $"Copying patched files for {channelTitle}";
        CopyFolder(tempFolder, titleFolder);

        // Repack the title with the patched files
        task = $"Repacking the title for {channelTitle}";
        PackWAD(titleFolder, outputChannel);

        // Delete unpack and unpack_patched folders
        Directory.Delete(titleFolder, true);
        Directory.Delete(tempFolder, true);
    }

    // This function patches the WiiConnect24 channels
    static void PatchWC24Channel(string channelName, string channelTitle, int channelVersion, Region? channelRegion, string titleID, List<string> patchFile, List<string> appFile)
    {
        // Define the necessary paths and filenames
        string titleFolder = Path.Join(tempDir, "Unpack");
        string tempFolder = Path.Join(tempDir, "Unpack_Patched");
        string patchFolder = Path.Join(tempDir, "Patches", channelName);

        // Name the output WAD file
        // Append the region to the output WAD name if it has a region
        string outputWad;
        if (channelName == "ktv" || channelRegion == null)
            outputWad = Path.Join("WAD", $"{channelTitle} (WiiLink).wad");
        else
            outputWad = Path.Join("WAD", $"{channelTitle} [{channelRegion}] (WiiLink).wad");

        // Create unpack and unpack-patched folders
        Directory.CreateDirectory(titleFolder);
        Directory.CreateDirectory(tempFolder);

        string fileURL = $"{wiiLinkPatcherUrl}/{channelName.ToLower()}/{titleID}";

        // Define the URLs and file paths
        var files = new Dictionary<string, string>
        {
            {".cert", Path.Join(titleFolder, $"{titleID}.cert")},
            {".tmd", Path.Join(titleFolder, $"tmd.{channelVersion}")},
            {".tik", Path.Join(titleFolder, "cetk")}
        };

        // Download the necessary files for the channel
        task = $"Downloading necessary files for {channelTitle}";

        // Download the files
        foreach (var file in files)
        {
            string url = $"{fileURL}{file.Key}";
            try // Try to download the file
            {
                DownloadFile(url, file.Value, $"{channelTitle} {file.Key}", noError: true);
            }
            catch (Exception)
            {
                // File doesn't exist, move on to the next one
                continue;
            }
        }

        // Extract the necessary files for the channel
        task = $"Extracting stuff for {channelTitle}";
        DownloadNUS(titleID, titleFolder, channelVersion.ToString(), true);

        // Rename the extracted files
        task = $"Renaming files for {channelTitle}";
        File.Move(Path.Join(titleFolder, $"tmd.{channelVersion}"), Path.Join(titleFolder, $"{titleID}.tmd"));
        File.Move(Path.Join(titleFolder, "cetk"), Path.Join(titleFolder, $"{titleID}.tik"));

        // Download the patched TMD file for Kirby TV Channel to make it region-free
        if (channelName == "ktv")
        {
            string tmdURL = $"{wiiLinkPatcherUrl}/{channelName.ToLower()}/{titleID}.tmd";
            DownloadFile(tmdURL, Path.Join(titleFolder, $"{titleID}.tmd"), $"{channelTitle} .tmd");
        }

        // Apply the delta patches to the app file
        task = $"Applying delta patch for {channelTitle}";
        foreach (var (app, patch) in appFile.Zip(patchFile, (app, patch) => (app, patch)))
        {
            ApplyPatch(File.OpenRead(Path.Join(titleFolder, $"{app}.app")), File.OpenRead(Path.Join(patchFolder, $"{patch}.delta")), File.OpenWrite(Path.Join(tempFolder, $"{app}.app")));
        }

        // Copy the patched files to the unpack folder
        task = $"Copying patched files for {channelTitle}";
        try
        {
            CopyFolder(tempFolder, titleFolder);
        }
        catch (Exception e)
        {
            ErrorScreen(e.HResult, e.Message);
        }

        // Delete the unpack_patched folder
        Directory.Delete(tempFolder, true);

        // Repack the title into a WAD file
        task = $"Repacking the title for {channelTitle}";
        PackWAD(titleFolder, outputWad);

        // Delete the unpack and unpack_patched folders
        Directory.Delete(titleFolder, true);
    }

    // Downloads WC24 channel withouth patching (to get stock channel)
    static void DownloadWC24Channel(string channelName, string channelTitle, int channelVersion, Region? channelRegion, string titleID)
    {
        // Define the necessary paths and filenames
        string titleFolder = Path.Join(tempDir, "Unpack");

        // Create WAD folder in current directory if it doesn't exist
        if (!Directory.Exists(Path.Join("WAD")))
            Directory.CreateDirectory(Path.Join("WAD"));

        // Name the output WAD file
        string outputWad;
        if (channelName == "ktv" || channelRegion == null)
            outputWad = Path.Join("WAD", $"{channelTitle} (WiiLink).wad");
        else
            outputWad = Path.Join("WAD", $"{channelTitle} [{channelRegion}] (WiiLink).wad");

        // Create unpack and unpack-patched folders
        Directory.CreateDirectory(titleFolder);

        // Extract the necessary files for the channel
        task = $"Extracting stuff for {channelTitle}";
        DownloadNUS(titleID, titleFolder, channelVersion.ToString());

        // Rename the extracted files
        task = $"Renaming files for {channelTitle}";

        // Move resulting WAD to output folder
        File.Move(Path.Join(titleFolder, $"{titleID}v{channelVersion}.wad"), outputWad);

        // Delete the unpack folder
        Directory.Delete(titleFolder, true);
    }

    // Install Choose (Express Install)
    static void WiiLinkChannels_LangSetup()
    {
        while (true)
        {
            PrintHeader();

            // Express Install Header Text
            string EIHeader = patcherLang == PatcherLanguage.en
                ? "Express Install"
                : $"{localizedText?["ExpressInstall"]?["Header"]}";
            AnsiConsole.MarkupLine($"[bold springgreen2_1]{EIHeader}[/]\n");

            // Step 2 Text
            string step1Message = patcherLang == PatcherLanguage.en
                ? "Step 2: Choose WiiLink's regional channels language"
                : $"{localizedText?["ExpressInstall"]?["WiiLinkChannels_LangSetup"]?["step1Message"]}";
            AnsiConsole.MarkupLine($"[bold]{step1Message}[/]\n");

            // Instructions Text
            string instructions = patcherLang == PatcherLanguage.en
                ? "For [bold]Wii Room[/], [bold]Photo Prints Channel[/], and [bold]Food Channel[/], which language would you like to select?"
                : $"{localizedText?["ExpressInstall"]?["WiiLinkChannels_LangSetup"]?["instructions"]}";
            AnsiConsole.MarkupLine($"{instructions}\n");

            // User Choices
            string englishTranslation = patcherLang == PatcherLanguage.en
                ? "Translated (eg. English, French, etc.)"
                : $"{localizedText?["ExpressInstall"]?["WiiLinkChannels_LangSetup"]?["englishOption"]}";
            string japanese = patcherLang == PatcherLanguage.en
                ? "Japanese"
                : $"{localizedText?["ExpressInstall"]?["WiiLinkChannels_LangSetup"]?["japaneseOption"]}";
            string goBackToMainMenu = patcherLang == PatcherLanguage.en
                ? "Go Back to Main Menu"
                : $"{localizedText?["ExpressInstall"]?["WiiLinkChannels_LangSetup"]?["goBackToMainMenu"]}";

            AnsiConsole.MarkupLine($"1. {englishTranslation}");
            AnsiConsole.MarkupLine($"2. {japanese}\n");

            AnsiConsole.MarkupLine($"3. {goBackToMainMenu}\n");

            int choice = UserChoose("123");
            switch (choice)
            {
                case 1:
                    lang = Language.English;
                    WiiRoomConfiguration();
                    break;
                case 2:
                    lang = Language.Japan;
                    wiiRoomLang = Language.Japan;
                    demaeVersion = DemaeVersion.Standard;
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

    // Configure Wii Room Channel (choosing language) [Express Install]
    static void WiiRoomConfiguration()
    {
        while (true)
        {
            PrintHeader();

            // Express Install Header Text
            string EIHeader = patcherLang == PatcherLanguage.en
                ? "Express Install"
                : $"{localizedText?["ExpressInstall"]?["Header"]}";
            AnsiConsole.MarkupLine($"[bold springgreen2_1]{EIHeader}[/]\n");

            // Step 2A Text
            string stepNumber = patcherLang == PatcherLanguage.en
                ? "Step 2A"
                : $"{localizedText?["ExpressInstall"]?["WiiRoomConfiguration"]?["stepNum"]}";
            string step1aTitle = patcherLang == PatcherLanguage.en
                ? "Choose Wii Room language"
                : $"{localizedText?["ExpressInstall"]?["WiiRoomConfiguration"]?["stepTitle"]}";
            AnsiConsole.MarkupLine($"[bold]{stepNumber}: {step1aTitle}[/]\n");

            // Instructions Text
            string instructions = patcherLang == PatcherLanguage.en
                ? "For [bold]Wii Room[/], which language would you like to select?"
                : $"{localizedText?["ExpressInstall"]?["WiiRoomConfiguration"]?["instructions"]}";
            AnsiConsole.MarkupLine($"{instructions}\n");

            // User Choices
            string english = patcherLang == PatcherLanguage.en
                ? "English"
                : $"{localizedText?["ExpressInstall"]?["WiiRoomConfiguration"]?["english"]}";
            string spanish = patcherLang == PatcherLanguage.en
                ? "Español"
                : $"{localizedText?["ExpressInstall"]?["WiiRoomConfiguration"]?["spanish"]}";
            string french = patcherLang == PatcherLanguage.en
                ? "Français"
                : $"{localizedText?["ExpressInstall"]?["WiiRoomConfiguration"]?["french"]}";
            string german = patcherLang == PatcherLanguage.en
                ? "Deutsch"
                : $"{localizedText?["ExpressInstall"]?["WiiRoomConfiguration"]?["german"]}";
            string italian = patcherLang == PatcherLanguage.en
                ? "Italiano"
                : $"{localizedText?["ExpressInstall"]?["WiiRoomConfiguration"]?["italian"]}";
            string dutch = patcherLang == PatcherLanguage.en
                ? "Nederlands"
                : $"{localizedText?["ExpressInstall"]?["WiiRoomConfiguration"]?["dutch"]}";
            string portuguese = patcherLang == PatcherLanguage.en
                ? "Português (Brasil)"
                : $"{localizedText?["ExpressInstall"]?["WiiRoomConfiguration"]?["portuguese"]}";
            string russian = patcherLang == PatcherLanguage.en
                ? "Русский"
                : $"{localizedText?["ExpressInstall"]?["WiiRoomConfiguration"]?["russian"]}";
            string goBackToMainMenu = patcherLang == PatcherLanguage.en
                ? "Go Back to Main Menu"
                : $"{localizedText?["ExpressInstall"]?["WiiRoomConfiguration"]?["goBackToMainMenu"]}";

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
                    wiiRoomLang = Language.English;
                    DemaeConfiguration();
                    break;
                case 2:
                    wiiRoomLang = Language.Spanish;
                    DemaeConfiguration();
                    break;
                case 3:
                    wiiRoomLang = Language.French;
                    DemaeConfiguration();
                    break;
                case 4:
                    wiiRoomLang = Language.German;
                    DemaeConfiguration();
                    break;
                case 5:
                    wiiRoomLang = Language.Italian;
                    DemaeConfiguration();
                    break;
                case 6:
                    wiiRoomLang = Language.Dutch;
                    DemaeConfiguration();
                    break;
                case 7:
                    wiiRoomLang = Language.Portuguese;
                    DemaeConfiguration();
                    break;
                case 8:
                    wiiRoomLang = Language.Russian;
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
            string EIHeader = patcherLang == PatcherLanguage.en
                ? "Express Install"
                : $"{localizedText?["ExpressInstall"]?["Header"]}";
            AnsiConsole.MarkupLine($"[bold springgreen2_1]{EIHeader}[/]\n");

            // Step 2B Text
            string stepNumber = patcherLang == PatcherLanguage.en
                ? "Step 2B"
                : $"{localizedText?["ExpressInstall"]?["DemaeConfiguration"]?["stepNum"]}";
            string step1bTitle = patcherLang == PatcherLanguage.en
                ? "Choose Food Channel version"
                : $"{localizedText?["ExpressInstall"]?["DemaeConfiguration"]?["stepTitle"]}";
            AnsiConsole.MarkupLine($"[bold]{stepNumber}: {step1bTitle}[/]\n");

            // Instructions Text
            string instructions = patcherLang == PatcherLanguage.en
                ? "For [bold]Food Channel[/], which version would you like to install?"
                : $"{localizedText?["ExpressInstall"]?["DemaeConfiguration"]?["instructions"]}";
            AnsiConsole.MarkupLine($"{instructions}\n");

            // User Choices
            string demaeStandard = patcherLang == PatcherLanguage.en
                ? "Standard [bold](Fake Ordering)[/]"
                : $"{localizedText?["ExpressInstall"]?["DemaeConfiguration"]?["demaeStandard"]}";
            string demaeDominos = patcherLang == PatcherLanguage.en
                ? "Domino's [bold](US and Canada only)[/]"
                : $"{localizedText?["ExpressInstall"]?["DemaeConfiguration"]?["demaeDominos"]}";
            string goBackToMainMenu = patcherLang == PatcherLanguage.en
                ? "Go Back to Main Menu"
                : $"{localizedText?["ExpressInstall"]?["DemaeConfiguration"]?["goBackToMainMenu"]}";

            AnsiConsole.MarkupLine($"1. {demaeStandard}");
            AnsiConsole.MarkupLine($"2. {demaeDominos}\n");

            AnsiConsole.MarkupLine($"3. {goBackToMainMenu}\n");

            int choice = UserChoose("123");
            switch (choice)
            {
                case 1:
                    demaeVersion = DemaeVersion.Standard;
                    ChoosePlatform();
                    break;
                case 2:
                    demaeVersion = DemaeVersion.Dominos;
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
            string EIHeader = patcherLang == PatcherLanguage.en
                ? "Express Install"
                : $"{localizedText?["ExpressInstall"]?["Header"]}";
            AnsiConsole.MarkupLine($"[bold springgreen2_1]{EIHeader}[/]\n");

            // Would you like to install WiiLink's regional channel services Text
            string wouldYouLike = patcherLang == PatcherLanguage.en
                ? "Would you like to install [bold][springgreen2_1]WiiLink[/]'s regional channel services[/]?"
                : $"{localizedText?["ExpressInstall"]?["WiiLinkSetup"]?["wouldYouLike"]}";
            AnsiConsole.MarkupLine($"{wouldYouLike}\n");

            // Services that would be installed Text
            string toBeInstalled = patcherLang == PatcherLanguage.en
                ? "Services that would be installed:"
                : $"{localizedText?["ExpressInstall"]?["WiiLinkSetup"]?["toBeInstalled"]}";
            AnsiConsole.MarkupLine($"{toBeInstalled}\n");

            // Channel Names
            string wiiRoom = patcherLang == PatcherLanguage.en
                ? "Wii Room [bold](Wii no Ma)[/]"
                : $"{localizedText?["ExpressInstall"]?["WiiLinkSetup"]?["WiiRoom"]}";
            string photoPrints = patcherLang == PatcherLanguage.en
                ? "Photo Prints Channel [bold](Digicam Print Channel)[/]"
                : $"{localizedText?["ExpressInstall"]?["WiiLinkSetup"]?["PhotoPrints"]}";
            string foodChannel = patcherLang == PatcherLanguage.en
                ? "Food Channel [bold](Demae Channel)[/]"
                : $"{localizedText?["ExpressInstall"]?["WiiLinkSetup"]?["FoodChannel"]}";
            string kirbyTV = patcherLang == PatcherLanguage.en
                ? "Kirby TV Channel"
                : $"{localizedText?["ExpressInstall"]?["WiiLinkSetup"]?["KirbyTV"]}";

            AnsiConsole.MarkupLine($"  ● {wiiRoom}");
            AnsiConsole.MarkupLine($"  ● {photoPrints}");
            AnsiConsole.MarkupLine($"  ● {foodChannel}");
            AnsiConsole.MarkupLine($"  ● {kirbyTV}\n");

            // Yes or No Text
            string yes = patcherLang == PatcherLanguage.en
                ? "Yes"
                : $"{localizedText?["ExpressInstall"]?["WiiLinkSetup"]?["yes"]}";
            string no = patcherLang == PatcherLanguage.en
                ? "No"
                : $"{localizedText?["ExpressInstall"]?["WiiLinkSetup"]?["no"]}";

            Console.WriteLine($"1. {yes}");
            Console.WriteLine($"2. {no}\n");

            // Go Back to Main Menu Text
            string goBackToMainMenu = patcherLang == PatcherLanguage.en
                ? "Go Back to Main Menu"
                : $"{localizedText?["ExpressInstall"]?["WiiLinkSetup"]?["goBackToMainMenu"]}";
            Console.WriteLine($"3. {goBackToMainMenu}\n");

            int choice = UserChoose("123");
            switch (choice)
            {
                case 1:
                    installRegionalChannels = true;
                    WiiLinkChannels_LangSetup();
                    break;
                case 2:
                    installRegionalChannels = false;
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
            string EIHeader = patcherLang == PatcherLanguage.en
                ? "Express Install"
                : $"{localizedText?["ExpressInstall"]?["Header"]}";
            AnsiConsole.MarkupLine($"[bold springgreen2_1]{EIHeader}[/]\n");

            // Welcome the user to the Express Install of WiiLink
            string welcome = patcherLang == PatcherLanguage.en
                ? "[bold]Welcome to the Express Install of [springgreen2_1]WiiLink[/]![/]"
                : $"{localizedText?["ExpressInstall"]?["WC24Setup"]?["welcome"]}";
            AnsiConsole.MarkupLine($"{welcome}\n");

            // Step 1 Text
            string stepNum = patcherLang == PatcherLanguage.en
                ? "Step 1"
                : $"{localizedText?["ExpressInstall"]?["WC24Setup"]?["stepNum"]}";
            string stepTitle = patcherLang == PatcherLanguage.en
                ? "Choose region for WiiConnect24 services"
                : $"{localizedText?["ExpressInstall"]?["WC24Setup"]?["stepTitle"]}";

            AnsiConsole.MarkupLine($"[bold]{stepNum}: {stepTitle}[/]\n");

            // Instructions Text
            string instructions = patcherLang == PatcherLanguage.en
                ? "For the WiiConnect24 services, which region would you like to install?"
                : $"{localizedText?["ExpressInstall"]?["WC24Setup"]?["instructions"]}";
            AnsiConsole.MarkupLine($"{instructions}\n");

            // User Choices
            string northAmerica = patcherLang == PatcherLanguage.en
                ? "North America (NTSC-U)"
                : $"{localizedText?["ExpressInstall"]?["WC24Setup"]?["northAmerica"]}";
            string pal = patcherLang == PatcherLanguage.en
                ? "Europe (PAL)"
                : $"{localizedText?["ExpressInstall"]?["WC24Setup"]?["pal"]}";
            string japan = patcherLang == PatcherLanguage.en
                ? "Japan (NTSC-J)"
                : $"{localizedText?["ExpressInstall"]?["WC24Setup"]?["japan"]}";
            string goBackToMainMenu = patcherLang == PatcherLanguage.en
                ? "Go Back to Main Menu"
                : $"{localizedText?["ExpressInstall"]?["WC24Setup"]?["goBackToMainMenu"]}";

            AnsiConsole.MarkupLine($"1. {northAmerica}");
            AnsiConsole.MarkupLine($"2. {pal}");
            AnsiConsole.MarkupLine($"3. {japan}\n");

            AnsiConsole.MarkupLine($"4. {goBackToMainMenu}\n");

            int choice = UserChoose("1234");
            switch (choice)
            {
                case 1: // USA
                    wc24_reg = Region.USA;
                    WiiLinkRegionalChannelsSetup();
                    break;
                case 2: // PAL
                    wc24_reg = Region.PAL;
                    WiiLinkRegionalChannelsSetup();
                    break;
                case 3: // Japan
                    wc24_reg = Region.Japan;
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
            string EIHeader = patcherLang == PatcherLanguage.en
                ? "Express Install"
                : $"{localizedText?["ExpressInstall"]?["Header"]}";
            AnsiConsole.MarkupLine($"[bold springgreen2_1]{EIHeader}[/]\n");

            // Change step number depending on if WiiConnect24 is being installed or not
            string stepNum = patcherLang == PatcherLanguage.en
                ? !installRegionalChannels ? "Step 2" : "Step 3"
                : $"{localizedText?["ExpressInstall"]?["ChoosePlatform"]?[!installRegionalChannels ? "ifNoWC24" : "ifWC24"]?["stepNum"]}";
            string stepTitle = patcherLang == PatcherLanguage.en
                ? "Choose console platform"
                : $"{localizedText?["ExpressInstall"]?["ChoosePlatform"]?["stepTitle"]}";

            AnsiConsole.MarkupLine($"[bold]{stepNum}: {stepTitle}[/]\n");

            // Instructions Text
            string instructions = patcherLang == PatcherLanguage.en
                ? "Which Wii version are you installing to?"
                : $"{localizedText?["ExpressInstall"]?["ChoosePlatform"]?["instructions"]}";
            AnsiConsole.MarkupLine($"{instructions}\n");

            // User Choices
            string wii = patcherLang == PatcherLanguage.en
                ? "Wii [bold][/]"
                : $"{localizedText?["ExpressInstall"]?["ChoosePlatform"]?["wii"]}";
            string vWii = patcherLang == PatcherLanguage.en
                ? "vWii [bold](Wii U)[/]"
                : $"{localizedText?["ExpressInstall"]?["ChoosePlatform"]?["vWii"]}";
            string Dolphin = patcherLang == PatcherLanguage.en
                ? "Dolphin Emulator[bold][/]"
                : $"{localizedText?["ExpressInstall"]?["ChoosePlatform"]?["dolphin"]}";
            string goBackToMainMenu = patcherLang == PatcherLanguage.en
                ? "Go Back to Main Menu"
                : $"{localizedText?["ExpressInstall"]?["ChoosePlatform"]?["goBackToMainMenu"]}";

            AnsiConsole.MarkupLine($"1. {wii}");
            AnsiConsole.MarkupLine($"2. {vWii}");
            AnsiConsole.MarkupLine($"3. {Dolphin}\n");

            AnsiConsole.MarkupLine($"4. {goBackToMainMenu}\n");

            int choice = UserChoose("1234");
            switch (choice)
            {
                case 1:
                    platformType = Platform.Wii;
                    SDSetup();
                    break;
                case 2:
                    platformType = Platform.vWii;
                    SDSetup();
                    break;
                case 3:
                    platformType = Platform.Dolphin;
                        sdcard = null;
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
            string stepNum = patcherLang == PatcherLanguage.en
                ? isCustomSetup ? "Step 4" : (!installRegionalChannels ? "Step 3" : "Step 4")
                : $"{localizedText?["SDSetup"]?[isCustomSetup ? "ifCustom" : "ifExpress"]?[installRegionalChannels ? "ifWC24" : "ifNoWC24"]?["stepNum"]}";

            // Change header depending on if it's Express Install or Custom Install
            string installType = patcherLang == PatcherLanguage.en
                ? isCustomSetup ? "Custom Install" : "Express Install"
                : isCustomSetup ? $"{localizedText?["ExpressInstall"]?["Header"]}" : $"{localizedText?["CustomInstall"]?["Header"]}";

            // Step title
            string stepTitle = patcherLang == PatcherLanguage.en
                ? "Insert SD Card / USB Drive (if applicable)"
                : $"{localizedText?["SDSetup"]?["stepTitle"]}";

            // After passing this step text
            string afterPassingThisStep = patcherLang == PatcherLanguage.en
                ? "After passing this step, any user interaction won't be needed, so sit back and relax!"
                : $"{localizedText?["SDSetup"]?["afterPassingThisStep"]}";

            // Download to SD card text
            string downloadToSD = patcherLang == PatcherLanguage.en
                ? "You can download everything directly to your Wii SD Card / USB Drive if you insert it before starting the patching\nprocess. Otherwise, everything will be saved in the same folder as this patcher on your computer."
                : $"{localizedText?["SDSetup"]?["downloadToSD"]}";



            // SD card detected text
            string sdDetected = patcherLang == PatcherLanguage.en
                ? sdcard != null ? $"SD card detected: [bold springgreen2_1]{sdcard}[/]" : ""
                : sdcard != null ? $"{localizedText?["SDSetup"]?["sdDetected"]}: [bold springgreen2_1]{sdcard}[/]" : "";

            // Go Back to Main Menu Text
            string goBackToMainMenu = patcherLang == PatcherLanguage.en
                ? "Go Back to Main Menu"
                : $"{localizedText?["ExpressInstall"]?["SDSetup"]?["goBackToMainMenu"]}";

            AnsiConsole.MarkupLine($"[bold springgreen2_1]{installType}[/]\n");

            AnsiConsole.MarkupLine($"[bold]{stepNum}: {stepTitle}[/]\n");

            Console.WriteLine($"{afterPassingThisStep}\n");

            Console.WriteLine($"{downloadToSD}\n");

            if (platformType == Platform.vWii && !isCustomSetup)
            {
                string eulaChannel = patcherLang == PatcherLanguage.en
                ? "[bold]NOTE:[/] For [bold deepskyblue1]vWii[/] users, The EULA channel will also be included."
                : $"{localizedText?["ExpressInstall"]?["SDSetup"]?["eulaChannel"]}";
                AnsiConsole.MarkupLine($"{eulaChannel}\n");
            }

            // User Choices
            string startOption = patcherLang == PatcherLanguage.en
                ? sdcard != null ? "Start [bold]with[/] SD Card / USB Drive" : "Start [bold]without[/] SD Card / USB Drive"
                : sdcard != null ? $"{localizedText?["SDSetup"]?["start_withSD"]}" : $"{localizedText?["SDSetup"]?["start_noSD"]}";
            string startWithoutSDOption = patcherLang == PatcherLanguage.en
                ? "Start [bold]without[/] SD Card / USB Drive"
                : $"{localizedText?["SDSetup"]?["start_noSD"]}";
            string manualDetection = patcherLang == PatcherLanguage.en
                ? "Manually Select SD Card / USB Drive Path\n"
                : $"{localizedText?["SDSetup"]?["manualDetection"]}";

            AnsiConsole.MarkupLine($"1. {startOption}");
            AnsiConsole.MarkupLine($"2. {(sdcard != null ? startWithoutSDOption : manualDetection)}");
            AnsiConsole.MarkupLine($"3. {(sdcard != null ? manualDetection : goBackToMainMenu)}");

            if (sdcard != null)
            {
                AnsiConsole.MarkupLine($"4. {goBackToMainMenu}\n");

                AnsiConsole.MarkupLine($"{sdDetected}");
            }

            AnsiConsole.MarkupLine("");
            int choice = sdcard != null ? UserChoose("1234") : UserChoose("123");

            switch (choice)
            {
                case 1: // Check if WAD folder exists before starting patching process
                    WADFolderCheck(isCustomSetup);
                    break;
                case 2: // Start patching process without SD card or Manually select SD card
                    if (sdcard != null)
                    {
                        sdcard = null;
                        WADFolderCheck(isCustomSetup);
                    }
                    else
                    {
                        SDCardSelect();
                    }
                    break;
                case 3: // Manually select SD card or Go back to main menu
                    if (sdcard != null)
                    {
                        SDCardSelect();
                    }
                    else
                    {
                        MainMenu();
                    }
                    break;
                case 4: // Go back to main menu
                    if (sdcard != null)
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
                string installType = patcherLang == PatcherLanguage.en
                    ? isCustomSetup ? "Custom Install" : "Express Install"
                    : isCustomSetup ? $"{localizedText?["ExpressInstall"]?["Header"]}" : $"{localizedText?["CustomInstall"]?["Header"]}";
                string stepNum = patcherLang == PatcherLanguage.en
                    ? isCustomSetup ? "Step 5" : (!installRegionalChannels ? "Step 4" : "Step 5")
                    : isCustomSetup ? $"{localizedText?["WADFolderCheck"]?["ifCustom"]?["stepNum"]}" : $"{localizedText?["WADFolderCheck"]?["ifExpress"]?[installRegionalChannels ? "ifWC24" : "ifNoWC24"]?["stepNum"]}";

                AnsiConsole.MarkupLine($"[bold springgreen2_1]{installType}[/]\n");

                // Step title
                string stepTitle = patcherLang == PatcherLanguage.en
                    ? "WAD folder detected"
                    : $"{localizedText?["WADFolderCheck"]?["stepTitle"]}";

                AnsiConsole.MarkupLine($"[bold]{stepNum}: {stepTitle}[/]\n");

                // WAD folder detected text
                string wadFolderDetected = patcherLang == PatcherLanguage.en
                    ? "A [bold]WAD[/] folder has been detected in the current directory. This folder is used to store the WAD files that are downloaded during the patching process. If you choose to delete this folder, it will be recreated when you start the patching process again."
                    : $"{localizedText?["WADFolderCheck"]?["wadFolderDetected"]}";

                AnsiConsole.MarkupLine($"{wadFolderDetected}\n");

                // User Choices
                string deleteWADFolder = patcherLang == PatcherLanguage.en
                    ? "Delete WAD folder"
                    : $"{localizedText?["WADFolderCheck"]?["deleteWADFolder"]}";
                string keepWADFolder = patcherLang == PatcherLanguage.en
                    ? "Keep WAD folder"
                    : $"{localizedText?["WADFolderCheck"]?["keepWADFolder"]}";
                string goBackToMainMenu = patcherLang == PatcherLanguage.en
                    ? "Go Back to Main Menu"
                    : $"{localizedText?["WADFolderCheck"]?["goBackToMainMenu"]}";

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
                            string pressAnyKey = patcherLang == PatcherLanguage.en
                                ? "Press any key to try again..."
                                : $"{localizedText?["WADFolderCheck"]?["pressAnyKey"]}";
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
        if (!Directory.Exists(tempDir))
            Directory.CreateDirectory(tempDir);

        // Demae version text to be used in the patching progress for Demae Channel (eg. "Food Channel (English) [Standard]")
        string demaeVerTxt = demaeVersion == DemaeVersion.Standard
            ? "Standard"
            : "Domino's";

        // Define WiiLink channels titles
        string demae_title = patcherLang == PatcherLanguage.en // Demae Channel
            ? lang == Language.English
                ? $"Food Channel [bold](English)[/] [bold][[{demaeVerTxt}]][/]"
                : $"Demae Channel [bold](Japanese)[/] [bold][[{demaeVerTxt}]][/]"
            : $"{localizedText?["ChannelNames"]?[$"{lang}]?[${(lang == Language.English ? "food" : "demae")}"]} [bold]({lang})[/] [bold][[{demaeVerTxt}]][/]";

        string wiiroom_title = patcherLang == PatcherLanguage.en // Wii no Ma
            ? wiiRoomLang != Language.Japan
                ? $"Wii Room [bold]({wiiRoomLang})[/]"
                : "Wii no Ma [bold](Japanese)[/]"
            : $"{localizedText?["ChannelNames"]?[$"{wiiRoomLang}"]?[$"{(wiiRoomLang != Language.Japan ? "wiiRoom" : "wiiNoMa")}"]} [bold]({wiiRoomLang})[/]";

        string digicam_title = patcherLang == PatcherLanguage.en // Digicam Print Channel
            ? lang == Language.English
                ? "Photo Prints Channel [bold](English)[/]"
                : "Digicam Print Channel [bold](Japanese)[/]"
            : $"{localizedText?["ChannelNames"]?[$"{lang}"]?[$"{(lang == Language.English ? "photoPrints" : "digicam")}"]} [bold]({lang})[/]";

        string kirbytv_title = "Kirby TV Channel"; // Kirby TV Channel

        // Define the channelMessages dictionary with WC24 channel titles
        string internationalOrJapanese = (wc24_reg == Region.USA || wc24_reg == Region.PAL) ? "International" : "Japanese";
        string NCTitle, forecastTitle, newsTitle, evcTitle, cmocTitle;

        if (patcherLang == PatcherLanguage.en)
        {
            NCTitle = $"{(wc24_reg == Region.USA || wc24_reg == Region.PAL ? "Nintendo Channel" : "Minna no Nintendo Channel")} [bold]({wc24_reg})[/]";
            forecastTitle = $"Forecast Channel [bold]({wc24_reg})[/]";
            newsTitle = $"News Channel [bold]({wc24_reg})[/]";
            evcTitle = $"Everybody Votes Channel [bold]({wc24_reg})[/]";
            cmocTitle = $"{(wc24_reg == Region.USA ? "Check Mii Out Channel" : "Mii Contest Channel")} [bold]({wc24_reg})[/]";
        }
        else
        {
            NCTitle = $"{localizedText?["ChannelNames"]?[internationalOrJapanese]?["nintendoChn"]} [bold]({wc24_reg})[/]";
            forecastTitle = $"{localizedText?["ChannelNames"]?["International"]?["forecastChn"]} [bold]({wc24_reg})[/]";
            newsTitle = $"{localizedText?["ChannelNames"]?["International"]?["newsChn"]} [bold]({wc24_reg})[/]";
            evcTitle = $"{localizedText?["ChannelNames"]?["International"]?["everybodyVotes"]} [bold]({wc24_reg})[/]";
            cmocTitle = $"{localizedText?["ChannelNames"]?["International"]?["cmoc"]} [bold]({wc24_reg})[/]";
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
        if (installRegionalChannels)
        {
            channelMessages.Add("wiiroom", wiiroom_title);
            channelMessages.Add("digicam", digicam_title);
            channelMessages.Add("demae", demae_title);
            channelMessages.Add("kirbytv", kirbytv_title);
        }

        // Setup patching process list
        var patching_functions = new List<Action>
        {
            DownloadAllPatches,
            () => NC_Patch(wc24_reg),
            () => Forecast_Patch(wc24_reg),
            () => News_Patch(wc24_reg),
            () => EVC_Patch(wc24_reg),
            () => CheckMiiOut_Patch(wc24_reg)
        };

        // Add other patching functions if applicable
        if (installRegionalChannels)
        {
            patching_functions.Add(() => WiiRoom_Patch(wiiRoomLang));
            patching_functions.Add(() => Digicam_Patch(lang));
            patching_functions.Add(() => Demae_Patch(lang, demaeVersion));
            patching_functions.Add(KirbyTV_Patch);
        }

        patching_functions.Add(FinishSDCopy);

        //// Set up patching progress dictionary ////
        // Flush dictionary and downloading patches
        patchingProgress_express.Clear();
        patchingProgress_express.Add("downloading", "in_progress");

        // Patching WiiConnect24 channels
        foreach (string channel in new string[] { "nc", "forecast", "news", "evc", "cmoc" })
            patchingProgress_express.Add(channel, "not_started");

        if (installRegionalChannels)
        {
            // Patching Regional Channels
            foreach (string channel in new string[] { "wiiroom", "digicam", "demae", "kirbytv" })
                patchingProgress_express.Add(channel, "not_started");
        }

        // Finishing up
        patchingProgress_express.Add("finishing", "not_started");

        // While the patching process is not finished
        while (patchingProgress_express["finishing"] != "done")
        {
            PrintHeader();

            // Progress bar and completion display
            string patching = patcherLang == PatcherLanguage.en
                ? "Patching... this can take some time depending on the processing speed (CPU) of your computer."
                : $"{localizedText?["PatchingProgress"]?["patching"]}";
            string progress = patcherLang == PatcherLanguage.en
                ? "Progress"
                : $"{localizedText?["PatchingProgress"]?["progress"]}";
            AnsiConsole.MarkupLine($"[bold][[*]] {patching}[/]\n");
            AnsiConsole.Markup($"    {progress}: ");

            //// Progress bar and completion display ////
            // Calculate percentage based on how many channels are completed
            int percentage = (int)((float)patchingProgress_express.Where(x => x.Value == "done").Count() / (float)patchingProgress_express.Count * 100.0f);

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
            string percentComplete = patcherLang == PatcherLanguage.en
                ? "completed"
                : $"{localizedText?["PatchingProgress"]?["percentComplete"]}";
            string pleaseWait = patcherLang == PatcherLanguage.en
                ? "Please wait while the patching process is in progress..."
                : $"{localizedText?["PatchingProgress"]?["pleaseWait"]}";
            AnsiConsole.Markup($" [bold]{percentage}%[/] {percentComplete}\n\n");
            AnsiConsole.MarkupLine($"{pleaseWait}\n");

            //// Display progress for each channel ////

            // Pre-Patching Section: Downloading files
            string prePatching = patcherLang == PatcherLanguage.en
                ? "Pre-Patching"
                : $"{localizedText?["PatchingProgress"]?["prePatching"]}";
            string downloadingFiles = patcherLang == PatcherLanguage.en
                ? "Downloading files..."
                : $"{localizedText?["PatchingProgress"]?["downloadingFiles"]}";
            AnsiConsole.MarkupLine($"[bold]{prePatching}:[/]");
            switch (patchingProgress_express["downloading"])
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
            string patchingWiiConnect24Channels = patcherLang == PatcherLanguage.en
                ? "Patching WiiConnect24 Channels"
                : $"{localizedText?["PatchingProgress"]?["patchingWiiConnect24Channels"]}";

            AnsiConsole.MarkupLine($"\n[bold]{patchingWiiConnect24Channels}:[/]");
            foreach (string channel in new string[] { "nc", "forecast", "news", "evc", "cmoc" })
            {
                switch (patchingProgress_express[channel])
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
            if (installRegionalChannels)
            {
                string patchingWiiLinkChannels = patcherLang == PatcherLanguage.en
                    ? "Patching Regional Channels"
                    : $"{localizedText?["PatchingProgress"]?["patchingWiiLinkChannels"]}";
                AnsiConsole.MarkupLine($"\n[bold]{patchingWiiLinkChannels}:[/]");
                foreach (string channel in new string[] { "wiiroom", "digicam", "demae", "kirbytv" })
                {
                    switch (patchingProgress_express[channel])
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
            string postPatching = patcherLang == PatcherLanguage.en
                ? "Post-Patching"
                : $"{localizedText?["PatchingProgress"]?["postPatching"]}";
            string finishingUp = patcherLang == PatcherLanguage.en
                ? "Finishing up..."
                : $"{localizedText?["PatchingProgress"]?["finishingUp"]}";
            AnsiConsole.MarkupLine($"\n[bold]{postPatching}:[/]");
            switch (patchingProgress_express["finishing"])
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
        task = "Patching channels...";
        int counter_done = 0;
        int partCompleted = 0;

        // List of channels to patch
        List<string> channelsToPatch = [.. wiiConnect24Channels_selection, .. wiiLinkChannels_selection];


        // Set up patching progress dictionary
        patchingProgress_custom.Clear(); // Flush dictionary
        patchingProgress_custom.Add("downloading", "in_progress"); // Downloading patches
        foreach (string channel in channelsToPatch) // Patching channels
            patchingProgress_custom.Add(channel, "not_started");
        patchingProgress_custom.Add("finishing", "not_started"); // Finishing up

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
            { "food_en", "Food Channel [bold](Standard) [[English]][/]" },
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
            { "kirbytv", "Kirby TV Channel" }
        };

        // Setup patching process arrays based on the selected channels
        var channelPatchingFunctions = new Dictionary<string, Action>()
        {
            { "wiiroom_en", () => WiiRoom_Patch(Language.English) },
            { "wiiroom_es", () => WiiRoom_Patch(Language.Spanish) },
            { "wiinoma_jp", () => WiiRoom_Patch(Language.Japan) },
            { "wiiroom_fr", () => WiiRoom_Patch(Language.French) },
            { "wiiroom_de", () => WiiRoom_Patch(Language.German) },
            { "wiiroom_it", () => WiiRoom_Patch(Language.Italian) },
            { "wiiroom_du", () => WiiRoom_Patch(Language.Dutch) },
            { "wiiroom_ptbr", () => WiiRoom_Patch(Language.Portuguese) },
            { "wiiroom_ru", () => WiiRoom_Patch(Language.Russian) },
            { "digicam_en", () => Digicam_Patch(Language.English) },
            { "digicam_jp", () => Digicam_Patch(Language.Japan) },
            { "food_en", () => Demae_Patch(Language.English, DemaeVersion.Standard) },
            { "demae_jp", () => Demae_Patch(Language.Japan, DemaeVersion.Standard) },
            { "food_dominos", () => Demae_Patch(Language.English, DemaeVersion.Dominos) },
            { "kirbytv", KirbyTV_Patch },
            { "nc_us", () => NC_Patch(Region.USA) },
            { "nc_eu", () => NC_Patch(Region.PAL) },
            { "mnnc_jp", () => NC_Patch(Region.Japan) },
            { "forecast_us", () => Forecast_Patch(Region.USA) },
            { "forecast_eu", () => Forecast_Patch(Region.PAL) },
            { "forecast_jp", () => Forecast_Patch(Region.Japan) },
            { "news_us", () => News_Patch(Region.USA) },
            { "news_eu", () => News_Patch(Region.PAL) },
            { "news_jp", () => News_Patch(Region.Japan) },
            { "evc_us", () => EVC_Patch(Region.USA) },
            { "evc_eu", () => EVC_Patch(Region.PAL) },
            { "evc_jp", () => EVC_Patch(Region.Japan) },
            { "cmoc_us", () => CheckMiiOut_Patch(Region.USA) },
            { "cmoc_eu", () => CheckMiiOut_Patch(Region.PAL) },
            { "cmoc_jp", () => CheckMiiOut_Patch(Region.Japan) }
        };

        // Create a list of patching functions to execute
        var selectedPatchingFunctions = new List<Action>
        {
            // Add the patching functions to the list
            () => DownloadCustomPatches(channelsToPatch)
        };

        foreach (string selectedChannel in channelsToPatch)
            selectedPatchingFunctions.Add(channelPatchingFunctions[selectedChannel]);

        selectedPatchingFunctions.Add(FinishSDCopy);

        // Start patching
        int totalChannels = channelsToPatch.Count;
        while (patchingProgress_custom["finishing"] != "done")
        {
            PrintHeader();

            // Progress text
            string patching = patcherLang == PatcherLanguage.en
                ? "Patching... this can take some time depending on the processing speed (CPU) of your computer."
                : $"{localizedText?["PatchingProgress"]?["patching"]}";
            string progress = patcherLang == PatcherLanguage.en
                ? "Progress"
                : $"{localizedText?["PatchingProgress"]?["progress"]}";
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
            string percentComplete = patcherLang == PatcherLanguage.en
                ? "completed"
                : $"{localizedText?["PatchingProgress"]?["percentComplete"]}";
            string pleaseWait = patcherLang == PatcherLanguage.en
                ? "Please wait while the patching process is in progress..."
                : $"{localizedText?["PatchingProgress"]?["pleaseWait"]}";
            AnsiConsole.Markup($" [bold]{percentage}%[/] {percentComplete}\n\n");
            AnsiConsole.MarkupLine($"{pleaseWait}\n");

            //// Display progress for each channel ////

            // Pre-Patching Section: Downloading files
            string prePatching = patcherLang == PatcherLanguage.en
                ? "Pre-Patching"
                : $"{localizedText?["PatchingProgress"]?["prePatching"]}";
            string downloadingFiles = patcherLang == PatcherLanguage.en
                ? "Downloading files..."
                : $"{localizedText?["PatchingProgress"]?["downloadingFiles"]}";
            AnsiConsole.MarkupLine($"[bold]{prePatching}:[/]");
            switch (patchingProgress_custom["downloading"])
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
            if (wiiConnect24Channels_selection.Count > 0)
            {
                //AnsiConsole.MarkupLine("\n[bold]Patching WiiConnect24 Channels:[/]");
                string patchingWC24Channels = patcherLang == PatcherLanguage.en
                    ? "Patching WiiConnect24 Channels"
                    : $"{localizedText?["PatchingProgress"]?["patchingWC24Channels"]}";
                AnsiConsole.MarkupLine($"\n[bold]{patchingWC24Channels}:[/]");
                foreach (string wiiConnect24Channel in channelsToPatch)
                {
                    List<string> wiiConnect24Channels = ["nc_us", "nc_eu", "mnnc_jp", "forecast_us", "forecast_eu", "forecast_jp", "news_us", "news_eu", "news_jp", "evc_us", "evc_eu", "evc_jp", "cmoc_us", "cmoc_eu", "cmoc_jp"];
                    if (wiiConnect24Channels.Contains(wiiConnect24Channel))
                    {
                        switch (patchingProgress_custom[wiiConnect24Channel])
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
            if (wiiLinkChannels_selection.Count > 0)
            {
                string patchingWiiLinkChannels = patcherLang == PatcherLanguage.en
                    ? "Patching Regional Channels"
                    : $"{localizedText?["PatchingProgress"]?["patchingWiiLinkChannels"]}";
                AnsiConsole.MarkupLine($"\n[bold]{patchingWiiLinkChannels}:[/]");
                foreach (string jpnChannel in channelsToPatch)
                {
                    List<string> jpnChannels = ["wiiroom_en", "wiiroom_es", "wiiroom_fr", "wiinoma_jp", "wiiroom_de", "wiiroom_it", "wiiroom_du", "wiiroom_ptbr", "wiiroom_ru", "digicam_en", "digicam_jp", "food_en", "demae_jp", "food_dominos", "kirbytv"];
                    if (jpnChannels.Contains(jpnChannel))
                    {
                        switch (patchingProgress_custom[jpnChannel])
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

            // Post-Patching Section: Finishing up
            string postPatching = patcherLang == PatcherLanguage.en
                ? "Post-Patching"
                : $"{localizedText?["PatchingProgress"]?["postPatching"]}";
            string finishingUp = patcherLang == PatcherLanguage.en
                ? "Finishing up..."
                : $"{localizedText?["PatchingProgress"]?["finishingUp"]}";
            AnsiConsole.MarkupLine($"\n[bold]{postPatching}:[/]");
            switch (patchingProgress_custom["finishing"])
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
            task = "Moving to next patch";
            partCompleted++;

            switch (partCompleted)
            {
                case 1:
                    // If we're on the first channel, mark downloading as done and the first channel as in progress
                    patchingProgress_custom["downloading"] = "done";
                    patchingProgress_custom[channelsToPatch[0]] = "in_progress";
                    break;
                case int n when n > 1 && n < totalChannels + 1:
                    // If we're on a channel that's not the first or last, mark the previous channel as done and the current channel as in progress
                    patchingProgress_custom[channelsToPatch[partCompleted - 2]] = "done";
                    patchingProgress_custom[channelsToPatch[partCompleted - 1]] = "in_progress";
                    break;
                case int n when n == totalChannels + 1:
                    // If we're on the last channel, mark the previous channel as done and finishing as in progress
                    patchingProgress_custom[channelsToPatch[partCompleted - 2]] = "done";
                    patchingProgress_custom["finishing"] = "in_progress";
                    break;
                case int n when n == totalChannels + 2:
                    // If we're done patching, mark finishing as done
                    patchingProgress_custom["finishing"] = "done";
                    break;
            }
        }

        // We're finally done patching!
        Thread.Sleep(2000);
        Finished();
    }


    static void DownloadAllPatches()
    {
        task = "Downloading patches";

        // Download SPD if English is selected
        if (lang != Language.Japan)
            DownloadSPD(platformType);
        else
        {
            if (!Directory.Exists("WAD"))
                Directory.CreateDirectory("WAD");
        }

        //// Downloading All Channel Patches ////

        if (installRegionalChannels)
        {
            // Wii no Ma (Wii Room) //
            DownloadPatch("WiinoMa", $"WiinoMa_0_Universal.delta", $"WiinoMa_0_Universal.delta", "Wii no Ma");

            bool notRussianOrPortuguese = wiiRoomLang != Language.Russian && wiiRoomLang != Language.Portuguese;
            if (notRussianOrPortuguese)
                DownloadPatch("WiinoMa", $"WiinoMa_1_Universal.delta", $"WiinoMa_1_Universal.delta", "Wii no Ma");
            else
                DownloadPatch("WiinoMa", $"WiinoMa_1_{wiiRoomLang}.delta", $"WiinoMa_1_{wiiRoomLang}.delta", "Wii no Ma");

            // For all languages, including Japanese, use their respective patches for 2
            DownloadPatch("WiinoMa", $"WiinoMa_2_{wiiRoomLang}.delta", $"WiinoMa_2_{wiiRoomLang}.delta", "Wii no Ma");

            // Special handling for Portuguese, (patches 3, 4, and D)
            if (wiiRoomLang == Language.Portuguese)
            {
                DownloadPatch("WiinoMa", $"WiinoMa_3_{Language.Portuguese}.delta", $"WiinoMa_3_{Language.Portuguese}.delta", "Wii no Ma");
                DownloadPatch("WiinoMa", $"WiinoMa_4_{Language.Portuguese}.delta", $"WiinoMa_4_{Language.Portuguese}.delta", "Wii no Ma");
                DownloadPatch("WiinoMa", $"WiinoMa_D_{Language.Portuguese}.delta", $"WiinoMa_D_{Language.Portuguese}.delta", "Wii no Ma");
            }

            // Special handling for Russian, (patches 3, 4, 9, C, D and E)
            if (wiiRoomLang == Language.Russian)
            {
                DownloadPatch("WiinoMa", $"WiinoMa_3_{Language.Russian}.delta", $"WiinoMa_3_{Language.Russian}.delta", "Wii no Ma");
                DownloadPatch("WiinoMa", $"WiinoMa_4_{Language.Russian}.delta", $"WiinoMa_4_{Language.Russian}.delta", "Wii no Ma");
                DownloadPatch("WiinoMa", $"WiinoMa_9_{Language.Russian}.delta", $"WiinoMa_9_{Language.Russian}.delta", "Wii no Ma");
                DownloadPatch("WiinoMa", $"WiinoMa_C_{Language.Russian}.delta", $"WiinoMa_C_{Language.Russian}.delta", "Wii no Ma");
                DownloadPatch("WiinoMa", $"WiinoMa_D_{Language.Russian}.delta", $"WiinoMa_D_{Language.Russian}.delta", "Wii no Ma");
                DownloadPatch("WiinoMa", $"WiinoMa_E_{Language.Russian}.delta", $"WiinoMa_E_{Language.Russian}.delta", "Wii no Ma");
            }

            // Photo Prints Channel / Digicam Print Channel
            if (lang == Language.English)
                DownloadPatch("Digicam", $"Digicam_0_{lang}.delta", $"Digicam_0_{lang}.delta", "Digicam Print Channel");
            DownloadPatch("Digicam", $"Digicam_1_{lang}.delta", $"Digicam_1_{lang}.delta", "Digicam Print Channel");
            if (lang == Language.English)
                DownloadPatch("Digicam", $"Digicam_2_{lang}.delta", $"Digicam_2_{lang}.delta", "Digicam Print Channel");

            // Demae Channel
            switch (demaeVersion)
            {
                case DemaeVersion.Standard:
                    if (lang == Language.English)
                        DownloadPatch("Demae", $"Demae_0_{lang}.delta", $"Demae_0_{lang}.delta", "Demae Channel (Standard)");
                    DownloadPatch("Demae", $"Demae_1_{lang}.delta", $"Demae_1_{lang}.delta", "Demae Channel (Standard)");
                    if (lang == Language.English)
                        DownloadPatch("Demae", $"Demae_2_{lang}.delta", $"Demae_2_{lang}.delta", "Demae Channel (Standard)");
                    break;
                case DemaeVersion.Dominos:
                    DownloadPatch("Dominos", $"Dominos_0.delta", "Dominos_0.delta", "Demae Channel (Dominos)");
                    DownloadPatch("Dominos", $"Dominos_1.delta", "Dominos_1.delta", "Demae Channel (Dominos)");
                    DownloadPatch("Dominos", $"Dominos_2.delta", "Dominos_2.delta", "Demae Channel (Dominos)");
                    break;
            }

            // Downloading Get Console ID (for Demae Domino's) from OSC
            if (demaeVersion == DemaeVersion.Dominos)
                DownloadOSCApp("GetConsoleID");

            // Kirby TV Channel (only if user chose to install it)
            DownloadPatch("ktv", $"ktv_2.delta", "KirbyTV_2.delta", "Kirby TV Channel");
        }

        if (platformType != Platform.Dolphin) {
            // Download yawmME from OSC for installing WADs on the Wii
            DownloadOSCApp("yawmME");
        }

        if (platformType == Platform.Wii) {
            // Download sntp from OSC for Syncing the Clock on the Wii
            DownloadOSCApp("sntp");
        }

        // Download WC24 patches if applicable
        // Nintendo Channel
        DownloadPatch("nc", $"NC_1_{wc24_reg}.delta", $"NC_1_{wc24_reg}.delta", "Nintendo Channel");

        // Forecast Channel
        DownloadPatch("forecast", $"Forecast_1.delta", "Forecast_1.delta", "Forecast Channel");
        DownloadPatch("forecast", $"Forecast_5.delta", "Forecast_5.delta", "Forecast Channel");

        // News Channel
        DownloadPatch("news", $"News_1.delta", $"News_1.delta", "News Channel");

        // Download AnyGlobe_Changer from OSC for use with the Forecast Channel
        DownloadAGC();

        // Everybody Votes Channel and Region Select Channel
        DownloadPatch("evc", $"EVC_1_{wc24_reg}.delta", $"EVC_1_{wc24_reg}.delta", "Everybody Votes Channel");
        DownloadPatch("RegSel", $"RegSel_1.delta", "RegSel_1.delta", "Region Select");

        // Check Mii Out/Mii Contest Channel
        DownloadPatch("cmoc", $"CMOC_1_{wc24_reg}.delta", $"CMOC_1_{wc24_reg}.delta", "Check Mii Out Channel");

        // Download ww-43db-patcher for vWii if applicable
        if (platformType == Platform.vWii) {
            // DownloadOSCApp("ww-43db-patcher");

            // Also download EULA for each region for vWii users
            string EULATitleID = wc24_reg switch
            {
                Region.USA => "0001000848414b45",
                Region.PAL => "0001000848414b50",
                Region.Japan => "0001000848414b4a",
                _ => throw new NotImplementedException()
            };

            DownloadWC24Channel("EULA", "EULA", 3, wc24_reg, EULATitleID);

        }

        if (platformType != Platform.Dolphin) {
        // Install the RC24 Mail Patcher
            DownloadOSCApp("Mail-Patcher");
        }

        // Downloading stuff is finished!
        patchingProgress_express["downloading"] = "done";
        patchingProgress_express["nc"] = "in_progress";
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
        task = "Custom Install (Part 1 - Select WiiLink channels)";

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
            { "Food Channel [bold](Standard) [[English]][/]", "food_en" },
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
        if (combinedChannels_selection.Count == 0) // Only do this
            combinedChannels_selection = channelMap.Values.Select(_ => "[grey]Not selected[/]").ToList();

        // Page setup
        const int ITEMS_PER_PAGE = 9;
        int currentPage = 1;

        while (true)
        {
            PrintHeader();

            // Print title
            string customInstall = patcherLang == PatcherLanguage.en
                ? "Custom Install"
                : $"{localizedText?["CustomSetup"]?["Header"]}";
            AnsiConsole.MarkupLine($"[bold springgreen2_1]{customInstall}[/]\n");

            // Print step number and title
            string stepNum = patcherLang == PatcherLanguage.en
                ? "Step 1"
                : $"{localizedText?["CustomSetup"]?["wiiLinkChannels_Setup"]?["stepNum"]}";
            string stepTitle = patcherLang == PatcherLanguage.en
                ? "Select WiiConnect24 / Regional channel(s) to install"
                : $"{localizedText?["CustomSetup"]?["wiiLinkChannels_Setup"]?["stepTitle"]}";
            AnsiConsole.MarkupLine($"[bold]{stepNum}:[/] {stepTitle}\n");

            // Display WiiLink channel selection menu
            string selectWiiLinkChns = patcherLang == PatcherLanguage.en
                ? "Select WiiConnect24 / Regional channel(s) to install:"
                : $"{localizedText?["CustomSetup"]?["wiiLinkChannels_Setup"]?["selectWiiLinkChns"]}";
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
                grid.AddRow($"[bold][[{i - start + 1}]][/] {channel.Key}", combinedChannels_selection[i]);

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
                string pageNum = patcherLang == PatcherLanguage.en
                    ? $"Page {currentPage} of {totalPages}"
                    : $"{localizedText?["CustomSetup"]?["pageNum"]}"
                        .Replace("{currentPage}", currentPage.ToString())
                        .Replace("{totalPages}", totalPages.ToString());
                AnsiConsole.Markup($"[bold]{pageNum}[/] ");

                // If the current page is less than total pages, display a bold white '>?' for next page navigation
                // Otherwise, display a space '  '
                AnsiConsole.Markup(currentPage < totalPages ? "[bold white]>>[/]" : "  ");

                // Print instructions
                //AnsiConsole.MarkupLine(" [grey](Press [bold white]<-[/] or [bold white]->[/] to navigate pages)[/]\n");
                string pageInstructions = patcherLang == PatcherLanguage.en
                    ? "(Press [bold white]<-[/] or [bold white]->[/] to navigate pages)"
                    : $"{localizedText?["CustomSetup"]?["pageInstructions"]}";
                AnsiConsole.MarkupLine($" [grey]{pageInstructions}[/]\n");
            }

            // Print regular instructions
            string regInstructions = patcherLang == PatcherLanguage.en
                ? "< Press [bold white]a number[/] to select/deselect a channel, [bold white]ENTER[/] to continue, [bold white]Backspace[/] to go back, [bold white]ESC[/] to go back to exit setup >"
                : $"{localizedText?["CustomSetup"]?["regInstructions"]}";
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
            string notSelected = patcherLang == PatcherLanguage.en
                ? "Not selected"
                : $"{localizedText?["CustomSetup"]?["notSelected"]}";
            string selectedText = patcherLang == PatcherLanguage.en
                ? "Selected"
                : $"{localizedText?["CustomSetup"]?["selected"]}";

            // Handle user input
            switch (choice)
            {
                case -1: // Escape
                case -2: // Backspace
                    // Clear selection list
                    wiiLinkChannels_selection.Clear();
                    wiiConnect24Channels_selection.Clear();
                    combinedChannels_selection.Clear();
                    MainMenu();
                    break;
                case 0: // Enter
                    // Save selected channels to global variable if any are selected, divide them into WiiLink and WC24 channels
                    foreach (string channel in channelMap.Values.Where(combinedChannels_selection.Contains))
                    {
                        if (wiiLinkChannelMap.ContainsValue(channel) && !wiiLinkChannels_selection.Contains(channel))
                            wiiLinkChannels_selection.Add(channel);
                        else if (wc24ChannelMap.ContainsValue(channel) && !wiiConnect24Channels_selection.Contains(channel))
                            wiiConnect24Channels_selection.Add(channel);
                    }
                    // If selection is empty, display error message
                    if (!channelMap.Values.Any(combinedChannels_selection.Contains))
                    {
                        //AnsiConsole.MarkupLine("\n[bold red]ERROR:[/] You must select at least one channel to proceed!");
                        string mustSelectOneChannel = patcherLang == PatcherLanguage.en
                            ? "[bold red]ERROR:[/] You must select at least one channel to proceed!"
                            : $"{localizedText?["CustomSetup"]?["mustSelectOneChannel"]}";
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
                        if (combinedChannels_selection.Contains(channelName))
                        {
                            combinedChannels_selection = combinedChannels_selection.Where(val => val != channelName).ToList();
                            combinedChannels_selection[index] = $"[grey]{notSelected}[/]";
                        }
                        else
                        {
                            combinedChannels_selection = combinedChannels_selection.Append(channelName).ToList();
                            combinedChannels_selection[index] = $"[bold springgreen2_1]{selectedText}[/]";
                        }
                    }
                    break;
            }
        }
    }

    // Custom Install (Part 2 - Select Console Platform)
    static void CustomInstall_ConsolePlatform_Setup()
    {
        task = "Custom Install (Part 2 - Select Console Platform)";
        while (true)
        {
            PrintHeader();

            // Print title
            string customInstall = patcherLang == PatcherLanguage.en
                ? "Custom Install"
                : $"{localizedText?["CustomSetup"]?["Header"]}";
            AnsiConsole.MarkupLine($"[bold springgreen2_1]{customInstall}[/]\n");

            // Print step number and title
            string stepNum = patcherLang == PatcherLanguage.en
                ? "Step 2"
                : $"{localizedText?["CustomSetup"]?["ConsolePlatform_Setup"]?["stepNum"]}";
            string stepTitle = patcherLang == PatcherLanguage.en
                ? "Select Console Platform"
                : $"{localizedText?["CustomSetup"]?["ConsolePlatform_Setup"]?["stepTitle"]}";
            AnsiConsole.MarkupLine($"[bold]{stepNum}:[/] {stepTitle}\n");

            // Display console platform selection menu
            string selectConsolePlatform = patcherLang == PatcherLanguage.en
                ? "Which console platform are you installing these channels on?"
                : $"{localizedText?["CustomSetup"]?["ConsolePlatform_Setup"]?["selectConsolePlatform"]}";
            AnsiConsole.MarkupLine($"[bold]{selectConsolePlatform}[/]\n");

            // Print Console Platform options
            string onWii = patcherLang == PatcherLanguage.en
                ? "[bold]Wii[/]"
                : $"{localizedText?["CustomSetup"]?["ConsolePlatform_Setup"]?["onWii"]}";
            string onvWii = patcherLang == PatcherLanguage.en
                ? "[bold]vWii (Wii U)[/]"
                : $"{localizedText?["CustomSetup"]?["ConsolePlatform_Setup"]?["onvWii"]}";
            string onDolphin = patcherLang == PatcherLanguage.en
                ? "[bold]Dolphin Emulator[/]"
                : $"{localizedText?["CustomSetup"]?["ConsolePlatform_Setup"]?["onDolphin"]}";
            AnsiConsole.MarkupLine($"[bold]1.[/] {onWii}");
            AnsiConsole.MarkupLine($"[bold]2.[/] {onvWii}");
            AnsiConsole.MarkupLine($"[bold]3.[/] {onDolphin}\n");

            // Print instructions
            string platformInstructions = patcherLang == PatcherLanguage.en
                ? "< Press [bold white]a number[/] to select platform, [bold white]Backspace[/] to go back, [bold white]ESC[/] to go back to exit setup >"
                : $"{localizedText?["CustomSetup"]?["ConsolePlatform_Setup"]?["platformInstructions"]}";
            AnsiConsole.MarkupLine($"[grey]{platformInstructions}[/]\n");

            int choice = UserChoose("123");

            // Use a switch statement to handle user's SPD version selection
            switch (choice)
            {
                case -1: // Escape
                    combinedChannels_selection.Clear();
                    MainMenu();
                    break;
                case -2: // Backspace
                    CustomInstall_WiiLinkChannels_Setup();
                    break;
                case 1:
                    platformType_custom = Platform.Wii;
                    platformType = Platform.Wii;
                    CustomInstall_SummaryScreen(showSPD: true);
                    break;
                case 2:
                    platformType_custom = Platform.vWii;
                    platformType = Platform.vWii;
                    CustomInstall_SummaryScreen(showSPD: true);
                    break;
                case 3:
                    platformType_custom = Platform.Dolphin;
                    platformType = Platform.Dolphin;
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
        task = "Custom Install (Part 3 - Show summary of selected channels to be installed)";
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
            { "food_en", "● Food Channel [bold](Standard) [[English]][/]" },
            { "demae_jp", "● Demae Channel [bold](Standard) [[Japanese]][/]" },
            { "food_dominos", "● Food Channel [bold](Dominos) [[English]][/]" },
            { "kirbytv", "● Kirby TV Channel" }
        };

        var selectedRegionalChannels = new List<string>();
        if (wiiLinkChannels_selection.Count > 0)
        {
            foreach (string channel in combinedChannels_selection)
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
        foreach (string channel in combinedChannels_selection)
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
            string customInstall = patcherLang == PatcherLanguage.en
                ? "Custom Install"
                : $"{localizedText?["CustomSetup"]?["Header"]}";
            string summaryHeader = patcherLang == PatcherLanguage.en
                ? "Summary of selected channels to be installed:"
                : $"{localizedText?["CustomSetup"]?["summaryScreen"]?["summaryHeader"]}";
            AnsiConsole.MarkupLine($"[bold springgreen2_1]{customInstall}[/]\n");
            AnsiConsole.MarkupLine($"[bold]{summaryHeader}[/]\n");

            // Display summary of selected channels in two columns using a grid
            var grid = new Grid();
            grid.AddColumn();
            grid.AddColumn();

            // Grid header text
            string regionalChannels = patcherLang == PatcherLanguage.en
                ? "Regional channels:"
                : $"{localizedText?["CustomSetup"]?["summaryScreen"]?["wiiLinkChannels"]}";
            string wiiConnect24Channels = patcherLang == PatcherLanguage.en
                ? "WiiConnect24 Channels:"
                : $"{localizedText?["CustomSetup"]?["summaryScreen"]?["wiiConnect24Channels"]}";
            string consoleVersion = patcherLang == PatcherLanguage.en
                ? "Console Platform:"
                : $"{localizedText?["CustomSetup"]?["summaryScreen"]?["ConsoleVersion"]}";

            grid.AddColumn();

            grid.AddRow($"[bold deepskyblue1]{wiiConnect24Channels}[/]", $"[bold springgreen2_1]{regionalChannels}[/]", $"[bold]{consoleVersion}[/]");

            if (platformType_custom == Platform.Wii)
                grid.AddRow(string.Join("\n", selectedWiiConnect24Channels), string.Join("\n", selectedRegionalChannels),"● [bold]Wii[/]");
            else if (platformType_custom == Platform.vWii)
                grid.AddRow(string.Join("\n", selectedWiiConnect24Channels), string.Join("\n", selectedRegionalChannels),"● [bold]vWii (Wii U)[/]");
            else
                grid.AddRow(string.Join("\n", selectedWiiConnect24Channels), string.Join("\n", selectedRegionalChannels),"● [bold]Dolphin Emulator[/]");

            AnsiConsole.Write(grid);

            // If user chose vWii as their platform, notify that the EULA channel will be included
            if (platformType_custom == Platform.vWii && wiiConnect24Channels_selection.Any())
            {
                string eulaChannel = patcherLang == PatcherLanguage.en
                    ? "[bold]NOTE:[/] For [bold deepskyblue1]vWii[/] users, The EULA channel will be included."
                    : $"{localizedText?["CustomSetup"]?["summaryScreen"]?["eulaChannel"]}";
                AnsiConsole.MarkupLine($"\n{eulaChannel}");
            }

            // If user chose Russian Wii Room, provide extra instructions
            if (combinedChannels_selection.Contains("wiiroom_ru"))
            {
                AnsiConsole.MarkupLine("\n[bold yellow]NOTICE FOR RUSSIAN USERS[/]\n");
                AnsiConsole.MarkupLine("Proper functionality is not guaranteed for systems without the Russian Wii menu.\n");
                AnsiConsole.MarkupLine("Read the installation guide here (Russian only for now):");
                AnsiConsole.MarkupLine("[bold link springgreen2_1]https://wii.zazios.ru/rus_menu[/]");
            }

            // Print instructions
            string prompt = patcherLang == PatcherLanguage.en
                ? "Are you sure you want to install these selected channels?"
                : $"{localizedText?["CustomSetup"]?["summaryScreen"]?["confirmation"]?["prompt"]}";

            // User confirmation strings
            string yes = patcherLang == PatcherLanguage.en
                ? "Yes"
                : $"{localizedText?["CustomSetup"]?["summaryScreen"]?["confirmation"]?["yes"]}";
            string noStartOver = patcherLang == PatcherLanguage.en
                ? "No, start over"
                : $"{localizedText?["CustomSetup"]?["summaryScreen"]?["confirmation"]?["noStartOver"]}";
            string noGoBackToMainMenu = patcherLang == PatcherLanguage.en
                ? "No, go back to Main Menu"
                : $"{localizedText?["CustomSetup"]?["summaryScreen"]?["confirmation"]?["noGoBackToMainMenu"]}";

            AnsiConsole.MarkupLine($"\n[bold]{prompt}[/]\n");

            AnsiConsole.MarkupLine($"1. {yes}");
            AnsiConsole.MarkupLine($"2. {noStartOver}\n");

            AnsiConsole.MarkupLine($"3. {noGoBackToMainMenu}\n");

            var choice = UserChoose("123");

            // Handle user confirmation choice
            switch (choice)
            {
                case 1: // Yes
                    if (platformType_custom != Platform.Dolphin){
                        SDSetup(isCustomSetup: true);
                        break;
                    }
                    else {
                        sdcard = null;
                        WADFolderCheck(true);
                        break;
                    }
                case 2: // No, start over
                    combinedChannels_selection.Clear();
                    CustomInstall_WiiLinkChannels_Setup();
                    break;
                case 3: // No, go back to main menu
                    combinedChannels_selection.Clear();
                    MainMenu();
                    break;
                default:
                    break;
            }
        }
    }

    // Download respective patches for selected core and WiiConnect24 channels (and SPD if English is selected for WiiLink channels)
    static void DownloadCustomPatches(List<string> channelSelection)
    {
        task = "Downloading selected patches";

        // Download SPD if any of the following channels are selected
        if (wiiLinkChannels_selection.Any(channel => channel.Contains("food_en") || channel.Contains("food_dominos") || channel.Contains("digicam_en")))
            DownloadSPD(platformType_custom);
        else
            Directory.CreateDirectory("WAD");

        // Download ww-43db-patcher for vWii if applicable
        if (platformType_custom == Platform.vWii)
        {
            // DownloadOSCApp("ww-43db-patcher");

            // Download the below if any WiiConnect24 channels are selected
            if (wiiConnect24Channels_selection.Any())
            {
                // Create a dictionary mapping EULA title IDs to their respective regions
                Dictionary<string, Region> EULATitleIDs = new()
                {
                    { "0001000848414b45", Region.USA },
                    { "0001000848414b50", Region.PAL },
                    { "0001000848414b4a", Region.Japan },
                };

                // Iterate over the dictionary
                foreach ((string titleID, Region region) in EULATitleIDs)
                {
                    // Use the deconstructed variables in the DownloadWC24Channel function call
                    DownloadWC24Channel("EULA", "EULA", 3, region, titleID);
                }
            }
        }

        // Download patches for selected WiiLink channels
        // !! The English patches are the base patches (0 and 1) for the translated patches, patches 2 and up will change the language !! //
        foreach (string channel in channelSelection)
        {
            switch (channel)
            {
                case "wiiroom_en":
                    task = "Downloading Wii Room (English)";
                    DownloadPatch("WiinoMa", $"WiinoMa_0_Universal.delta", "WiinoMa_0_Universal.delta", "Wii Room");
                    DownloadPatch("WiinoMa", $"WiinoMa_1_Universal.delta", "WiinoMa_1_Universal.delta", "Wii Room");
                    DownloadPatch("WiinoMa", $"WiinoMa_2_English.delta", "WiinoMa_2_English.delta", "Wii Room");
                    break;
                case "wiiroom_es":
                    task = "Downloading Wii Room (Español)";
                    DownloadPatch("WiinoMa", $"WiinoMa_0_Universal.delta", "WiinoMa_0_Universal.delta", "Wii Room");
                    DownloadPatch("WiinoMa", $"WiinoMa_1_Universal.delta", "WiinoMa_1_Universal.delta", "Wii Room");
                    DownloadPatch("WiinoMa", $"WiinoMa_2_Spanish.delta", "WiinoMa_2_Spanish.delta", "Wii Room");
                    break;
                case "wiiroom_fr":
                    task = "Downloading Wii Room (Français)";
                    DownloadPatch("WiinoMa", $"WiinoMa_0_Universal.delta", "WiinoMa_0_Universal.delta", "Wii Room");
                    DownloadPatch("WiinoMa", $"WiinoMa_1_Universal.delta", "WiinoMa_1_Universal.delta", "Wii Room");
                    DownloadPatch("WiinoMa", $"WiinoMa_2_French.delta", "WiinoMa_2_French.delta", "Wii Room");
                    break;
                case "wiiroom_de":
                    task = "Downloading Wii Room (Deutsch)";
                    DownloadPatch("WiinoMa", $"WiinoMa_0_Universal.delta", "WiinoMa_0_Universal.delta", "Wii Room");
                    DownloadPatch("WiinoMa", $"WiinoMa_1_Universal.delta", "WiinoMa_1_Universal.delta", "Wii Room");
                    DownloadPatch("WiinoMa", $"WiinoMa_2_German.delta", "WiinoMa_2_German.delta", "Wii Room");
                    break;
                case "wiiroom_it":
                    task = "Downloading Wii Room (Italiano)";
                    DownloadPatch("WiinoMa", $"WiinoMa_0_Universal.delta", "WiinoMa_0_Universal.delta", "Wii Room");
                    DownloadPatch("WiinoMa", $"WiinoMa_1_Universal.delta", "WiinoMa_1_Universal.delta", "Wii Room");
                    DownloadPatch("WiinoMa", $"WiinoMa_2_Italian.delta", "WiinoMa_2_Italian.delta", "Wii Room");
                    break;
                case "wiiroom_du":
                    task = "Downloading Wii Room (Nederlands)";
                    DownloadPatch("WiinoMa", $"WiinoMa_0_Universal.delta", "WiinoMa_0_Universal.delta", "Wii Room");
                    DownloadPatch("WiinoMa", $"WiinoMa_1_Universal.delta", "WiinoMa_1_Universal.delta", "Wii Room");
                    DownloadPatch("WiinoMa", $"WiinoMa_2_Dutch.delta", "WiinoMa_2_Dutch.delta", "Wii Room");
                    break;
                case "wiiroom_ptbr":
                    task = "Downloading Wii Room (Português-Brasil)";
                    DownloadPatch("WiinoMa", $"WiinoMa_0_Universal.delta", "WiinoMa_0_Universal.delta", "Wii Room");
                    DownloadPatch("WiinoMa", $"WiinoMa_1_Portuguese.delta", "WiinoMa_1_Portuguese.delta", "Wii Room");
                    DownloadPatch("WiinoMa", $"WiinoMa_2_Portuguese.delta", "WiinoMa_2_Portuguese.delta", "Wii Room");
                    DownloadPatch("WiinoMa", $"WiinoMa_3_Portuguese.delta", "WiinoMa_3_Portuguese.delta", "Wii Room");
                    DownloadPatch("WiinoMa", $"WiinoMa_4_Portuguese.delta", "WiinoMa_4_Portuguese.delta", "Wii Room");
                    DownloadPatch("WiinoMa", $"WiinoMa_D_Portuguese.delta", "WiinoMa_D_Portuguese.delta", "Wii Room");
                    break;
                case "wiiroom_ru":
                    task = "Downloading Wii Room (Русский)";
                    DownloadPatch("WiinoMa", $"WiinoMa_0_Universal.delta", "WiinoMa_0_Universal.delta", "Wii Room");
                    DownloadPatch("WiinoMa", $"WiinoMa_1_Russian.delta", "WiinoMa_1_Russian.delta", "Wii Room");
                    DownloadPatch("WiinoMa", $"WiinoMa_2_Russian.delta", "WiinoMa_2_Russian.delta", "Wii Room");
                    DownloadPatch("WiinoMa", $"WiinoMa_3_Russian.delta", "WiinoMa_3_Russian.delta", "Wii Room");
                    DownloadPatch("WiinoMa", $"WiinoMa_4_Russian.delta", "WiinoMa_4_Russian.delta", "Wii Room");
                    DownloadPatch("WiinoMa", $"WiinoMa_9_Russian.delta", "WiinoMa_9_Russian.delta", "Wii Room");
                    DownloadPatch("WiinoMa", $"WiinoMa_C_Russian.delta", "WiinoMa_C_Russian.delta", "Wii Room");
                    DownloadPatch("WiinoMa", $"WiinoMa_D_Russian.delta", "WiinoMa_D_Russian.delta", "Wii Room");
                    DownloadPatch("WiinoMa", $"WiinoMa_E_Russian.delta", "WiinoMa_E_Russian.delta", "Wii Room");
                    break;
                case "wiinoma_jp":
                    task = "Downloading Wii no Ma (Japan)";
                    DownloadPatch("WiinoMa", $"WiinoMa_0_Universal.delta", "WiinoMa_0_Universal.delta", "Wii no Ma");
                    DownloadPatch("WiinoMa", $"WiinoMa_1_Universal.delta", "WiinoMa_1_Universal.delta", "Wii no Ma");
                    DownloadPatch("WiinoMa", $"WiinoMa_2_Japan.delta", "WiinoMa_2_Japan.delta", "Wii no Ma");
                    break;
                case "digicam_en":
                    task = "Downloading Photo Prints Channel (English)";
                    DownloadPatch("Digicam", $"Digicam_0_English.delta", "Digicam_0_English.delta", "Photo Prints Channel");
                    DownloadPatch("Digicam", $"Digicam_1_English.delta", "Digicam_1_English.delta", "Photo Prints Channel");
                    DownloadPatch("Digicam", $"Digicam_2_English.delta", "Digicam_2_English.delta", "Photo Prints Channel");
                    break;
                case "digicam_jp":
                    task = "Downloading Digicam Print Channel (Japan)";
                    DownloadPatch("Digicam", $"Digicam_1_Japan.delta", "Digicam_1_Japan.delta", "Digicam Print Channel");
                    break;
                case "food_en":
                    task = "Downloading Food Channel (English)";
                    DownloadPatch("Demae", $"Demae_0_English.delta", "Demae_0_English.delta", "Food Channel (Standard)");
                    DownloadPatch("Demae", $"Demae_1_English.delta", "Demae_1_English.delta", "Food Channel (Standard)");
                    DownloadPatch("Demae", $"Demae_2_English.delta", "Demae_2_English.delta", "Food Channel (Standard)");
                    break;
                case "demae_jp":
                    task = "Downloading Demae Channel (Japan)";
                    DownloadPatch("Demae", $"Demae_1_Japan.delta", "Demae_1_Japan.delta", "Demae Channel");
                    break;
                case "food_dominos":
                    task = "Downloading Food Channel (Domino's)";
                    DownloadPatch("Dominos", $"Dominos_0.delta", "Dominos_0.delta", "Food Channel (Domino's)");
                    DownloadPatch("Dominos", $"Dominos_1.delta", "Dominos_1.delta", "Food Channel (Domino's)");
                    DownloadPatch("Dominos", $"Dominos_2.delta", "Dominos_2.delta", "Food Channel (Domino's)");
                    DownloadOSCApp("GetConsoleID");
                    break;
                case "nc_us":
                    task = "Downloading Nintendo Channel (USA)";
                    DownloadPatch("nc", $"NC_1_USA.delta", "NC_1_USA.delta", "Nintendo Channel");
                    break;
                case "mnnc_jp":
                    task = "Downloading Nintendo Channel (Japan)";
                    DownloadPatch("nc", $"NC_1_Japan.delta", "NC_1_Japan.delta", "Nintendo Channel");
                    break;
                case "nc_eu":
                    task = "Downloading Nintendo Channel (Europe)";
                    DownloadPatch("nc", $"NC_1_PAL.delta", "NC_1_PAL.delta", "Nintendo Channel");
                    break;
                case "forecast_us": // Forecast Patch works for all regions now
                case "forecast_jp":
                case "forecast_eu":
                    task = "Downloading Forecast Channel";
                    DownloadPatch("forecast", $"Forecast_1.delta", "Forecast_1.delta", "Forecast Channel");
                    DownloadPatch("forecast", $"Forecast_5.delta", "Forecast_5.delta", "Forecast Channel");
                    DownloadAGC(); // Download AnyGlobe_Changer from OSC for use with the Forecast Channel
                    break;
                case "news_us":
                case "news_eu":
                case "news_jp":
                    task = "Downloading News Channel";
                    DownloadPatch("news", $"News_1.delta", $"News_1.delta", "News Channel");
                    break;
                case "evc_us":
                    task = $"Downloading Everybody Votes Channel (USA)";
                    DownloadPatch("evc", $"EVC_1_USA.delta", "EVC_1_USA.delta", "Everybody Votes Channel");
                    DownloadPatch("RegSel", "RegSel_1.delta", "RegSel_1.delta", "Region Select");
                    break;
                case "evc_eu":
                    task = $"Downloading Everybody Votes Channel (PAL)";
                    DownloadPatch("evc", $"EVC_1_PAL.delta", "EVC_1_PAL.delta", "Everybody Votes Channel");
                    DownloadPatch("RegSel", "RegSel_1.delta", "RegSel_1.delta", "Region Select");
                    break;
                case "evc_jp":
                    task = $"Downloading Everybody Votes Channel (Japan)";
                    DownloadPatch("evc", $"EVC_1_Japan.delta", "EVC_1_Japan.delta", "Everybody Votes Channel");
                    DownloadPatch("RegSel", "RegSel_1.delta", "RegSel_1.delta", "Region Select");
                    break;
                case "cmoc_us":
                    task = $"Downloading Check Mii Out Channel (USA)";
                    DownloadPatch("cmoc", $"CMOC_1_USA.delta", "CMOC_1_USA.delta", "Check Mii Out Channel");
                    break;
                case "cmoc_eu":
                    task = $"Downloading Mii Contest Channel (Europe)";
                    DownloadPatch("cmoc", $"CMOC_1_PAL.delta", "CMOC_1_PAL.delta", "Mii Contest Channel");
                    break;
                case "cmoc_jp":
                    task = $"Downloading Mii Contest Channel (Japan)";
                    DownloadPatch("cmoc", $"CMOC_1_Japan.delta", "CMOC_1_Japan.delta", "Mii Contest Channel");
                    break;
                case "kirbytv":
                    task = "Downloading Kirby TV Channel";
                    DownloadPatch("ktv", $"ktv_2.delta", "KirbyTV_2.delta", "Kirby TV Channel");
                    break;
            }
        }

        if (platformType_custom != Platform.Dolphin) {
        // Downloading yawmME from OSC
            DownloadOSCApp("yawmME");
        // Install the RC24 Mail Patcher
            DownloadOSCApp("Mail-Patcher");
        }

        if (platformType_custom == Platform.Wii)
        // Downloading sntp from OSC
            DownloadOSCApp("sntp");
    }

    // Patching Wii no Ma
    static void WiiRoom_Patch(Language language)
    {
        task = "Patching Wii no Ma";

        // Patches 00 and 01 are universal (except 01 for Russian and Portuguese), 02 has language-specific patches
        // Russian has patches 01, 02, 03, 04, 09, 0C, 0D, and 0E
        // Portuguese has patches 01, 02, 03, 04, and 0D

        bool notRussianOrPortuguese = language != Language.Russian && language != Language.Portuguese;

        // Generate patch list for Wii Room
        var wiiRoomPatchList = new List<KeyValuePair<string, string>>
        {
            new("WiinoMa_0_Universal", "00000000"),
            new($"WiinoMa_1_{(notRussianOrPortuguese ? "Universal" : language)}", "00000001"),
            new($"WiinoMa_2_{language}", "00000002")
        };

        switch (language)
        {
            case Language.Russian:
                wiiRoomPatchList.AddRange(
                [
                    new KeyValuePair<string, string>("WiinoMa_3_Russian", "00000003"),
                    new KeyValuePair<string, string>("WiinoMa_4_Russian", "00000004"),
                    new KeyValuePair<string, string>("WiinoMa_9_Russian", "00000009"),
                    new KeyValuePair<string, string>("WiinoMa_C_Russian", "0000000C"),
                    new KeyValuePair<string, string>("WiinoMa_D_Russian", "0000000D"),
                    new KeyValuePair<string, string>("WiinoMa_E_Russian", "0000000E")
                ]);
                break;
            case Language.Portuguese:
                wiiRoomPatchList.AddRange(
                [
                    new KeyValuePair<string, string>("WiinoMa_3_Portuguese", "00000003"),
                    new KeyValuePair<string, string>("WiinoMa_4_Portuguese", "00000004"),
                    new KeyValuePair<string, string>("WiinoMa_D_Portuguese", "0000000D")
                ]);
                break;
        }

        // Name the channel based on the language chosen
        string channelTitle = language switch
        {
            Language.Japan => "Wii no Ma",
            _ => "Wii Room"
        };

        PatchRegionalChannel("WiinoMa", channelTitle, "000100014843494a", wiiRoomPatchList, lang: language);

        // Finished patching Wii no Ma
        patchingProgress_express["wiiroom"] = "done";
        patchingProgress_express["digicam"] = "in_progress";
    }

    // Patching Digicam Print Channel
    static void Digicam_Patch(Language language)
    {
        task = "Patching Digicam Print Channel";

        // Dictionary for which files to patch
        var digicamPatchList = new List<KeyValuePair<string, string>>()
        {
            new($"Digicam_0_{language}", "00000000"),
            new($"Digicam_1_{language}", "00000001"),
            new($"Digicam_2_{language}", "00000002")
        };

        string channelTitle = language switch
        {
            Language.English => "Photo Prints Channel",
            _ => "Digicam Print Channel"
        };

        PatchRegionalChannel("Digicam", channelTitle, "000100014843444a", digicamPatchList, lang: language);

        // Finished patching Digicam Print Channel
        patchingProgress_express["digicam"] = "done";
        patchingProgress_express["demae"] = "in_progress";
    }

    // Patching Demae Channel
    static void Demae_Patch(Language language, DemaeVersion demaeVersion)
    {
        // Assign channel title based on language chosen
        string channelTitle = language switch
        {
            Language.English => "Food Channel",
            _ => "Demae Channel"
        };

        task = $"Patching {channelTitle}";

        // Generate patch list for Demae Channel
        List<KeyValuePair<string, string>> GeneratePatchList(string prefix, bool appendLang)
        {
            return
            [
                new($"{prefix}_0{(appendLang ? $"_{language}" : "")}", "00000000"),
                new($"{prefix}_1{(appendLang ? $"_{language}" : "")}", "00000001"),
                new($"{prefix}_2{(appendLang ? $"_{language}" : "")}", "00000002")
            ];
        }

        // Map DemaeVersion to patch list and folder name (Patch list, folder name)
        var demaeData = new Dictionary<DemaeVersion, (List<KeyValuePair<string, string>>, string)>
        {
            [DemaeVersion.Standard] = (GeneratePatchList("Demae", true), "Demae"),
            [DemaeVersion.Dominos] = (GeneratePatchList("Dominos", false), "Dominos")
        };

        // Get patch list and folder name for the current version
        var (demaePatchList, folderName) = demaeData[demaeVersion];

        PatchRegionalChannel(folderName, $"{channelTitle} ({demaeVersion})", "000100014843484a", demaePatchList, lang: language);

        // Finished patching Demae Channel
        patchingProgress_express["demae"] = "done";
        patchingProgress_express["kirbytv"] = "in_progress";
    }

    // Patching Kirby TV Channel (if applicable)
    static void KirbyTV_Patch()
    {
        task = "Patching Kirby TV Channel";

        List<string> patches = ["KirbyTV_2"];
        List<string> appNums = ["0000000e"];

        PatchWC24Channel("ktv", $"Kirby TV Channel", 257, null, "0001000148434d50", patches, appNums);

        // Finished patching Kirby TV Channel
        patchingProgress_express["kirbytv"] = "done";
        patchingProgress_express["finishing"] = "in_progress";
    }


    // Patching Nintendo Channel
    static void NC_Patch(Region region)
    {
        task = "Patching Nintendo Channel";

        // Define a dictionary to map Region to channelID, appNum, and channel_title
        Dictionary<Region, (string channelID, string appNum, string channel_title)> regionData = new()
        {
            { Region.USA, ("0001000148415445", "0000002c", "Nintendo Channel") },
            { Region.PAL, ("0001000148415450", "0000002d", "Nintendo Channel") },
            { Region.Japan, ("000100014841544a", "0000003e", "Minna no Nintendo Channel") },
        };

        // Get the data for the current region
        var (channelID, appNum, channel_title) = regionData[region];

        List<string> patches = [$"NC_1_{region}"];
        List<string> appNums = [appNum];

        PatchWC24Channel("nc", $"{channel_title}", 1792, region, channelID, patches, appNums);

        // Finished patching Nintendo Channel
        patchingProgress_express["nc"] = "done";
        patchingProgress_express["forecast"] = "in_progress";
    }

    // Patching Forecast Channel
    static void Forecast_Patch(Region region)
    {
        task = "Patching Forecast Channel";

        // Properly set Forecast Channel titleID
        string channelID = region switch
        {
            Region.USA => "0001000248414645",
            Region.PAL => "0001000248414650",
            Region.Japan => "000100024841464a",
            _ => throw new NotImplementedException(),
        };

        List<string> patches = ["Forecast_1", "Forecast_5"];
        List<string> appNums = ["0000000d", "0000000f"];

        PatchWC24Channel("forecast", $"Forecast Channel", 7, region, channelID, patches, appNums);

        // Finished patching Forecast Channel
        patchingProgress_express["forecast"] = "done";
        patchingProgress_express["news"] = "in_progress";
    }

    // Patching News Channel
    static void News_Patch(Region region)
    {
        task = "Patching News Channel";

        // Properly set News Channel titleID
        string channelID = region switch
        {
            Region.USA => "0001000248414745",
            Region.PAL => "0001000248414750",
            Region.Japan => "000100024841474a",
            _ => throw new NotImplementedException(),
        };

        List<string> patches = ["News_1"];
        List<string> appNums = ["0000000b"];

        PatchWC24Channel("news", $"News Channel", 7, region, channelID, patches, appNums);

        // Finished patching News Channel
        patchingProgress_express["news"] = "done";
        patchingProgress_express["evc"] = "in_progress";
    }

    // Patching Everybody Votes Channel
    static void EVC_Patch(Region region)
    {

        //// Patching Everybody Votes Channel
        task = "Patching Everybody Votes Channel";

        // Properly set Everybody Votes Channel titleID
        string channelID = region switch
        {
            Region.USA => "0001000148414a45",
            Region.PAL => "0001000148414a50",
            Region.Japan => "0001000148414a4a",
            _ => throw new NotImplementedException(),
        };

        List<string> patches = [$"EVC_1_{region}"];
        List<string> appNums = ["00000019"];

        PatchWC24Channel("evc", $"Everybody Votes Channel", 512, region, channelID, patches, appNums);

        //// Patching Region Select for Everybody Votes Channel
        RegSel_Patch(region);

        // Finished patching Everybody Votes Channel
        patchingProgress_express["evc"] = "done";
        patchingProgress_express["cmoc"] = "in_progress";
    }

    // Patching Check Mii Out Channel
    static void CheckMiiOut_Patch(Region region)
    {
        task = "Patching Check Mii Out Channel";

        // Properly set Check Mii Out Channel titleID based on region
        string channelID = region switch
        {
            Region.USA => "0001000148415045",
            Region.PAL => "0001000148415050",
            Region.Japan => "000100014841504a",
            _ => throw new NotImplementedException(),
        };

        // Set Check Mii Out Channel title based on region
        string channelTitle = region switch
        {
            Region.USA => "Check Mii Out Channel",
            _ => "Mii Contest Channel",
        };

        List<string> patches = [$"CMOC_1_{region}"];
        List<string> appNums = ["0000000c"];

        PatchWC24Channel("cmoc", $"{channelTitle}", 512, region, channelID, patches, appNums);

        // Finished patching Check Mii Out Channel
        patchingProgress_express["cmoc"] = "done";
        patchingProgress_express["wiiroom"] = "in_progress";
    }

    // Patching Region Select
    static void RegSel_Patch(Region regSel_reg)
    {
        task = "Patching Region Select";

        // Properly set Region Select titleID based on region
        string channelID = regSel_reg switch
        {
            Region.USA => "0001000848414c45",
            Region.PAL => "0001000848414c50",
            Region.Japan => "0001000848414c4a",
            _ => throw new NotImplementedException(),
        };

        List<string> patches = ["RegSel_1"];
        List<string> appNums = ["00000009"];

        PatchWC24Channel("RegSel", $"Region Select", 2, regSel_reg, channelID, patches, appNums);
    }

    // Finish SD Copy
    static void FinishSDCopy()
    {
        // Copying files to SD card and user is not running the patcher on the removable drive
        task = "Copying files to SD card";

        if (sdcard != null && curDir != sdcard)
        {
            // Copying files to SD card
            string copyingFiles = patcherLang == PatcherLanguage.en
                ? "Copying files to SD card, which may take a while."
                : $"{localizedText?["FinishSDCopy"]?["copyingFiles"]}";
            AnsiConsole.MarkupLine($" [bold][[*]] {copyingFiles}[/]");

            try
            {
                // Copy apps and WAD folder to SD card
                CopyFolder("apps", Path.Join(sdcard, "apps"));
                CopyFolder("WAD", Path.Join(sdcard, "WAD"));
            }
            catch (Exception e)
            {
                // Error message
                string pressAnyKey_error = patcherLang == PatcherLanguage.en
                    ? "Press any key to try again..."
                    : $"{localizedText?["FinishSDCopy"]?["pressAnyKey_error"]}";
                AnsiConsole.MarkupLine($"[bold red]ERROR:[/] {e.Message}\n{pressAnyKey_error}");
                Console.ReadKey();
                FinishSDCopy();
            }

            // Delete the WAD and apps folder if they exist
            if (Directory.Exists("WAD"))
                Directory.Delete("WAD", true);
            if (Directory.Exists("apps"))
                Directory.Delete("apps", true);
        }

        // Delete WiiLink_Patcher folder
        if (Directory.Exists("WiiLink_Patcher"))
            Directory.Delete("WiiLink_Patcher", true);

        // Finished patching
        patchingProgress_express["finishing"] = "done";
    }

    static void Finished()
    {
        // Clear all lists (just in case it's Custom Setup)
        wiiLinkChannels_selection.Clear();
        wiiConnect24Channels_selection.Clear();
        combinedChannels_selection.Clear();

        // Detect SD card (in case the user chose to not copy files to SD card)
        sdcard = DetectRemovableDrive;

        while (true)
        {
            PrintHeader();
            // Patching Completed text
            string completed = patcherLang == PatcherLanguage.en
                ? "Patching Completed!"
                : $"{localizedText?["Finished"]?["completed"]}";
            AnsiConsole.MarkupLine($"[bold slowblink springgreen2_1]{completed}[/]\n");

            if (sdcard != null)
            {
                // Every file is in its place text
                string everyFileInPlace = patcherLang == PatcherLanguage.en
                    ? "Every file is in its place on your SD Card / USB Drive!"
                    : $"{localizedText?["Finished"]?["withSD/USB"]?["everyFileInPlace"]}";
                AnsiConsole.MarkupLine($"{everyFileInPlace}\n");
            }
            else
            {
                if (platformType != Platform.Dolphin) {
                    // Please connect text
                    string connectDrive = patcherLang == PatcherLanguage.en
                        ? "Please connect your Wii SD Card / USB Drive and copy the [u]WAD[/] and [u]apps[/] folders to the root (main folder) of your SD Card / USB Drive."
                        : $"{localizedText?["Finished"]?["withoutSD/USB"]?["connectDrive"]}";
                    AnsiConsole.MarkupLine($"{connectDrive}\n");

                    // Open the folder text
                    string canFindFolders = patcherLang == PatcherLanguage.en
                        ? "You can find these folders in the [u]{curDir}[/] folder of your computer."
                        : $"{localizedText?["Finished"]?["canFindFolders"]}";
                    canFindFolders = canFindFolders.Replace("{curDir}", curDir);
                    AnsiConsole.MarkupLine($"{canFindFolders}\n");
                }
            }

            // Please proceed text
            if ( platformType == Platform.Wii ) {
                string pleaseProceed = patcherLang == PatcherLanguage.en
                    ? "Please proceed with the tutorial that you can find on [bold springgreen2_1 link]https://www.wiilink24.com/guide/wii/#section-ii---installing-wads-and-patching-wii-mail[/]"
                    : $"{localizedText?["Finished"]?["pleaseProceed"]}";
                AnsiConsole.MarkupLine($"{pleaseProceed}\n");
            }
            else if ( platformType == Platform.vWii ) {
                string pleaseProceed = patcherLang == PatcherLanguage.en
                    ? "Please proceed with the tutorial that you can find on [bold springgreen2_1 link]https://www.wiilink24.com/guide/vwii/#section-iii---installing-wads-and-patching-wii-mail[/]"
                    : $"{localizedText?["Finished"]?["pleaseProceed"]}";
                AnsiConsole.MarkupLine($"{pleaseProceed}\n");
            }
            else {
                string pleaseProceed = patcherLang == PatcherLanguage.en
                    ? "Please proceed with the tutorial that you can find on [bold springgreen2_1 link]https://www.wiilink24.com/guide/dolphin/#section-ii---installing-wads[/]"
                    : $"{localizedText?["Finished"]?["pleaseProceed"]}";
                AnsiConsole.MarkupLine($"{pleaseProceed}\n");
            }

            // What would you like to do now text
            string whatWouldYouLikeToDo = patcherLang == PatcherLanguage.en
                ? "What would you like to do now?"
                : $"{localizedText?["Finished"]?["whatWouldYouLikeToDo"]}";
            AnsiConsole.MarkupLine($"{whatWouldYouLikeToDo}\n");

            // User choices
            string openFolder = patcherLang == PatcherLanguage.en
                ? sdcard != null ? "Open the SD Card / USB Drive folder" : "Open the folder"
                : sdcard != null ? $"{localizedText?["Finished"]?["withSD/USB"]?["openFolder"]}" : $"{localizedText?["Finished"]?["withoutSD/USB"]?["openFolder"]}";
            string goBackToMainMenu = patcherLang == PatcherLanguage.en
                ? "Go back to the main menu"
                : $"{localizedText?["Finished"]?["goBackToMainMenu"]}";
            string exitProgram = patcherLang == PatcherLanguage.en
                ? "Exit the program"
                : $"{localizedText?["Finished"]?["exitProgram"]}";

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
                            ArgumentList = { sdcard ?? curDir },
                            UseShellExecute = false,
                        };
                        Process.Start(psi);
                    }
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    {
                        var psi = new ProcessStartInfo
                        {
                            FileName = "xdg-open",
                            ArgumentList = { sdcard ?? curDir },
                            UseShellExecute = false,
                        };
                        Process.Start(psi);
                    }
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    {
                        var psi = new ProcessStartInfo
                        {
                            FileName = "open",
                            ArgumentList = { sdcard ?? curDir },
                            UseShellExecute = false,
                        };
                        Process.Start(psi);
                    }
                    break;
                case 2:
                    // Check to see if removable drive is still connected
                    sdcard = DetectRemovableDrive;
                    MainMenu();
                    break;
                case 3:
                    Console.Clear();
                    ExitApp();
                    break;
                default:
                    break;
            }
        }
    }


    // Manually select your SD card path
    static void SDCardSelect()
    {
        while (true)
        {
            PrintHeader();

            // Manual SD card selection header
            string header = patcherLang == PatcherLanguage.en
                ? "Manually Select SD Card / USB Drive Path"
                : $"{localizedText?["SDCardSelect"]?["header"]}";
            AnsiConsole.MarkupLine($"[bold springgreen2_1]{header}[/]\n");

            string inputMessage = "";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                inputMessage = patcherLang == PatcherLanguage.en
                    ? "Please enter the drive letter of your SD card/USB drive (e.g. E)"
                    : $"{localizedText?["SDCardSelect"]?["inputMessage"]?["windows"]}";
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                inputMessage = patcherLang == PatcherLanguage.en
                    ? "Please enter the mount name of your SD card/USB drive (e.g. /media/username/Wii)"
                    : $"{localizedText?["SDCardSelect"]?["inputMessage"]?["linux"]}";
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                inputMessage = patcherLang == PatcherLanguage.en
                    ? "Please enter the volume name of your SD card/USB drive (e.g. /Volumes/Wii)"
                    : $"{localizedText?["SDCardSelect"]?["inputMessage"]?["osx"]}";
            AnsiConsole.MarkupLine($"{inputMessage}");

            // Type EXIT to go back to previous menu
            string exitMessage = patcherLang == PatcherLanguage.en
                ? "(Type [bold]EXIT[/] to go back to the previous menu)"
                : $"{localizedText?["SDCardSelect"]?["exitMessage"]}";
            AnsiConsole.MarkupLine($"{exitMessage}\n");

            // New SD card/USB drive text
            string newSDCardMessage = "";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                newSDCardMessage = patcherLang == PatcherLanguage.en
                    ? "New SD card/USB drive:"
                    : $"{localizedText?["SDCardSelect"]?["newSDCardMessage"]?["windows"]}";
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                newSDCardMessage = patcherLang == PatcherLanguage.en
                    ? "New SD card/USB drive volume:"
                    : $"{localizedText?["SDCardSelect"]?["newSDCardMessage"]?["linux"]}";
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                newSDCardMessage = patcherLang == PatcherLanguage.en
                    ? "New SD card/USB drive volume:"
                    : $"{localizedText?["SDCardSelect"]?["newSDCardMessage"]?["osx"]}";
            AnsiConsole.Markup($"{newSDCardMessage} ");

            // Get user input, if user presses ESC (without needing to press ENTER), go back to previous menu
            string? sdcard_new = Console.ReadLine();
            string? inputUpper = sdcard_new?.ToUpper();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                sdcard_new = inputUpper;

            // Restart SDCardSelect if user input is empty
            if (inputUpper == "")
                SDCardSelect();
            else if (inputUpper == "EXIT")
                return;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // Error if drive letter is more than 1 character
                if (sdcard_new?.Length > 1)
                {
                    // Driver letter must be 1 character text
                    string driveLetterError = patcherLang == PatcherLanguage.en
                        ? "Drive letter must be 1 character!"
                        : $"{localizedText?["SDCardSelect"]?["driveLetterError"]}";
                    AnsiConsole.MarkupLine($"[bold red]{driveLetterError}[/]");
                    Thread.Sleep(2000);
                    continue;
                }
            }

            // Format SD card path depending on OS
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                sdcard_new += ":\\";
            else
            {
                // If / is already at the end of the path, remove it
                if (sdcard_new?.EndsWith("/") == true)
                    sdcard_new = sdcard_new.Remove(sdcard_new.Length - 1);
            }

            // Prevent user from selecting boot drive
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                if (sdcard_new == "/")
                {
                    // You cannot select your boot drive text
                    string bootDriveError = patcherLang == PatcherLanguage.en
                        ? "You cannot select your boot drive!"
                        : $"{localizedText?["SDCardSelect"]?["bootDriveError"]}";
                    AnsiConsole.MarkupLine($"[bold red]{bootDriveError}[/]");
                    Thread.Sleep(2000);
                    continue;
                }
            }
            else if (Path.GetPathRoot(sdcard_new) == Path.GetPathRoot(Path.GetPathRoot(Environment.SystemDirectory)))
            {
                // You cannot select your boot drive text
                string bootDriveError = patcherLang == PatcherLanguage.en
                    ? "You cannot select your boot drive!"
                    : $"{localizedText?["SDCardSelect"]?["bootDriveError"]}";
                AnsiConsole.MarkupLine($"[bold red]{bootDriveError}[/]");
                Thread.Sleep(2000);
                continue;
            }

            // On Windows, don't allow the user to pick a drive that's not removable
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (sdcard_new == null)
                    continue;

                DriveInfo driveInfo = new(sdcard_new);
                if (!driveInfo.IsReady || driveInfo.DriveType != DriveType.Removable)
                {
                    // Drive is not removable text
                    string driveNotRemovableError = patcherLang == PatcherLanguage.en
                        ? "Drive selected is not a removable drive! Please select a removable drive (e.g. SD card or USB drive)."
                        : $"{localizedText?["SDCardSelect"]?["driveNotRemovableError"]}";
                    AnsiConsole.MarkupLine($"[bold red]{driveNotRemovableError}[/]");
                    Thread.Sleep(5000);
                    continue;
                }
            }

            // Check if new SD card path is the same as the old one
            if (sdcard_new == sdcard)
            {
                // You have already selected this SD card/USB drive text
                string alreadySelectedError = patcherLang == PatcherLanguage.en
                    ? "You have already selected this SD card/USB drive!"
                    : $"{localizedText?["SDCardSelect"]?["alreadySelectedError"]}";
                AnsiConsole.MarkupLine($"[bold red]{alreadySelectedError}[/]");
                Thread.Sleep(2000);
                continue;
            }

            // Check if drive/volume exists
            if (!Directory.Exists(sdcard_new))
            {
                // Drive does not exist text
                string driveNotExistError = "";
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    driveNotExistError = patcherLang == PatcherLanguage.en
                        ? "Drive does not exist!"
                        : $"{localizedText?["SDCardSelect"]?["driveNotExistError"]?["windows"]}";
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    driveNotExistError = patcherLang == PatcherLanguage.en
                        ? "Volume does not exist!"
                        : $"{localizedText?["SDCardSelect"]?["driveNotExistError"]?["linux"]}";
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    driveNotExistError = patcherLang == PatcherLanguage.en
                        ? "Volume does not exist!"
                        : $"{localizedText?["SDCardSelect"]?["driveNotExistError"]?["osx"]}";
                AnsiConsole.MarkupLine($"[bold red]{driveNotExistError}[/]");

                Thread.Sleep(2000);
                continue;
            }

            // Check if SD card has /apps folder (using PathCombine)
            if (Directory.Exists(Path.Join(sdcard_new, "apps")))
            {
                // SD card is valid
                sdcard = sdcard_new;
                break;
            }
            else
            {
                // SD card is invalid text
                string noAppsFolderError_message = patcherLang == PatcherLanguage.en
                    ? "Drive detected, but no /apps folder found!"
                    : $"{localizedText?["SDCardSelect"]?["noAppsFolderError"]?["message"]}";
                string noAppsFolderError_instructions = patcherLang == PatcherLanguage.en
                    ? "Please create it first and then try again."
                    : $"{localizedText?["SDCardSelect"]?["noAppsFolderError"]?["instructions"]}";
                AnsiConsole.MarkupLine($"[bold]{noAppsFolderError_message}[/]");
                AnsiConsole.MarkupLine($"{noAppsFolderError_instructions}\n");

                // Press any key to continue text
                string pressAnyKey = patcherLang == PatcherLanguage.en
                    ? "Press any key to continue..."
                    : $"{localizedText?["SDCardSelect"]?["pressAnyKey"]}";
                AnsiConsole.MarkupLine($"{pressAnyKey}");
                Console.ReadKey();
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
            var languages = new Dictionary<PatcherLanguage, string>
            {
                {PatcherLanguage.en, "English"},
                // Add other language codes here
            };

            // Choose a Language text
            string chooseALanguage = patcherLang == PatcherLanguage.en
                ? "Choose a Language"
                : $"{localizedText?["LanguageSettings"]?["chooseALanguage"]}";
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
            string goBack = patcherLang == PatcherLanguage.en
                ? "Go back to Settings Menu"
                : $"{localizedText?["LanguageSettings"]?["goBack"]}";
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
                patcherLang = langCode;

                // Since English is hardcoded, there's no language pack for it
                if (patcherLang == PatcherLanguage.en)
                {
                    SettingsMenu();
                    break;
                }

                // Download language pack
                DownloadLanguagePack(langCode.ToString());

                // Set localizedText to use the language pack
                localizedText = JObject.Parse(File.ReadAllText(Path.Join(tempDir, "LanguagePack", $"LocalizedText.{langCode}.json")));

                SettingsMenu();
            }
            else if (choice == languages.Count + 1)
            {
                SettingsMenu();
            }
        }
    }

    static void DownloadLanguagePack(string languageCode)
    {
        string URL = "http://pabloscorner.akawah.net/WL24-Patcher/TextLocalization";

        AnsiConsole.MarkupLine($"\n[bold springgreen2_1]Checking for Language Pack updates ({languageCode})[/]");

        // Create LanguagePack folder if it doesn't exist
        var languagePackDir = Path.Join(tempDir, "LanguagePack");
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
            HttpResponseMessage response = httpClient.Send(request);
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
            DownloadFile(languageFileUrl, languageFilePath, $"Language Pack ({languageCode})");
        }
        else
        {
            AnsiConsole.MarkupLine($"[bold springgreen2_1]Language Pack ({languageCode}) is up to date[/]");
            Thread.Sleep(500);
        }
    }

    static void SettingsMenu()
    {
        while (true)
        {
            PrintHeader();
            PrintNotice();

            // Settings text
            string settings = patcherLang == PatcherLanguage.en
                ? "Settings"
                : $"{localizedText?["SettingsMenu"]?["settings"]}";
            AnsiConsole.MarkupLine($"[bold springgreen2_1]{settings}[/]\n");

            if (!inCompatabilityMode)
            {
                // User choices
                string changeLanguage = patcherLang == PatcherLanguage.en
                    ? "Change Language"
                    : $"{localizedText?["SettingsMenu"]?["changeLanguage"]}";
                string credits = patcherLang == PatcherLanguage.en
                    ? "Credits"
                    : $"{localizedText?["SettingsMenu"]?["credits"]}";
                string goBack = patcherLang == PatcherLanguage.en
                    ? "Go back to Main Menu"
                    : $"{localizedText?["SettingsMenu"]?["goBack"]}";

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
                case 1 when !inCompatabilityMode:
                    LanguageMenu();
                    break;
                case 1 when inCompatabilityMode:
                    CreditsScreen();
                    break;
                case 2 when !inCompatabilityMode:
                    CreditsScreen();
                    break;
                case 2 when inCompatabilityMode:
                    MainMenu();
                    break;
                case 3 when !inCompatabilityMode:
                    MainMenu();
                    break;
                default:
                    break;
            }
        }
    }


    // Main Menu function
    static void MainMenu()
    {
        // Delete specific folders in temp folder
        string tempPath = Path.Join(tempDir);
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
            string welcomeMessage = patcherLang == PatcherLanguage.en
                ? "Welcome to the [springgreen2_1]WiiLink[/] Patcher!"
                : $"{localizedText?["MainMenu"]?["welcomeMessage"]}";

            // Express Install text
            string startExpressSetup = patcherLang == PatcherLanguage.en
                ? "Start Express Install Setup [bold springgreen2_1](Recommended)[/]"
                : $"{localizedText?["MainMenu"]?["startExpressSetup"]}";

            // Custom Install text
            string startCustomSetup = patcherLang == PatcherLanguage.en
                ? "Start Custom Install Setup [bold](Advanced)[/]"
                : $"{localizedText?["MainMenu"]?["startCustomSetup"]}";

            // Settings text
            string settings = patcherLang == PatcherLanguage.en
                ? "Settings"
                : $"{localizedText?["MainMenu"]?["settings"]}";

            // Visit the GitHub repository text
            string visitGitHub = patcherLang == PatcherLanguage.en
                ? "Visit the GitHub Repository"
                : $"{localizedText?["MainMenu"]?["visitGitHub"]}";

            // Visit the WiiLink website text
            string visitWiiLink = patcherLang == PatcherLanguage.en
                ? "Visit the WiiLink Website"
                : $"{localizedText?["MainMenu"]?["visitWiiLink"]}";

            // Exit Patcher text
            string exitPatcher = patcherLang == PatcherLanguage.en
                ? "Exit Patcher"
                : $"{localizedText?["MainMenu"]?["exitPatcher"]}";

            // Print all the text
            AnsiConsole.MarkupLine($"[bold]{welcomeMessage}[/]\n");

            AnsiConsole.MarkupLine($"1. {startExpressSetup}");
            AnsiConsole.MarkupLine($"2. {startCustomSetup}");
            AnsiConsole.MarkupLine($"3. {settings}\n");

            AnsiConsole.MarkupLine($"4. {visitGitHub}");

            AnsiConsole.MarkupLine($"5. {visitWiiLink}\n");

            AnsiConsole.MarkupLine($"6. {exitPatcher}\n");

            // Detect SD Card / USB Drive text
            string SDDetectedOrNot = sdcard != null
                ? $"[bold springgreen2_1]{(patcherLang == PatcherLanguage.en
                    ? "Detected SD Card / USB Drive:"
                    : localizedText?["MainMenu"]?["sdCardDetected"])}[/] {sdcard}"
                : $"[bold red]{(patcherLang == PatcherLanguage.en
                    ? "Could not detect your SD Card / USB Drive!"
                    : localizedText?["MainMenu"]?["noSDCard"])}[/]";
            AnsiConsole.MarkupLine(SDDetectedOrNot);

            // Automatically detect SD Card / USB Drive text
            string automaticDetection = patcherLang == PatcherLanguage.en
                ? "R. Automatically detect SD Card / USB Drive"
                : $"{localizedText?["MainMenu"]?["automaticDetection"]}";
            AnsiConsole.MarkupLine(automaticDetection);

            // Manually select SD Card / USB Drive text
            string manualDetection = patcherLang == PatcherLanguage.en
                ? "M. Manually select SD Card / USB Drive path\n"
                : $"{localizedText?["MainMenu"]?["manualDetection"]}\n";
            AnsiConsole.MarkupLine(manualDetection);

            // User chooses an option
            int choice = UserChoose("123456RrMm");
            switch (choice)
            {
                case 1: // Start Express Install
                    WC24Setup();
                    break;
                case 2: // Start Custom Install
                    CustomInstall_WiiLinkChannels_Setup();
                    break;
                case 3: // Settings                
                    SettingsMenu();
                    break;
                case 4: // Visit GitHub
                    VisitWebsite("https://github.com/WiiLink24/WiiLink24-Patcher");
                    break;
                case 5: // Visit WiiLink website
                    VisitWebsite("https://wiilink24.com");
                    break;
                case 6: // Clear console and Exit app
                    Console.Clear();
                    ExitApp();
                    break;
                case 7: // Automatically detect SD Card path (R/r)
                case 8:
                    sdcard = DetectRemovableDrive;
                    break;
                case 9: // Manually select SD Card path (M/m)
                case 10:
                    SDCardSelect();
                    break;
                default:
                    break;
            }
        }
    }

    private static void VisitWebsite(string url)
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

    public static (bool, int, string) CheckServer(string serverURL)
    {
        // Define the URL to the connection test file and the expected response
        string url = $"{serverURL}/connectiontest.txt";
        string expectedResponse = "If the patcher can read this, the connection test succeeds.";

        PrintHeader();

        // Display server status check message
        string checkingServerStatus = patcherLang == PatcherLanguage.en
            ? "Checking server status..."
            : $"{localizedText?["CheckServerStatus"]}";
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
                string success = patcherLang == PatcherLanguage.en
                    ? "Successfully connected to server!"
                    : $"{localizedText?["CheckServerStatus"]}";
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

    static void ConnectionFailed(int statusCode, string errorMsg)
    {
        PrintHeader();

        // Connection to server failed text
        string connectionFailed = patcherLang == PatcherLanguage.en
            ? "Connection to server failed!"
            : $"{localizedText?["ServerDown"]?["connectionFailed"]}";
        AnsiConsole.MarkupLine($"[bold blink red]{connectionFailed}[/]\n");

        // Check internet connection text
        string checkInternet = patcherLang == PatcherLanguage.en
            ? "Connection to the server failed. Please check your internet connection and try again."
            : $"{localizedText?["ServerDown"]?["checkInternet"]}";
        string serverOrInternet = patcherLang == PatcherLanguage.en
            ? "It seems that either the server is down or your internet connection is not working."
            : $"{localizedText?["ServerDown"]?["serverOrInternet"]}";
        string reportIssue = patcherLang == PatcherLanguage.en
            ? "If you are sure that your internet connection is working, please join our [link=https://discord.gg/wiilink bold springgreen2_1]Discord Server[/] and report this issue."
            : $"{localizedText?["ServerDown"]?["reportIssue"]}";
        AnsiConsole.MarkupLine($"{checkInternet}\n");
        AnsiConsole.MarkupLine($"{serverOrInternet}\n");
        AnsiConsole.MarkupLine($"{reportIssue}\n");

        // Status code text
        string statusCodeText = patcherLang == PatcherLanguage.en
            ? "Status code:"
            : $"{localizedText?["ServerDown"]?["statusCode"]}";
        string messageText = patcherLang == PatcherLanguage.en
            ? "Message:"
            : $"{localizedText?["ServerDown"]?["message"]}";
        string exitMessage = patcherLang == PatcherLanguage.en
            ? "Press any key to exit..."
            : $"{localizedText?["ServerDown"]?["exitMessage"]}";
        AnsiConsole.MarkupLine($"{statusCodeText} {statusCode}");
        AnsiConsole.MarkupLine($"{messageText} {errorMsg}\n");

        AnsiConsole.MarkupLine($"[bold yellow]{exitMessage}[/]");

        Console.ReadKey();
        ExitApp();
    }

    public static async Task CheckForUpdates(string currentVersion)
    {
        PrintHeader();

        // Check for updates text
        string checkingForUpdates = patcherLang == PatcherLanguage.en
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
            string errorRetrievingUpdateInfo = patcherLang == PatcherLanguage.en
                ? $"Error retrieving update information: [bold red]{ex.Message}[/]"
                : $"{localizedText?["CheckForUpdates"]?["errorChecking"]}"
                    .Replace("{ex.Message}", ex.Message);
            string skippingUpdateCheck = patcherLang == PatcherLanguage.en
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
                PrintHeader();

                // Prompt user to download the latest version
                string updateAvailable = patcherLang == PatcherLanguage.en
                    ? "A new version is available! Would you like to download it now?"
                    : $"{localizedText?["CheckForUpdates"]?["updateAvailable"]}";
                string currentVersionText = patcherLang == PatcherLanguage.en
                    ? "Current version:"
                    : $"{localizedText?["CheckForUpdates"]?["currentVersion"]}";
                string latestVersionText = patcherLang == PatcherLanguage.en
                    ? "Latest version:"
                    : $"{localizedText?["CheckForUpdates"]?["latestVersion"]}";

                AnsiConsole.MarkupLine($"{updateAvailable}\n");

                AnsiConsole.MarkupLine($"{currentVersionText} {currentVersion}");
                AnsiConsole.MarkupLine($"{latestVersionText} [bold springgreen2_1]{latestVersion}[/]\n");

                // Show changelog via Github link
                string changelogLink = patcherLang == PatcherLanguage.en
                    ? "Changelog:"
                    : $"{localizedText?["CheckForUpdates"]?["changelogLink"]}";
                AnsiConsole.MarkupLine($"[bold]{changelogLink}[/] [link springgreen2_1]https://github.com/WiiLink24/WiiLink24-Patcher/releases/tag/{latestVersion}[/]\n");

                // Yes/No text
                string yes = patcherLang == PatcherLanguage.en
                    ? "Yes"
                    : $"{localizedText?["CheckForUpdates"]?["yes"]}";
                string no = patcherLang == PatcherLanguage.en
                    ? "No"
                    : $"{localizedText?["CheckForUpdates"]?["no"]}";

                AnsiConsole.MarkupLine($"1. {yes}");
                AnsiConsole.MarkupLine($"2. {no}\n");

                // Get user's choice
                int choice = UserChoose("12");

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
                        string downloadingFrom = patcherLang == PatcherLanguage.en
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
                            string downloadFailed = patcherLang == PatcherLanguage.en
                                ? $"An error occurred while downloading the latest version:[/] {response.StatusCode}"
                                : $"{localizedText?["CheckForUpdates"]?["downloadFailed"]}"
                                    .Replace("{response.StatusCode}", response.StatusCode.ToString());
                            string pressAnyKey = patcherLang == PatcherLanguage.en
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
                            PrintHeader();
                            // Download complete text
                            string downloadComplete = patcherLang == PatcherLanguage.en
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
            string onLatestVersion = patcherLang == PatcherLanguage.en
                ? "You are running the latest version!"
                : $"{localizedText?["CheckForUpdates"]?["onLatestVersion"]}";
            AnsiConsole.MarkupLine($"[bold springgreen2_1]{onLatestVersion}[/]");
            Thread.Sleep(1000);
        }
    }

    static void ErrorScreen(int exitCode, string msg = "")
    {
        // Clear regional and WiiConnect24 channel selections if they exist
        wiiLinkChannels_selection.Clear();
        wiiConnect24Channels_selection.Clear();

        PrintHeader();

        // An error has occurred text
        string errorOccurred = patcherLang == PatcherLanguage.en
            ? "An error has occurred."
            : $"{localizedText?["ErrorScreen"]?["title"]}";
        AnsiConsole.MarkupLine($"[bold red]{errorOccurred}[/]\n");

        // Error details text
        string errorDetails = patcherLang == PatcherLanguage.en
            ? "ERROR DETAILS:"
            : $"{localizedText?["ErrorScreen"]?["details"]}";
        string taskText = patcherLang == PatcherLanguage.en
            ? "Task: "
            : $"{localizedText?["ErrorScreen"]?["task"]}";
        string commandText = patcherLang == PatcherLanguage.en
            ? "Command:"
            : $"{localizedText?["ErrorScreen"]?["command"]}";
        string messageText = patcherLang == PatcherLanguage.en
            ? "Message:"
            : $"{localizedText?["ErrorScreen"]?["message"]}";
        string exitCodeText = patcherLang == PatcherLanguage.en
            ? "Exit code:"
            : $"{localizedText?["ErrorScreen"]?["exitCode"]}";

        AnsiConsole.MarkupLine($"{errorDetails}\n");
        AnsiConsole.MarkupLine($" * {taskText} {task}");
        AnsiConsole.MarkupLine(msg == null ? $" * {commandText} {curCmd}" : $" * {messageText} {msg}");
        AnsiConsole.MarkupLine($" * {exitCodeText} {exitCode}\n");

        // Please open an issue text
        string openAnIssue = patcherLang == PatcherLanguage.en
            ? "Please open an issue on our GitHub page ([link bold springgreen2_1]https://github.com/WiiLink24/WiiLink24-Patcher/issues[/]) and describe the\nerror you encountered. Please include the error details above in your issue."
            : $"{localizedText?["ErrorScreen"]?["githubIssue"]}";
        AnsiConsole.MarkupLine($"{openAnIssue}\n");

        // Press any key to go back to the main menu text
        string pressAnyKey = patcherLang == PatcherLanguage.en
            ? "Press any key to go back to the main menu..."
            : $"{localizedText?["ErrorScreen"]?["pressAnyKey"]}";
        AnsiConsole.MarkupLine($"[bold]{pressAnyKey}[/]");
        Console.ReadKey();

        // Go back to the main menu
        MainMenu();
    }

    private static void CopyFolder(string sourcePath, string destinationPath)
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
        PrintHeader();

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
    static void ExitApp()
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
            ConnectionFailed(result.Item2, result.Item3);

        // Check latest version if not on a nightly build or release candidate
        if (!version.Contains("Nightly") && !version.Contains("RC"))
            await CheckForUpdates(version);

        // Go to the main menu
        MainMenu();
    }
}