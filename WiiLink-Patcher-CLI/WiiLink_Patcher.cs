using System.Diagnostics;
using System.Text;
using System.Runtime.InteropServices;
using Spectre.Console;
using libWiiSharp;
using System.Net;
using Newtonsoft.Json.Linq;

// Author: PablosCorner and WiiLink Team
// Project: WiiLink Patcher (CLI Version)
// Description: WiiLink Patcher (CLI Version) is a command-line interface to patch and revive the exclusive Japanese Wii Channels that were shut down, along with some WiiConnect24 Channels.

class WiiLink_Patcher
{
    //// Build Info ////
    static readonly string version = "v1.2.1";
    static readonly string copyrightYear = DateTime.Now.Year.ToString();
    static readonly string buildDate = "September 4th, 2023";
    static readonly string buildTime = "12:06 PM";
    static string? sdcard = DetectSDCard();
    static readonly string wiiLinkPatcherUrl = "https://patcher.wiilink24.com";
    ////////////////////

    //// Setup Info ////
    // Express Install variables
    static public Language lang;
    static public DemaeVersion demaeVersion;
    static public bool installWC24 = false;
    static public Region nc_reg;
    static public Region forecast_reg;
    static public Region evc_reg;
    static public Platform platformType;
    static public bool installKirbyTV = false;
    private static Dictionary<string, string> patchingProgress_express = new();

    // Custom Install variables
    private static List<string> japaneseChannels_selection = new();
    private static List<string> wiiConnect24Channels_selection = new();
    static Platform spdVersion_custom;
    static bool inCompatabilityMode = false;
    private static Dictionary<string, string> patchingProgress_custom = new();

    // Misc. variables
    static public string task = "";
    static public string curCmd = "";
    static readonly string curDir = Directory.GetCurrentDirectory();
    static readonly string tempDir = Path.Join(Path.GetTempPath(), "WiiLink_Patcher");
    static bool DEBUG_MODE = false;
    static public string patcherLang = "en"; // English by default
    static JObject? localizedText = null;

    // Enums
    public enum Region : int { USA, PAL, Japan }
    public enum Language : int { English, Japan, Russian, Catalan, Portuguese }
    public enum DemaeVersion : int { Standard, Dominos, Deliveroo }
    public enum Platform : int { Wii, vWii }

    // Get current console window size
    static int console_width = 0;
    static int console_height = 0;
    ////////////////////

    static void PrintHeader()
    {
        Console.Clear();

        string headerText = $"WiiLink Patcher {version} - (c) {copyrightYear} WiiLink";
        if (patcherLang != "en")
        {
            headerText = $"{localizedText?["Header"]}"
                .Replace("{version}", version)
                .Replace("{copyrightYear}", copyrightYear);
        }

        AnsiConsole.MarkupLine(headerText);

        char borderChar = '=';
        string borderLine = new(borderChar, Console.WindowWidth);

        AnsiConsole.MarkupLine($"{borderLine}\n");
    }

    static void PrintNotice()
    {
        string title = patcherLang == "en"
            ? "Notice"
            : $"{localizedText?["Notice"]?["noticeTitle"]}";
        string text = patcherLang == "en"
            ? "If you have any issues with the patcher or services offered by WiiLink, please report them on our [lime link=https://discord.gg/WiiLink]Discord Server[/]!"
            : $"{localizedText?["Notice"]?["noticeMsg"]}";

        var panel = new Panel($"[bold]{text}[/]")
        {
            Header = new PanelHeader($"[bold lime] {title} [/]", Justify.Center),
            Border = inCompatabilityMode ? BoxBorder.Ascii : BoxBorder.Heavy,
            BorderStyle = new Style(Color.Lime),
            Expand = true,
        };

        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();
    }

