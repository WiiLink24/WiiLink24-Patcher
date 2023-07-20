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
    static readonly string version = "v1.2.0";
    static readonly string copyrightYear = DateTime.Now.Year.ToString();
    static readonly string buildDate = "July 19th, 2023";
    static readonly string buildTime = "7:40 PM";
    static string? sdcard = DetectSDCard();
    static readonly string wiiLinkPatcherUrl = "https://patcher.wiilink24.com";
    ////////////////////

    //// Setup Info ////
    // Express Install variables
    static public string reg = "";
    static public string lang = "";
    static public string demae_version = "";
    static public bool installWC24 = false;
    static public string nc_reg = "";
    static public string forecast_reg = "";
    static public string platformType = "";
    static public bool installKirbyTV = false;
    static Dictionary<string, string> patchingProgress_express = new Dictionary<string, string>();

    // Custom Install variables
    static List<string> coreChannels_selection = new List<string>();
    static List<string> wiiConnect24Channels_selection = new List<string>();
    static string spdVersion_custom = "";
    static bool inCompatabilityMode = false;
    static Dictionary<string, string> patchingProgress_custom = new Dictionary<string, string>();

    // Misc. variables
    static public string task = "";
    static public string curCmd = "";
    static public int exitCode = -1;
    static readonly string curDir = Directory.GetCurrentDirectory();
    static readonly string tempDir = Path.Join(Path.GetTempPath(), "WiiLink_Patcher");
    static bool DEBUG_MODE = false;
    static public string localizeLang = "en"; // English by default
    static JObject? localizedText = null;

    // Get current console window size
    static int console_width = 0;
    static int console_height = 0;
    ////////////////////

    static void PrintHeader()
    {
        Console.Clear();

        string headerText = $"WiiLink Patcher {version} - (c) {copyrightYear} WiiLink";
        if (localizeLang != "en")
        {
            headerText = $"{localizedText?["Header"]}"
                .Replace("{version}", version)
                .Replace("{copyrightYear}", copyrightYear);
        }

        AnsiConsole.MarkupLine(headerText);

        string borderChar = "=";
        string borderLine = new string(borderChar[0], Console.WindowWidth);

        AnsiConsole.MarkupLine($"{borderLine}\n");
    }

    static void PrintNotice()
    {
        string title = $"[bold lime] Notice [/]";
        string text = inCompatabilityMode ? "[bold]If you have any issues with the patcher or services offered by WiiLink, please report them on our [bold]Discord Server[/]:[/]\n[bold link]https://discord.gg/WiiLink[/]" : $"[bold]If you have any issues with the patcher or services offered by WiiLink, please report them on our [lime link=https://discord.gg/WiiLink]Discord Server[/]![/]";

        var panel = new Panel(text)
        {
            Header = new PanelHeader(title, Justify.Center),
            Border = inCompatabilityMode ? BoxBorder.Ascii : BoxBorder.Heavy,
            BorderStyle = new Style(Color.Lime),
            Expand = true,
        };

        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();

    }

    static string? DetectSDCard()
    {
        // Check for drive with apps folder
        foreach (DriveInfo drive in DriveInfo.GetDrives())
        {
            if (drive.DriveType == DriveType.Removable && Directory.Exists(Path.Join(drive.RootDirectory.FullName, "apps")))
                return drive.RootDirectory.FullName;
        }
        return null;
    }

    // User choice
    static int UserChoose(string choices)
    {
        ConsoleKeyInfo keyPressed;
        do
        {
            Console.Write($"Choose: ");
            keyPressed = Console.ReadKey(intercept: true);

            switch (keyPressed.Key)
            {
                case ConsoleKey.Escape:
                    return -1;
                case ConsoleKey.Enter:
                    Console.WriteLine();
                    return 0;
                case ConsoleKey.Backspace:
                    if (Console.CursorLeft > 0)
                        Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                    else
                    {
                        Console.SetCursorPosition(0, Console.CursorTop - 1);
                        Console.Write(new string(' ', Console.WindowWidth));
                        Console.SetCursorPosition(0, Console.CursorTop - 1);
                    }
                    return -2;
                default:
                    if (choices.Contains(keyPressed.KeyChar))
                    {
                        Console.WriteLine();
                        return choices.IndexOf(keyPressed.KeyChar) + 1;
                    }
                    else
                    {
                        if (Console.CursorLeft > 8)
                            Console.SetCursorPosition(Console.CursorLeft - 8, Console.CursorTop);
                        else
                            Console.SetCursorPosition(0, Console.CursorTop);
                    }
                    break;
            }
        } while (true);
    }

    // Credits function
    static void CreditsScreen()
    {
        PrintHeader();

        // Build info
        AnsiConsole.MarkupLine($"This build was compiled on [bold lime]{buildDate}[/] at [bold lime]{buildTime}[/].\n");

        // Credits table
        var creditTable = new Table().Border(inCompatabilityMode ? TableBorder.None : TableBorder.DoubleEdge);
        creditTable.AddColumn(new TableColumn($"[bold lime]Credits[/]").Centered());

        // Credits grid
        var creditGrid = new Grid().AddColumn().AddColumn();

        creditGrid.AddRow(new Text("Sketch", new Style(Color.Lime, null, Decoration.Bold)).RightJustified(), new Text("WiiLink Founder", new Style(null, null, Decoration.Bold)));
        creditGrid.AddRow(new Text("PablosCorner", new Style(Color.Lime, null, Decoration.Bold)).RightJustified(), new Text("WiiLink Patcher Developer", new Style(null, null, Decoration.Bold)));
        creditGrid.AddRow(new Text("leathl and WiiDatabase", new Style(Color.Lime, null, Decoration.Bold)).RightJustified(), new Text("libWiiSharp developers", new Style(null, null, Decoration.Bold)));
        creditGrid.AddRow(new Text("SnowflakePowered", new Style(Color.Lime, null, Decoration.Bold)).RightJustified(), new Text("VCDiff", new Style(null, null, Decoration.Bold)));

        // Add the grid to the table
        creditTable.AddRow(creditGrid).Centered();
        AnsiConsole.Write(creditTable);

        // Special thanks grid
        AnsiConsole.MarkupLine("\n[bold lime]Special thanks to:[/]\n");

        var specialThanksGrid = new Grid().AddColumn().AddColumn();

        specialThanksGrid.AddRow("  ● [bold]TheShadowEevee[/]", "- Pointing me in the right direction with implementing libWiiSharp!");
        specialThanksGrid.AddRow("  ● [bold]Our Testers[/]", "- For testing the patcher and reporting bugs/anomalies!");
        specialThanksGrid.AddRow("  ● [bold]You![/]", "- For your continued support of WiiLink!");

        AnsiConsole.Write(specialThanksGrid);

        AnsiConsole.MarkupLine("\n[bold lime]WiiLink website:[/]   [link]https://wiilink24.com[/]");
        AnsiConsole.MarkupLine("[bold lime]Github repository:[/] [link]https://github.com/WiiLink24/WiiLink24-Patcher[/]\n");

        AnsiConsole.MarkupLine("[bold]Press any key to go back to settings...[/]");
        Console.ReadKey();
    }

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
                using (var client = new HttpClient())
                {
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
            }
            catch (Exception e)
            {
                AnsiConsole.MarkupLine($"[bold red]ERROR:[/] {e.Message}");
                AnsiConsole.MarkupLine("Press any key to try again...");
                Console.ReadKey(true);
            }
        }
    }

    static string DownloadNUS(string titleID, string outputDir, string? appVer = null, bool isWC24 = false)
    {
        string task = $"Downloading {titleID}";

        // Create a new NusClient instance to handle the download.
        NusClient nus = new NusClient();

        // Create a list of store types to download.
        List<StoreType> store = new List<StoreType> { isWC24 ? StoreType.DecryptedContent : StoreType.WAD };

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
        WAD wad = new WAD();

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
        WAD wad = new WAD();

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
            using (var decoder = new VCDiff.Decoders.VcDecoder(original, patch, output))
            {
                // Decode the patch and write the result to the output file.
                decoder.Decode(out _);
            }
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

    static void DownloadSPD(string platformType = "")
    {
        string spdUrl = "";
        string spdDestinationPath = "";

        // Create WAD folder in current directory if it doesn't exist
        if (!Directory.Exists(Path.Join("WAD")))
            Directory.CreateDirectory(Path.Join("WAD"));

        switch (platformType)
        {
            case "Wii":
                spdUrl = $"{wiiLinkPatcherUrl}/spd/SPD_Wii.wad";
                spdDestinationPath = Path.Join("WAD", "WiiLink SPD (Wii).wad");
                break;
            case "vWii":
                spdUrl = $"{wiiLinkPatcherUrl}/spd/SPD_vWii.wad";
                spdDestinationPath = Path.Join("WAD", "WiiLink SPD (vWii).wad");
                break;
        }

        DownloadFile(spdUrl, spdDestinationPath, "SPD");
    }


    // Patches the Japanese-exclusive channels
    static void PatchCoreChannel(string channelName, string channelTitle, string titleID, List<KeyValuePair<string, string>> patchFilesDict, string? appVer = null, string? lang = null)
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
        if (reg == "EN" || channelName == "Dominos")
            ApplyPatch(File.OpenRead(Path.Join(titleFolder, $"{patchFilesDict[0].Value}.app")), File.OpenRead(Path.Join(patchFolder, $"{patchFilesDict[0].Key}.delta")), File.OpenWrite(Path.Join(tempFolder, $"{patchFilesDict[0].Value}.app")));

        // Second delta patch
        ApplyPatch(File.OpenRead(Path.Join(titleFolder, $"{patchFilesDict[1].Value}.app")), File.OpenRead(Path.Join(patchFolder, $"{patchFilesDict[1].Key}.delta")), File.OpenWrite(Path.Join(tempFolder, $"{patchFilesDict[1].Value}.app")));

        // Third delta patch
        if (reg == "EN" || channelName == "Dominos" || channelName == "WiinoMa")
            ApplyPatch(File.OpenRead(Path.Join(titleFolder, $"{patchFilesDict[2].Value}.app")), File.OpenRead(Path.Join(patchFolder, $"{patchFilesDict[2].Key}.delta")), File.OpenWrite(Path.Join(tempFolder, $"{patchFilesDict[2].Value}.app")));

        // Copy patched files to unpack folder
        task = $"Copying patched files for {channelTitle}";
        CopyFolder(tempFolder, titleFolder);

        // Repack the title with the patched files
        task = $"Repacking the title for {channelTitle}";
        PackWAD(titleFolder, outputChannel);

        // Delete unpack and unpack-patched folders
        Directory.Delete(titleFolder, true);
        Directory.Delete(tempFolder, true);
    }

    // This function patches the WiiConnect24 channels
    static void PatchWC24Channel(string channelName, string channelTitle, int channelVersion, string? channelRegion, string titleID, string patchFile, string appFile)
    {
        // Define the necessary paths and filenames
        string titleFolder = Path.Join(tempDir, "Unpack");
        string tempFolder = Path.Join(tempDir, "Unpack_Patched");
        string patchFolder = Path.Join(tempDir, "Patches", channelName);

        // Name the output WAD file
        string outputWad;
        if (string.IsNullOrEmpty(channelRegion))
            outputWad = Path.Join("WAD", $"{channelTitle} (WiiLink).wad");
        else
            outputWad = Path.Join("WAD", $"{channelTitle} [{channelRegion}] (WiiLink).wad");

        // Create unpack and unpack-patched folders
        Directory.CreateDirectory(titleFolder);
        Directory.CreateDirectory(tempFolder);

        // Define the URLs for the necessary files
        var discordURLs = new Dictionary<string, Dictionary<string, string>>{
            // Nintendo Channel Certs and Tiks
            {"000100014841544a", new Dictionary<string, string> {
                {"cert", "https://cdn.discordapp.com/attachments/253286648291393536/1123709388641800263/000100014841544a.cert"},
                {"tmd", ""},
                {"tik", "https://cdn.discordapp.com/attachments/253286648291393536/1123709425149038612/000100014841544a.tik"}
            }},
            {"0001000148415445", new Dictionary<string, string> {
                {"cert", "https://cdn.discordapp.com/attachments/253286648291393536/1123709388998324235/0001000148415445.cert"},
                {"tmd", ""},
                {"tik", "https://cdn.discordapp.com/attachments/253286648291393536/1123709425518129173/0001000148415445.tik"}
            }},
            {"0001000148415450", new Dictionary<string, string> {
                {"cert", "https://cdn.discordapp.com/attachments/253286648291393536/1123709389329678417/0001000148415450.cert"},
                {"tmd", ""},
                {"tik", "https://cdn.discordapp.com/attachments/253286648291393536/1123709425950130236/0001000148415450.tik"}
            }},
            // Forecast Channel Certs
            {"000100024841464a", new Dictionary<string, string> {
                {"cert", "https://cdn.discordapp.com/attachments/253286648291393536/1123709479372980326/000100024841464a.cert"},
                {"tmd", ""},
                {"tik", ""}
            }},
            {"0001000248414645", new Dictionary<string, string> {
                {"cert", "https://cdn.discordapp.com/attachments/253286648291393536/1123709478697709638/0001000248414645.cert"},
                {"tmd", ""},
                {"tik", ""}
            }},
            {"0001000248414650", new Dictionary<string, string> {
                {"cert", "https://cdn.discordapp.com/attachments/253286648291393536/1123709479016484967/0001000248414650.cert"},
                {"tmd", ""},
                {"tik", ""}
            }},
            // Kirby TV Channel Cert, TMD, and TIK
            {"0001000148434d50", new Dictionary<string, string> {
                {"cert", "https://cdn.discordapp.com/attachments/253286648291393536/1123828090754314261/0001000148434d50.cert"},
                {"tmd", "https://cdn.discordapp.com/attachments/253286648291393536/1123828527918231563/0001000148434d50.tmd"},
                {"tik", "https://cdn.discordapp.com/attachments/253286648291393536/1123828811860017203/0001000148434d50.tik"}
            }}
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
        ApplyPatch(File.OpenRead(Path.Join(titleFolder, $"{appFile}.app")), File.OpenRead(Path.Join(patchFolder, $"{patchFile}.delta")), File.OpenWrite(Path.Join(tempFolder, $"{appFile}.app")));

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

        // Delete the unpack and unpack-patched folders
        Directory.Delete(titleFolder, true);
        Directory.Delete(tempFolder, true);
    }


    // Install Choose (Express Install)
    static void CoreChannel_LangSetup()
    {
        while (true)
        {
            PrintHeader();

            AnsiConsole.MarkupLine("[bold lime]Express Install[/]\n");

            AnsiConsole.MarkupLine($"Hello [bold lime]{Environment.UserName}[/]! Welcome to the Express Installation of WiiLink!");
            AnsiConsole.MarkupLine("The patcher will download any files that are required to run the patcher.\n");

            AnsiConsole.MarkupLine("[bold]Step 1: Choose core channel language[/]\n");

            AnsiConsole.MarkupLine("For [bold]Wii Room[/], [bold]Digicam Print Channel[/], and [bold]Food Channel[/], which language would you like to select?\n");

            AnsiConsole.MarkupLine("1. English Translation");
            AnsiConsole.MarkupLine("2. Japanese\n");

            AnsiConsole.MarkupLine("3. Go Back to Main Menu\n");

            int choice = UserChoose("123");
            switch (choice)
            {
                case 1:
                    reg = "EN";
                    lang = "English";
                    DemaeConfiguration();
                    break;
                case 2:
                    reg = "JP";
                    lang = "Japan";
                    demae_version = "standard";
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

            AnsiConsole.MarkupLine("[bold lime]Express Install[/]\n");

            AnsiConsole.MarkupLine("[bold]Step 1B: Choose Food Channel version[/]\n");

            AnsiConsole.MarkupLine("For [bold]Food Channel[/], which version would you like to install?\n");

            AnsiConsole.MarkupLine("1. Standard [bold](Fake Ordering)[/]");
            AnsiConsole.MarkupLine("2. Domino's [bold](US and Canada only)[/]");
            AnsiConsole.MarkupLine("3. Deliveroo [bold](Select EU countries only)[/]\n");

            Console.WriteLine("4. Go Back to Main Menu\n");

            int choice = UserChoose("1234");
            switch (choice)
            {
                case 1:
                    demae_version = "standard";
                    WiiConnect24Setup();
                    break;
                case 2:
                    demae_version = "dominos";
                    WiiConnect24Setup();
                    break;
                case 3:
                    demae_version = "deliveroo";
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

            AnsiConsole.MarkupLine("[bold lime]Express Install[/]\n");

            AnsiConsole.MarkupLine("Would you like to install [bold]WiiLink's WiiConnect24 services[/]?\n");

            AnsiConsole.MarkupLine("Services that would be installed:\n");

            AnsiConsole.MarkupLine("  ● Nintendo Channel");
            AnsiConsole.MarkupLine("  ● Forecast Channel\n");

            Console.WriteLine("1. Yes");
            Console.WriteLine("2. No\n");

            Console.WriteLine("3. Go Back to Main Menu\n");

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

            AnsiConsole.MarkupLine("[bold lime]Express Install[/]\n");

            AnsiConsole.MarkupLine("[bold]Step 2: Choose Nintendo Channel region[/]\n");

            AnsiConsole.MarkupLine("For [bold]Nintendo Channel[/], which region would you like to install?\n");

            Console.WriteLine("1. North America");
            Console.WriteLine("2. PAL");
            Console.WriteLine("3. Japan\n");

            Console.WriteLine("4. Go Back to Main Menu");
            Console.WriteLine();

            int choice = UserChoose("1234");
            switch (choice)
            {
                case 1:
                    nc_reg = "USA";
                    ForecastSetup();
                    break;
                case 2:
                    nc_reg = "PAL";
                    ForecastSetup();
                    break;
                case 3:
                    nc_reg = "Japan";
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

            AnsiConsole.MarkupLine("[bold lime]Express Install[/]\n");

            AnsiConsole.MarkupLine("[bold]Step 3: Choose Forecast Channel region[/]\n");

            AnsiConsole.MarkupLine("For [bold]Forecast Channel[/], which region would you like to install?\n");

            Console.WriteLine("1. North America");
            Console.WriteLine("2. PAL");
            Console.WriteLine("3. Japan\n");

            Console.WriteLine("4. Go Back to Main Menu\n");

            int choice = UserChoose("1234");
            switch (choice)
            {
                case 1:
                    forecast_reg = "USA";
                    KirbyTVSetup();
                    break;
                case 2:
                    forecast_reg = "PAL";
                    KirbyTVSetup();
                    break;
                case 3:
                    forecast_reg = "Japan";
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
        string stepNum = !installWC24 ? "Step 2" : "Step 4";

        while (true)
        {
            PrintHeader();

            AnsiConsole.MarkupLine("[bold lime]Express Install[/]\n");

            AnsiConsole.MarkupLine($"[bold]{stepNum}: Choose to install Kirby TV Channel[/]\n");

            AnsiConsole.MarkupLine("Would you like to install [bold]Kirby TV Channel[/]?\n");

            Console.WriteLine("1. Yes");
            Console.WriteLine("2. No\n");

            Console.WriteLine("3. Go Back to Main Menu\n");

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
        string stepNum = !installWC24 ? "Step 3" : "Step 5";

        while (true)
        {
            PrintHeader();

            AnsiConsole.MarkupLine("[bold lime]Express Install[/]\n");

            AnsiConsole.MarkupLine($"[bold]{stepNum}: Choose console platform[/]\n");

            Console.WriteLine("Which Wii version are you installing to?\n");

            AnsiConsole.MarkupLine("1. Wii [bold](or Dolphin Emulator)[/]");
            AnsiConsole.MarkupLine("2. vWii [bold](Wii U)[/]\n");

            Console.WriteLine("3. Go Back to Main Menu\n");

            int choice = UserChoose("123");
            switch (choice)
            {
                case 1:
                    platformType = "Wii";
                    SDSetup();
                    break;
                case 2:
                    platformType = "vWii";
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

            string stepNum = isCustomSetup ? "Step 4" : (!installWC24 ? "Step 4" : "Step 6");
            string installType = isCustomSetup ? "Custom Install" : "Express Install";

            AnsiConsole.MarkupLine($"[bold lime]{installType}[/]\n");

            AnsiConsole.MarkupLine($"[bold]{stepNum}: Insert SD Card / USB Drive (if applicable)[/]\n");

            Console.WriteLine("After passing this step, any user interaction won't be needed, so sit back and relax!\n");

            Console.WriteLine($"You can download everything directly to your Wii SD Card / USB Drive if you insert it before starting the patching\nprocess. Otherwise, everything will be saved in the same folder as this patcher on your computer.\n");

            AnsiConsole.MarkupLine(sdcard != null ? $"1. Start" : $"1. Start without SD Card / USB Drive");
            AnsiConsole.MarkupLine("2. Manually Select SD Card / USB Drive Path\n");

            if (sdcard != null)
                AnsiConsole.MarkupLine($"[[SD card detected: [bold lime]{sdcard}[/]]]\n");

            Console.WriteLine("3. Go Back to Main Menu\n");

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
                string installType = isCustomSetup ? "Custom Install" : "Express Install";
                string stepNum = isCustomSetup ? "Step 5" : (!installWC24 ? "Step 5" : "Step 7");

                AnsiConsole.MarkupLine($"[bold lime]{installType}[/]\n");

                AnsiConsole.MarkupLine($"[bold]{stepNum}: WAD folder detected[/]\n");

                AnsiConsole.MarkupLine("A [bold]WAD[/] folder has been detected in the current directory. This folder is used to store the WAD files that are downloaded during the patching process. If you choose to delete this folder, it will be recreated when you start the patching process again.\n");

                AnsiConsole.MarkupLine("1. [bold]Delete[/] WAD folder");
                AnsiConsole.MarkupLine("2. [bold]Keep[/] WAD folder\n");

                AnsiConsole.MarkupLine("3. [bold]Go Back to Main Menu[/]\n");

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
                            AnsiConsole.MarkupLine($"[bold red]ERROR:[/] {e.Message}");
                            Console.WriteLine();
                            Console.WriteLine("Press any key to try again...");
                            Console.ReadKey();
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

        // Dictionary version of the above switch statement
        Dictionary<string, string> demaeProgMsg_dict = new Dictionary<string, string>()
        {
            {"standard", "[[Standard]]"},
            {"dominos", "[[Domino's]]"},
            {"deliveroo", "[[Deliveroo]]"}
        };

        // Define a dictionary to store the different channel titles
        Dictionary<string, Dictionary<string, string>> corechannel_titles_dict = new Dictionary<string, Dictionary<string, string>>()
        {
            {"EN", new Dictionary<string, string>()
                {
                    {"demae", $"Food Channel [bold](English)[/] [bold]{demaeProgMsg_dict[demae_version]}[/]"},
                    {"wiiroom", "Wii Room [bold](English)[/]"},
                    {"digicam", "Digicam Print Channel [bold](English)[/]"}
                }
            },
            {"JP", new Dictionary<string, string>()
                {
                    {"demae", $"Demae Channel [bold](Japanese)[/] [bold]{demaeProgMsg_dict[demae_version]}[/]"},
                    {"wiiroom", "Wii no Ma [bold](Japanese)[/]"},
                    {"digicam", "Digicam Print Channel [bold](Japanese)[/]"}
                }
            }
        };

        // Set the channel titles based on the region
        string demae_title = corechannel_titles_dict[reg]["demae"];
        string wiiroom_title = corechannel_titles_dict[reg]["wiiroom"];
        string digicam_title = corechannel_titles_dict[reg]["digicam"];

        // Define a dictionary to store the different titles
        Dictionary<string, string> NCTitles_dict = new Dictionary<string, string>()
        {
            {"Japan", "Minna no Nintendo Channel [bold](Japan)[/]"},
            {"USA", "Nintendo Channel [bold](USA)[/]"},
            {"PAL", "Nintendo Channel [bold](PAL)[/]"},
        };

        // Set the Nintendo Channel title based on the region
        string NCTitle = NCTitles_dict[nc_reg];

        // Define a dictionary to store the different titles for the Forecast Channel
        Dictionary<string, Dictionary<string, string>> forecastTitles_dict = new Dictionary<string, Dictionary<string, string>>()
        {
            {"Wii", new Dictionary<string, string>()
                {
                    {"USA", "Forecast Channel [bold](USA)[/] [bold grey][[Wii]][/]"},
                    {"PAL", "Forecast Channel [bold](PAL)[/] [bold grey][[Wii]][/]"},
                    {"Japan", "Forecast Channel [bold](Japan)[/] [bold grey][[Wii]][/]"},
                }
            },
            {"vWii", new Dictionary<string, string>()
                {
                    {"USA", "Forecast Channel [bold](USA)[/] [bold deepskyblue1][[vWii]][/]"},
                    {"PAL", "Forecast Channel [bold](PAL)[/] [bold deepskyblue1][[vWii]][/]"},
                    {"Japan", "Forecast Channel [bold](Japan)[/] [bold deepskyblue1][[vWii]][/]"},
                }
            }
        };

        // Set the forecast channel title based on the platform type and region
        string forecastTitle = forecastTitles_dict[platformType][forecast_reg];

        // Define the channel_messages dictionary
        Dictionary<string, string> channelMessages = new Dictionary<string, string>()
        {
            { "wiiroom", wiiroom_title },
            { "digicam", digicam_title },
            { "demae", demae_title },
            { "nc", NCTitle },
            { "forecast", forecastTitle }
        };

        //// Setup patching process list ////
        List<Action> patching_functions = new List<Action>();

        patching_functions.Add(() => DownloadAllPatches());

        patching_functions.Add(() => WiiRoom_Patch(reg, lang));
        patching_functions.Add(() => Digicam_Patch(reg, lang));
        patching_functions.Add(() => Demae_Patch(reg, demae_version, lang));

        // Add Kirby TV Channel patching function if applicable
        if (installKirbyTV)
            patching_functions.Add(() => KirbyTV_Patch());

        // Add WiiConnect24 patching functions if applicable
        if (installWC24)
        {
            patching_functions.Add(() => NC_Patch(nc_reg));
            patching_functions.Add(() => Forecast_Patch(forecast_reg));
        }

        patching_functions.Add(() => FinishSDCopy());

        //// Set up patching progress dictionary ////
        // Flush dictionary and downloading patches
        patchingProgress_express.Clear();
        patchingProgress_express.Add("downloading", "in_progress");

        // Patching core channels
        foreach (string channel in new string[] { "wiiroom", "digicam", "demae" })
            patchingProgress_express.Add(channel, "not_started");

        // Patching Kirby TV Channel (if applicable)
        if (installKirbyTV)
            patchingProgress_express.Add("kirbytv", "not_started");

        // Patching WiiConnect24 channels
        if (installWC24)
        {
            foreach (string channel in new string[] { "nc", "forecast" })
                patchingProgress_express.Add(channel, "not_started");
        }

        // Finishing up
        patchingProgress_express.Add("finishing", "not_started");


        // While the patching process is not finished
        while (patchingProgress_express["finishing"] != "done")
        {
            PrintHeader();

            AnsiConsole.MarkupLine("[bold][[*]] Patching... this can take some time depending on the processing speed (CPU) of your computer.[/]\n");
            Console.Write("    Progress: ");

            //// Progress bar and completion display ////
            // Calculate percentage based on how many channels are completed
            int percentage = (int)((float)patchingProgress_express.Where(x => x.Value == "done").Count() / (float)patchingProgress_express.Count * 100.0f);

            // Calculate progress bar
            counter_done = (int)((float)percentage / 10.0f);
            StringBuilder progressBar = new StringBuilder("[[");
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

            AnsiConsole.Markup($" [bold]{percentage}%[/] completed\n\n");
            AnsiConsole.MarkupLine("Please wait while the patching process is in progress...\n");

            //// Display progress for each channel ////

            // Pre-Patching Section: Downloading files
            AnsiConsole.MarkupLine("[bold]Pre-Patching:[/]");
            switch (patchingProgress_express["downloading"])
            {
                case "not_started":
                    AnsiConsole.MarkupLine($"○ Downloading files...");
                    break;
                case "in_progress":
                    AnsiConsole.MarkupLine($"[slowblink yellow]●[/] Downloading files...");
                    break;
                case "done":
                    AnsiConsole.MarkupLine($"[bold lime]●[/] Downloading files...");
                    break;
            }

            // Patching Section: Patching core channels
            AnsiConsole.MarkupLine("\n[bold]Patching Core Channels:[/]");
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

            // Patching Kirby TV Channel (if applicable) in Core Channels section
            if (installKirbyTV)
            {
                switch (patchingProgress_express["kirbytv"])
                {
                    case "not_started":
                        AnsiConsole.MarkupLine($"○ Kirby TV Channel");
                        break;
                    case "in_progress":
                        AnsiConsole.MarkupLine($"[slowblink yellow]●[/] Kirby TV Channel");
                        break;
                    case "done":
                        AnsiConsole.MarkupLine($"[bold lime]●[/] Kirby TV Channel");
                        break;
                }
            }

            // Patching Section: Patching WiiConnect24 channels (if applicable)
            if (installWC24)
            {
                AnsiConsole.MarkupLine("\n[bold]Patching WiiConnect24 Channels:[/]");
                foreach (string channel in new string[] { "nc", "forecast" })
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



            // Post-Patching Section: Finishing up
            AnsiConsole.MarkupLine("\n[bold]Post-Patching:[/]");
            switch (patchingProgress_express["finishing"])
            {
                case "not_started":
                    AnsiConsole.MarkupLine($"○ Finishing up...");
                    break;
                case "in_progress":
                    AnsiConsole.MarkupLine($"[slowblink yellow]●[/] Finishing up...");
                    break;
                case "done":
                    AnsiConsole.MarkupLine($"[bold lime]●[/] Finishing up...");
                    break;
            }
            Console.WriteLine();

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

        // List of channels to patch, based on coreChannels_selection and wiiConnect24Channels_selection
        List<string> channelsToPatch = new List<string>();
        foreach (string channel in coreChannels_selection)
            channelsToPatch.Add(channel);
        foreach (string channel in wiiConnect24Channels_selection)
            channelsToPatch.Add(channel);
        /*         foreach (string channel in miscChannels_selection)
                    channelsToPatch.Add(channel); */

        // Set up patching progress dictionary
        patchingProgress_custom.Clear(); // Flush dictionary
        patchingProgress_custom.Add("downloading", "in_progress"); // Downloading patches
        foreach (string channel in channelsToPatch) // Patching channels
            patchingProgress_custom.Add(channel, "not_started");
        patchingProgress_custom.Add("finishing", "not_started"); // Finishing up

        // Give each core channel a proper name
        Dictionary<string, string> channelMap = new Dictionary<string, string>()
        {
            { "wiiroom_en", "Wii Room [bold](English)[/]" },
            { "wiinoma_jp", "Wii no Ma [bold](Japanese)[/]" },
            { "digicam_en", "Digicam Print Channel [bold](English)[/]" },
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
            { "kirbytv", "Kirby TV Channel" }
        };

        // Setup patching process arrays based on the selected channels
        Dictionary<string, Action> channelPatchingFunctions = new Dictionary<string, Action>()
        {
            { "wiiroom_en", () => WiiRoom_Patch("EN", "English") },
            { "wiinoma_jp", () => WiiRoom_Patch("JP", "Japanese") },
            { "digicam_en", () => Digicam_Patch("EN", "English") },
            { "digicam_jp", () => Digicam_Patch("JP", "Japanese") },
            { "food_en", () => Demae_Patch("EN", "standard", "English") },
            { "demae_jp", () => Demae_Patch("JP", "standard", "Japanese") },
            { "food_dominos", () => Demae_Patch("EN", "dominos", "English") },
            { "food_deliveroo", () => Demae_Patch("EN", "deliveroo", "English") },
            { "kirbytv", () => KirbyTV_Patch() },
            { "nc_us", () => NC_Patch("USA") },
            { "nc_eu", () => NC_Patch("PAL") },
            { "mnnc_jp", () => NC_Patch("Japan") },
            { "forecast_us", () => Forecast_Patch("USA") },
            { "forecast_eu", () => Forecast_Patch("PAL") },
            { "forecast_jp", () => Forecast_Patch("Japan") }
            
        };

        // Create a list of patching functions to execute
        List<Action> selectedPatchingFunctions = new List<Action>();

        // Add the patching functions to the list
        selectedPatchingFunctions.Add(() => DownloadCustomPatches(channelsToPatch));

        foreach (string selectedChannel in channelsToPatch)
            selectedPatchingFunctions.Add(channelPatchingFunctions[selectedChannel]);

        selectedPatchingFunctions.Add(() => FinishSDCopy());

        // Start patching
        int totalChannels = channelsToPatch.Count;
        while (patchingProgress_custom["finishing"] != "done")
        {
            PrintHeader();

            AnsiConsole.MarkupLine("[bold][[*]] Patching... this can take some time depending on the processing speed (CPU) of your computer.[/]\n");
            Console.Write("    Progress: ");

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
            AnsiConsole.Markup($" [bold]{percentage}%[/] completed\n\n");
            AnsiConsole.MarkupLine("Please wait while the patching process is in progress...\n");

            //// Display progress for each channel ////

            // Pre-Patching Section: Downloading files
            AnsiConsole.MarkupLine("[bold]Pre-Patching:[/]");
            switch (patchingProgress_custom["downloading"])
            {
                case "not_started":
                    AnsiConsole.MarkupLine($"○ Downloading files...");
                    break;
                case "in_progress":
                    AnsiConsole.MarkupLine($"[slowblink yellow]●[/] Downloading files...");
                    break;
                case "done":
                    AnsiConsole.MarkupLine($"[bold lime]●[/] Downloading files...");
                    break;
            }

            // Patching Section: Patching core channels
            if (coreChannels_selection.Count > 0)
            {
                AnsiConsole.MarkupLine("\n[bold]Patching Core Channels:[/]");
                foreach (string coreChannel in channelsToPatch)
                {
                    List<string> coreChannels = new List<string> { "wiiroom_en", "wiinoma_jp", "digicam_en", "digicam_jp", "food_en", "demae_jp", "food_dominos", "food_deliveroo", "kirbytv" };
                    if (coreChannels.Contains(coreChannel))
                    {
                        switch (patchingProgress_custom[coreChannel])
                        {
                            case "not_started":
                                AnsiConsole.MarkupLine($"○ {channelMap[coreChannel]}");
                                break;
                            case "in_progress":
                                AnsiConsole.MarkupLine($"[slowblink yellow]●[/] {channelMap[coreChannel]}");
                                break;
                            case "done":
                                AnsiConsole.MarkupLine($"[bold lime]●[/] {channelMap[coreChannel]}");
                                break;
                        }
                    }
                }
            }

            // Patching Section: Patching WiiConnect24 channels
            if (wiiConnect24Channels_selection.Count > 0)
            {
                AnsiConsole.MarkupLine("\n[bold]Patching WiiConnect24 Channels:[/]");
                foreach (string wiiConnect24Channel in channelsToPatch)
                {
                    List<string> wiiConnect24Channels = new List<string> { "nc_us", "nc_eu", "mnnc_jp", "forecast_us", "forecast_eu", "forecast_jp" };
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
            AnsiConsole.MarkupLine("\n[bold]Post-Patching:[/]");
            switch (patchingProgress_custom["finishing"])
            {
                case "not_started":
                    AnsiConsole.MarkupLine($"○ Finishing up...");
                    break;
                case "in_progress":
                    AnsiConsole.MarkupLine($"[slowblink yellow]●[/] Finishing up...");
                    break;
                case "done":
                    AnsiConsole.MarkupLine($"[bold lime]●[/] Finishing up...");
                    break;
            }
            Console.WriteLine();

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
        if (reg == "EN")
            DownloadSPD(platformType);
        else
        {
            if (!Directory.Exists("WAD"))
                Directory.CreateDirectory("WAD");
        }

        //// Downloading All Channel Patches ////

        // Wii no Ma (Wii Room)
        if (reg == "EN")
            DownloadPatch("WiinoMa", $"WiinoMa_0_{lang}.delta", "WiinoMa_0.delta", "Wii no Ma");
        DownloadPatch("WiinoMa", $"WiinoMa_1_{lang}.delta", "WiinoMa_1.delta", "Wii no Ma");
        DownloadPatch("WiinoMa", $"WiinoMa_2_{lang}.delta", "WiinoMa_2.delta", "Wii no Ma");

        // Digicam Print Channel
        if (reg == "EN")
            DownloadPatch("Digicam", $"Digicam_0_{lang}.delta", "Digicam_0.delta", "Digicam Print Channel");
        DownloadPatch("Digicam", $"Digicam_1_{lang}.delta", "Digicam_1.delta", "Digicam Print Channel");
        if (reg == "EN")
            DownloadPatch("Digicam", $"Digicam_2_{lang}.delta", "Digicam_2.delta", "Digicam Print Channel");

        // Demae Channel
        switch (demae_version)
        {
            case "standard":
                if (reg == "EN")
                    DownloadPatch("Demae", $"Demae_0_{lang}.delta", "Demae_0.delta", "Demae Channel (Standard)");
                DownloadPatch("Demae", $"Demae_1_{lang}.delta", "Demae_1.delta", "Demae Channel (Standard)");
                if (reg == "EN")
                    DownloadPatch("Demae", $"Demae_2_{lang}.delta", "Demae_2.delta", "Demae Channel (Standard)");
                break;
            case "dominos":
                DownloadPatch("Dominos", $"Dominos_0.delta", "Dominos_0.delta", "Demae Channel (Dominos)");
                DownloadPatch("Dominos", $"Dominos_1.delta", "Dominos_1.delta", "Demae Channel (Dominos)");
                DownloadPatch("Dominos", $"Dominos_2.delta", "Dominos_2.delta", "Demae Channel (Dominos)");
                break;
            case "deliveroo":
                DownloadPatch("Deliveroo", $"Deliveroo_0.delta", "Deliveroo_0.delta", "Demae Channel (Deliveroo)");
                DownloadPatch("Deliveroo", $"Deliveroo_1.delta", "Deliveroo_1.delta", "Demae Channel (Deliveroo)");
                DownloadPatch("Deliveroo", $"Deliveroo_2.delta", "Deliveroo_2.delta", "Demae Channel (Deliveroo)");
                break;
        }

        // If /apps/yawmME folder doesn't exist, create it
        if (!Directory.Exists(Path.Join("apps", "yawmME")))
            Directory.CreateDirectory(Path.Join("apps", "yawmME"));

        // Downloading YAWM ModMii Edition
        task = "Downloading YAWM ModMii Edition";
        DownloadFile($"https://hbb1.oscwii.org/unzipped_apps/yawmME/apps/yawmME/boot.dol", Path.Join("apps", "yawmME", "boot.dol"), "YAWM ModMii Edition");
        DownloadFile($"https://hbb1.oscwii.org/unzipped_apps/yawmME/apps/yawmME/meta.xml", Path.Join("apps", "yawmME", "meta.xml"), "YAWM ModMii Edition");
        DownloadFile($"https://hbb1.oscwii.org/hbb/yawmME.png", Path.Join("apps", "yawmME", "icon.png"), "YAWM ModMii Edition");

        // Downloading Get Console ID (for Dominos Demae Channel)
        if (demae_version == "dominos" || demae_version == "deliveroo")
        {
            task = "Downloading Get Console ID";

            // If /apps/GetConsoleID folder doesn't exist, create it
            if (!Directory.Exists(Path.Join("apps", "GetConsoleID")))
                Directory.CreateDirectory(Path.Join("apps", "GetConsoleID"));

            DownloadFile($"https://hbb1.oscwii.org/unzipped_apps/GetConsoleID/apps/GetConsoleID/boot.dol", Path.Join("apps", "GetConsoleID", "boot.dol"), "Get Console ID");
            DownloadFile($"https://hbb1.oscwii.org/unzipped_apps/GetConsoleID/apps/GetConsoleID/meta.xml", Path.Join("apps", "GetConsoleID", "meta.xml"), "Get Console ID");
            DownloadFile($"https://hbb1.oscwii.org/hbb/GetConsoleID.png", Path.Join("apps", "GetConsoleID", "icon.png"), "Get Console ID");
        }

        // Nintendo Channel
        DownloadPatch("nc", $"NC_1_{nc_reg}.delta", "NC_1.delta", "Nintendo Channel");

        // Forecast Channel
        DownloadPatch("forecast", $"Forecast_1.delta", "Forecast_1.delta", "Forecast Channel");

        // Kirby TV Channel (only if user chose to install it)
        if (installKirbyTV)
            DownloadPatch("ktv", $"ktv_2.delta", "KirbyTV_2.delta", "Kirby TV Channel");

        // Downloading stuff is finished!
        patchingProgress_express["downloading"] = "done";
        patchingProgress_express["wiiroom"] = "in_progress";
    }

    // Custom Install (Part 1 - Select core channels)
    static void CustomInstall_CoreChannel_Setup()
    {
        task = "Custom Install (Part 1 - Select core channels)";

        // Flush the list of selected channels (in case the user goes back to the previous menu)
        coreChannels_selection.Clear();
        wiiConnect24Channels_selection.Clear();

        // List of selected channels
        HashSet<string> selectedChannels = new HashSet<string>();

        // Define a dictionary to map channel names to easy-to-read format
        Dictionary<string, string> channelMap = new Dictionary<string, string>()
        {
            { "Wii Room [bold](English)[/]", "wiiroom_en" },
            { "Wii no Ma [bold](Japanese)[/]", "wiinoma_jp" },
            { "Digicam Print Channel [bold](English)[/]", "digicam_en" },
            { "Digicam Print Channel [bold](Japanese)[/]", "digicam_jp" },
            { "Food Channel [bold](Standard) [[English]][/]", "food_en" },
            { "Demae Channel [bold](Standard) [[Japanese]][/]", "demae_jp" },
            { "Food Channel [bold](Dominos) [[English]][/]", "food_dominos" },
            { "Food Channel [bold](Deliveroo) [[English]][/]", "food_deliveroo" },
            { "Kirby TV Channel", "kirbytv" }
        };

        // Initialize selection array to "Not selected" using LINQ
        string[] selected = channelMap.Values.Select(_ => "[grey]Not selected[/]").ToArray();

        while (true)
        {
            PrintHeader();

            // Print title
            AnsiConsole.MarkupLine("[bold lime]Custom Install[/]");
            Console.WriteLine();
            AnsiConsole.MarkupLine("[bold]Step 1:[/] Select core channel(s) to install\n");

            // Display core channel selection menu
            AnsiConsole.MarkupLine("[bold]Select core channel(s) to install:[/]\n");
            var grid = new Grid();

            // Add channels to grid
            grid.AddColumn();
            grid.AddColumn();

            // Display list of channels
            for (int i = 1; i <= channelMap.Count; i++)
            {
                KeyValuePair<string, string> channel = channelMap.ElementAt(i - 1);
                grid.AddRow($"[bold]{i}.[/] {channel.Key}", selected[i - 1]);
            }

            AnsiConsole.Write(grid);
            Console.WriteLine();

            // Print instructions
            AnsiConsole.MarkupLine("[grey]< Press [bold white]a number[/] to select/deselect a channel, [bold white]ENTER[/] to continue, [bold white]Backspace[/] to go back, [bold white]ESC[/] to go back to exit setup >[/]\n");

            int choice = UserChoose("123456789");

            // Handle user input
            switch (choice)
            {
                case -1: // Escape
                    coreChannels_selection.Clear();
                    MainMenu();
                    break;
                case 0: // Enter
                    // Save selected channels to global variable if any are selected
                    if (selectedChannels.Count != 0)
                    {
                        foreach (string channel in selectedChannels)
                            coreChannels_selection.Add(channel);
                    }

                    CustomInstall_WiiConnect24_Setup();
                    break;
                case -2: // Backspace
                    coreChannels_selection.Clear();
                    MainMenu();
                    break;
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                    string channelName = channelMap.Values.ElementAt(choice - 1);
                    if (selectedChannels.Contains(channelName))
                    {
                        selectedChannels.Remove(channelName);
                        selected[choice - 1] = "[grey]Not selected[/]";
                    }
                    else
                    {
                        selectedChannels.Add(channelName);
                        selected[choice - 1] = "[bold lime]Selected[/]";
                    }
                    break;


                default:
                    break;
            }
        }
    }

    // Custom Install (Part 2 - Select WiiConnect24 channels)
    static void CustomInstall_WiiConnect24_Setup()
    {
        task = "Custom Install (Part 2 - Select WiiConnect24 channels)";

        // List of selected channels
        HashSet<string> selectedChannels = new HashSet<string>();

        // Define a dictionary to map channel names to easy-to-read format
        Dictionary<string, string> channelMap = new Dictionary<string, string>()
        {
            { "Nintendo Channel [bold](USA)[/]", "nc_us" },
            { "Nintendo Channel [bold](Europe)[/]", "nc_eu" },
            { "Minna no Nintendo Channel [bold](Japan)[/]", "mnnc_jp" },
            { "Forecast Channel [bold](USA)[/]", "forecast_us" },
            { "Forecast Channel [bold](Europe)[/]", "forecast_eu" },
            { "Forecast Channel [bold](Japan)[/]", "forecast_jp" }
        };

        // Initialize selection array to "Not selected" using LINQ
        string[] selected = channelMap.Values.Select(_ => "[grey]Not selected[/]").ToArray();

        while (true)
        {
            PrintHeader();

            // Print title
            AnsiConsole.MarkupLine("[bold lime]Custom Install[/]");
            Console.WriteLine();
            AnsiConsole.MarkupLine("[bold]Step 2:[/] Select WiiConnect24 channel(s)\n");


            // Display WC24 channel selection menu
            AnsiConsole.MarkupLine("[bold]Select WiiConnect24 channel(s) to install:[/]\n");
            var grid = new Grid();

            // Add channels to grid
            grid.AddColumn();
            grid.AddColumn();

            // Display list of channels
            for (int i = 1; i <= channelMap.Count; i++)
            {
                KeyValuePair<string, string> channel = channelMap.ElementAt(i - 1);
                grid.AddRow($"[bold]{i}.[/] {channel.Key}", selected[i - 1]);
            }

            AnsiConsole.Write(grid);
            Console.WriteLine();

            // Print instructions
            AnsiConsole.MarkupLine("[grey]< Press [bold white]a number[/] to select/deselect a channel, [bold white]ENTER[/] to continue, [bold white]Backspace[/] to go back, [bold white]ESC[/] to go back to exit setup >[/]\n");

            int choice = UserChoose("123456789");

            // Handle user input
            switch (choice)
            {
                case -1: // Escape
                    wiiConnect24Channels_selection.Clear();
                    coreChannels_selection.Clear();
                    MainMenu();
                    break;
                case 0: // Enter
                    // Save selected channels to global variable if any are selected
                    if (selectedChannels.Count != 0)
                    {
                        foreach (string channel in selectedChannels)
                            wiiConnect24Channels_selection.Add(channel);
                    }

                    // If both coreChannels_selection and wiiConnect24Channels_selection are empty, error out
                    if (coreChannels_selection.Count == 0 && wiiConnect24Channels_selection.Count == 0)
                    {
                        AnsiConsole.MarkupLine("\n[bold red]ERROR:[/] You must select at least one channel to proceed!");
                        Thread.Sleep(3000);
                        continue;
                    }

                    // If any selected core channels have "_en" in their name, go to SPD setup
                    if (coreChannels_selection.Any(channel => channel.Contains("_en")) || coreChannels_selection.Contains("food_dominos") || coreChannels_selection.Contains("food_deliveroo"))
                        CustomInstall_SPD_Setup();
                    else
                        CustomInstall_SummaryScreen();
                    break;
                case -2: // Backspace
                    wiiConnect24Channels_selection.Clear();
                    coreChannels_selection.Clear();
                    CustomInstall_CoreChannel_Setup();
                    break;
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                    string channelName = channelMap.Values.ElementAt(choice - 1);
                    if (selectedChannels.Contains(channelName))
                    {
                        selectedChannels.Remove(channelName);
                        selected[choice - 1] = "[grey]Not selected[/]";
                    }
                    else
                    {
                        selectedChannels.Add(channelName);
                        selected[choice - 1] = "[bold lime]Selected[/]";
                    }
                    break;
                default:
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
            AnsiConsole.MarkupLine("[bold lime]Custom Install[/]");
            Console.WriteLine();
            AnsiConsole.MarkupLine("[bold]Step 3:[/] Select WiiLink SPD version\n");

            // Display SPD version selection menu
            AnsiConsole.MarkupLine("[bold]Select WiiLink SPD version to install:[/]\n");
            AnsiConsole.MarkupLine("[bold]1.[/] WiiLink SPD [bold grey][[Wii]][/]");
            AnsiConsole.MarkupLine("[bold]2.[/] WiiLink SPD [bold deepskyblue1][[vWii]][/]");
            Console.WriteLine();

            // Prompt user to select SPD version (Press a number to select a version, Backspace to go back a step, ESC to go back to the main menu)
            AnsiConsole.MarkupLine("[grey]< Press [bold white]a number[/] to select a version, [bold white]Backspace[/] to go back, [bold white]ESC[/] to go back to exit setup >[/]\n");

            int choice = UserChoose("12");

            // Use a switch statement to handle user's SPD version selection
            switch (choice)
            {
                case -1: // Escape
                    wiiConnect24Channels_selection.Clear();
                    coreChannels_selection.Clear();
                    MainMenu();
                    break;
                case -2: // Backspace
                    wiiConnect24Channels_selection.Clear();
                    CustomInstall_WiiConnect24_Setup();
                    break;
                case 1:
                    spdVersion_custom = "Wii";
                    CustomInstall_SummaryScreen(showSPD: true);
                    break;
                case 2:
                    spdVersion_custom = "vWii";
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
        // Convert core channel names to proper names
        Dictionary<string, string> coreChannelMap = new Dictionary<string, string>()
        {
            { "wiiroom_en", "● Wii Room [bold](English)[/]" },
            { "wiinoma_jp", "● Wii no Ma [bold](Japanese)[/]" },
            { "digicam_en", "● Digicam Print Channel [bold](English)[/]" },
            { "digicam_jp", "● Digicam Print Channel [bold](Japanese)[/]" },
            { "food_en", "● Food Channel [bold](Standard) [[English]][/]" },
            { "demae_jp", "● Demae Channel [bold](Standard) [[Japanese]][/]" },
            { "food_dominos", "● Food Channel [bold](Dominos) [[English]][/]" },
            { "food_deliveroo", "● Food Channel [bold](Deliveroo) [[English]][/]" },
            { "kirbytv", "● Kirby TV Channel" }
        };
        List<string> selectedCoreChannels = new List<string>();
        if (coreChannels_selection.Count > 0)
        {
            foreach (string channel in coreChannels_selection)
            {
                if (coreChannelMap.TryGetValue(channel, out string? modifiedChannel))
                    selectedCoreChannels.Add(modifiedChannel);
            }
        }
        else
        {
            selectedCoreChannels.Add("● [grey]N/A[/]");
        }

        // Convert WiiConnect24 channel names to proper names
        Dictionary<string, string> wiiConnect24ChannelMap = new Dictionary<string, string>()
        {
            { "nc_us", "● Nintendo Channel [bold](USA)[/]" },
            { "nc_eu", "● Nintendo Channel [bold](Europe)[/]" },
            { "mnnc_jp", "● Minna no Nintendo Channel [bold](Japan)[/]" },
            { "forecast_us", "● Forecast Channel [bold](USA)[/]" },
            { "forecast_eu", "● Forecast Channel [bold](Europe)[/]"},
            { "forecast_jp", "● Forecast Channel [bold](Japan)[/]"}
        };
        List<string> selectedWiiConnect24Channels = new List<string>();
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
            AnsiConsole.MarkupLine("[bold lime]Custom Install[/]\n");
            AnsiConsole.MarkupLine("[bold]Summary of selected channels to be installed:[/]\n");

            // Display summary of selected channels in two columns using a grid
            var grid = new Grid();
            grid.AddColumn();
            grid.AddColumn();

            if (showSPD)
            {
                grid.AddColumn();
                grid.AddRow("[bold lime]Core Channels:[/]", "[bold lime]WiiConnect24 Channels:[/]", "[bold lime]SPD Version:[/]");
                grid.AddRow(string.Join("\n", selectedCoreChannels), string.Join("\n", selectedWiiConnect24Channels), spdVersion_custom == "Wii" ? "● [bold grey]Wii[/]" : "● [bold deepskyblue1]vWii[/]");
            }
            else
            {
                grid.AddRow("[bold lime]Core Channels:[/]", "[bold lime]WiiConnect24 Channels:[/]");
                grid.AddRow(string.Join("\n", selectedCoreChannels), string.Join("\n", selectedWiiConnect24Channels));
            }
            AnsiConsole.Write(grid);

            // User confirmation prompt
            AnsiConsole.MarkupLine("\n[bold]Are you sure you want to install these selected channels?[/]\n");
            AnsiConsole.MarkupLine("1. Yes");
            AnsiConsole.MarkupLine("2. No, start over\n");
            AnsiConsole.MarkupLine("3. No, go back to main menu\n");
            var choice = UserChoose("123");

            // Handle user confirmation choice
            switch (choice)
            {
                case 1: // Yes
                    SDSetup(isCustomSetup: true);
                    break;
                case 2: // No, start over
                    coreChannels_selection.Clear();
                    wiiConnect24Channels_selection.Clear();
                    CustomInstall_CoreChannel_Setup();
                    break;
                case 3: // No, go back to main menu
                    coreChannels_selection.Clear();
                    wiiConnect24Channels_selection.Clear();
                    MainMenu();
                    break;
                default:
                    break;
            }
        }
    }

    // Download respective patches for selected core and WiiConnect24 channels (and SPD if English is selected for core channels)
    static void DownloadCustomPatches(List<string> channelSelection)
    {
        task = "Downloading selected patches";

        // Download SPD if English is selected for core channels
        if (channelSelection.Any(x => x.Contains("_en")) || channelSelection.Contains("food_dominos") || channelSelection.Contains("food_deliveroo"))
            DownloadSPD(spdVersion_custom);
        else
            Directory.CreateDirectory("WAD");

        // Download patches for selected core channels
        foreach (string channel in channelSelection)
        {
            switch (channel)
            {
                case "wiiroom_en":
                    task = "Downloading Wii Room (English)";
                    DownloadPatch("WiinoMa", $"WiinoMa_0_English.delta", "WiinoMa_0.delta", "Wii Room");
                    DownloadPatch("WiinoMa", $"WiinoMa_1_English.delta", "WiinoMa_1.delta", "Wii Room");
                    DownloadPatch("WiinoMa", $"WiinoMa_2_English.delta", "WiinoMa_2.delta", "Wii Room");
                    break;
                case "wiinoma_jp":
                    task = "Downloading Wii no Ma (Japan)";
                    DownloadPatch("WiinoMa", $"WiinoMa_1_Japan.delta", "WiinoMa_1.delta", "Wii no Ma");
                    DownloadPatch("WiinoMa", $"WiinoMa_2_Japan.delta", "WiinoMa_2.delta", "Wii no Ma");
                    break;
                case "digicam_en":
                    task = "Downloading Digicam Print Channel (English)";
                    DownloadPatch("Digicam", $"Digicam_0_English.delta", "Digicam_0.delta", "Digicam Print Channel");
                    DownloadPatch("Digicam", $"Digicam_1_English.delta", "Digicam_1.delta", "Digicam Print Channel");
                    DownloadPatch("Digicam", $"Digicam_2_English.delta", "Digicam_2.delta", "Digicam Print Channel");
                    break;
                case "digicam_jp":
                    task = "Downloading Digicam Print Channel (Japan)";
                    DownloadPatch("Digicam", $"Digicam_1_Japan.delta", "Digicam_1.delta", "Digicam Print Channel");
                    break;
                case "food_en":
                    task = "Downloading Food Channel (English)";
                    DownloadPatch("Demae", $"Demae_0_English.delta", "Demae_0.delta", "Food Channel (Standard)");
                    DownloadPatch("Demae", $"Demae_1_English.delta", "Demae_1.delta", "Food Channel (Standard)");
                    DownloadPatch("Demae", $"Demae_2_English.delta", "Demae_2.delta", "Food Channel (Standard)");
                    break;
                case "demae_jp":
                    task = "Downloading Demae Channel (Japan)";
                    DownloadPatch("Demae", $"Demae_1_Japan.delta", "Demae_1.delta", "Demae Channel");
                    break;
                case "food_dominos":
                    task = "Downloading Food Channel (Domino's)";
                    DownloadPatch("Dominos", $"Dominos_0.delta", "Dominos_0.delta", "Food Channel (Domino's)");
                    DownloadPatch("Dominos", $"Dominos_1.delta", "Dominos_1.delta", "Food Channel (Domino's)");
                    DownloadPatch("Dominos", $"Dominos_2.delta", "Dominos_2.delta", "Food Channel (Domino's)");

                    task = "Downloading Get Console ID";
                    if (!Directory.Exists(Path.Join("apps", "GetConsoleID")))
                        Directory.CreateDirectory(Path.Join("apps", "GetConsoleID"));
                    DownloadFile($"https://hbb1.oscwii.org/unzipped_apps/GetConsoleID/apps/GetConsoleID/boot.dol", Path.Join("apps", "GetConsoleID", "boot.dol"), "Get Console ID");
                    DownloadFile($"https://hbb1.oscwii.org/unzipped_apps/GetConsoleID/apps/GetConsoleID/meta.xml", Path.Join("apps", "GetConsoleID", "meta.xml"), "Get Console ID");
                    DownloadFile($"https://hbb1.oscwii.org/hbb/GetConsoleID.png", Path.Join("apps", "GetConsoleID", "icon.png"), "Get Console ID");
                    break;
                case "food_deliveroo":
                    task = "Downloading Food Channel (Deliveroo)";
                    DownloadPatch("Deliveroo", $"Deliveroo_0.delta", "Deliveroo_0.delta", "Food Channel (Deliveroo)");
                    DownloadPatch("Deliveroo", $"Deliveroo_1.delta", "Deliveroo_1.delta", "Food Channel (Deliveroo)");
                    DownloadPatch("Deliveroo", $"Deliveroo_2.delta", "Deliveroo_2.delta", "Food Channel (Deliveroo)");

                    task = "Downloading Get Console ID";
                    if (!Directory.Exists(Path.Join("apps", "GetConsoleID")))
                        Directory.CreateDirectory(Path.Join("apps", "GetConsoleID"));
                    DownloadFile($"https://hbb1.oscwii.org/unzipped_apps/GetConsoleID/apps/GetConsoleID/boot.dol", Path.Join("apps", "GetConsoleID", "boot.dol"), "Get Console ID");
                    DownloadFile($"https://hbb1.oscwii.org/unzipped_apps/GetConsoleID/apps/GetConsoleID/meta.xml", Path.Join("apps", "GetConsoleID", "meta.xml"), "Get Console ID");
                    DownloadFile($"https://hbb1.oscwii.org/hbb/GetConsoleID.png", Path.Join("apps", "GetConsoleID", "icon.png"), "Get Console ID");
                    break;
                case "nc_us":
                    task = "Downloading Nintendo Channel (USA)";
                    DownloadPatch("nc", $"NC_1_USA.delta", "NC_1.delta", "Nintendo Channel");
                    break;
                case "mnnc_jp":
                    task = "Downloading Nintendo Channel (Japan)";
                    DownloadPatch("nc", $"NC_1_Japan.delta", "NC_1.delta", "Nintendo Channel");
                    break;
                case "nc_eu":
                    task = "Downloading Nintendo Channel (Europe)";
                    DownloadPatch("nc", $"NC_1_PAL.delta", "NC_1.delta", "Nintendo Channel");
                    break;
                case "forecast_us": // Forecast Patch works for all regions now
                case "forecast_jp":
                case "forecast_eu":
                    task = "Downloading Forecast Channel";
                    DownloadPatch("forecast", $"Forecast_1.delta", "Forecast_1.delta", "Forecast Channel");
                    break;
                case "kirbytv":
                    task = "Downloading Kirby TV Channel";
                    DownloadPatch("ktv", $"ktv_2.delta", "KirbyTV_2.delta", "Kirby TV Channel");
                    break;
            }
        }

        // Downloading YAWM ModMii Edition
        task = "Downloading YAWM ModMii Edition";
        if (!Directory.Exists(Path.Join("apps", "yawmME")))
            Directory.CreateDirectory(Path.Join("apps", "yawmME"));
        DownloadFile($"https://hbb1.oscwii.org/unzipped_apps/yawmME/apps/yawmME/boot.dol", Path.Join("apps", "yawmME", "boot.dol"), "YAWM ModMii Edition");
        DownloadFile($"https://hbb1.oscwii.org/unzipped_apps/yawmME/apps/yawmME/meta.xml", Path.Join("apps", "yawmME", "meta.xml"), "YAWM ModMii Edition");
        DownloadFile($"https://hbb1.oscwii.org/hbb/yawmME.png", Path.Join("apps", "yawmME", "icon.png"), "YAWM ModMii Edition");
    }

    // Patching Wii no Ma
    static void WiiRoom_Patch(string reg = "", string lang = "")
    {
        task = "Patching Wii no Ma";

        // Dictionary for which files to patch
        var wiiroom_patch_list = new List<KeyValuePair<string, string>>()
        {
            new KeyValuePair<string, string>("WiinoMa_0", "00000000"),
            new KeyValuePair<string, string>("WiinoMa_1", "00000001"),
            new KeyValuePair<string, string>("WiinoMa_2", "00000002")
        };

        // If English, change channel title to "Wii Room"
        string wiiroom_title = reg == "EN" ? "Wii Room" : "Wii no Ma";

        PatchCoreChannel("WiinoMa", wiiroom_title, "000100014843494a", wiiroom_patch_list, lang: lang);

        // Finished patching Wii no Ma
        patchingProgress_express["wiiroom"] = "done";
        patchingProgress_express["digicam"] = "in_progress";
    }

    // Patching Digicam Print Channel
    static void Digicam_Patch(string reg = "", string lang = "")
    {
        task = "Patching Digicam Print Channel";

        // Dictionary for which files to patch
        var digicam_patch_list = new List<KeyValuePair<string, string>>()
        {
            new KeyValuePair<string, string>("Digicam_0", "00000000"),
            new KeyValuePair<string, string>("Digicam_1", "00000001"),
            new KeyValuePair<string, string>("Digicam_2", "00000002")
        };

        PatchCoreChannel("Digicam", "Digicam Print Channel", "000100014843444a", digicam_patch_list, lang: lang);

        // Finished patching Digicam Print Channel
        patchingProgress_express["digicam"] = "done";
        patchingProgress_express["demae"] = "in_progress";
    }

    // Patching Demae Channel
    static void Demae_Patch(string reg = "", string demae_version = "", string lang = "")
    {
        task = "Patching Demae Channel";

        // If reg is EN, change channel title to "Food Channel", else "Demae Channel"
        string demae_title = reg == "EN" ? "Food Channel" : "Demae Channel";

        // Map demae_version to corresponding patch list
        var patchLists = new Dictionary<string, List<KeyValuePair<string, string>>>()
        {
            ["dominos"] = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Dominos_0", "00000000"),
                new KeyValuePair<string, string>("Dominos_1", "00000001"),
                new KeyValuePair<string, string>("Dominos_2", "00000002")
            },
            ["deliveroo"] = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Deliveroo_0", "00000000"),
                new KeyValuePair<string, string>("Deliveroo_1", "00000001"),
                new KeyValuePair<string, string>("Deliveroo_2", "00000002")
            },
            ["standard"] = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Demae_0", "00000000"),
                new KeyValuePair<string, string>("Demae_1", "00000001"),
                new KeyValuePair<string, string>("Demae_2", "00000002")
            }
        };

        // Get patch list based on demae_version
        var demae_patch_list = patchLists.TryGetValue(demae_version, out var patchList) ? patchList : patchLists[""];

        // Dictionary to properly name demae folders and versions
        var demaeNameVer_dict = new Dictionary<string, (string, string)>
        {
            // (Demae folder name, Demae version)
            ["dominos"] = ("Dominos", "Dominos"),
            ["deliveroo"] = ("Deliveroo", "Deliveroo"),
            ["standard"] = ("Demae", "Standard")
        };

        // Set demae_folder and demae_ver based on demae_version
        if (demaeNameVer_dict.TryGetValue(demae_version, out var demaeInfo))
        {
            var (demaeFolder, demaeVer) = demaeInfo;
            PatchCoreChannel(demaeFolder, $"{demae_title} ({demaeVer})", "000100014843484a", demae_patch_list, lang: lang);
        }

        // Finished patching Demae Channel
        patchingProgress_express["demae"] = "done";
        patchingProgress_express[!installKirbyTV ? "nc" : "kirbytv"] = "in_progress";
    }

    // Patching Kirby TV Channel (if applicable)
    static void KirbyTV_Patch()
    {
        task = "Patching Kirby TV Channel";

        PatchWC24Channel("ktv", "Kirby TV Channel", 257, null, "0001000148434d50", "KirbyTV_2", "0000000e");

        // Finished patching Kirby TV Channel
        patchingProgress_express["kirbytv"] = "done";
        patchingProgress_express["nc"] = "in_progress";
    }


    // Patching Nintendo Channel
    static void NC_Patch(string nc_reg = "")
    {
        task = "Patching Nintendo Channel";

        // Properly set Nintendo Channel titleID, appNum, and channel_title
        var ncRegData_dict = new Dictionary<string, (string, string, string)>
        {
            {"USA", ("0001000148415445", "0000002c", "Nintendo Channel")},
            {"PAL", ("0001000148415450", "0000002d", "Nintendo Channel")},
            {"Japan", ("000100014841544a", "0000003e", "Minna no Nintendo Channel")}
        };

        if (ncRegData_dict.TryGetValue(nc_reg, out var data))
        {
            var (NC_titleID, appNum, channel_title) = data;
            PatchWC24Channel("nc", $"{channel_title}", 1792, nc_reg, NC_titleID, "NC_1", appNum);
        }

        // Finished patching Nintendo Channel
        patchingProgress_express["nc"] = "done";
        patchingProgress_express["forecast"] = "in_progress";
    }

    // Patching Forecast Channel
    static void Forecast_Patch(string forecast_reg = "")
    {
        task = "Patching Forecast Channel";

        // Properly set Forecast Channel titleID
        var forecastRegData_dict = new Dictionary<string, string>
        {
            {"USA", "0001000248414645"},
            {"PAL", "0001000248414650"},
            {"Japan", "000100024841464a"}
        };

        if (forecastRegData_dict.TryGetValue(forecast_reg, out var forecastTitleID))
            PatchWC24Channel("forecast", $"Forecast Channel", 7, forecast_reg, forecastTitleID, "Forecast_1", "0000000d");

        // Finished patching Forecast Channel
        patchingProgress_express["forecast"] = "done";
        patchingProgress_express["finishing"] = "in_progress";
    }

    // Finish SD Copy
    static void FinishSDCopy()
    {
        // Copying files to SD card
        task = "Copying files to SD card";

        if (sdcard != null)
        {
            AnsiConsole.Markup(" [bold][[*]] Copying files to SD card, which may take a while.[/]\n");

            try
            {
                // Copy apps and WAD folder to SD card
                CopyFolder("apps", Path.Join(sdcard, "apps"));
                CopyFolder("WAD", Path.Join(sdcard, "WAD"));
            }
            catch (Exception e)
            {
                AnsiConsole.MarkupLine($"[bold red]ERROR:[/] {e.Message}");
                Console.WriteLine("Press any key to try again...");
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
            AnsiConsole.MarkupLine("[bold slowblink lime]Patching Completed![/]\n");

            if (sdcard != null)
                Console.WriteLine("Every file is in it's place on your SD Card / USB Drive!\n");
            else
            {
                AnsiConsole.MarkupLine("[bold]Please connect your Wii SD Card / USB Drive and copy the [u]WAD[/] and [u]apps[/] folders to the root (main folder) of your\nSD Card / USB Drive.[/]\n");

                AnsiConsole.MarkupLine($"You can find these folders in the [u]{curDir}[/] folder of your computer.\n");
            }

            AnsiConsole.MarkupLine("Please proceed with the tutorial that you can find on [bold lime link]https://wii.guide/wiilink[/]\n");

            AnsiConsole.MarkupLine("[bold]What would you like to do now?[/]\n");

            AnsiConsole.MarkupLine(sdcard != null ? "1. Open the SD Card / USB Drive folder" : "1. Open the folder");
            AnsiConsole.MarkupLine("2. Go back to the main menu");
            AnsiConsole.MarkupLine("3. Exit the program\n");

            int choice = UserChoose("123");
            switch (choice)
            {
                case 1:
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        Process.Start(@"explorer.exe", sdcard != null ? sdcard : curDir);
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                        Process.Start("xdg-open", sdcard != null ? sdcard : curDir);
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                        Process.Start("open", sdcard != null ? sdcard : curDir);
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
        string? sdcard_new = "";
        string? inputUpper = "";

        while (true)
        {
            PrintHeader();

            AnsiConsole.MarkupLine("[bold lime]Manually Select SD Card / USB Drive Path[/]\n");

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                Console.WriteLine("Please enter the drive letter of your SD card/USB drive (e.g. E)");
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                Console.WriteLine("Please enter the mount name of your SD card/USB drive (e.g. /media/username/Wii)");
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                Console.WriteLine("Please enter the volume name of your SD card/USB drive (e.g. /Volumes/Wii)");

            AnsiConsole.MarkupLine("(Type [bold]EXIT[/] to go back to the previous menu)\n");

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                Console.Write("New SD card/USB drive: ");
            else
                Console.Write("New SD card/USB drive volume: ");

            // Get user input, if user presses ESC (without needing to press ENTER), go back to previous menu
            sdcard_new = Console.ReadLine();
            inputUpper = sdcard_new?.ToUpper();

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
                    AnsiConsole.MarkupLine("[bold red]Drive letter must be 1 character![/]");
                    System.Threading.Thread.Sleep(2000);
                    continue;
                }
            }

            // Format SD card path depending on OS
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                sdcard_new = sdcard_new + ":\\";
            else
            {
                // If / is already at the end of the path, remove it
                if (sdcard_new?.EndsWith("/") == true)
                    sdcard_new = sdcard_new.Remove(sdcard_new.Length - 1);
            }

            // Prevent user from selecting boot drive
            if (Path.GetPathRoot(sdcard_new) == Path.GetPathRoot(Path.GetPathRoot(Environment.SystemDirectory)))
            {
                AnsiConsole.MarkupLine("[bold red]You cannot select your boot drive![/]");
                System.Threading.Thread.Sleep(2000);
                continue;
            }

            // Check if new SD card path is the same as the old one
            if (sdcard_new == sdcard)
            {
                AnsiConsole.MarkupLine("[bold red]You have already selected this SD card/USB drive![/]");
                System.Threading.Thread.Sleep(2000);
                continue;
            }

            // Check if drive/volume exists
            if (!Directory.Exists(sdcard_new))
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    AnsiConsole.MarkupLine("[bold red]Drive does not exist![/]");
                else
                    AnsiConsole.MarkupLine("[bold red]Volume does not exist![/]");

                System.Threading.Thread.Sleep(2000);
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
                // SD card is invalid
                AnsiConsole.MarkupLine("\n[bold]Drive detected, but no /apps folder found![/]");
                Console.WriteLine("Please create it first and then try again.\n");

                // Press any key to continue
                Console.WriteLine("Press any key to continue...");
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

            List<string> languages = new List<string>();
            languages.Add("English");

            AnsiConsole.MarkupLine($"[bold]Notice:[/] This feature is a work in progress, so only English is available for now.\n");

            AnsiConsole.MarkupLine($"[bold lime]Choose a Language[/]\n");

            // Display languages
            for (int i = 0; i < languages.Count; i++)
                AnsiConsole.MarkupLine($"{i + 1}. {languages[i]}");

            AnsiConsole.MarkupLine($"\n{languages.Count + 1}. Go back to Settings\n");


            var languageCodes = new Dictionary<int, string>();
            languageCodes.Add(1, "EN");

            // Choices (build from languages + 1 sequentially)
            string choices = "";
            for (int i = 1; i <= languages.Count + 1; i++)
                choices += i.ToString();

            int choice = UserChoose(choices);

            // Map choice to language code
            if (languageCodes.TryGetValue(choice, out var langCode))
            {
                // Set programLang to chosen language code
                localizeLang = langCode.ToLower();

                // Since English is hardcoded, there's no language pack for it
                if (localizeLang == "en")
                {
                    SettingsMenu();
                    break;
                }

                // Download language pack if it doesn't exist
                if (!File.Exists(Path.Join(tempDir, "LanguagePack", $"LocalizedText.{langCode}.json")))
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

        AnsiConsole.MarkupLine($"\n[bold lime]Downloading Language Pack ({languageCode})[/]\n");

        // Create LanguagePack folder if it doesn't exist
        var languagePackDir = Path.Join(tempDir, "LanguagePack");
        if (!Directory.Exists(languagePackDir))
            Directory.CreateDirectory(languagePackDir);

        // Download language file
        var languageFileUrl = $"{URL}/LocalizedText.{languageCode.ToLower()}.json";
        var languageFilePath = Path.Join(languagePackDir, $"LocalizedText.{languageCode.ToLower()}.json");

        DownloadFile(languageFileUrl, languageFilePath, $"Language Pack ({languageCode})");
    }

    static void SettingsMenu()
    {
        while (true)
        {
            PrintHeader();
            PrintNotice();

            AnsiConsole.MarkupLine($"[bold lime]Settings[/]\n");

            if (!inCompatabilityMode)
            {
                Console.WriteLine($"1. Change Language");
                Console.WriteLine($"2. Credits\n");

                Console.WriteLine($"3. Go back to Main Menu\n");
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
            string welcomeMessage = localizeLang == "en"
                ? "Welcome to the WiiLink Patcher!\n"
                : $"{localizedText?["MainMenu"]?["welcomeMessage"]}\n";
            AnsiConsole.MarkupLine(welcomeMessage);

            AnsiConsole.MarkupLine($"{(localizeLang == "en" ? "1. Start Express Install Setup" : localizedText?["MainMenu"]?["startExpressSetup"])} [bold lime](Recommended)[/]");
            AnsiConsole.MarkupLine($"{(localizeLang == "en" ? "2. Start Custom Install Setup" : localizedText?["MainMenu"]?["startCustomSetup"])} [bold](Advanced)[/]");
            AnsiConsole.MarkupLine($"{(localizeLang == "en" ? "3. Settings" : localizedText?["MainMenu"]?["settings"])}\n");
            AnsiConsole.MarkupLine($"{(localizeLang == "en" ? "4. Exit Patcher" : localizedText?["MainMenu"]?["exitPatcher"])}\n");

            string SDDetectedOrNot = sdcard != null
                ? $"[bold lime]{(localizeLang == "en" ? "Detected SD Card / USB Drive:" : localizedText?["MainMenu"]?["sdCardDetected"])}[/] {sdcard}"
                : $"[bold red]{(localizeLang == "en" ? "Could not detect your SD Card / USB Drive!" : localizedText?["MainMenu"]?["noSDCard"])}[/]";
            AnsiConsole.MarkupLine(SDDetectedOrNot);

            AnsiConsole.MarkupLine($"{(localizeLang == "en" ? "R. Automatically detect SD Card / USB Drive" : localizedText?["MainMenu"]?["automaticDetection"])}");
            AnsiConsole.MarkupLine($"{(localizeLang == "en" ? "M. Manually select SD Card / USB Drive path" : localizedText?["MainMenu"]?["manualSelection"])}\n");

            // User chooses an option
            int choice = UserChoose("1234RrMm");
            switch (choice)
            {
                case 1: // Start Express Install
                    CoreChannel_LangSetup();
                    break;
                case 2: // Start Custom Install
                    CustomInstall_CoreChannel_Setup();
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
        Console.WriteLine($"Checking server status...");

        for (int i = 0; i < maxRetries; i++)
        {
            try
            {
                using (var response = await httpClient.GetAsync(url))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        AnsiConsole.MarkupLine("[bold lime]Successfully connected to server![/]");
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
                    AnsiConsole.MarkupLine("[bold red]Connection to server failed![/]\n");
                    return (ex.StatusCode ?? System.Net.HttpStatusCode.InternalServerError, ex.Message);
                }
            }

            if (i < maxRetries - 1)
            {
                AnsiConsole.MarkupLine($"Retrying in [bold]{retryDelayMs / 1000}[/] seconds...\n");
                await Task.Delay(retryDelayMs);
            }
        }

        return (System.Net.HttpStatusCode.ServiceUnavailable, "Connection to server failed!");
    }

    static void ConnectionFailed(System.Net.HttpStatusCode statusCode, string msg)
    {
        PrintHeader();

        AnsiConsole.MarkupLine("[bold blink red]Connection to server failed![/]\n");

        Console.WriteLine("Connection to the server failed. Please check your internet connection and try again.\n");
        Console.WriteLine("It seems that either the server is down or your internet connection is not working.\n");
        AnsiConsole.MarkupLine("If you are sure that your internet connection is working, please join our [link=https://discord.gg/WiiLink bold lime]Discord Server[/] and report this issue.\n");

        AnsiConsole.MarkupLine("[bold]Status code:[/] " + statusCode);
        AnsiConsole.MarkupLine("[bold]Message:[/] " + msg);

        AnsiConsole.MarkupLine("\n[bold yellow]Press any key to exit...[/]");

        Console.ReadKey();
        ExitApp();
    }

    public static async Task CheckForUpdates(string currentVersion)
    {
        PrintHeader();
        Console.WriteLine("Checking for updates...");

        // URL of the text file containing the latest version number
        string updateUrl = "https://raw.githubusercontent.com/PablosCorner/wiilink-patcher-version/main/version.txt";

        // Download the latest version number from the server
        HttpClient client = new HttpClient();
        string updateInfo = "";
        try
        {
            updateInfo = await client.GetStringAsync(updateUrl);
        }
        catch (HttpRequestException ex)
        {
            AnsiConsole.MarkupLine($"Error retrieving update information: [bold red]{ex.Message}[/]\n");

            AnsiConsole.MarkupLine("[bold]Skipping update check...[/]");
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
                AnsiConsole.MarkupLine("[bold]A new version is available! Would you like to download it now?[/]\n");
                AnsiConsole.MarkupLine($"Current version: {currentVersion}");
                AnsiConsole.MarkupLine($"Latest version: [bold lime]{latestVersion}[/]\n");

                // Show changelog via Github link
                AnsiConsole.MarkupLine($"[bold]Changelog:[/] [link lime]https://github.com/WiiLink24/WiiLink24-Patcher/releases/tag/{latestVersion}[/]\n");

                AnsiConsole.MarkupLine("1. Yes");
                AnsiConsole.MarkupLine("2. No\n");

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
                        AnsiConsole.MarkupLine($"\n[bold]Downloading [lime]{latestVersion}[/] for [lime]{osName}[/]...[/]");
                        Console.Out.Flush();

                        // Download the latest version and save it to a file
                        HttpResponseMessage response;
                        response = await client.GetAsync(downloadUrl);
                        if (!response.IsSuccessStatusCode) // Ideally shouldn't happen if version.txt is set up correctly
                        {
                            AnsiConsole.MarkupLine($"\n[red]An error occurred while downloading the latest version:[/] {response.StatusCode}");
                            AnsiConsole.MarkupLine("[red]Press any key to exit...[/]");
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
                            AnsiConsole.MarkupLine($"[bold lime]Download complete![/] Exiting in [bold lime]{i}[/] seconds...");
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
            AnsiConsole.MarkupLine("[bold lime]You are running the latest version![/]");
            Thread.Sleep(1000);
        }
    }

    static void ErrorScreen(int exitCode, string msg = "")
    {
        PrintHeader();

        AnsiConsole.MarkupLine("[bold red]An error has occurred.[/]\n");

        AnsiConsole.MarkupLine("[bold]ERROR DETAILS:[/]\n");
        AnsiConsole.MarkupLine($" * [bold]Task:[/] {task}");
        AnsiConsole.MarkupLine(msg == null ? $" * [bold]Command:[/] {curCmd}" : $" * [bold]Message:[/] {msg}");
        AnsiConsole.MarkupLine($" * [bold]Exit code:[/] {exitCode}\n");

        AnsiConsole.MarkupLine("Please open an issue on our GitHub page ([link bold lime]https://github.com/WiiLink24/WiiLink24-Patcher/issues[/]) and describe the");
        AnsiConsole.MarkupLine("error you encountered. Please include the error details above in your issue.\n");

        // Press any key to go back to the main menu
        AnsiConsole.MarkupLine("\n[bold]Press any key to go back to the main menu...[/]");
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