    static string? DetectSDCard()
    {
        // Define the base paths to check for each platform
        var basePaths = new List<string>();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            foreach (var drive in DriveInfo.GetDrives())
            {
                if (Directory.Exists(Path.Join(drive.Name, "apps")))
                    return drive.Name;
            }
        }
        else
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                basePaths.Add("/Volumes");
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                basePaths.Add("/media");
                basePaths.Add("/run/media");
            }

            foreach (var basePath in basePaths)
            {
                var drive = DriveInfo.GetDrives()
                    .FirstOrDefault(d => d.Name.StartsWith(Path.Join(basePath, Environment.UserName)) && 
                                         Directory.Exists(Path.Join(d.RootDirectory.FullName, "apps")));

                if (drive != null)
                    return drive.RootDirectory.FullName;
            }
        }

        return null;
    }

    // User choice
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
        string chooseText = patcherLang == "en"
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
        string buildInfo = patcherLang == "en"
            ? $"This build was compiled on [bold lime]{buildDate}[/] at [bold lime]{buildTime}[/]."
            : $"{localizedText?["Credits"]?["buildInfo"]}"
                .Replace("{buildDate}", buildDate)
                .Replace("{buildTime}", buildTime);
        AnsiConsole.MarkupLine($"{buildInfo}\n");

        // Credits table
        var creditTable = new Table().Border(inCompatabilityMode ? TableBorder.None : TableBorder.DoubleEdge);

        // Credits header
        string credits = patcherLang == "en"
            ? "Credits"
            : $"{localizedText?["Credits"]?["credits"]}";
        creditTable.AddColumn(new TableColumn($"[bold lime]{credits}[/]").Centered());

        // Credits grid
        var creditGrid = new Grid().AddColumn().AddColumn();

        // Credits text
        string sketchDesc = patcherLang == "en"
            ? "WiiLink Founder"
            : $"{localizedText?["Credits"]?["sketchDesc"]}";
        string pablosDesc = patcherLang == "en"
            ? "WiiLink Patcher Developer"
            : $"{localizedText?["Credits"]?["pablosDesc"]}";
        string lunaDesc = patcherLang == "en"
            ? "Lead Translator"
            : $"{localizedText?["Credits"]?["lunaDesc"]}";
        string leathlWiiDatabase = patcherLang == "en"
            ? "leathl and WiiDatabase"
            : $"{localizedText?["Credits"]?["leathlWiiDatabase"]}";
        string leathlWiiDatabaseDesc = patcherLang == "en"
            ? "libWiiSharp developers"
            : $"{localizedText?["Credits"]?["leathlWiiDatabaseDesc"]}";

        creditGrid.AddRow(new Text("Sketch", new Style(Color.Lime, null, Decoration.Bold)).RightJustified(), new Text(sketchDesc, new Style(null, null, Decoration.Bold)));
        creditGrid.AddRow(new Text("PablosCorner", new Style(Color.Lime, null, Decoration.Bold)).RightJustified(), new Text(pablosDesc, new Style(null, null, Decoration.Bold)));
        creditGrid.AddRow(new Text("Luna", new Style(Color.Lime, null, Decoration.Bold)).RightJustified(), new Text(lunaDesc, new Style(null, null, Decoration.Bold)));
        creditGrid.AddRow(new Text(leathlWiiDatabase, new Style(Color.Lime, null, Decoration.Bold)).RightJustified(), new Text(leathlWiiDatabaseDesc, new Style(null, null, Decoration.Bold)));
        creditGrid.AddRow(new Text("SnowflakePowered", new Style(Color.Lime, null, Decoration.Bold)).RightJustified(), new Text("VCDiff", new Style(null, null, Decoration.Bold)));

        // Add the grid to the table
        creditTable.AddRow(creditGrid).Centered();
        AnsiConsole.Write(creditTable);

        // Special thanks grid
        string specialThanksTo = patcherLang == "en"
            ? "Special thanks to:"
            : $"{localizedText?["Credits"]?["specialThanksTo"]}";
        AnsiConsole.MarkupLine($"\n[bold lime]{specialThanksTo}[/]\n");

        var specialThanksGrid = new Grid().AddColumn().AddColumn();

        // Special thanks text
        string theshadoweeveeRole = patcherLang == "en"
            ? "- Pointing me in the right direction with implementing libWiiSharp!"
            : $"{localizedText?["Credits"]?["theshadoweeveeRole"]}";
        string ourTesters = patcherLang == "en"
            ? "Our Testers"
            : $"{localizedText?["Credits"]?["ourTesters"]}";
        string ourTestersRole = patcherLang == "en"
            ? "- For testing the patcher and reporting bugs/anomalies!"
            : $"{localizedText?["Credits"]?["ourTestersRole"]}";
        string you = patcherLang == "en"
            ? "You!"
            : $"{localizedText?["Credits"]?["you"]}";
        string youRole = patcherLang == "en"
            ? "- For your continued support of WiiLink!"
            : $"{localizedText?["Credits"]?["youRole"]}";

        specialThanksGrid.AddRow($"  ● [bold lime]TheShadowEevee[/]", theshadoweeveeRole);
        specialThanksGrid.AddRow($"  ● [bold lime]{ourTesters}[/]", ourTestersRole);
        specialThanksGrid.AddRow($"  ● [bold lime]{you}[/]", youRole);

        AnsiConsole.Write(specialThanksGrid);
        AnsiConsole.MarkupLine("");

        // Links grid
        string wiilinkSite = patcherLang == "en"
            ? "WiiLink website"
            : $"{localizedText?["Credits"]?["wiilinkSite"]}";
        string githubRepo = patcherLang == "en"
            ? "Github repository"
            : $"{localizedText?["Credits"]?["githubRepo"]}";

        var linksGrid = new Grid().AddColumn().AddColumn();

        linksGrid.AddRow($"[bold lime]{wiilinkSite}[/]:", "[link]https://wiilink24.com[/]");
        linksGrid.AddRow($"[bold lime]{githubRepo}[/]:", "[link]https://github.com/WiiLink24/WiiLink24-Patcher[/]");

        AnsiConsole.Write(linksGrid);
        AnsiConsole.MarkupLine("");

        // Press any key to go back to settings
        string pressAnyKey = patcherLang == "en"
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
        DownloadFile($"https://hbb1.oscwii.org/hbb/{appName}.png", Path.Join(appPath, "icon.png"), appName);
    }

    /// <summary>
    /// Downloads a file from the specified URL to the specified destination with the specified name.
    /// </summary>
    /// <param name="URL">The URL to download the file from.</param>
    /// <param name="dest">The destination to save the file to.</param>
    /// <param name="name">The name of the file.</param>
    static void DownloadFile(string URL, string dest, string name)
    {
        // Loop until the file is successfully downloaded.
        while (true)
        {
            task = $"Downloading {name}";
            curCmd = $"DownloadFile({URL}, {dest}, {name})";
            if (DEBUG_MODE)
                AnsiConsole.MarkupLine($"[lime]Downloading [bold]{name}[/] from [bold]{URL}[/] to [bold]{dest}[/][/]...");
            try
            {
                // Create a new HttpClient instance to handle the download.
                using var client = new HttpClient();
                // Send a GET request to the specified URL.
                var response = client.GetAsync(URL).Result;
                if (response.IsSuccessStatusCode)
                {
                    // If the response is successful, create a new file at the specified destination and save the response stream to it.
                    using (var stream = response.Content.ReadAsStream())
                    using (var fileStream = File.Create(dest))
                    {
                        stream.CopyTo(fileStream);
                    }
                    break;
                }
                else
                {
                    int statusCode = (int)response.StatusCode;
                    ErrorScreen(statusCode, $"Failed to download [bold]{name}[/] from [bold]{URL}[/] to [bold]{dest}[/]");
                }
            }
            catch (Exception e)
            {
                AnsiConsole.MarkupLine($"[bold red]ERROR:[/] {e.Message}");
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

/*         if (folderName == "IOS31")
        {
            switch (patchInput)
            {
                case "IOS31_Wii.delta":
                    patchUrl = "https://cdn.discordapp.com/attachments/253286648291393536/1148277389445566647/IOS31_Wii.delta";
                    break;
                case "IOS31_vWii_8.delta":
                    patchUrl = "https://cdn.discordapp.com/attachments/253286648291393536/1148277390204731412/IOS31_vWii_8.delta";
                    break;
                case "IOS31_vWii_E.delta":
                    patchUrl = "https://cdn.discordapp.com/attachments/253286648291393536/1148277389818863748/IOS31_vWii_E.delta";
                    break;
            }
        } */

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
        string spdUrl = "";
        string spdDestinationPath = "";

        // Create WAD folder in current directory if it doesn't exist
        if (!Directory.Exists(Path.Join("WAD")))
            Directory.CreateDirectory(Path.Join("WAD"));

        switch (platformType)
        {
            case Platform.Wii:
                spdUrl = $"{wiiLinkPatcherUrl}/spd/SPD_Wii.wad";
                spdDestinationPath = Path.Join("WAD", "WiiLink SPD (Wii).wad");
                break;
            case Platform.vWii:
                spdUrl = $"{wiiLinkPatcherUrl}/spd/SPD_vWii.wad";
                spdDestinationPath = Path.Join("WAD", "WiiLink SPD (vWii).wad");
                break;
        }

        DownloadFile(spdUrl, spdDestinationPath, "SPD");
    }


    // Patches the Japanese-exclusive channels
    static void PatchCoreChannel(string channelName, string channelTitle, string titleID, List<KeyValuePair<string, string>> patchFilesDict, string? appVer = null, Language? lang = null)
    {
        // Set up folder paths and file names
        string titleFolder = Path.Join(tempDir, "Unpack");
        string tempFolder = Path.Join(tempDir, "Unpack_Patched");
        string patchFolder = Path.Join(tempDir, "Patches", channelName);
        string outputChannel = lang == null ? Path.Join("WAD", $"{channelTitle}.wad") : Path.Join("WAD", $"{channelTitle} ({lang}).wad");
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

        // First delta patch
        if (lang == Language.English || channelName == "Dominos")
            ApplyPatch(File.OpenRead(Path.Join(titleFolder, $"{patchFilesDict[0].Value}.app")), File.OpenRead(Path.Join(patchFolder, $"{patchFilesDict[0].Key}.delta")), File.OpenWrite(Path.Join(tempFolder, $"{patchFilesDict[0].Value}.app")));

        // Second delta patch
        ApplyPatch(File.OpenRead(Path.Join(titleFolder, $"{patchFilesDict[1].Value}.app")), File.OpenRead(Path.Join(patchFolder, $"{patchFilesDict[1].Key}.delta")), File.OpenWrite(Path.Join(tempFolder, $"{patchFilesDict[1].Value}.app")));

        // Third delta patch
        if (lang == Language.English || channelName == "Dominos" || channelName == "WiinoMa")
            ApplyPatch(File.OpenRead(Path.Join(titleFolder, $"{patchFilesDict[2].Value}.app")), File.OpenRead(Path.Join(patchFolder, $"{patchFilesDict[2].Key}.delta")), File.OpenWrite(Path.Join(tempFolder, $"{patchFilesDict[2].Value}.app")));

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
    static void PatchWC24Channel(string channelName, string channelTitle, int channelVersion, Region? channelRegion, string titleID, string[] patchFile, string[] appFile)
    {
        // Define the necessary paths and filenames
        string titleFolder = Path.Join(tempDir, "Unpack");
        string tempFolder = Path.Join(tempDir, "Unpack_Patched");
        string patchFolder = Path.Join(tempDir, "Patches", channelName);

        // Name the output WAD file
        string outputWad;
        if (channelRegion == null)
            outputWad = Path.Join("WAD", $"{channelTitle} (WiiLink).wad");
        else
            outputWad = Path.Join("WAD", $"{channelTitle} [{channelRegion}] (WiiLink).wad");

        // Create unpack and unpack-patched folders
        Directory.CreateDirectory(titleFolder);
        Directory.CreateDirectory(tempFolder);

        // Define the URLs for the necessary files
        var discordURLs = new Dictionary<string, Dictionary<string, string>>{
            // Nintendo Channel Certs and Tiks
            {"000100014841544a", new Dictionary<string, string> { // JPN
                {"cert", "https://cdn.discordapp.com/attachments/253286648291393536/1123709388641800263/000100014841544a.cert"},
                {"tmd", ""},
                {"tik", "https://cdn.discordapp.com/attachments/253286648291393536/1123709425149038612/000100014841544a.tik"}
            }},
            {"0001000148415445", new Dictionary<string, string> { // USA
                {"cert", "https://cdn.discordapp.com/attachments/253286648291393536/1123709388998324235/0001000148415445.cert"},
                {"tmd", ""},
                {"tik", "https://cdn.discordapp.com/attachments/253286648291393536/1123709425518129173/0001000148415445.tik"}
            }},
            {"0001000148415450", new Dictionary<string, string> { // PAL
                {"cert", "https://cdn.discordapp.com/attachments/253286648291393536/1123709389329678417/0001000148415450.cert"},
                {"tmd", ""},
                {"tik", "https://cdn.discordapp.com/attachments/253286648291393536/1123709425950130236/0001000148415450.tik"}
            }},
            // Forecast Channel Certs
            {"000100024841464a", new Dictionary<string, string> { // JPN
                {"cert", "https://cdn.discordapp.com/attachments/253286648291393536/1123709479372980326/000100024841464a.cert"},
                {"tmd", ""},
                {"tik", ""}
            }},
            {"0001000248414645", new Dictionary<string, string> { // USA
                {"cert", "https://cdn.discordapp.com/attachments/253286648291393536/1123709478697709638/0001000248414645.cert"},
                {"tmd", ""},
                {"tik", ""}
            }},
            {"0001000248414650", new Dictionary<string, string> { // PAL
                {"cert", "https://cdn.discordapp.com/attachments/253286648291393536/1123709479016484967/0001000248414650.cert"},
                {"tmd", ""},
                {"tik", ""}
            }},
            // Everybody Votes Channel Certs and TIK
            {"0001000148414a4a", new Dictionary<string, string> { // JPN
                {"cert", "https://cdn.discordapp.com/attachments/253286648291393536/1144052128361496638/0001000148414a4a.cert"},
                {"tmd", ""},
                {"tik", "https://cdn.discordapp.com/attachments/253286648291393536/1144052253469200534/0001000148414a4a.tik"}
            }},
            {"0001000148414a45", new Dictionary<string, string> { // USA
                {"cert", "https://cdn.discordapp.com/attachments/253286648291393536/1144052127832997959/0001000148414a45.cert"},
                {"tmd", ""},
                {"tik", "https://cdn.discordapp.com/attachments/253286648291393536/1144052253091692645/0001000148414a45.tik"}
            }},
            {"0001000148414a50", new Dictionary<string, string> { // PAL
                {"cert", "https://cdn.discordapp.com/attachments/253286648291393536/1144052128764145755/0001000148414a50.cert"},
                {"tmd", ""},
                {"tik", "https://cdn.discordapp.com/attachments/253286648291393536/1144052253804736632/0001000148414a50.tik"}
            }},
            // IOS31 Cert
            {"000000010000001f", new Dictionary<string, string> { // JPN
                {"cert", "https://cdn.discordapp.com/attachments/253286648291393536/1148279472404049951/000000010000001f.cert"},
                {"tmd", ""},
                {"tik", ""}
            }},
            // Kirby TV Channel Cert, TMD, and TIK
            {"0001000148434d50", new Dictionary<string, string> { // Global
                {"cert", "https://cdn.discordapp.com/attachments/253286648291393536/1123828090754314261/0001000148434d50.cert"},
                {"tmd", "https://cdn.discordapp.com/attachments/253286648291393536/1123828527918231563/0001000148434d50.tmd"},
                {"tik", "https://cdn.discordapp.com/attachments/253286648291393536/1123828811860017203/0001000148434d50.tik"}
            }},
            // Region Select Certs
            {"0001000848414c4a", new Dictionary<string, string> { // JPN
                {"cert", "https://cdn.discordapp.com/attachments/253286648291393536/1147995629080035448/0001000848414c4a.cert"},
                {"tmd", ""},
                {"tik", ""}
            }},
            {"0001000848414c45", new Dictionary<string, string> { // USA
                {"cert", "https://cdn.discordapp.com/attachments/253286648291393536/1147995628740288673/0001000848414c45.cert"},
                {"tmd", ""},
                {"tik", ""}
            }},
            {"0001000848414c50", new Dictionary<string, string> { // PAL
                {"cert", "https://cdn.discordapp.com/attachments/253286648291393536/1147995629461708890/0001000848414c50.cert"},
                {"tmd", ""},
                {"tik", ""}
            }},
        };

        //// Download the necessary files for the channel ////
        task = $"Downloading necessary files for {channelTitle}";

        // Download the cetk file
        DownloadFile(discordURLs[titleID]["cert"], Path.Join(titleFolder, $"{titleID}.cert"), $"{channelTitle} cert");

        // Download the tik file if it exists
        if (discordURLs[titleID]["tik"] != "")
            DownloadFile(discordURLs[titleID]["tik"], Path.Join(titleFolder, "cetk"), $"{channelTitle} tik");

        // Extract the necessary files for the channel
        task = $"Extracting stuff for {channelTitle}";
        DownloadNUS(titleID, titleFolder, channelVersion.ToString(), true);

        // Download the TMD file if it exists
        if (discordURLs[titleID]["tmd"] != "")
            DownloadFile(discordURLs[titleID]["tmd"], Path.Join(titleFolder, $"tmd.{channelVersion}"), $"{channelTitle} TMD");

        // Rename the extracted files
        task = $"Renaming files for {channelTitle}";
        File.Move(Path.Join(titleFolder, $"tmd.{channelVersion}"), Path.Join(titleFolder, $"{titleID}.tmd"));
        File.Move(Path.Join(titleFolder, "cetk"), Path.Join(titleFolder, $"{titleID}.tik"));

        // Apply the delta patch to the app file
        task = $"Applying delta patch for {channelTitle}";
        ApplyPatch(File.OpenRead(Path.Join(titleFolder, $"{appFile[0]}.app")), File.OpenRead(Path.Join(patchFolder, $"{patchFile[0]}.delta")), File.OpenWrite(Path.Join(tempFolder, $"{appFile[0]}.app")));

        // If there are more than one delta patches, apply the rest of them
        for (int i = 1; i < patchFile.Length; i++)
        {
            ApplyPatch(File.OpenRead(Path.Join(tempFolder, $"{appFile[i]}.app")), File.OpenRead(Path.Join(patchFolder, $"{patchFile[i]}.delta")), File.OpenWrite(Path.Join(tempFolder, $"{appFile[i]}.app")));
        }

        // Apply the special second Forecast patch if the channel is Forecast
        if (channelName == "forecast")
        {
            task = $"Applying Forecast Channel Fix";
            // Delete existing 0000000f.app and replace it with this one, then rename it to 0000000f.app
            File.Delete(Path.Join(titleFolder, "0000000f.app"));

            // Since the patch doesn't seem to work correctly, I'm using this as a workaround 
            DownloadFile("https://cdn.discordapp.com/attachments/1061653146872582204/1127695471666810950/banner_rso.arc", Path.Join(tempFolder, "banner_rso.arc"), "Forecast Channel Fix");

            File.Move(Path.Join(tempFolder, "banner_rso.arc"), Path.Join(titleFolder, "0000000f.app"));
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

        // Repack the title into a WAD file
        task = $"Repacking the title for {channelTitle}";
        PackWAD(titleFolder, outputWad);

        // Delete the unpack and unpack_patched folders
        Directory.Delete(titleFolder, true);
        Directory.Delete(tempFolder, true);
    }


    // Install Choose (Express Install)
    static void JapaneseChannel_LangSetup()
    {
        while (true)
        {
            PrintHeader();

            // Express Install Header Text
            string EIHeader = patcherLang == "en"
                ? "Express Install"
                : $"{localizedText?["ExpressInstall"]?["Header"]}";
            AnsiConsole.MarkupLine($"[bold lime]{EIHeader}[/]\n");

            // Express Install Welcome Message Text
            string welcomeMessage = patcherLang == "en"
                ? $"Hello [bold lime]{Environment.UserName}[/]! Welcome to the Express Installation of WiiLink!"
                : $"{localizedText?["ExpressInstall"]?["CoreChannel_LangSetup"]?["welcomeMessage"]}";
            if (patcherLang != "en")
                welcomeMessage = welcomeMessage.Replace("{userName}", $"[bold lime]{Environment.UserName}[/]");
            AnsiConsole.MarkupLine($"{welcomeMessage}\n");

            // Patcher Will Download Text
            string patcherWillDownload = patcherLang == "en"
                ? "The patcher will download any files that are required to run the patcher."
                : $"{localizedText?["ExpressInstall"]?["CoreChannel_LangSetup"]?["patcherWillDownload"]}";
            AnsiConsole.MarkupLine($"{patcherWillDownload}\n");

            // Step 1 Text
            string step1Message = patcherLang == "en"
                ? "Step 1: Choose Japanese channel language"
                : $"{localizedText?["ExpressInstall"]?["CoreChannel_LangSetup"]?["step1Message"]}";
            AnsiConsole.MarkupLine($"[bold]{step1Message}[/]\n");

            // Instructions Text
            string instructions = patcherLang == "en"
                ? "For [bold]Wii Room[/], [bold]Photo Prints Channel[/], and [bold]Food Channel[/], which language would you like to select?"
                : $"{localizedText?["ExpressInstall"]?["CoreChannel_LangSetup"]?["instructions"]}";
            AnsiConsole.MarkupLine($"{instructions}\n");

            // User Choices
            string englishTranslation = patcherLang == "en"
                ? "English Translation"
                : $"{localizedText?["ExpressInstall"]?["CoreChannel_LangSetup"]?["englishOption"]}";
            string japanese = patcherLang == "en"
                ? "Japanese"
                : $"{localizedText?["ExpressInstall"]?["CoreChannel_LangSetup"]?["japaneseOption"]}";
            string goBackToMainMenu = patcherLang == "en"
                ? "Go Back to Main Menu"
                : $"{localizedText?["ExpressInstall"]?["CoreChannel_LangSetup"]?["goBackToMainMenu"]}";

            AnsiConsole.MarkupLine($"1. {englishTranslation}");
            AnsiConsole.MarkupLine($"2. {japanese}\n");

            AnsiConsole.MarkupLine($"3. {goBackToMainMenu}\n");

            int choice = UserChoose("123");
            switch (choice)
            {
                case 1:
                    lang = Language.English;
                    DemaeConfiguration();
                    break;
                case 2:
                    lang = Language.Japan;
                    demaeVersion = DemaeVersion.Standard;
                    WiiConnect24Setup();
                    break;
                case 3:
                    MainMenu(); // Go back to main menu
                    return;
                default:
                    break;
            }
        }
    }


    // Configure Demae Channel (if English was selected) [Express Install]
    static void DemaeConfiguration()
    {
        while (true)
        {
            PrintHeader();

            // Express Install Header Text
            string EIHeader = patcherLang == "en"
                ? "Express Install"
                : $"{localizedText?["ExpressInstall"]?["Header"]}";
            AnsiConsole.MarkupLine($"[bold lime]{EIHeader}[/]\n");

            // Step 1B Text
            string step1bTitle = patcherLang == "en"
                ? "Step 1B: Choose Food Channel version"
                : $"{localizedText?["ExpressInstall"]?["DemaeConfiguration"]?["step1bTitle"]}";
            AnsiConsole.MarkupLine($"[bold]{step1bTitle}[/]\n");

            // Instructions Text
            string instructions = patcherLang == "en"
                ? "For [bold]Food Channel[/], which version would you like to install?"
                : $"{localizedText?["ExpressInstall"]?["DemaeConfiguration"]?["instructions"]}";
            AnsiConsole.MarkupLine($"{instructions}\n");

            // User Choices
            string demaeStandard = patcherLang == "en"
                ? "Standard [bold](Fake Ordering)[/]"
                : $"{localizedText?["ExpressInstall"]?["DemaeConfiguration"]?["demaeStandard"]}";
            string demaeDominos = patcherLang == "en"
                ? "Domino's [bold](US and Canada only)[/]"
                : $"{localizedText?["ExpressInstall"]?["DemaeConfiguration"]?["demaeDominos"]}";
            string demaeDeliveroo = patcherLang == "en"
                ? "Deliveroo [bold](Select countries only)[/]"
                : $"{localizedText?["ExpressInstall"]?["DemaeConfiguration"]?["demaeDeliveroo"]}";
            string goBackToMainMenu = patcherLang == "en"
                ? "Go Back to Main Menu"
                : $"{localizedText?["ExpressInstall"]?["DemaeConfiguration"]?["goBackToMainMenu"]}";

            AnsiConsole.MarkupLine($"1. {demaeStandard}");
            AnsiConsole.MarkupLine($"2. {demaeDominos}");
            AnsiConsole.MarkupLine($"3. {demaeDeliveroo}\n");

            AnsiConsole.MarkupLine($"4. {goBackToMainMenu}\n");

            int choice = UserChoose("1234");
            switch (choice)
            {
                case 1:
                    demaeVersion = DemaeVersion.Standard;
                    WiiConnect24Setup();
                    break;
                case 2:
                    demaeVersion = DemaeVersion.Dominos;
                    WiiConnect24Setup();
                    break;
                case 3:
                    demaeVersion = DemaeVersion.Deliveroo;
                    WiiConnect24Setup();
                    break;
                case 4: // Go back to main menu
                    MainMenu();
                    break;
                default:
                    break;
            }
        }
    }

    // Ask user if they want to install WiiLink's WiiConnect24 services (Express Install)
    static void WiiConnect24Setup()
    {
        while (true)
        {
            PrintHeader();

            // Express Install Header Text
            string EIHeader = patcherLang == "en"
                ? "Express Install"
                : $"{localizedText?["ExpressInstall"]?["Header"]}";
            AnsiConsole.MarkupLine($"[bold lime]{EIHeader}[/]\n");

            // Would you like to install WiiLink's WiiConnect24 services? Text
            string wouldYouLike = patcherLang == "en"
                ? "Would you like to install [bold]WiiLink's WiiConnect24 services[/]?"
                : $"{localizedText?["ExpressInstall"]?["WiiConnect24Setup"]?["wouldYouLike"]}";
            AnsiConsole.MarkupLine($"{wouldYouLike}\n");

            // Services that would be installed Text
            string toBeInstalled = patcherLang == "en"
                ? "Services that would be installed:"
                : $"{localizedText?["ExpressInstall"]?["WiiConnect24Setup"]?["toBeInstalled"]}";
            AnsiConsole.MarkupLine($"{toBeInstalled}\n");

            // Channel Names
            string nintendoChannel = patcherLang == "en"
                ? "Nintendo Channel"
                : $"{localizedText?["ExpressInstall"]?["WiiConnect24Setup"]?["NintendoChannel"]}";
            string forecastChannel = patcherLang == "en"
                ? "Forecast Channel"
                : $"{localizedText?["ExpressInstall"]?["WiiConnect24Setup"]?["ForecastChannel"]}";
            string everybodyVotesChannel = patcherLang == "en"
                ? "Everybody Votes Channel"
                : $"{localizedText?["ExpressInstall"]?["WiiConnect24Setup"]?["EverybodyVotesChannel"]}";

            AnsiConsole.MarkupLine($"  ● {nintendoChannel}");
            AnsiConsole.MarkupLine($"  ● {forecastChannel}");
            AnsiConsole.MarkupLine($"  ● {everybodyVotesChannel}\n");

            // Yes or No Text
            string yes = patcherLang == "en"
                ? "Yes"
                : $"{localizedText?["ExpressInstall"]?["WiiConnect24Setup"]?["yes"]}";
            string no = patcherLang == "en"
                ? "No"
                : $"{localizedText?["ExpressInstall"]?["WiiConnect24Setup"]?["no"]}";

            Console.WriteLine($"1. {yes}");
            Console.WriteLine($"2. {no}\n");

            // Go Back to Main Menu Text
            string goBackToMainMenu = patcherLang == "en"
                ? "Go Back to Main Menu"
                : $"{localizedText?["ExpressInstall"]?["WiiConnect24Setup"]?["goBackToMainMenu"]}";
            Console.WriteLine($"3. {goBackToMainMenu}\n");

            int choice = UserChoose("123");
            switch (choice)
            {
                case 1:
                    installWC24 = true;
                    NCSetup();
                    break;
                case 2:
                    installWC24 = false;
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

    // Configure Nintendo Channel (Express Install)
    static void NCSetup()
    {
        while (true)
        {
            PrintHeader();

            // Express Install Header Text
            string EIHeader = patcherLang == "en"
                ? "Express Install"
                : $"{localizedText?["ExpressInstall"]?["Header"]}";
            AnsiConsole.MarkupLine($"[bold lime]{EIHeader}[/]\n");

            // Step 2 Text
            string stepNum = patcherLang == "en"
                ? "Step 2"
                : $"{localizedText?["ExpressInstall"]?["NintendoChannelSetup"]?["stepNum"]}";
            string stepTitle = patcherLang == "en"
                ? "Choose Nintendo Channel region"
                : $"{localizedText?["ExpressInstall"]?["NintendoChannelSetup"]?["stepTitle"]}";

            AnsiConsole.MarkupLine($"[bold]{stepNum}: {stepTitle}[/]\n");

            // Instructions Text
            string instructions = patcherLang == "en"
                ? "For [bold]Nintendo Channel[/], which region would you like to install?"
                : $"{localizedText?["ExpressInstall"]?["NintendoChannelSetup"]?["instructions"]}";
            AnsiConsole.MarkupLine($"{instructions}\n");

            // User Choices
            string northAmerica = patcherLang == "en"
                ? "North America"
                : $"{localizedText?["ExpressInstall"]?["NintendoChannelSetup"]?["northAmerica"]}";
            string pal = patcherLang == "en"
                ? "PAL"
                : $"{localizedText?["ExpressInstall"]?["NintendoChannelSetup"]?["pal"]}";
            string japan = patcherLang == "en"
                ? "Japan"
                : $"{localizedText?["ExpressInstall"]?["NintendoChannelSetup"]?["japan"]}";
            string goBackToMainMenu = patcherLang == "en"
                ? "Go Back to Main Menu"
                : $"{localizedText?["ExpressInstall"]?["NintendoChannelSetup"]?["goBackToMainMenu"]}";

            AnsiConsole.MarkupLine($"1. {northAmerica}");
            AnsiConsole.MarkupLine($"2. {pal}");
            AnsiConsole.MarkupLine($"3. {japan}\n");

            AnsiConsole.MarkupLine($"4. {goBackToMainMenu}\n");

            int choice = UserChoose("1234");
            switch (choice)
            {
                case 1: // USA
                    nc_reg = Region.USA;
                    ForecastSetup();
                    break;
                case 2: // PAL
                    nc_reg = Region.PAL;
                    ForecastSetup();
                    break;
                case 3: // Japan
                    nc_reg = Region.Japan;
                    ForecastSetup();
                    break;
                case 4: // Go back to main menu
                    MainMenu();
                    break;
                default:
                    break;
            }
        }
    }


    // Configure Forecast Channel (Express Install)
    static void ForecastSetup()
    {
        while (true)
        {
            PrintHeader();

            // Express Install Header Text
            string EIHeader = patcherLang == "en"
                ? "Express Install"
                : $"{localizedText?["ExpressInstall"]?["Header"]}";
            AnsiConsole.MarkupLine($"[bold lime]{EIHeader}[/]\n");

            // Step 3 Text
            string stepNum = patcherLang == "en"
                ? "Step 3"
                : $"{localizedText?["ExpressInstall"]?["ForecastChannelSetup"]?["stepNum"]}";
            string stepTitle = patcherLang == "en"
                ? "Choose Forecast Channel region"
                : $"{localizedText?["ExpressInstall"]?["ForecastChannelSetup"]?["stepTitle"]}";

            AnsiConsole.MarkupLine($"[bold]{stepNum}: {stepTitle}[/]\n");

            // Instructions Text
            string instructions = patcherLang == "en"
                ? "For [bold]Forecast Channel[/], which region would you like to install?"
                : $"{localizedText?["ExpressInstall"]?["ForecastChannelSetup"]?["instructions"]}";
            AnsiConsole.MarkupLine($"{instructions}\n");

            // User Choices
            string northAmerica = patcherLang == "en"
                ? "North America"
                : $"{localizedText?["ExpressInstall"]?["ForecastChannelSetup"]?["northAmerica"]}";
            string pal = patcherLang == "en"
                ? "PAL"
                : $"{localizedText?["ExpressInstall"]?["ForecastChannelSetup"]?["pal"]}";
            string japan = patcherLang == "en"
                ? "Japan"
                : $"{localizedText?["ExpressInstall"]?["ForecastChannelSetup"]?["japan"]}";
            string goBackToMainMenu = patcherLang == "en"
                ? "Go Back to Main Menu"
                : $"{localizedText?["ExpressInstall"]?["ForecastChannelSetup"]?["goBackToMainMenu"]}";

            AnsiConsole.MarkupLine($"1. {northAmerica}");
            AnsiConsole.MarkupLine($"2. {pal}");
            AnsiConsole.MarkupLine($"3. {japan}\n");

            AnsiConsole.MarkupLine($"4. {goBackToMainMenu}\n");

            int choice = UserChoose("1234");
            switch (choice)
            {
                case 1: // USA
                    forecast_reg = Region.USA;
                    EVCSetup();
                    break;
                case 2: // PAL
                    forecast_reg = Region.PAL;
                    EVCSetup();
                    break;
                case 3: // Japan
                    forecast_reg = Region.Japan;
                    EVCSetup();
                    break;
                case 4: // Go back to main menu
                    MainMenu();
                    break;
                default:
                    break;
            }
        }
    }

    // Configure Everybody Votes Channel (Express Install)
    static void EVCSetup()
    {
        while (true)
        {
            PrintHeader();

            // Express Install Header Text
            string EIHeader = patcherLang == "en"
                ? "Express Install"
                : $"{localizedText?["ExpressInstall"]?["Header"]}";
            AnsiConsole.MarkupLine($"[bold lime]{EIHeader}[/]\n");

            // Step 4 Text
            string stepNum = patcherLang == "en"
                ? "Step 4"
                : $"{localizedText?["ExpressInstall"]?["EVCSetup"]?["stepNum"]}";
            string stepTitle = patcherLang == "en"
                ? "Choose Everybody Votes Channel region"
                : $"{localizedText?["ExpressInstall"]?["EVCSetup"]?["stepTitle"]}";

            AnsiConsole.MarkupLine($"[bold]{stepNum}: {stepTitle}[/]\n");

            // Instructions Text
            string instructions = patcherLang == "en"
                ? "For [bold]Everybody Votes Channel[/], which region would you like to install?"
                : $"{localizedText?["ExpressInstall"]?["EVCSetup"]?["instructions"]}";
            AnsiConsole.MarkupLine($"{instructions}\n");

            // User Choices
            string northAmerica = patcherLang == "en"
                ? "North America"
                : $"{localizedText?["ExpressInstall"]?["EVCSetup"]?["northAmerica"]}";
            string pal = patcherLang == "en"
                ? "PAL"
                : $"{localizedText?["ExpressInstall"]?["EVCSetup"]?["pal"]}";
            string japan = patcherLang == "en"
                ? "Japan"
                : $"{localizedText?["ExpressInstall"]?["EVCSetup"]?["japan"]}";
            string goBackToMainMenu = patcherLang == "en"
                ? "Go Back to Main Menu"
                : $"{localizedText?["ExpressInstall"]?["EVCSetup"]?["goBackToMainMenu"]}";

            AnsiConsole.MarkupLine($"1. {northAmerica}");
            AnsiConsole.MarkupLine($"2. {pal}");
            AnsiConsole.MarkupLine($"3. {japan}\n");

            AnsiConsole.MarkupLine($"4. {goBackToMainMenu}\n");

            int choice = UserChoose("1234");
            switch (choice)
            {
                case 1: // USA
                    evc_reg = Region.USA;
                    KirbyTVSetup();
                    break;
                case 2: // PAL
                    evc_reg = Region.PAL;
                    KirbyTVSetup();
                    break;
                case 3: // Japan
                    evc_reg = Region.Japan;
                    KirbyTVSetup();
                    break;
                case 4: // Go back to main menu
                    MainMenu();
                    break;
                default:
                    break;
            }
        }
    }


    // Ask user if they want to install Kirby TV Channel (Express Install)
    static void KirbyTVSetup()
    {
        while (true)
        {
            PrintHeader();

            // Express Install Header Text
            string EIHeader = patcherLang == "en"
                ? "Express Install"
                : $"{localizedText?["ExpressInstall"]?["Header"]}";
            AnsiConsole.MarkupLine($"[bold lime]{EIHeader}[/]\n");

            // Change step number depending on if WiiConnect24 is being installed or not
            string stepNum = patcherLang == "en"
                ? !installWC24 ? "Step 2" : "Step 5"
                : $"{localizedText?["ExpressInstall"]?["KirbyTVSetup"]?[!installWC24 ? "ifNoWC24" : "ifWC24"]?["stepNum:"]}";

            // Step Text
            string stepTitle = patcherLang == "en"
                ? $"Choose to install [bold]Kirby TV Channel[/]"
                : $"{localizedText?["ExpressInstall"]?["KirbyTVSetup"]?["stepTitle"]}";
            AnsiConsole.MarkupLine($"[bold]{stepNum}: {stepTitle}[/]\n");

            // Instructions Text
            string instructions = patcherLang == "en"
                ? "Would you like to install [bold]Kirby TV Channel[/]?"
                : $"{localizedText?["ExpressInstall"]?["KirbyTVSetup"]?["instructions"]}";
            AnsiConsole.MarkupLine($"{instructions}\n");

            // User Choices
            string yes = patcherLang == "en"
                ? "Yes"
                : $"{localizedText?["ExpressInstall"]?["KirbyTVSetup"]?["yes"]}";
            string no = patcherLang == "en"
                ? "No"
                : $"{localizedText?["ExpressInstall"]?["KirbyTVSetup"]?["no"]}";
            string goBackToMainMenu = patcherLang == "en"
                ? "Go Back to Main Menu"
                : $"{localizedText?["ExpressInstall"]?["KirbyTVSetup"]?["goBackToMainMenu"]}";

            AnsiConsole.MarkupLine($"1. {yes}");
            AnsiConsole.MarkupLine($"2. {no}\n");

            AnsiConsole.MarkupLine($"3. {goBackToMainMenu}\n");

            int choice = UserChoose("123");
            switch (choice)
            {
                case 1:
                    installKirbyTV = true;
                    ChoosePlatform();
                    break;
                case 2:
                    installKirbyTV = false;
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


    // Choose console platformType (Wii [Dolphin Emulator] or vWii [Wii U]) [Express Install]
    static void ChoosePlatform()
    {
        while (true)
        {
            PrintHeader();

            // Express Install Header Text
            string EIHeader = patcherLang == "en"
                ? "Express Install"
                : $"{localizedText?["ExpressInstall"]?["Header"]}";
            AnsiConsole.MarkupLine($"[bold lime]{EIHeader}[/]\n");

            // Change step number depending on if WiiConnect24 is being installed or not
            string stepNum = patcherLang == "en"
                ? !installWC24 ? "Step 3" : "Step 6"
                : $"{localizedText?["ExpressInstall"]?["ChoosePlatform"]?[!installWC24 ? "ifNoWC24" : "ifWC24"]?["stepNum"]}";
            string stepTitle = patcherLang == "en"
                ? "Choose console platform"
                : $"{localizedText?["ExpressInstall"]?["ChoosePlatform"]?["stepTitle"]}";

            AnsiConsole.MarkupLine($"[bold]{stepNum}: {stepTitle}[/]\n");

            // Instructions Text
            string instructions = patcherLang == "en"
                ? "Which Wii version are you installing to?"
                : $"{localizedText?["ExpressInstall"]?["ChoosePlatform"]?["instructions"]}";
            AnsiConsole.MarkupLine($"{instructions}\n");

            // User Choices
            string wii = patcherLang == "en"
                ? "Wii [bold](or Dolphin Emulator)[/]"
                : $"{localizedText?["ExpressInstall"]?["ChoosePlatform"]?["wii"]}";
            string vWii = patcherLang == "en"
                ? "vWii [bold](Wii U)[/]"
                : $"{localizedText?["ExpressInstall"]?["ChoosePlatform"]?["vWii"]}";
            string goBackToMainMenu = patcherLang == "en"
                ? "Go Back to Main Menu"
                : $"{localizedText?["ExpressInstall"]?["ChoosePlatform"]?["goBackToMainMenu"]}";

            AnsiConsole.MarkupLine($"1. {wii}");
            AnsiConsole.MarkupLine($"2. {vWii}\n");

            AnsiConsole.MarkupLine($"3. {goBackToMainMenu}\n");

            int choice = UserChoose("123");
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
                case 3: // Go back to main menu
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
            string stepNum = patcherLang == "en"
                ? isCustomSetup ? "Step 4" : (!installWC24 ? "Step 4" : "Step 7")
                : $"{localizedText?["SDSetup"]?[isCustomSetup ? "ifCustom" : "ifExpress"]?[installWC24 ? "ifWC24" : "ifNoWC24"]?["stepNum"]}";

            // Change header depending on if it's Express Install or Custom Install
            string installType = patcherLang == "en"
                ? isCustomSetup ? "Custom Install" : "Express Install"
                : isCustomSetup ? $"{localizedText?["ExpressInstall"]?["Header"]}" : $"{localizedText?["CustomInstall"]?["Header"]}";

            // Step title
            string stepTitle = patcherLang == "en"
                ? "Insert SD Card / USB Drive (if applicable)"
                : $"{localizedText?["SDSetup"]?["stepTitle"]}";

            // After passing this step text
            string afterPassingThisStep = patcherLang == "en"
                ? "After passing this step, any user interaction won't be needed, so sit back and relax!"
                : $"{localizedText?["SDSetup"]?["afterPassingThisStep"]}";

            // Download to SD card text
            string downloadToSD = patcherLang == "en"
                ? "You can download everything directly to your Wii SD Card / USB Drive if you insert it before starting the patching\nprocess. Otherwise, everything will be saved in the same folder as this patcher on your computer."
                : $"{localizedText?["SDSetup"]?["downloadToSD"]}";

            // User Choices
            string startOption = patcherLang == "en"
                ? sdcard != null ? "Start" : "Start without SD Card / USB Drive"
                : sdcard != null ? $"{localizedText?["SDSetup"]?["start_withSD"]}" : $"{localizedText?["SDSetup"]?["start_noSD"]}";

            // User Choices
            string manualDetection = patcherLang == "en"
                ? "Manually Select SD Card / USB Drive Path"
                : $"{localizedText?["SDSetup"]?["manualDetection"]}";

            // SD card detected text
            string sdDetected = patcherLang == "en"
                ? sdcard != null ? $"SD card detected: [bold lime]{sdcard}[/]" : ""
                : sdcard != null ? $"{localizedText?["SDSetup"]?["sdDetected"]}: [bold lime]{sdcard}[/]" : "";

            // Go Back to Main Menu Text
            string goBackToMainMenu = patcherLang == "en"
                ? "Go Back to Main Menu"
                : $"{localizedText?["ExpressInstall"]?["SDSetup"]?["goBackToMainMenu"]}";

            AnsiConsole.MarkupLine($"[bold lime]{installType}[/]\n");

            AnsiConsole.MarkupLine($"[bold]{stepNum}: {stepTitle}[/]\n");

            Console.WriteLine($"{afterPassingThisStep}\n");

            Console.WriteLine($"{downloadToSD}\n");

            AnsiConsole.MarkupLine($"1. {startOption}");
            AnsiConsole.MarkupLine($"2. {manualDetection}\n");
            if (sdcard != null)
                AnsiConsole.MarkupLine(sdDetected);
            Console.WriteLine($"3. {goBackToMainMenu}\n");

            int choice = UserChoose("123");
            switch (choice)
            {
                case 1: // Check if WAD folder exists
                    WADFolderCheck(isCustomSetup);
                    break;
                case 2: // Manually select SD card
                    SDCardSelect();
                    break;
                case 3: // Go back to main menu
                    MainMenu();
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
                string installType = patcherLang == "en"
                    ? isCustomSetup ? "Custom Install" : "Express Install"
                    : isCustomSetup ? $"{localizedText?["ExpressInstall"]?["Header"]}" : $"{localizedText?["CustomInstall"]?["Header"]}";
                string stepNum = patcherLang == "en"
                    ? isCustomSetup ? "Step 5" : (!installWC24 ? "Step 5" : "Step 8")
                    : isCustomSetup ? $"{localizedText?["WADFolderCheck"]?["ifCustom"]?["stepNum"]}" : $"{localizedText?["WADFolderCheck"]?["ifExpress"]?[installWC24 ? "ifWC24" : "ifNoWC24"]?["stepNum"]}";

                AnsiConsole.MarkupLine($"[bold lime]{installType}[/]\n");

                // Step title
                string stepTitle = patcherLang == "en"
                    ? "WAD folder detected"
                    : $"{localizedText?["WADFolderCheck"]?["stepTitle"]}";

                AnsiConsole.MarkupLine($"[bold]{stepNum}: {stepTitle}[/]\n");

                // WAD folder detected text
                string wadFolderDetected = patcherLang == "en"
                    ? "A [bold]WAD[/] folder has been detected in the current directory. This folder is used to store the WAD files that are downloaded during the patching process. If you choose to delete this folder, it will be recreated when you start the patching process again."
                    : $"{localizedText?["WADFolderCheck"]?["wadFolderDetected"]}";

                AnsiConsole.MarkupLine($"{wadFolderDetected}\n");

                // User Choices
                string deleteWADFolder = patcherLang == "en"
                    ? "Delete WAD folder"
                    : $"{localizedText?["WADFolderCheck"]?["deleteWADFolder"]}";
                string keepWADFolder = patcherLang == "en"
                    ? "Keep WAD folder"
                    : $"{localizedText?["WADFolderCheck"]?["keepWADFolder"]}";
                string goBackToMainMenu = patcherLang == "en"
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
                            string pressAnyKey = patcherLang == "en"
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
            : demaeVersion == DemaeVersion.Dominos
                ? "Domino's"
                : "Deliveroo";

        // Define Japanese channel titles
        string demae_title = patcherLang == "en" // Demae Channel
            ? lang == Language.English
                ? $"Food Channel [bold](English)[/] [bold][[{demaeVerTxt}]][/]"
                : $"Demae Channel [bold](Japanese)[/] [bold][[{demaeVerTxt}]][/]"
            : $"{localizedText?["ChannelNames"]?[$"{lang}]?[${(lang == Language.English ? "food" : "demae")}"]} [bold]({lang})[/] [bold][[{demaeVerTxt}]][/]";

        string wiiroom_title = patcherLang == "en" // Wii no Ma
            ? lang == Language.English
                ? "Wii Room [bold](English)[/]"
                : "Wii no Ma [bold](Japanese)[/]"
            : $"{localizedText?["ChannelNames"]?[$"{lang}"]?[$"{(lang == Language.English ? "wiiRoom" : "wiiNoMa")}"]} [bold]({lang})[/]";

        string digicam_title = patcherLang == "en" // Digicam Print Channel
            ? lang == Language.English
                ? "Photo Prints Channel [bold](English)[/]"
                : "Digicam Print Channel [bold](Japanese)[/]"
            : $"{localizedText?["ChannelNames"]?[$"{lang}"]?[$"{(lang == Language.English ? "photoPrints" : "digicam")}"]} [bold]({lang})[/]";

        // Define the channelMessages dictionary
        var channelMessages = new Dictionary<string, string>
        {
            { "wiiroom", wiiroom_title },
            { "digicam", digicam_title },
            { "demae", demae_title }
        };

        // Define and add WC24 channel titles to the channelMessages dictionary (if applicable)
        if (installWC24)
        {
            string NCTitle = patcherLang == "en" // Nintendo Channel
                ? $"{(nc_reg == Region.USA || nc_reg == Region.PAL ? "Nintendo Channel" : "Minna no Nintendo Channel")} [bold]({nc_reg})[/]"
                : $"{localizedText?["ChannelNames"]?[(nc_reg == Region.USA || nc_reg == Region.PAL ? "International" : "Japanese")]?["nintendoChn"]} [bold]({nc_reg})[/]";

            string forecastTitle = patcherLang == "en" // Forecast Channel
                ? $"Forecast Channel [bold]({forecast_reg})[/]"
                : $"{localizedText?["ChannelNames"]?["International"]?["forecastChn"]} [bold]({forecast_reg})[/]";

            string evcTitle = patcherLang == "en" // Everybody Votes Channel
                ? $"Everybody Votes Channel [bold]({evc_reg})[/]"
                : $"{localizedText?["ChannelNames"]?["International"]?["everybodyVotes"]} [bold]({evc_reg})[/]";

            channelMessages.Add("nc", NCTitle);
            channelMessages.Add("forecast", forecastTitle);
            channelMessages.Add("evc", evcTitle);
            //channelMessages.Add("ios31", $"{(platformType == Platform.Wii ? "IOS31 Patch [bold][[Wii]][/]" : "IOS31 Patches [bold][[vWii]][/]")}");
        }

        //// Setup patching process list ////
        var patching_functions = new List<Action>
        {
            () => DownloadAllPatches(),
            () => WiiRoom_Patch(lang),
            () => Digicam_Patch(lang),
            () => Demae_Patch(lang, demaeVersion)
        };

        // Add Kirby TV Channel patching function if applicable
        if (installKirbyTV)
            patching_functions.Add(() => KirbyTV_Patch());

        // Add WiiConnect24 patching functions if applicable
        if (installWC24)
        {
            patching_functions.Add(() => NC_Patch(nc_reg));
            patching_functions.Add(() => Forecast_Patch(forecast_reg));
            patching_functions.Add(() => EVC_Patch(evc_reg));
        }


        patching_functions.Add(() => FinishSDCopy());

        //// Set up patching progress dictionary ////
        // Flush dictionary and downloading patches
        patchingProgress_express.Clear();
        patchingProgress_express.Add("downloading", "in_progress");

        // Patching Japanese channels
        foreach (string channel in new string[] { "wiiroom", "digicam", "demae" })
            patchingProgress_express.Add(channel, "not_started");

        // Patching Kirby TV Channel (if applicable)
        if (installKirbyTV)
            patchingProgress_express.Add("kirbytv", "not_started");

        // Patching WiiConnect24 channels
        if (installWC24)
        {
            foreach (string channel in new string[] { "nc", "forecast", "evc" })
                patchingProgress_express.Add(channel, "not_started");
        }

        // Finishing up
        patchingProgress_express.Add("finishing", "not_started");

        // While the patching process is not finished
        while (patchingProgress_express["finishing"] != "done")
        {
            PrintHeader();

            // Progress bar and completion display
            string patching = patcherLang == "en"
                ? "Patching... this can take some time depending on the processing speed (CPU) of your computer."
                : $"{localizedText?["PatchingProgress"]?["patching"]}";
            string progress = patcherLang == "en"
                ? "Progress"
                : $"{localizedText?["PatchingProgress"]?["progress"]}";
            AnsiConsole.MarkupLine($"[bold][[*]] {patching}[/]\n");
            AnsiConsole.Markup($"    {progress}: ");

            //// Progress bar and completion display ////
            // Calculate percentage based on how many channels are completed
            int percentage = (int)((float)patchingProgress_express.Where(x => x.Value == "done").Count() / (float)patchingProgress_express.Count * 100.0f);

            // Calculate progress bar
            counter_done = (int)((float)percentage / 10.0f);
            StringBuilder progressBar = new("[[");
            for (int i = 0; i < counter_done; i++)
            {
                progressBar.Append("[bold lime]■[/]");
            }
            for (int i = counter_done; i < 10; i++)
            {
                progressBar.Append(" ");
            }
            progressBar.Append("]]");
            AnsiConsole.Markup(progressBar.ToString());

            // Display percentage
            string percentComplete = patcherLang == "en"
                ? "completed"
                : $"{localizedText?["PatchingProgress"]?["percentComplete"]}";
            string pleaseWait = patcherLang == "en"
                ? "Please wait while the patching process is in progress..."
                : $"{localizedText?["PatchingProgress"]?["pleaseWait"]}";
            AnsiConsole.Markup($" [bold]{percentage}%[/] {percentComplete}\n\n");
            AnsiConsole.MarkupLine($"{pleaseWait}\n");

            //// Display progress for each channel ////

            // Pre-Patching Section: Downloading files
            string prePatching = patcherLang == "en"
                ? "Pre-Patching"
                : $"{localizedText?["PatchingProgress"]?["prePatching"]}";
            string downloadingFiles = patcherLang == "en"
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
                    AnsiConsole.MarkupLine($"[bold lime]●[/] {downloadingFiles}");
                    break;
            }

            // Patching Section: Patching Japanese channels
            string patchingJapaneseChannels = patcherLang == "en"
                ? "Patching Japanese channels"
                : $"{localizedText?["PatchingProgress"]?["patchingJapaneseChannels"]}";
            AnsiConsole.MarkupLine($"\n[bold]{patchingJapaneseChannels}:[/]");
            foreach (string channel in new string[] { "wiiroom", "digicam", "demae" })
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
                        AnsiConsole.MarkupLine($"[bold lime]●[/] {channelMessages[channel]}");
                        break;
                }
            }

            // Patching Kirby TV Channel (if applicable) in Japanese Channels section
            string kirbyTVChannel = patcherLang == "en"
                ? "Kirby TV Channel"
                : $"{localizedText?["PatchingProgress"]?["kirbyTVChannel"]}";
            if (installKirbyTV)
            {
                switch (patchingProgress_express["kirbytv"])
                {
                    case "not_started":
                        AnsiConsole.MarkupLine($"○ {kirbyTVChannel}");
                        break;
                    case "in_progress":
                        AnsiConsole.MarkupLine($"[slowblink yellow]●[/] {kirbyTVChannel}");
                        break;
                    case "done":
                        AnsiConsole.MarkupLine($"[bold lime]●[/] {kirbyTVChannel}");
                        break;
                }
            }

            // Patching Section: Patching WiiConnect24 channels (if applicable)
            string patchingWiiConnect24Channels = patcherLang == "en"
                ? "Patching WiiConnect24 Channels"
                : $"{localizedText?["PatchingProgress"]?["patchingWiiConnect24Channels"]}";
            if (installWC24)
            {
                AnsiConsole.MarkupLine($"\n[bold]{patchingWiiConnect24Channels}:[/]");
                foreach (string channel in new string[] { "nc", "forecast", "evc" })
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
                            AnsiConsole.MarkupLine($"[bold lime]●[/] {channelMessages[channel]}");
                            break;
                    }
                }
            }
            AnsiConsole.MarkupLine("");

            // Post-Patching Section: Finishing up
            string postPatching = patcherLang == "en"
                ? "Post-Patching"
                : $"{localizedText?["PatchingProgress"]?["postPatching"]}";
            string finishingUp = patcherLang == "en"
                ? "Finishing up..."
                : $"{localizedText?["PatchingProgress"]?["finishingUp"]}";
            AnsiConsole.MarkupLine($"[bold]{postPatching}:[/]");
            switch (patchingProgress_express["finishing"])
            {
                case "not_started":
                    AnsiConsole.MarkupLine($"○ {finishingUp}");
                    break;
                case "in_progress":
                    AnsiConsole.MarkupLine($"[slowblink yellow]●[/] {finishingUp}");
                    break;
                case "done":
                    AnsiConsole.MarkupLine($"[bold lime]●[/] {finishingUp}");
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
        List<string> channelsToPatch = new List<string>();
        foreach (string channel in japaneseChannels_selection)
            channelsToPatch.Add(channel);
        foreach (string channel in wiiConnect24Channels_selection)
            channelsToPatch.Add(channel);

        // If any WC24 channels are selected, add IOS31 to the list
        /*         if (wiiConnect24Channels_selection.Count > 0)
                    channelsToPatch.Add("ios31"); */

        // Set up patching progress dictionary
        patchingProgress_custom.Clear(); // Flush dictionary
        patchingProgress_custom.Add("downloading", "in_progress"); // Downloading patches
        foreach (string channel in channelsToPatch) // Patching channels
            patchingProgress_custom.Add(channel, "not_started");
        patchingProgress_custom.Add("finishing", "not_started"); // Finishing up

        // Give each Japanese channel a proper name
        var channelMap = new Dictionary<string, string>()
        {
            { "wiiroom_en", "Wii Room [bold](English)[/]" },
            { "wiinoma_jp", "Wii no Ma [bold](Japanese)[/]" },
            { "digicam_en", "Photo Prints Channel [bold](English)[/]" },
            { "digicam_jp", "Digicam Print Channel [bold](Japanese)[/]" },
            { "food_en", "Food Channel [bold](Standard) [[English]][/]" },
            { "demae_jp", "Demae Channel [bold](Standard) [[Japanese]][/]" },
            { "food_dominos", "Food Channel [bold](Dominos) [[English]][/]" },
            { "food_deliveroo", "Food Channel [bold](Deliveroo) [[English]][/]" },
            { "nc_us", "Nintendo Channel [bold](USA)[/]" },
            { "nc_eu", "Nintendo Channel [bold](Europe)[/]" },
            { "mnnc_jp", "Minna no Nintendo Channel [bold](Japan)[/]" },
            { "forecast_us", "Forecast Channel [bold](USA)[/]" },
            { "forecast_eu", "Forecast Channel [bold](Europe)[/]" },
            { "forecast_jp", "Forecast Channel [bold](Japan)[/]" },
            { "evc_us", "Everybody Votes Channel [bold](USA)[/]" },
            { "evc_eu", "Everybody Votes Channel [bold](Europe)[/]" },
            { "evc_jp", "Everybody Votes Channel [bold](Japan)[/]" },
            { "kirbytv", "Kirby TV Channel" }
        };

        // Setup patching process arrays based on the selected channels
        var channelPatchingFunctions = new Dictionary<string, Action>()
        {
            { "wiiroom_en", () => WiiRoom_Patch(Language.English) },
            { "wiinoma_jp", () => WiiRoom_Patch(Language.Japan) },
            { "digicam_en", () => Digicam_Patch(Language.English) },
            { "digicam_jp", () => Digicam_Patch(Language.Japan) },
            { "food_en", () => Demae_Patch(Language.English, DemaeVersion.Standard) },
            { "demae_jp", () => Demae_Patch(Language.Japan, DemaeVersion.Standard) },
            { "food_dominos", () => Demae_Patch(Language.English, DemaeVersion.Dominos) },
            { "food_deliveroo", () => Demae_Patch(Language.English, DemaeVersion.Deliveroo) },
            { "kirbytv", () => KirbyTV_Patch() },
            { "nc_us", () => NC_Patch(Region.USA) },
            { "nc_eu", () => NC_Patch(Region.PAL) },
            { "mnnc_jp", () => NC_Patch(Region.Japan) },
            { "forecast_us", () => Forecast_Patch(Region.USA) },
            { "forecast_eu", () => Forecast_Patch(Region.PAL) },
            { "forecast_jp", () => Forecast_Patch(Region.Japan) },
            { "evc_us", () => EVC_Patch(Region.USA) },
            { "evc_eu", () => EVC_Patch(Region.PAL) },
            { "evc_jp", () => EVC_Patch(Region.Japan) }
        };

        // Create a list of patching functions to execute
        var selectedPatchingFunctions = new List<Action>
        {
            // Add the patching functions to the list
            () => DownloadCustomPatches(channelsToPatch)
        };

        foreach (string selectedChannel in channelsToPatch)
            selectedPatchingFunctions.Add(channelPatchingFunctions[selectedChannel]);

        selectedPatchingFunctions.Add(() => FinishSDCopy());

        // Start patching
        int totalChannels = channelsToPatch.Count;
        while (patchingProgress_custom["finishing"] != "done")
        {
            PrintHeader();

            // Progress text
            string patching = patcherLang == "en"
                ? "Patching... this can take some time depending on the processing speed (CPU) of your computer."
                : $"{localizedText?["PatchingProgress"]?["patching"]}";
            string progress = patcherLang == "en"
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
            StringBuilder progressBar = new StringBuilder("[[");
            for (int i = 0; i < counter_done; i++)
                progressBar.Append("[bold lime]■[/]");
            for (int i = counter_done; i < 10; i++)
                progressBar.Append(" ");
            progressBar.Append("]]");

            AnsiConsole.Markup(progressBar.ToString());

            // Display percentage
            string percentComplete = patcherLang == "en"
                ? "completed"
                : $"{localizedText?["PatchingProgress"]?["percentComplete"]}";
            string pleaseWait = patcherLang == "en"
                ? "Please wait while the patching process is in progress..."
                : $"{localizedText?["PatchingProgress"]?["pleaseWait"]}";
            AnsiConsole.Markup($" [bold]{percentage}%[/] {percentComplete}\n\n");

            //// Display progress for each channel ////

            // Pre-Patching Section: Downloading files
            string prePatching = patcherLang == "en"
                ? "Pre-Patching"
                : $"{localizedText?["PatchingProgress"]?["prePatching"]}";
            string downloadingFiles = patcherLang == "en"
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
                    AnsiConsole.MarkupLine($"[bold lime]●[/] {downloadingFiles}");
                    break;
            }

            // Patching Section: Patching Japanese channels
            if (japaneseChannels_selection.Count > 0)
            {
                string patchingJapaneseChannels = patcherLang == "en"
                    ? "Patching Japanese Channels"
                    : $"{localizedText?["PatchingProgress"]?["patchingJapaneseChannels"]}";
                AnsiConsole.MarkupLine($"\n[bold]{patchingJapaneseChannels}:[/]");
                foreach (string jpnChannel in channelsToPatch)
                {
                    List<string> jpnChannels = new() { "wiiroom_en", "wiinoma_jp", "digicam_en", "digicam_jp", "food_en", "demae_jp", "food_dominos", "food_deliveroo", "kirbytv" };
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
                                AnsiConsole.MarkupLine($"[bold lime]●[/] {channelMap[jpnChannel]}");
                                break;
                        }
                    }
                }
            }

            // Patching Section: Patching WiiConnect24 channels
            if (wiiConnect24Channels_selection.Count > 0)
            {
                //AnsiConsole.MarkupLine("\n[bold]Patching WiiConnect24 Channels:[/]");
                string patchingWC24Channels = patcherLang == "en"
                    ? "Patching WiiConnect24 Channels"
                    : $"{localizedText?["PatchingProgress"]?["patchingWC24Channels"]}";
                AnsiConsole.MarkupLine($"\n[bold]{patchingWC24Channels}:[/]");
                foreach (string wiiConnect24Channel in channelsToPatch)
                {
                    List<string> wiiConnect24Channels = new() { "nc_us", "nc_eu", "mnnc_jp", "forecast_us", "forecast_eu", "forecast_jp", "evc_us", "evc_eu", "evc_jp" };
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
                                AnsiConsole.MarkupLine($"[bold lime]●[/] {channelMap[wiiConnect24Channel]}");
                                break;
                        }
                    }
                }
            }

            // Post-Patching Section: Finishing up
            string postPatching = patcherLang == "en"
                ? "Post-Patching"
                : $"{localizedText?["PatchingProgress"]?["postPatching"]}";
            string finishingUp = patcherLang == "en"
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
                    AnsiConsole.MarkupLine($"[bold lime]●[/] {finishingUp}");
                    break;
            }
            AnsiConsole.MarkupLine("");

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
                case int n when (n > 1 && n < totalChannels + 1):
                    // If we're on a channel that's not the first or last, mark the previous channel as done and the current channel as in progress
                    patchingProgress_custom[channelsToPatch[partCompleted - 2]] = "done";
                    patchingProgress_custom[channelsToPatch[partCompleted - 1]] = "in_progress";
                    break;
                case int n when (n == totalChannels + 1):
                    // If we're on the last channel, mark the previous channel as done and finishing as in progress
                    patchingProgress_custom[channelsToPatch[partCompleted - 2]] = "done";
                    patchingProgress_custom["finishing"] = "in_progress";
                    break;
                case int n when (n == totalChannels + 2):
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
        if (lang == Language.English)
            DownloadSPD(platformType);
        else
        {
            if (!Directory.Exists("WAD"))
                Directory.CreateDirectory("WAD");
        }

        //// Downloading All Channel Patches ////

        // Wii no Ma (Wii Room)
        if (lang == Language.English)
            DownloadPatch("WiinoMa", $"WiinoMa_0_{lang}.delta", $"WiinoMa_0_{lang}.delta", "Wii no Ma");
        DownloadPatch("WiinoMa", $"WiinoMa_1_{lang}.delta", $"WiinoMa_1_{lang}.delta", "Wii no Ma");
        DownloadPatch("WiinoMa", $"WiinoMa_2_{lang}.delta", $"WiinoMa_2_{lang}.delta", "Wii no Ma");

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
            case DemaeVersion.Deliveroo:
                DownloadPatch("Deliveroo", $"Deliveroo_0.delta", "Deliveroo_0.delta", "Demae Channel (Deliveroo)");
                DownloadPatch("Deliveroo", $"Deliveroo_1.delta", "Deliveroo_1.delta", "Demae Channel (Deliveroo)");
                DownloadPatch("Deliveroo", $"Deliveroo_2.delta", "Deliveroo_2.delta", "Demae Channel (Deliveroo)");
                break;
        }

        // Download yawmME from OSC for installing WADs on the Wii
        DownloadOSCApp("yawmME");

        // Downloading Get Console ID (for Demae Domino's and Deliveroo) from OSC
        if (demaeVersion == DemaeVersion.Dominos || demaeVersion == DemaeVersion.Deliveroo)
            DownloadOSCApp("GetConsoleID");

        // Download WC24 patches if applicable
        if (installWC24)
        {
            // Nintendo Channel
            DownloadPatch("nc", $"NC_1_{nc_reg}.delta", $"NC_1_{nc_reg}.delta", "Nintendo Channel");

            // Forecast Channel
            DownloadPatch("forecast", $"Forecast_1.delta", "Forecast_1.delta", "Forecast Channel");

            // Download AnyGlobe_Changer from OSC for use with the Forecast Channel
            DownloadOSCApp("AnyGlobe_Changer");

            // Everybody Votes Channel and Region Select Channel
            DownloadPatch("evc", $"EVC_1_{evc_reg}.delta", $"EVC_1_{evc_reg}.delta", "Everybody Votes Channel");
            DownloadPatch("RegSel", $"RegSel_1.delta", "RegSel_1.delta", "Region Select");

            // Download EVC-Transfer-Tool from OSC for use with the Everybody Votes Channel
            DownloadOSCApp("EVC-Transfer-Tool");

            // Download IOS31 patch based on platform type
            /*             if (platformType == Platform.Wii)
                        {
                            DownloadPatch("IOS31", $"IOS31_Wii.delta", "IOS31_Wii.delta", "IOS31 Patch (Wii)");
                        } else {
                            DownloadPatch("IOS31", $"IOS31_vWii_8.delta", "IOS31_vWii_8.delta", "IOS31 Patch (vWii)");
                            DownloadPatch("IOS31", $"IOS31_vWii_E.delta", "IOS31_vWii_E.delta", "IOS31 Patch (vWii)");
                        } */
        }

        // Kirby TV Channel (only if user chose to install it)
        if (installKirbyTV)
            DownloadPatch("ktv", $"ktv_2.delta", "KirbyTV_2.delta", "Kirby TV Channel");

        // Downloading stuff is finished!
        patchingProgress_express["downloading"] = "done";
        patchingProgress_express["wiiroom"] = "in_progress";
    }

    // Page selection function
    public static (int, int) GetPageIndices(int currentPage, int totalItems, int itemsPerPage)
    {
        int start = (currentPage - 1) * itemsPerPage;
        int end = Math.Min(start + itemsPerPage, totalItems);
        return (start, end);
    }

    // Custom Install (Part 1 - Select Japanese channels)
    static void CustomInstall_JapaneseChannel_Setup()
    {
        task = "Custom Install (Part 1 - Select Japanese channels)";

        // Flush the list of selected channels (in case the user goes back to the previous menu)
        japaneseChannels_selection.Clear();
        wiiConnect24Channels_selection.Clear();

        // Define a dictionary to map channel names to easy-to-read format
        var channelMap = new Dictionary<string, string>()
        {
            { "Wii Room [bold](English)[/]", "wiiroom_en" },
            { "Wii no Ma [bold](Japanese)[/]", "wiinoma_jp" },
            { "Photo Prints Channel [bold](English)[/]", "digicam_en" },
            { "Digicam Print Channel [bold](Japanese)[/]", "digicam_jp" },
            { "Food Channel [bold](Standard) [[English]][/]", "food_en" },
            { "Demae Channel [bold](Standard) [[Japanese]][/]", "demae_jp" },
            { "Food Channel [bold](Dominos) [[English]][/]", "food_dominos" },
            { "Food Channel [bold](Deliveroo) [[English]][/]", "food_deliveroo" },
            { "Kirby TV Channel", "kirbytv" }
        };

        // Initialize selection array to "Not selected" using LINQ
        string[] selected = channelMap.Values.Select(_ => "[grey]Not selected[/]").ToArray();

        // Page setup
        const int ITEMS_PER_PAGE = 9;
        int currentPage = 1;

        while (true)
        {
            PrintHeader();

            // Print title
            //AnsiConsole.MarkupLine("[bold lime]Custom Install[/]");
            string customInstall = patcherLang == "en"
                ? "Custom Install"
                : $"{localizedText?["CustomSetup"]?["Header"]}";
            AnsiConsole.MarkupLine($"[bold lime]{customInstall}[/]\n");

            //AnsiConsole.MarkupLine("[bold]Step 1:[/] Select Japanese channel(s) to install\n");
            // Print step number and title
            string stepNum = patcherLang == "en"
                ? "Step 1"
                : $"{localizedText?["CustomSetup"]?["japaneseChannel_Setup"]?["stepNum"]}";
            string stepTitle = patcherLang == "en"
                ? "Select Japanese channel(s) to install"
                : $"{localizedText?["CustomSetup"]?["japaneseChannel_Setup"]?["stepTitle"]}";
            AnsiConsole.MarkupLine($"[bold]{stepNum}:[/] {stepTitle}\n");

            // Display Japanese channel selection menu
            //AnsiConsole.MarkupLine("[bold]Select Japanese channel(s) to install:[/]\n");
            string selectJapanese = patcherLang == "en"
                ? "Select Japanese channel(s) to install:"
                : $"{localizedText?["CustomSetup"]?["japaneseChannel_Setup"]?["selectJapanese"]}";
            AnsiConsole.MarkupLine($"[bold]{selectJapanese}[/]\n");
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
                grid.AddRow($"[bold]{i - start + 1}.[/] {channel.Key}", selected[i]);
            }

            AnsiConsole.Write(grid);
            Console.WriteLine();

            // Page navigation
            double totalPages = Math.Ceiling((double)channelMap.Count / ITEMS_PER_PAGE);

            // Only display page navigation and number if there's more than one page
            if (totalPages > 1)
            {
                // If the current page is greater than 1, display a bold white '<' for previous page navigation
                // Otherwise, display a grey '<'
                AnsiConsole.Markup(currentPage > 1 ? "[bold white]<[/] " : "[grey]<[/] ");

                // Print page number
                string pageNum = patcherLang == "en"
                    ? $"Page {currentPage} of {totalPages}"
                    : $"{localizedText?["CustomSetup"]?["pageNum"]}"
                        .Replace("{currentPage}", currentPage.ToString())
                        .Replace("{totalPages}", totalPages.ToString());
                AnsiConsole.Markup($"[bold]{pageNum}[/] ");

                // If the current page is less than total pages, display a bold white '>' for next page navigation
                // Otherwise, display a grey '>'
                AnsiConsole.Markup(currentPage < totalPages ? "[bold white]>[/]" : "[grey]>[/]");


                // Print instructions
                //AnsiConsole.MarkupLine(" [grey](Press [bold white]<-[/] or [bold white]->[/] to navigate pages)[/]\n");
                string pageInstructions = patcherLang == "en"
                    ? "(Press [bold white]<-[/] or [bold white]->[/] to navigate pages)"
                    : $"{localizedText?["CustomSetup"]?["pageInstructions"]}";
                AnsiConsole.MarkupLine($" [grey]{pageInstructions}[/]\n");
            }

            string regInstructions = patcherLang == "en"
                ? "< Press [bold white]a number[/] to select/deselect a channel, [bold white]ENTER[/] to continue, [bold white]Backspace[/] to go back, [bold white]ESC[/] to go back to exit setup >"
                : $"{localizedText?["CustomSetup"]?["regInstructions"]}";
            AnsiConsole.MarkupLine($"[grey]{regInstructions}[/]\n");

            // Print regular instructions
            //AnsiConsole.MarkupLine("[grey]< Press [bold white]a number[/] to select/deselect a channel, [bold white]ENTER[/] to continue, [bold white]Backspace[/] to go back, [bold white]ESC[/] to go back to exit setup >[/]\n");

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
            string notSelected = patcherLang == "en"
                ? "Not selected"
                : $"{localizedText?["CustomSetup"]?["notSelected"]}";
            string selectedText = patcherLang == "en"
                ? "Selected"
                : $"{localizedText?["CustomSetup"]?["selected"]}";

            // Handle user input
            switch (choice)
            {
                case -1: // Escape
                case -2: // Backspace
                    japaneseChannels_selection.Clear();
                    wiiConnect24Channels_selection.Clear();
                    MainMenu();
                    break;
                case 0: // Enter
                    // Save selected channels to global variable if any are selected
                    foreach (string channel in channelMap.Values.Where(selected.Contains))
                    {
                        japaneseChannels_selection.Add(channel);
                    }

                    CustomInstall_WiiConnect24_Setup();
                    break;
                default:
                    if (choice >= 1 && choice <= Math.Min(ITEMS_PER_PAGE, channelMap.Count - start))
                    {
                        int index = start + choice - 1;
                        string channelName = channelMap.Values.ElementAt(index);
                        if (selected.Contains(channelName))
                        {
                            selected = selected.Where(val => val != channelName).ToArray();
                            selected[index] = $"[grey]{notSelected}[/]";
                        }
                        else
                        {
                            selected = selected.Append(channelName).ToArray();
                            selected[index] = $"[bold lime]{selectedText}[/]";
                        }
                    }
                    break;
            }
        }
    }

    // Custom Install (Part 2 - Select WiiConnect24 channels)
    static void CustomInstall_WiiConnect24_Setup()
    {
        task = "Custom Install (Part 2 - Select WiiConnect24 channels)";

        // Define a dictionary to map channel names to easy-to-read format
        var channelMap = new Dictionary<string, string>()
        {
            { "Nintendo Channel [bold](USA)[/]", "nc_us" },
            { "Nintendo Channel [bold](Europe)[/]", "nc_eu" },
            { "Minna no Nintendo Channel [bold](Japan)[/]", "mnnc_jp" },
            { "Forecast Channel [bold](USA)[/]", "forecast_us" },
            { "Forecast Channel [bold](Europe)[/]", "forecast_eu" },
            { "Forecast Channel [bold](Japan)[/]", "forecast_jp" },
            { "Everybody Votes Channel [bold](USA)[/]", "evc_us" },
            { "Everybody Votes Channel [bold](Europe)[/]", "evc_eu" },
            { "Everybody Votes Channel [bold](Japan)[/]", "evc_jp" }
        };

        // Initialize selection array to "Not selected" using LINQ
        string[] selected = channelMap.Values.Select(_ => "[grey]Not selected[/]").ToArray();

        // Page setup
        const int ITEMS_PER_PAGE = 9;
        int currentPage = 1;

        while (true)
        {
            PrintHeader();

            // Print title
            //AnsiConsole.MarkupLine("[bold lime]Custom Install[/]\n");
            string customInstall = patcherLang == "en"
                ? "Custom Install"
                : $"{localizedText?["CustomSetup"]?["Header"]}";
            AnsiConsole.MarkupLine($"[bold lime]{customInstall}[/]\n");

            //AnsiConsole.MarkupLine("[bold]Step 2:[/] Select WiiConnect24 channel(s)\n");
            // Print step number and title
            string stepNum = patcherLang == "en"
                ? "Step 2"
                : $"{localizedText?["CustomSetup"]?["wiiConnect24_Setup"]?["stepNum"]}";
            string stepTitle = patcherLang == "en"
                ? "Choose WiiConnect24 channel(s) to install"
                : $"{localizedText?["CustomSetup"]?["wiiConnect24_Setup"]?["stepTitle"]}";
            AnsiConsole.MarkupLine($"[bold]{stepNum}:[/] {stepTitle}\n");

            // Display WC24 channel selection menu
            //AnsiConsole.MarkupLine("[bold]Select WiiConnect24 channel(s) to install:[/]\n");
            string selectWiiConnect24 = patcherLang == "en"
                ? "Select WiiConnect24 channel(s) to install:"
                : $"{localizedText?["CustomSetup"]?["wiiConnect24_Setup"]?["selectWiiConnect24"]}";
            AnsiConsole.MarkupLine($"[bold]{selectWiiConnect24}[/]\n");
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
                grid.AddRow($"[bold]{i - start + 1}.[/] {channel.Key}", selected[i]);
            }

            AnsiConsole.Write(grid);
            Console.WriteLine();

            // Page navigation
            double totalPages = Math.Ceiling((double)channelMap.Count / ITEMS_PER_PAGE);

            // Only display page navigation and number if there's more than one page
            if (totalPages > 1)
            {
                // If the current page is greater than 1, display a bold white '<' for previous page navigation
                // Otherwise, display a grey '<'
                AnsiConsole.Markup(currentPage > 1 ? "[bold white]<[/] " : "[grey]<[/] ");

                // Print page number
                string pageNum = patcherLang == "en"
                    ? $"Page {currentPage} of {totalPages}"
                    : $"{localizedText?["CustomSetup"]?["pageNum"]}"
                        .Replace("{currentPage}", currentPage.ToString())
                        .Replace("{totalPages}", totalPages.ToString());
                AnsiConsole.Markup($"[bold]{pageNum}[/] ");

                // If the current page is less than total pages, display a bold white '>' for next page navigation
                // Otherwise, display a grey '>'
                AnsiConsole.Markup(currentPage < totalPages ? "[bold white]>[/]" : "[grey]>[/]");


                // Print instructions
                //AnsiConsole.MarkupLine(" [grey](Press [bold white]<-[/] or [bold white]->[/] to navigate pages)[/]\n");
                string pageInstructions = patcherLang == "en"
                    ? "(Press [bold white]<-[/] or [bold white]->[/] to navigate pages)"
                    : $"{localizedText?["CustomSetup"]?["pageInstructions"]}";
                AnsiConsole.MarkupLine($" [grey]{pageInstructions}[/]\n");
            }
            string regInstructions = patcherLang == "en"
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
            string notSelected = patcherLang == "en"
                ? "Not selected"
                : $"{localizedText?["CustomSetup"]?["notSelected"]}";
            string selectedText = patcherLang == "en"
                ? "Selected"
                : $"{localizedText?["CustomSetup"]?["selected"]}";

            // Handle user input
            switch (choice)
            {
                case -1: // Escape
                    wiiConnect24Channels_selection.Clear();
                    japaneseChannels_selection.Clear();
                    MainMenu();
                    break;
                case -2: // Backspace
                    wiiConnect24Channels_selection.Clear();
                    japaneseChannels_selection.Clear();
                    CustomInstall_JapaneseChannel_Setup();
                    break;
                case 0: // Enter
                    // Save selected channels to global variable if any are selected
                    foreach (string channel in channelMap.Values.Where(selected.Contains))
                    {
                        wiiConnect24Channels_selection.Add(channel);
                    }

                    // If both coreChannels_selection and wiiConnect24Channels_selection are empty, error out
                    if (!japaneseChannels_selection.Any() && !wiiConnect24Channels_selection.Any())
                    {
                        //AnsiConsole.MarkupLine("\n[bold red]ERROR:[/] You must select at least one channel to proceed!");
                        string mustSelectOneChannel = patcherLang == "en"
                            ? "[bold red]ERROR:[/] You must select at least one channel to proceed!"
                            : $"{localizedText?["CustomSetup"]?["mustSelectOneChannel"]}";
                        AnsiConsole.MarkupLine($"\n{mustSelectOneChannel}");
                        Thread.Sleep(3000);
                        continue;
                    }

                    // If any selected Japanese channels have "_en" in their name, go to SPD setup
                    if (japaneseChannels_selection.Any(channel => channel.Contains("_en")) || japaneseChannels_selection.Contains("food_dominos") || japaneseChannels_selection.Contains("food_deliveroo"))
                        CustomInstall_SPD_Setup();
                    else
                        CustomInstall_SummaryScreen();
                    break;
                default:
                    if (choice >= 1 && choice <= Math.Min(ITEMS_PER_PAGE, channelMap.Count - start))
                    {
                        int index = start + choice - 1;
                        string channelName = channelMap.Values.ElementAt(index);
                        if (selected.Contains(channelName))
                        {
                            selected = selected.Where(val => val != channelName).ToArray();
                            selected[index] = $"[grey]{notSelected}[/]";
                        }
                        else
                        {
                            selected = selected.Append(channelName).ToArray();
                            selected[index] = $"[bold lime]{selectedText}[/]";
                        }
                    }
                    break;
            }
        }
    }

    // Custom Install (Part 3 - Select SPD version)
    static void CustomInstall_SPD_Setup()
    {
        task = "Custom Install (Part 3 - Select SPD version)";
        while (true)
        {
            PrintHeader();

            // Print title
            string customInstall = patcherLang == "en"
                ? "Custom Install"
                : $"{localizedText?["CustomSetup"]?["Header"]}";
            AnsiConsole.MarkupLine($"[bold lime]{customInstall}[/]\n");

            // Print step number and title
            string stepNum = patcherLang == "en"
                ? "Step 3"
                : $"{localizedText?["CustomSetup"]?["SPD_Setup"]?["stepNum"]}";
            string stepTitle = patcherLang == "en"
                ? "Select WiiLink SPD version"
                : $"{localizedText?["CustomSetup"]?["SPD_Setup"]?["stepTitle"]}";
            AnsiConsole.MarkupLine($"[bold]{stepNum}:[/] {stepTitle}\n");

            // Display SPD version selection menu
            string selectSPDVer = patcherLang == "en"
                ? "Select WiiLink SPD version to install:"
                : $"{localizedText?["CustomSetup"]?["SPD_Setup"]?["selectSPDVer"]}";
            AnsiConsole.MarkupLine($"[bold]{selectSPDVer}[/]\n");

            // Print SPD version options
            string spdVersionWii = patcherLang == "en"
                ? "WiiLink SPD [bold grey][[Wii]][/]"
                : $"{localizedText?["CustomSetup"]?["SPD_Setup"]?["spdVersionWii"]}";
            string spdVersionvWii = patcherLang == "en"
                ? "WiiLink SPD [bold deepskyblue1][[vWii]][/]"
                : $"{localizedText?["CustomSetup"]?["SPD_Setup"]?["spdVersionvWii"]}";
            AnsiConsole.MarkupLine($"[bold]1.[/] {spdVersionWii}");
            AnsiConsole.MarkupLine($"[bold]2.[/] {spdVersionvWii}\n");

            // Print instructions
            string spdInstructions = patcherLang == "en"
                ? "< Press [bold white]a number[/] to select a version, [bold white]Backspace[/] to go back, [bold white]ESC[/] to go back to exit setup >"
                : $"{localizedText?["CustomSetup"]?["SPD_Setup"]?["spdInstructions"]}";
            AnsiConsole.MarkupLine($"[grey]{spdInstructions}[/]\n");

            int choice = UserChoose("12");

            // Use a switch statement to handle user's SPD version selection
            switch (choice)
            {
                case -1: // Escape
                    wiiConnect24Channels_selection.Clear();
                    japaneseChannels_selection.Clear();
                    MainMenu();
                    break;
                case -2: // Backspace
                    wiiConnect24Channels_selection.Clear();
                    CustomInstall_WiiConnect24_Setup();
                    break;
                case 1:
                    spdVersion_custom = Platform.Wii;
                    CustomInstall_SummaryScreen(showSPD: true);
                    break;
                case 2:
                    spdVersion_custom = Platform.vWii;
                    CustomInstall_SummaryScreen(showSPD: true);
                    break;
                default:
                    break;
            }
        }
    }


    // Custom Install (Part 4 - Show summary of selected channels to be installed)
    static void CustomInstall_SummaryScreen(bool showSPD = false)
    {
        task = "Custom Install (Part 4 - Show summary of selected channels to be installed)";
        // Convert Japanese channel names to proper names
        var coreChannelMap = new Dictionary<string, string>()
        {
            { "wiiroom_en", "● Wii Room [bold](English)[/]" },
            { "wiinoma_jp", "● Wii no Ma [bold](Japanese)[/]" },
            { "digicam_en", "● Photo Prints Channel [bold](English)[/]" },
            { "digicam_jp", "● Digicam Print Channel [bold](Japanese)[/]" },
            { "food_en", "● Food Channel [bold](Standard) [[English]][/]" },
            { "demae_jp", "● Demae Channel [bold](Standard) [[Japanese]][/]" },
            { "food_dominos", "● Food Channel [bold](Dominos) [[English]][/]" },
            { "food_deliveroo", "● Food Channel [bold](Deliveroo) [[English]][/]" },
            { "kirbytv", "● Kirby TV Channel" }
        };

        var selectedJPChannels = new List<string>();
        if (japaneseChannels_selection.Count > 0)
        {
            foreach (string channel in japaneseChannels_selection)
            {
                if (coreChannelMap.TryGetValue(channel, out string? modifiedChannel))
                    selectedJPChannels.Add(modifiedChannel);
            }
        }
        else
        {
            selectedJPChannels.Add("● [grey]N/A[/]");
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
            { "evc_us", "● Everybody Votes Channel [bold](USA)[/]" },
            { "evc_eu", "● Everybody Votes Channel [bold](Europe)[/]" },
            { "evc_jp", "● Everybody Votes Channel [bold](Japan)[/]" }
        };

        var selectedWiiConnect24Channels = new List<string>();
        if (wiiConnect24Channels_selection.Count > 0)
        {
            foreach (string channel in wiiConnect24Channels_selection)
            {
                if (wiiConnect24ChannelMap.TryGetValue(channel, out string? modifiedChannel))
                    selectedWiiConnect24Channels.Add(modifiedChannel);
            }
        }
        else
        {
            selectedWiiConnect24Channels.Add("● [grey]N/A[/]");
        }

        while (true)
        {
            PrintHeader();

            // Print title
            string customInstall = patcherLang == "en"
                ? "Custom Install"
                : $"{localizedText?["CustomSetup"]?["Header"]}";
            string summaryHeader = patcherLang == "en"
                ? "Summary of selected channels to be installed:"
                : $"{localizedText?["CustomSetup"]?["summaryScreen"]?["summaryHeader"]}";
            AnsiConsole.MarkupLine($"[bold lime]{customInstall}[/]\n");
            AnsiConsole.MarkupLine($"[bold]{summaryHeader}[/]\n");

            // Display summary of selected channels in two columns using a grid
            var grid = new Grid();
            grid.AddColumn();
            grid.AddColumn();

            // Grid header text
            string japaneseChannels = patcherLang == "en"
                ? "Japanese channels:"
                : $"{localizedText?["CustomSetup"]?["summaryScreen"]?["japaneseChannels"]}";
            string wiiConnect24Channels = patcherLang == "en"
                ? "WiiConnect24 Channels:"
                : $"{localizedText?["CustomSetup"]?["summaryScreen"]?["wiiConnect24Channels"]}";
            string spdVersion = patcherLang == "en"
                ? "SPD Version:"
                : $"{localizedText?["CustomSetup"]?["summaryScreen"]?["SPDVersion"]}";

            if (showSPD)
            {
                grid.AddColumn();
                grid.AddRow($"[bold lime]{japaneseChannels}[/]", $"[bold lime]{wiiConnect24Channels}[/]", $"[bold lime]{spdVersion}[/]");
                grid.AddRow(string.Join("\n", selectedJPChannels), string.Join("\n", selectedWiiConnect24Channels), spdVersion_custom == Platform.Wii ? "● [bold grey]Wii[/]" : "● [bold deepskyblue1]vWii[/]");
            }
            else
            {
                grid.AddRow($"[bold lime]{japaneseChannels}[/]", $"[bold lime]{wiiConnect24Channels}[/]");
                grid.AddRow(string.Join("\n", selectedJPChannels), string.Join("\n", selectedWiiConnect24Channels));
            }
            AnsiConsole.Write(grid);

            // Print instructions
            string prompt = patcherLang == "en"
                ? "Are you sure you want to install these selected channels?"
                : $"{localizedText?["CustomSetup"]?["summaryScreen"]?["confirmation"]?["prompt"]}";

            // User confirmation strings
            string yes = patcherLang == "en"
                ? "Yes"
                : $"{localizedText?["CustomSetup"]?["summaryScreen"]?["confirmation"]?["yes"]}";
            string noStartOver = patcherLang == "en"
                ? "No, start over"
                : $"{localizedText?["CustomSetup"]?["summaryScreen"]?["confirmation"]?["noStartOver"]}";
            string noGoBackToMainMenu = patcherLang == "en"
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
                    SDSetup(isCustomSetup: true);
                    break;
                case 2: // No, start over
                    japaneseChannels_selection.Clear();
                    wiiConnect24Channels_selection.Clear();
                    CustomInstall_JapaneseChannel_Setup();
                    break;
                case 3: // No, go back to main menu
                    japaneseChannels_selection.Clear();
                    wiiConnect24Channels_selection.Clear();
                    MainMenu();
                    break;
                default:
                    break;
            }
        }
    }

    // Download respective patches for selected core and WiiConnect24 channels (and SPD if English is selected for Japanese channels)
    static void DownloadCustomPatches(List<string> channelSelection)
    {
        task = "Downloading selected patches";

        // Download SPD if English is selected for Japanese channels (or if Demae Domino's or Deliveroo is selected)
        if (japaneseChannels_selection.Any(channel => channel.Contains("_en")) || japaneseChannels_selection.Contains("food_dominos") || japaneseChannels_selection.Contains("food_deliveroo"))
            DownloadSPD(spdVersion_custom);
        else
            Directory.CreateDirectory("WAD");

        // Download IOS31 pacthes if any WC24 channels are selected
        /*         if (wiiConnect24Channels_selection.Count > 0)
                {
                    task = "Downloading IOS31 patches";
                    DownloadPatch("IOS31", $"IOS31_Wii.delta", "IOS31_Wii.delta", "IOS31 (Wii)");
                    DownloadPatch("IOS31", $"IOS31_vWii_8.delta", "IOS31_vWii_8.delta", "IOS31 (vWii)");
                    DownloadPatch("IOS31", $"IOS31_vWii_E.delta", "IOS31_vWii_E.delta", "IOS31 (vWii)");
                } */

        // Download patches for selected Japanese channels
        foreach (string channel in channelSelection)
        {
            switch (channel)
            {
                case "wiiroom_en":
                    task = "Downloading Wii Room (English)";
                    DownloadPatch("WiinoMa", $"WiinoMa_0_English.delta", "WiinoMa_0_English.delta", "Wii Room");
                    DownloadPatch("WiinoMa", $"WiinoMa_1_English.delta", "WiinoMa_1_English.delta", "Wii Room");
                    DownloadPatch("WiinoMa", $"WiinoMa_2_English.delta", "WiinoMa_2_English.delta", "Wii Room");
                    break;
                case "wiinoma_jp":
                    task = "Downloading Wii no Ma (Japan)";
                    DownloadPatch("WiinoMa", $"WiinoMa_1_Japan.delta", "WiinoMa_1_Japan.delta", "Wii no Ma");
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
                case "food_deliveroo":
                    task = "Downloading Food Channel (Deliveroo)";
                    DownloadPatch("Deliveroo", $"Deliveroo_0.delta", "Deliveroo_0.delta", "Food Channel (Deliveroo)");
                    DownloadPatch("Deliveroo", $"Deliveroo_1.delta", "Deliveroo_1.delta", "Food Channel (Deliveroo)");
                    DownloadPatch("Deliveroo", $"Deliveroo_2.delta", "Deliveroo_2.delta", "Food Channel (Deliveroo)");
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
                    DownloadOSCApp("AnyGlobe_Changer"); // Download AnyGlobe_Changer from OSC for use with the Forecast Channel
                    break;
                case "evc_us":
                    task = $"Downloading Everybody Votes Channel (USA)";
                    DownloadPatch("evc", $"EVC_1_USA.delta", "EVC_1_USA.delta", "Everybody Votes Channel");
                    DownloadPatch("RegSel", "RegSel_1.delta", "RegSel_1.delta", "Region Select");
                    DownloadOSCApp("EVC-Transfer-Tool"); // Download EVC-Transfer-Tool from OSC for use with the Everybody Votes Channel
                    break;
                case "evc_eu":
                    task = $"Downloading Everybody Votes Channel (PAL)";
                    DownloadPatch("evc", $"EVC_1_PAL.delta", "EVC_1_PAL.delta", "Everybody Votes Channel");
                    DownloadPatch("RegSel", "RegSel_1.delta", "RegSel_1.delta", "Region Select");
                    DownloadOSCApp("EVC-Transfer-Tool"); // Download EVC-Transfer-Tool from OSC for use with the Everybody Votes Channel
                    break;
                case "evc_jp":
                    task = $"Downloading Everybody Votes Channel (Japan)";
                    DownloadPatch("evc", $"EVC_1_Japan.delta", "EVC_1_Japan.delta", "Everybody Votes Channel");
                    DownloadPatch("RegSel", "RegSel_1.delta", "RegSel_1.delta", "Region Select");
                    DownloadOSCApp("EVC-Transfer-Tool"); // Download EVC-Transfer-Tool from OSC for use with the Everybody Votes Channel
                    break;
                case "kirbytv":
                    task = "Downloading Kirby TV Channel";
                    DownloadPatch("ktv", $"ktv_2.delta", "KirbyTV_2.delta", "Kirby TV Channel");
                    break;
            }
        }

        // Downloading yawmME from OSC
        DownloadOSCApp("yawmME");
    }

    // Patching Wii no Ma
    static void WiiRoom_Patch(Language lang)
    {
        task = "Patching Wii no Ma";

        // Dictionary for which files to patch
        var wiiRoomPatchList = new List<KeyValuePair<string, string>>()
        {
            new($"WiinoMa_0_{lang}", "00000000"),
            new($"WiinoMa_1_{lang}", "00000001"),
            new($"WiinoMa_2_{lang}", "00000002")
        };

        // If English, change channel title to "Wii Room"
        string wiiRoomTitle = lang == Language.English ? "Wii Room" : "Wii no Ma";

        PatchCoreChannel("WiinoMa", wiiRoomTitle, "000100014843494a", wiiRoomPatchList, lang: lang);

        // Finished patching Wii no Ma
        patchingProgress_express["wiiroom"] = "done";
        patchingProgress_express["digicam"] = "in_progress";
    }

    // Patching Digicam Print Channel
    static void Digicam_Patch(Language lang)
    {
        task = "Patching Digicam Print Channel";

        // Dictionary for which files to patch
        var digicamPatchList = new List<KeyValuePair<string, string>>()
        {
            new($"Digicam_0_{lang}", "00000000"),
            new($"Digicam_1_{lang}", "00000001"),
            new($"Digicam_2_{lang}", "00000002")
        };

        PatchCoreChannel("Digicam", $"{(lang == Language.English ? "Photo Prints Channel" : "Digicam Print Channel")}", "000100014843444a", digicamPatchList, lang: lang);

        // Finished patching Digicam Print Channel
        patchingProgress_express["digicam"] = "done";
        patchingProgress_express["demae"] = "in_progress";
    }

    // Patching Demae Channel
    static void Demae_Patch(Language lang, DemaeVersion demaeVersion)
    {
        task = "Patching Demae Channel";

        // If language chosen is English, change channel title to "Food Channel", else "Demae Channel"
        string demaeTitle = lang == Language.English ? "Food Channel" : "Demae Channel";

        // Generate patch list for Demae Channel based on version (Standard, Dominos, Deliveroo)
        List<KeyValuePair<string, string>> GeneratePatchList(string prefix, DemaeVersion version)
        {
            bool appendLang = version == DemaeVersion.Standard;
            return new List<KeyValuePair<string, string>>
            {
                new($"{prefix}_0{(appendLang ? $"_{lang}" : "")}", "00000000"),
                new($"{prefix}_1{(appendLang ? $"_{lang}" : "")}", "00000001"),
                new($"{prefix}_2{(appendLang ? $"_{lang}" : "")}", "00000002")
            };
        }

        // Map DemaeVersion to patch list and folder name (Patch list, folder name, version text)
        var demaeData = new Dictionary<DemaeVersion, (List<KeyValuePair<string, string>>, string)>()
        {
            [DemaeVersion.Standard] = (GeneratePatchList("Demae", DemaeVersion.Standard), "Demae"),
            [DemaeVersion.Dominos] = (GeneratePatchList("Dominos", DemaeVersion.Dominos), "Dominos"),
            [DemaeVersion.Deliveroo] = (GeneratePatchList("Deliveroo", DemaeVersion.Deliveroo), "Deliveroo")
        };

        // Get patch list and folder name based on demae_version
        var (demaePatchList, folderName) = demaeData[demaeVersion];

        // Get string representation of demaeVersion
        var demaeVerText = demaeVersion.ToString();

        PatchCoreChannel(folderName, $"{demaeTitle} ({demaeVerText})", "000100014843484a", demaePatchList, lang: lang);

        // Finished patching Demae Channel
        patchingProgress_express["demae"] = "done";
        patchingProgress_express[!installKirbyTV ? "nc" : "kirbytv"] = "in_progress";
    }

    // Patching Kirby TV Channel (if applicable)
    static void KirbyTV_Patch()
    {
        task = "Patching Kirby TV Channel";

        PatchWC24Channel("ktv", "Kirby TV Channel", 257, null, "0001000148434d50", new string[] { "KirbyTV_2" }, new string[] { "0000000e" });

        // Finished patching Kirby TV Channel
        patchingProgress_express["kirbytv"] = "done";
        patchingProgress_express["nc"] = "in_progress";
    }


    // Patching Nintendo Channel
    static void NC_Patch(Region nc_reg)
    {
        task = "Patching Nintendo Channel";

        // Properly set Nintendo Channel titleID, appNum, and channel_title
        var channelID = new Dictionary<Region, (string, string, string)>
        {
            {Region.USA, ("0001000148415445", "0000002c", "Nintendo Channel")},
            {Region.PAL, ("0001000148415450", "0000002d", "Nintendo Channel")},
            {Region.Japan, ("000100014841544a", "0000003e", "Minna no Nintendo Channel")}
        };

        var (NC_titleID, appNum, channel_title) = channelID[nc_reg];
        PatchWC24Channel("nc", $"{channel_title}", 1792, nc_reg, NC_titleID, new string[] { $"NC_1_{nc_reg}" }, new string[] { appNum });

        // Finished patching Nintendo Channel
        patchingProgress_express["nc"] = "done";
        patchingProgress_express["forecast"] = "in_progress";
    }

    // Patching Forecast Channel
    static void Forecast_Patch(Region forecast_reg)
    {
        task = "Patching Forecast Channel";

        // Properly set Forecast Channel titleID
        var channelID = new Dictionary<Region, string>
        {
            {Region.USA, "0001000248414645"},
            {Region.PAL, "0001000248414650"},
            {Region.Japan, "000100024841464a"}
        };

        PatchWC24Channel("forecast", $"Forecast Channel", 7, forecast_reg, channelID[forecast_reg], new string[] { $"Forecast_1" }, new string[] { "0000000d" });

        // Finished patching Forecast Channel
        patchingProgress_express["forecast"] = "done";
        patchingProgress_express["evc"] = "in_progress";
    }

    // Patching Everybody Votes Channel and Region Select
    static void EVC_Patch(Region evc_reg)
    {

        //// Patching Everybody Votes Channel
        task = "Patching Everybody Votes Channel";

        // Properly set Everybody Votes Channel titleID
        var channelID = new Dictionary<Region, string>
        {
            {Region.USA, "0001000148414a45"},
            {Region.PAL, "0001000148414a50"},
            {Region.Japan, "0001000148414a4a"}
        };

        PatchWC24Channel("evc", $"Everybody Votes Channel", 512, evc_reg, channelID[evc_reg], new string[] { $"EVC_1_{evc_reg}" }, new string[] { "00000019" });

        //// Patching Region Select
        RegSel_Patch(evc_reg);

        // Finished patching Everybody Votes Channel
        patchingProgress_express["evc"] = "done";
        patchingProgress_express["finishing"] = "in_progress";
    }

    // Patching Region Select
    static void RegSel_Patch(Region regSel_reg)
    {
        task = "Patching Region Select";

        // Properly set Region Select titleID
        var channelID = new Dictionary<Region, string>
        {
            {Region.USA, "0001000848414c45"},
            {Region.PAL, "0001000848414c50"},
            {Region.Japan, "0001000848414c4a"}
        };

        //PatchWC24Channel("RegSel", $"Region Select", 2, regSel_reg, channelID[regSel_reg], $"RegSel_1", "00000009");
        PatchWC24Channel("RegSel", $"Region Select", 2, regSel_reg, channelID[regSel_reg], new string[] { $"RegSel_1" }, new string[] { "00000009" });
    }

    // Patching IOS31
    /*     static void IOS31_Patch(bool isCustomSetup = false)
        {
            task = "Patching IOS31";

            if (!isCustomSetup){ // Patch only the selected IOS31 based on platform
                if (platformType == Platform.Wii) // Patch only the Wii IOS31
                    PatchWC24Channel("IOS31", $"IOS31 [Wii]", 3608, null, "000000010000001f", new string[] {$"IOS31_Wii"}, new string[] {"00000016"});
                else { // Patch only the vWii IOS31
                    PatchWC24Channel("IOS31", $"IOS31 [vWii]", 3608, null, "000000010000001f", new string[] {$"IOS31_vWii_8", $"IOS31_vWii_E"}, new string[] {"00000037", "00000035"});
                }
            } else { // Patch all of them
                PatchWC24Channel("IOS31", $"IOS31 [Wii]", 3608, null, "000000010000001f", new string[] {$"IOS31_Wii"}, new string[] {"00000016"});
                PatchWC24Channel("IOS31", $"IOS31 [vWii]", 3608, null, "000000010000001f", new string[] {$"IOS31_vWii_8", $"IOS31_vWii_E"}, new string[] {"00000037", "00000035"});
            }

            // Finished patching IOS31
            patchingProgress_express["ios31"] = "done";
            patchingProgress_express["finishing"] = "in_progress";
        } */

    // Finish SD Copy
    static void FinishSDCopy()
    {
        // Copying files to SD card
        task = "Copying files to SD card";

        if (sdcard != null)
        {
            // Copying files to SD card
            string copyingFiles = patcherLang == "en"
                ? "Copying files to SD card, which may take a while."
                : $"{localizedText?["FinishSDCopy"]?["copyingFiles"]}";
            AnsiConsole.MarkupLine($" [bold][[*]] {copyingFiles}[/]\n");

            try
            {
                // Copy apps and WAD folder to SD card
                CopyFolder("apps", Path.Join(sdcard, "apps"));
                CopyFolder("WAD", Path.Join(sdcard, "WAD"));
            }
            catch (Exception e)
            {
                // Error message
                string pressAnyKey_error = patcherLang == "en"
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
        while (true)
        {
            PrintHeader();
            // Patching Completed text
            string completed = patcherLang == "en"
                ? "Patching Completed!"
                : $"{localizedText?["Finished"]?["completed"]}";
            AnsiConsole.MarkupLine($"[bold slowblink lime]{completed}[/]\n");

            if (sdcard != null)
            {
                // Every file is in its place text
                string everyFileInPlace = patcherLang == "en"
                    ? "Every file is in its place on your SD Card / USB Drive!"
                    : $"{localizedText?["Finished"]?["withSD/USB"]?["everyFileInPlace"]}";
                AnsiConsole.MarkupLine($"{everyFileInPlace}\n");
            }
            else
            {
                // Please connect text
                string connectDrive = patcherLang == "en"
                    ? "Please connect your Wii SD Card / USB Drive and copy the [u]WAD[/] and [u]apps[/] folders to the root (main folder) of your SD Card / USB Drive."
                    : $"{localizedText?["Finished"]?["withoutSD/USB"]?["connectDrive"]}";
                AnsiConsole.MarkupLine($"{connectDrive}\n");

                // Open the folder text
                string canFindFolders = patcherLang == "en"
                    ? "You can find these folders in the [u]{curDir}[/] folder of your computer."
                    : $"{localizedText?["Finished"]?["canFindFolders"]}";
                canFindFolders = canFindFolders.Replace("{curDir}", curDir);
                AnsiConsole.MarkupLine($"{canFindFolders}\n");
            }

            // Please proceed text
            string pleaseProceed = patcherLang == "en"
                ? "Please proceed with the tutorial that you can find on [bold lime link]https://wii.guide/wiilink[/]"
                : $"{localizedText?["Finished"]?["pleaseProceed"]}";
            AnsiConsole.MarkupLine($"{pleaseProceed}\n");

            // What would you like to do now text
            string whatWouldYouLikeToDo = patcherLang == "en"
                ? "What would you like to do now?"
                : $"{localizedText?["Finished"]?["whatWouldYouLikeToDo"]}";
            AnsiConsole.MarkupLine($"{whatWouldYouLikeToDo}\n");

            // User choices
            string openFolder = patcherLang == "en"
                ? sdcard != null ? "Open the SD Card / USB Drive folder" : "Open the folder"
                : sdcard != null ? $"{localizedText?["Finished"]?["withSD/USB"]?["openFolder"]}" : $"{localizedText?["Finished"]?["withoutSD/USB"]?["openFolder"]}";
            string goBackToMainMenu = patcherLang == "en"
                ? "Go back to the main menu"
                : $"{localizedText?["Finished"]?["goBackToMainMenu"]}";
            string exitProgram = patcherLang == "en"
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
                        Process.Start(@"explorer.exe", sdcard ?? curDir);
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                        Process.Start("xdg-open", sdcard ?? curDir);
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                        Process.Start("open", sdcard ?? curDir);
                    break;
                case 2:
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
            string header = patcherLang == "en"
                ? "Manually Select SD Card / USB Drive Path"
                : $"{localizedText?["SDCardSelect"]?["header"]}";
            AnsiConsole.MarkupLine($"[bold lime]{header}[/]\n");

            string inputMessage = "";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                inputMessage = patcherLang == "en"
                    ? "Please enter the drive letter of your SD card/USB drive (e.g. E)"
                    : $"{localizedText?["SDCardSelect"]?["inputMessage"]?["windows"]}";
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                inputMessage = patcherLang == "en"
                    ? "Please enter the mount name of your SD card/USB drive (e.g. /media/username/Wii)"
                    : $"{localizedText?["SDCardSelect"]?["inputMessage"]?["linux"]}";
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                inputMessage = patcherLang == "en"
                    ? "Please enter the volume name of your SD card/USB drive (e.g. /Volumes/Wii)"
                    : $"{localizedText?["SDCardSelect"]?["inputMessage"]?["osx"]}";
            AnsiConsole.MarkupLine($"{inputMessage}");

            // Type EXIT to go back to previous menu
            string exitMessage = patcherLang == "en"
                ? "(Type [bold]EXIT[/] to go back to the previous menu)"
                : $"{localizedText?["SDCardSelect"]?["exitMessage"]}";
            AnsiConsole.MarkupLine($"{exitMessage}\n");

            // New SD card/USB drive text
            string newSDCardMessage = "";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                newSDCardMessage = patcherLang == "en"
                    ? "New SD card/USB drive:"
                    : $"{localizedText?["SDCardSelect"]?["newSDCardMessage"]?["windows"]}";
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                newSDCardMessage = patcherLang == "en"
                    ? "New SD card/USB drive volume:"
                    : $"{localizedText?["SDCardSelect"]?["newSDCardMessage"]?["linux"]}";
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                newSDCardMessage = patcherLang == "en"
                    ? "New SD card/USB drive volume:"
                    : $"{localizedText?["SDCardSelect"]?["newSDCardMessage"]?["osx"]}";
            AnsiConsole.MarkupLine($"{newSDCardMessage} ");

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
                    string driveLetterError = patcherLang == "en"
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
            if (Path.GetPathRoot(sdcard_new) == Path.GetPathRoot(Path.GetPathRoot(Environment.SystemDirectory)))
            {
                // You cannot select your boot drive text
                string bootDriveError = patcherLang == "en"
                    ? "You cannot select your boot drive!"
                    : $"{localizedText?["SDCardSelect"]?["bootDriveError"]}";
                AnsiConsole.MarkupLine($"[bold red]{bootDriveError}[/]");
                Thread.Sleep(2000);
                continue;
            }

            // Check if new SD card path is the same as the old one
            if (sdcard_new == sdcard)
            {
                // You have already selected this SD card/USB drive text
                string alreadySelectedError = patcherLang == "en"
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
                    driveNotExistError = patcherLang == "en"
                        ? "Drive does not exist!"
                        : $"{localizedText?["SDCardSelect"]?["driveNotExistError"]?["windows"]}";
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    driveNotExistError = patcherLang == "en"
                        ? "Volume does not exist!"
                        : $"{localizedText?["SDCardSelect"]?["driveNotExistError"]?["linux"]}";
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    driveNotExistError = patcherLang == "en"
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
                string noAppsFolderError_message = patcherLang == "en"
                    ? "Drive detected, but no /apps folder found!"
                    : $"{localizedText?["SDCardSelect"]?["noAppsFolderError"]?["message"]}";
                string noAppsFolderError_instructions = patcherLang == "en"
                    ? "Please create it first and then try again."
                    : $"{localizedText?["SDCardSelect"]?["noAppsFolderError"]?["instructions"]}";
                AnsiConsole.MarkupLine($"[bold]{noAppsFolderError_message}[/]");
                AnsiConsole.MarkupLine($"{noAppsFolderError_instructions}\n");

                // Press any key to continue text
                string pressAnyKey = patcherLang == "en"
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

            /*             var languages = new List<KeyValuePair<string, string>>
                        {
                            new("en", "English"),
                            new("esUS", "Español (USA)"),
                            new("esEU", "Español (Europeo)"),
                            new("fr", "Français"),
                            new("it", "Italiano"),
                            new("de", "Deutsch"),
                            new("ca", "Català"),
                            new("nl", "Nederlands")
                        }; */

            // languages but it has English, Español (USA), Japanese, and Catalan
            var languages = new List<KeyValuePair<string, string>>
            {
                new("en", "English")
            };

            // Choose a Language text
            string chooseALanguage = patcherLang == "en"
                ? "Choose a Language"
                : $"{localizedText?["LanguageSettings"]?["chooseALanguage"]}";
            AnsiConsole.MarkupLine($"[bold lime]{chooseALanguage}[/]\n");

            // More languages coming soon text
            AnsiConsole.MarkupLine($"[bold lime]More languages coming soon![/]\n");

            // Display languages
            StringBuilder choices = new();
            for (int i = 0; i < languages.Count; i++)
            {
                AnsiConsole.MarkupLine($"{i + 1}. {languages[i].Value}");
                choices.Append(i + 1);
            }
            choices.Append(languages.Count + 1); // So user can go back to Settings Menu

            // Go back to Settings Menu text
            string goBack = patcherLang == "en"
                ? "Go back to Settings Menu"
                : $"{localizedText?["LanguageSettings"]?["goBack"]}";
            AnsiConsole.MarkupLine($"\n{languages.Count + 1}. {goBack}\n");

            int choice = UserChoose(choices.ToString());

            // Map choice to language code
            if (choice <= languages.Count)
            {
                var selectedLanguage = languages.ElementAt(choice - 1);
                var langCode = selectedLanguage.Key;

                // Set programLang to chosen language code
                patcherLang = langCode;

                // Since English is hardcoded, there's no language pack for it
                if (patcherLang == "en")
                {
                    SettingsMenu();
                    break;
                }

                // Download language pack
                DownloadLanguagePack(langCode);

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

        AnsiConsole.MarkupLine($"\n[bold lime]Checking for Language Pack updates ({languageCode})[/]");

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
            using HttpClient client = new();
            HttpRequestMessage request = new(HttpMethod.Head, languageFileUrl);
            HttpResponseMessage response = client.Send(request);
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
            AnsiConsole.MarkupLine($"[bold lime]Downloading Language Pack ({languageCode})[/]");
            DownloadFile(languageFileUrl, languageFilePath, $"Language Pack ({languageCode})");
        }
        else
        {
            AnsiConsole.MarkupLine($"[bold lime]Language Pack ({languageCode}) is up to date[/]");
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
            string settings = patcherLang == "en"
                ? "Settings"
                : $"{localizedText?["SettingsMenu"]?["settings"]}";
            AnsiConsole.MarkupLine($"[bold lime]{settings}[/]\n");

            if (!inCompatabilityMode)
            {
                // User choices
                string changeLanguage = patcherLang == "en"
                    ? "Change Language"
                    : $"{localizedText?["SettingsMenu"]?["changeLanguage"]}";
                string credits = patcherLang == "en"
                    ? "Credits"
                    : $"{localizedText?["SettingsMenu"]?["credits"]}";
                string goBack = patcherLang == "en"
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
            string[] foldersToDelete = { "Patches", "Unpack", "Unpack_Patched" };
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
            string welcomeMessage = patcherLang == "en"
                ? "Welcome to the WiiLink Patcher!"
                : $"{localizedText?["MainMenu"]?["welcomeMessage"]}";
            AnsiConsole.MarkupLine($"[bold lime]{welcomeMessage}[/]\n");

            // Express Install text
            string startExpressSetup = patcherLang == "en"
                ? "1. Start Express Install Setup [bold lime](Recommended)[/]"
                : $"{localizedText?["MainMenu"]?["startExpressSetup"]}";
            AnsiConsole.MarkupLine(startExpressSetup);

            // Custom Install text
            string startCustomSetup = patcherLang == "en"
                ? "2. Start Custom Install Setup [bold](Advanced)[/]"
                : $"{localizedText?["MainMenu"]?["startCustomSetup"]}";
            AnsiConsole.MarkupLine(startCustomSetup);

            // Settings text
            string settings = patcherLang == "en"
                ? "3. Settings\n"
                : $"{localizedText?["MainMenu"]?["settings"]}\n";
            AnsiConsole.MarkupLine(settings);

            // Exit Patcher text
            string exitPatcher = patcherLang == "en"
                ? "4. Exit Patcher\n"
                : $"{localizedText?["MainMenu"]?["exitPatcher"]}\n";
            AnsiConsole.MarkupLine(exitPatcher);

            // Detect SD Card / USB Drive text
            string SDDetectedOrNot = sdcard != null
                ? $"[bold lime]{(patcherLang == "en" ? "Detected SD Card / USB Drive:" : localizedText?["MainMenu"]?["sdCardDetected"])}[/] {sdcard}"
                : $"[bold red]{(patcherLang == "en" ? "Could not detect your SD Card / USB Drive!" : localizedText?["MainMenu"]?["noSDCard"])}[/]";
            AnsiConsole.MarkupLine(SDDetectedOrNot);

            // Automatically detect SD Card / USB Drive text
            string automaticDetection = patcherLang == "en"
                ? "R. Automatically detect SD Card / USB Drive"
                : $"{localizedText?["MainMenu"]?["automaticDetection"]}";
            AnsiConsole.MarkupLine(automaticDetection);

            // Manually select SD Card / USB Drive text
            string manualDetection = patcherLang == "en"
                ? "M. Manually select SD Card / USB Drive path\n"
                : $"{localizedText?["MainMenu"]?["manualDetection"]}\n";
            AnsiConsole.MarkupLine(manualDetection);

            // User chooses an option
            int choice = UserChoose("1234RrMm");
            switch (choice)
            {
                case 1: // Start Express Install
                    JapaneseChannel_LangSetup();
                    break;
                case 2: // Start Custom Install
                    CustomInstall_JapaneseChannel_Setup();
                    break;
                case 3: // Settings                
                    SettingsMenu();
                    break;
                case 4: // Clear console and Exit app
                    Console.Clear();
                    ExitApp();
                    break;
                case 5:
                case 6: // Automatically detect SD Card path (R/r)
                    sdcard = DetectSDCard();
                    break;
                case 7:
                case 8: // Manually select SD Card path (M/m)
                    SDCardSelect();
                    break;
                default:
                    break;
            }
        }
    }

    // Check if server is up
    static async Task<(System.Net.HttpStatusCode, string)> CheckServerAsync(string serverURL, int maxRetries = 3, int retryDelayMs = 1000)
    {
        // Use the following URL to check if the server is up
        string url = $"{serverURL}/wiinoma/WiinoMa_1_English.delta";
        var httpClient = new HttpClient();

        PrintHeader();
        //Console.WriteLine($"Checking server status...");
        // Check server status text
        string checkingServerStatus = patcherLang == "en"
            ? "Checking server status..."
            : $"{localizedText?["CheckServerStatus"]?["checking"]}";
        AnsiConsole.MarkupLine($"{checkingServerStatus}\n");

        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                using (var response = await httpClient.GetAsync(url))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        //AnsiConsole.MarkupLine("[bold lime]Successfully connected to server![/]");
                        string success = patcherLang == "en"
                            ? "Successfully connected to server!"
                            : $"{localizedText?["CheckServerStatus"]?["success"]}";
                        AnsiConsole.MarkupLine($"[bold lime]{success}[/]\n");
                        await Task.Delay(1000); // Wait for 1 second
                        return (System.Net.HttpStatusCode.OK, "Successfully connected to server!");
                    }
                    else
                    {
                        return (response.StatusCode == default(HttpStatusCode) ? System.Net.HttpStatusCode.InternalServerError : response.StatusCode, response.ReasonPhrase ?? "Unknown error");
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                if (i == maxRetries - 1)
                {
                    //AnsiConsole.MarkupLine("[bold red]Connection to server failed![/]\n");
                    string failed = patcherLang == "en"
                        ? "Connection to server failed!"
                        : $"{localizedText?["CheckServerStatus"]?["failed"]}";
                    AnsiConsole.MarkupLine($"[bold red]{failed}[/]\n");
                    return (ex.StatusCode ?? System.Net.HttpStatusCode.InternalServerError, ex.Message);
                }
            }

            if (i < maxRetries - 1)
            {
                //AnsiConsole.MarkupLine($"Retrying in [bold]{retryDelayMs / 1000}[/] seconds...\n");
                string retrying = patcherLang == "en"
                    ? $"Retrying in [bold]{retryDelayMs / 1000}[/] seconds..."
                    : $"{localizedText?["CheckServerStatus"]?["retrying"]}"
                        .Replace("{retryDelayMs / 1000}", $"[bold]{retryDelayMs / 1000}[/]");
                AnsiConsole.MarkupLine($"{retrying}\n");
                await Task.Delay(retryDelayMs);
            }
        }

        return (System.Net.HttpStatusCode.ServiceUnavailable, "Connection to server failed!");
    }

    static void ConnectionFailed(System.Net.HttpStatusCode statusCode, string msg)
    {
        PrintHeader();

        // Connection to server failed text
        string connectionFailed = patcherLang == "en"
            ? "Connection to server failed!"
            : $"{localizedText?["ServerDown"]?["connectionFailed"]}";
        AnsiConsole.MarkupLine($"[bold blink red]{connectionFailed}[/]\n");

        // Check internet connection text
        string checkInternet = patcherLang == "en"
            ? "Connection to the server failed. Please check your internet connection and try again."
            : $"{localizedText?["ServerDown"]?["checkInternet"]}";
        string serverOrInternet = patcherLang == "en"
            ? "It seems that either the server is down or your internet connection is not working."
            : $"{localizedText?["ServerDown"]?["serverOrInternet"]}";
        string reportIssue = patcherLang == "en"
            ? "If you are sure that your internet connection is working, please join our [link=https://discord.gg/WiiLink bold lime]Discord Server[/] and report this issue."
            : $"{localizedText?["ServerDown"]?["reportIssue"]}";
        AnsiConsole.MarkupLine($"{checkInternet}\n");
        AnsiConsole.MarkupLine($"{serverOrInternet}\n");
        AnsiConsole.MarkupLine($"{reportIssue}\n");

        // Status code text
        string statusCodeText = patcherLang == "en"
            ? "Status code:"
            : $"{localizedText?["ServerDown"]?["statusCode"]}";
        string messageText = patcherLang == "en"
            ? "Message:"
            : $"{localizedText?["ServerDown"]?["message"]}";
        string exitMessage = patcherLang == "en"
            ? "Press any key to exit..."
            : $"{localizedText?["ServerDown"]?["exitMessage"]}";
        AnsiConsole.MarkupLine($"{statusCodeText} {statusCode}");
        AnsiConsole.MarkupLine($"{messageText} {msg}\n");

        AnsiConsole.MarkupLine($"[bold yellow]{exitMessage}[/]");

        Console.ReadKey();
        ExitApp();
    }

    public static async Task CheckForUpdates(string currentVersion)
    {
        PrintHeader();

        // Check for updates text
        string checkingForUpdates = patcherLang == "en"
            ? "Checking for updates..."
            : $"{localizedText?["CheckForUpdates"]?["checking"]}";
        AnsiConsole.MarkupLine($"{checkingForUpdates}\n");

        // URL of the text file containing the latest version number
        string updateUrl = "https://raw.githubusercontent.com/PablosCorner/wiilink-patcher-version/main/version.txt";

        // Download the latest version number from the server
        HttpClient client = new();
        string updateInfo = "";
        try
        {
            updateInfo = await client.GetStringAsync(updateUrl);
        }
        catch (HttpRequestException ex)
        {
            // Error retrieving update information text
            string errorRetrievingUpdateInfo = patcherLang == "en"
                ? $"Error retrieving update information: [bold red]{ex.Message}[/]"
                : $"{localizedText?["CheckForUpdates"]?["errorChecking"]}"
                    .Replace("{ex.Message}", ex.Message);
            string skippingUpdateCheck = patcherLang == "en"
                ? "Skipping update check..."
                : $"{localizedText?["CheckForUpdates"]?["skippingCheck"]}";

            AnsiConsole.MarkupLine($"{errorRetrievingUpdateInfo}\n");

            AnsiConsole.MarkupLine($"{skippingUpdateCheck}\n");
            Thread.Sleep(5000);
            return;
        }

        // Get the latest version number from the text file
        string latestVersion = updateInfo.Split('\n')[0].Trim();

        // Map operating system names to executable names
        var executables = new Dictionary<string, string>
        {
            { "Windows", $"WiiLink_Patcher_Windows_{latestVersion}.exe" },
            { "Linux", RuntimeInformation.ProcessArchitecture == Architecture.Arm64 ? $"WiiLink_Patcher_Linux-arm64_{latestVersion}" : $"WiiLink_Patcher_Linux-x64_{latestVersion}" },
            { "OSX", $"WiiLink_Patcher_macOS_{latestVersion}" }
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
                string updateAvailable = patcherLang == "en"
                    ? "A new version is available! Would you like to download it now?"
                    : $"{localizedText?["CheckForUpdates"]?["updateAvailable"]}";
                string currentVersionText = patcherLang == "en"
                    ? "Current version:"
                    : $"{localizedText?["CheckForUpdates"]?["currentVersion"]}";
                string latestVersionText = patcherLang == "en"
                    ? "Latest version:"
                    : $"{localizedText?["CheckForUpdates"]?["latestVersion"]}";

                AnsiConsole.MarkupLine($"{updateAvailable}\n");

                AnsiConsole.MarkupLine($"{currentVersionText} {currentVersion}");
                AnsiConsole.MarkupLine($"{latestVersionText} [bold lime]{latestVersion}[/]\n");

                // Show changelog via Github link
                string changelogLink = patcherLang == "en"
                    ? "Changelog:"
                    : $"{localizedText?["CheckForUpdates"]?["changelogLink"]}";
                AnsiConsole.MarkupLine($"[bold]{changelogLink}[/] [link lime]https://github.com/WiiLink24/WiiLink24-Patcher/releases/tag/{latestVersion}[/]\n");

                // Yes/No text
                string yes = patcherLang == "en"
                    ? "Yes"
                    : $"{localizedText?["CheckForUpdates"]?["yes"]}";
                string no = patcherLang == "en"
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
                        string? osName = System.Runtime.InteropServices.RuntimeInformation
                            .IsOSPlatform(OSPlatform.Windows) ? "Windows" :
                            System.Runtime.InteropServices.RuntimeInformation
                            .IsOSPlatform(OSPlatform.OSX) ? "macOS" :
                            System.Runtime.InteropServices.RuntimeInformation
                            .IsOSPlatform(OSPlatform.Linux) ? "Linux" : "Unknown";

                        // Log message
                        string downloadingFrom = patcherLang == "en"
                            ? $"Downloading [lime]{latestVersion}[/] for [lime]{osName}[/]..."
                            : $"{localizedText?["CheckForUpdates"]?["downloadingFrom"]}"
                                .Replace("{latestVersion}", latestVersion)
                                .Replace("{osName}", osName);
                        AnsiConsole.MarkupLine($"\n[bold]{downloadingFrom}[/]");
                        Console.Out.Flush();

                        // Download the latest version and save it to a file
                        HttpResponseMessage response;
                        response = await client.GetAsync(downloadUrl);
                        if (!response.IsSuccessStatusCode) // Ideally shouldn't happen if version.txt is set up correctly
                        {
                            // Download failed text
                            string downloadFailed = patcherLang == "en"
                                ? $"An error occurred while downloading the latest version:[/] {response.StatusCode}"
                                : $"{localizedText?["CheckForUpdates"]?["downloadFailed"]}"
                                    .Replace("{response.StatusCode}", response.StatusCode.ToString());
                            string pressAnyKey = patcherLang == "en"
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
                            string downloadComplete = patcherLang == "en"
                                ? $"Download complete![/] Exiting in [bold lime]{i}[/] seconds..."
                                : $"{localizedText?["CheckForUpdates"]?["downloadComplete"]}"
                                    .Replace("{i}", i.ToString());
                            AnsiConsole.MarkupLine($"\n[bold lime]{downloadComplete}[/]");
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
            string onLatestVersion = patcherLang == "en"
                ? "You are running the latest version!"
                : $"{localizedText?["CheckForUpdates"]?["onLatestVersion"]}";
            AnsiConsole.MarkupLine($"[bold lime]{onLatestVersion}[/]");
            Thread.Sleep(1000);
        }
    }

    static void ErrorScreen(int exitCode, string msg = "")
    {
        PrintHeader();

        // An error has occurred text
        string errorOccurred = patcherLang == "en"
            ? "An error has occurred."
            : $"{localizedText?["ErrorScreen"]?["title"]}";
        AnsiConsole.MarkupLine($"[bold red]{errorOccurred}[/]\n");

        // Error details text
        string errorDetails = patcherLang == "en"
            ? "ERROR DETAILS:"
            : $"{localizedText?["ErrorScreen"]?["details"]}";
        string taskText = patcherLang == "en"
            ? "Task: "
            : $"{localizedText?["ErrorScreen"]?["task"]}";
        string commandText = patcherLang == "en"
            ? "Command:"
            : $"{localizedText?["ErrorScreen"]?["command"]}";
        string messageText = patcherLang == "en"
            ? "Message:"
            : $"{localizedText?["ErrorScreen"]?["message"]}";
        string exitCodeText = patcherLang == "en"
            ? "Exit code:"
            : $"{localizedText?["ErrorScreen"]?["exitCode"]}";

        AnsiConsole.MarkupLine($"{errorDetails}\n");
        AnsiConsole.MarkupLine($" * {taskText} {task}");
        AnsiConsole.MarkupLine(msg == null ? $" * {commandText} {curCmd}" : $" * {messageText} {msg}");
        AnsiConsole.MarkupLine($" * {exitCodeText} {exitCode}\n");

        // Please open an issue text
        string openAnIssue = patcherLang == "en"
            ? "Please open an issue on our GitHub page ([link bold lime]https://github.com/WiiLink24/WiiLink24-Patcher/issues[/]) and describe the\nerror you encountered. Please include the error details above in your issue."
            : $"{localizedText?["ErrorScreen"]?["githubIssue"]}";
        AnsiConsole.MarkupLine($"{openAnIssue}\n");

        // Press any key to go back to the main menu text
        string pressAnyKey = patcherLang == "en"
            ? "Press any key to go back to the main menu..."
            : $"{localizedText?["ErrorScreen"]?["pressAnyKey"]}";
        AnsiConsole.MarkupLine($"[bold]{pressAnyKey}[/]");
        Console.ReadKey();

        // Go back to the main menu
        MainMenu();
    }

    private static void CopyFolder(string sourcePath, string destinationPath)
    {
        DirectoryInfo source = new DirectoryInfo(sourcePath);
        DirectoryInfo destination = new DirectoryInfo(destinationPath);

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

        AnsiConsole.MarkupLine("Otherwise, for the best visual experience, set your console font to [lime]Consolas[/] with a size of [lime]16[/].");
        AnsiConsole.MarkupLine("Also, set your console window and buffer size to [lime]120x30[/].\n");

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

    static async System.Threading.Tasks.Task Main(string[] args)
    {
        // Set console encoding to UTF-8
        Console.OutputEncoding = Encoding.UTF8;

        // Cache current console size to console_width and console_height
        console_width = Console.WindowWidth;
        console_height = Console.WindowHeight;

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Console.Title = $"WiiLink Patcher {version}";
            if (DEBUG_MODE) Console.Title += $" [DEBUG MODE]";
            if (version.Contains("T")) Console.Title += $" (Test Build)";
        }

        // Set console window size to 120x30 on macOS and Linux and on Windows, check for Windows version
        switch (RuntimeInformation.OSDescription)
        {
            case string os when os.Contains("macOS"):
                Console.Write("\u001b[8;30;120t");
                break;
            case string os when os.Contains("Linux"):
                if (console_width < 100 || console_height < 25)
                    Console.Write("\u001b[8;30;120t");
                break;
            case string os when os.Contains("Windows"):
                if (Environment.OSVersion.Version.Major < 10)
                    WinCompatWarning();
                break;
        }

        // Check if the server is up
        var result = await CheckServerAsync(wiiLinkPatcherUrl);
        if (result != (System.Net.HttpStatusCode.OK, "Successfully connected to server!"))
            ConnectionFailed(result.Item1, result.Item2);

        // Check latest version if not on a test build
        if (!version.Contains("T"))
            await CheckForUpdates(version);

        // Go to the main menu
        MainMenu();
    }
}