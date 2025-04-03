using Spectre.Console;
using libWiiSharp;
using System.IO.Compression;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;

public class PatchClass
{

    /// <summary>
    /// Dictionary for GitHub Repositorys in case of OSC not available
    /// </summary>
    private static readonly Dictionary<string, (string author, string repo)> githubRepos = new()
    {
        { "AnyGlobe_Changer", ("fishguy6564", "AnyGlobe-Changer") },
        { "wiilink-account-linker", ("WiiLink24", "AccountLinker") },
        { "yawmME", ("modmii", "YAWM-ModMii-Edition") },
        { "sntp", ("ErikAndren", "sntp") },
        { "Mail-Patcher", ("WiiLink24", "Mail-Patcher") },
        { "app_name", ("author", "repo") }, // Beispiel-Eintrag, ergänze eigene Werte
    };

    /// <summary>
    /// Downloads an app from the Open Shop Channel
    /// </summary>
    /// <param name="appName"></param>
    public static void DownloadOSCApp(string appName)
    {
        MainClass.task = $"Downloading {appName}";
        string appPath = Path.Join("WiiLink", "apps", appName);

        if (!Directory.Exists(appPath))
            Directory.CreateDirectory(appPath);

        string oscwiiBaseUrl = $"https://hbb1.oscwii.org/unzipped_apps/{appName}/apps/{appName}/";
        bool oscwiiAvailable = TestUrl(oscwiiBaseUrl + "boot.dol");

        if (oscwiiAvailable)
        {
            DownloadFile(oscwiiBaseUrl + "boot.dol", Path.Join(appPath, "boot.dol"), appName);
            DownloadFile(oscwiiBaseUrl + "meta.xml", Path.Join(appPath, "meta.xml"), appName);
            DownloadFile($"https://hbb1.oscwii.org/api/v3/contents/{appName}/icon.png", Path.Join(appPath, "icon.png"), appName);
        }
        else if (githubRepos.TryGetValue(appName, out var repoInfo))
        {
            string latestReleaseUrl = GetLatestGitHubRelease(repoInfo.author, repoInfo.repo);
            if (!string.IsNullOrEmpty(latestReleaseUrl))
            {
                string zipPath = Path.Join(appPath, $"{appName}.zip");
                DownloadFile(latestReleaseUrl, zipPath, appName);
                ExtractRequiredFiles(zipPath, appPath);
                File.Delete(zipPath); // ZIP-Datei nach Extraktion löschen
            }
        }
        else
        {
            AnsiConsole.MarkupLine($"[bold red]ERROR:[/] No alternative source found for {appName}");
        }
    }

    /// <summary>
    /// Tests if the URL is available
    /// </summary>
    /// <param name="url"></param>
    private static bool TestUrl(string url)
    {
        try
        {
            var response = MainClass.httpClient.Send(new HttpRequestMessage(HttpMethod.Head, url));
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
    
    /// <summary>
    /// Gets the latest GitHub Release
    /// </summary>
    /// <param name="author"></param>
    /// <param name="repo"></param>
    private static string GetLatestGitHubRelease(string author, string repo)
    {
        try
        {
            string apiUrl = $"https://api.github.com/repos/{author}/{repo}/releases";
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0");
            var response = httpClient.GetStringAsync(apiUrl).Result;

            var releases = JsonSerializer.Deserialize(response, GitHubJsonContext.Default.ListGitHubRelease);

            if (releases != null && releases.Count > 0)
            {
                return releases[0].assets.Count > 0 ? releases[0].assets[0].browser_download_url : "";
            }
        }
        catch (Exception e)
        {
            AnsiConsole.MarkupLine($"[bold red]ERROR:[/] GitHub API request failed: {e.Message}");
        }

        return "";
    }


    /// <summary>
    /// Extracts the required files from the .zip
    /// </summary>
    /// <param name="zipFilePath"></param>
    /// <param name="destination">The destination to save the file to.</param>
    private static void ExtractRequiredFiles(string zipFilePath, string destination)
    {
        using ZipArchive archive = ZipFile.OpenRead(zipFilePath);
        string[] requiredFiles = { "boot.dol", "meta.xml", "icon.png" };

        foreach (var entry in archive.Entries)
        {
            if (requiredFiles.Contains(entry.Name))
            {
                string outputPath = Path.Combine(destination, entry.Name);
                entry.ExtractToFile(outputPath, overwrite: true);
            }
        }
    }

    /// <summary>
    /// Downloads AnyGlobe Changer from OSC or GitHub, depending on platform, as the latest OSC release doesn't work with Dolphin.
    /// </summary>
    public static void DownloadAGC()
    {
        if (MainClass.platformType != MainClass.Platform.Dolphin)
        {
            DownloadOSCApp("AnyGlobe_Changer");
            return;
        }
        
        if (!Directory.Exists(Path.Join("WiiLink", "apps", "AnyGlobe Changer")))
        {
            // Dolphin users need v1.0 of AnyGlobe Changer, as the latest OSC release doesn't work with Dolphin, for some reason.
            MainClass.task = $"Downloading AnyGlobe_Changer";
            string appPath = Path.Join(MainClass.tempDir, "AGC");
            Directory.CreateDirectory(appPath);
            DownloadFile($"https://github.com/fishguy6564/AnyGlobe-Changer/releases/download/1.0/AnyGlobe.Changer.zip", 
                Path.Join(appPath, "AGC.zip"), 
                "AnyGlobe_Changer");
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
    public static void DownloadFile(string URL, string dest, string name, bool noError = false)
    {
        MainClass.task = $"Downloading {name}";
        MainClass.curCmd = $"DownloadFile({URL}, {dest}, {name})";

        if (MainClass.DEBUG_MODE)
            AnsiConsole.MarkupLine($"[springgreen2_1]Downloading [bold]{name}[/] from [bold]{URL}[/] to [bold]{dest}[/][/]...");

        try
        {
            // Send a GET request to the specified URL.
            var response = MainClass.httpClient.GetAsync(URL).Result;
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
                MenuClass.ErrorScreen(statusCode, $"Failed to download [bold]{name}[/] from [bold]{URL}[/] to [bold]{dest}[/]");
            }
        }
        // Timeout exception
        catch (TaskCanceledException)
        {
            if (!noError)
            {
                AnsiConsole.MarkupLine($"[bold red]ERROR:[/] Failed to download [bold]{name}[/] from [bold]{URL}[/] to [bold]{dest}[/]: Request timed out (1 minute)");
                AnsiConsole.MarkupLine("Press any key to try again...");
                Console.ReadKey(true);
                DownloadFile(URL, dest, name);
            }
        }
        catch (HttpRequestException e)
        {
            if (!noError)
            {
                AnsiConsole.MarkupLine($"[bold red]ERROR:[/] {e.Message}");
                AnsiConsole.MarkupLine("Press any key to try again...");
                Console.ReadKey(true);
                DownloadFile(URL, dest, name);
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
    public static string DownloadNUS(string titleID, string outputDir, string? appVer = null, bool isWC24 = false)
    {
        MainClass.task = $"Downloading {titleID}";

        // Create a new NusClient instance to handle the download.
        var nus = new NusClient();

        // Create a list of store types to download.
        var store = new List<StoreType> { isWC24 ? StoreType.DecryptedContent : StoreType.WAD };

        // Check that the title ID is the correct length.
        if (titleID.Length != 16)
        {
            MenuClass.ErrorScreen(16, "Title ID must be 16 characters long");
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
            MenuClass.ErrorScreen(e.HResult, e.Message);
            return "";
        }
    }


    public static void UnpackWAD(string wadFilePath, string outputDir)
    {
        MainClass.task = $"Unpacking WAD";
        WAD wad = new();

        try
        {
            wad.LoadFile(wadFilePath);
            wad.Unpack(outputDir);
        }
        catch (Exception e)
        {
            MenuClass.ErrorScreen(e.HResult, e.Message);
        }
    }

    public static void PackWAD(string unpackPath, string outputWADDir)
    {
        MainClass.task = $"Packing WAD";
        WAD wad = new();

        try
        {
            wad.CreateNew(unpackPath);
            wad.Save(outputWADDir);
        }
        catch (Exception e)
        {
            MenuClass.ErrorScreen(e.HResult, e.Message);
        }
    }

    public static void DownloadPatch(string folderName, string patchInput, string patchOutput, string channelName)
    {
        string patchUrl = $"{MainClass.wiiLinkPatcherUrl}/{folderName.ToLower()}/{patchInput}";
        string patchDestinationPath = Path.Join(MainClass.tempDir, "Patches", folderName, patchOutput);

        if (MainClass.DEBUG_MODE)
        {
            AnsiConsole.MarkupLine($"[bold yellow]URL:[/] {patchUrl}");
            AnsiConsole.MarkupLine($"[bold yellow]Destination:[/] {patchDestinationPath}");
            AnsiConsole.MarkupLine("------- Press any key to continue -------");
            Console.ReadKey(true);
        }

        // If tempDir/Patches/{folderName} doesn't exist, make it
        if (!Directory.Exists(Path.Join(MainClass.tempDir, "Patches", folderName)))
            Directory.CreateDirectory(Path.Join(MainClass.tempDir, "Patches", folderName));

        DownloadFile(patchUrl, patchDestinationPath, channelName);
    }

    public static void ApplyPatch(FileStream original, FileStream patch, FileStream output)
    {
        try
        {
            // Create a new VCDiff decoder with the original, patch, and output files.
            using var decoder = new VCDiff.Decoders.VcDecoder(original, patch, output);
            decoder.Decode(out _);  // Decode the patch and write the result to the output file.
        }
        catch (Exception e)
        {
            MenuClass.ErrorScreen(e.HResult, e.Message);
        }
        finally
        {
            // Close all file streams.
            original.Close();
            patch.Close();
            output.Close();
        }
    }

    public static void DownloadSPD(MainClass.Platform platformType)
    {
        // Create WiiLink/WAD folders in current directory if they don't exist
        if (!Directory.Exists(Path.Join("WiiLink", "WAD")))
            Directory.CreateDirectory(Path.Join("WiiLink", "WAD"));

        string spdUrl = $"{MainClass.wiiLinkPatcherUrl}/spd/WiiLink_SPD.wad";
        string spdDestinationPath = Path.Join("WiiLink", "WAD", $"WiiLink Address Settings.wad");

        DownloadFile(spdUrl, spdDestinationPath, "SPD");
    }


    // Patches the Japanese-exclusive channels
    public static void PatchRegionalChannel(string channelName, string channelTitle, string titleID, List<KeyValuePair<string, string>> patchFilesDict, string? appVer = null, MainClass.Language? lang = null)
    {
        // Set up folder paths and file names
        string titleFolder = Path.Join(MainClass.tempDir, "Unpack");
        string tempFolder = Path.Join(MainClass.tempDir, "Unpack_Patched");
        string patchFolder = Path.Join(MainClass.tempDir, "Patches", channelName);
        string outputChannel = lang == null ? Path.Join("WiiLink", "WAD", $"{channelTitle}.wad") : Path.Join("WiiLink", "WAD", $"{channelTitle} [{lang}] (WiiLink).wad");
        string urlSubdir = channelName.ToLower();

        // Create unpack and unpack-patched folders
        Directory.CreateDirectory(titleFolder);
        Directory.CreateDirectory(tempFolder);

        // Download and extract the Wii channel files
        MainClass.task = $"Downloading and extracting files for {channelTitle}";
        appVer = DownloadNUS(titleID, titleFolder, appVer);
        string outputWad = Path.Join(titleFolder, $"{titleID}v{appVer}.wad");
        UnpackWAD(outputWad, titleFolder);

        // Download the patched TMD file and rename it to title_id.tmd
        MainClass.task = $"Downloading patched TMD file for {channelTitle}";
        DownloadFile($"{MainClass.wiiLinkPatcherUrl}/{urlSubdir}/{channelName}.tmd", Path.Join(titleFolder, $"{titleID}.tmd"), channelTitle);

        //// Apply delta patches to the app files ////
        MainClass.task = $"Applying delta patches for {channelTitle}";

        bool translated = lang == MainClass.Language.English || lang == MainClass.Language.Russian || lang == MainClass.Language.Catalan || lang == MainClass.Language.Portuguese || lang == MainClass.Language.French || lang == MainClass.Language.Italian || lang == MainClass.Language.German || lang == MainClass.Language.Dutch;

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
        MainClass.task = $"Copying patched files for {channelTitle}";
        MainClass.CopyFolder(tempFolder, titleFolder);

        // Repack the title with the patched files
        MainClass.task = $"Repacking the title for {channelTitle}";
        PackWAD(titleFolder, outputChannel);

        // Delete unpack and unpack_patched folders
        Directory.Delete(titleFolder, true);
        Directory.Delete(tempFolder, true);
    }

    // This function patches the WiiConnect24 channels
    public static void PatchWC24Channel(string channelName, string channelTitle, int channelVersion, MainClass.Region? channelRegion, string titleID, List<string> patchFile, List<string> appFile)
    {
        // Define the necessary paths and filenames
        string titleFolder = Path.Join(MainClass.tempDir, "Unpack");
        string tempFolder = Path.Join(MainClass.tempDir, "Unpack_Patched");
        string patchFolder = Path.Join(MainClass.tempDir, "Patches", channelName);

        // Name the output WAD file
        // Append the region to the output WAD name if it has a region
        string outputWad;
        if (channelName == "ktv" || channelRegion == null)
            outputWad = Path.Join("WiiLink", "WAD", $"{channelTitle} (WiiLink).wad");
        else if (channelName == "ws")
            outputWad = Path.Join("WiiLink", "WAD", $"{channelTitle} [{channelRegion}] (Wiimmfi).wad");
        else
            outputWad = Path.Join("WiiLink", "WAD", $"{channelTitle} [{channelRegion}] (WiiLink).wad");

        // Create unpack and unpack-patched folders
        Directory.CreateDirectory(titleFolder);
        Directory.CreateDirectory(tempFolder);

        string fileURL = $"{MainClass.wiiLinkPatcherUrl}/{channelName.ToLower()}/{titleID}";

        // Define the URLs and file paths
        var files = new Dictionary<string, string>
        {
            {".cert", Path.Join(titleFolder, $"{titleID}.cert")},
            {".tmd", Path.Join(titleFolder, $"tmd.{channelVersion}")},
            {".tik", Path.Join(titleFolder, "cetk")}
        };

        // Download the necessary files for the channel
        MainClass.task = $"Downloading necessary files for {channelTitle}";

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
        MainClass.task = $"Extracting stuff for {channelTitle}";
        DownloadNUS(titleID, titleFolder, channelVersion.ToString(), true);

        // Rename the extracted files
        MainClass.task = $"Renaming files for {channelTitle}";
        File.Move(Path.Join(titleFolder, $"tmd.{channelVersion}"), Path.Join(titleFolder, $"{titleID}.tmd"));
        File.Move(Path.Join(titleFolder, "cetk"), Path.Join(titleFolder, $"{titleID}.tik"));

        // Download the patched TMD file for Kirby TV Channel to make it region-free
        if (channelName == "ktv")
        {
            string tmdURL = $"{MainClass.wiiLinkPatcherUrl}/{channelName.ToLower()}/{titleID}.tmd";
            DownloadFile(tmdURL, Path.Join(titleFolder, $"{titleID}.tmd"), $"{channelTitle} .tmd");
        }

        // Apply the delta patches to the app file
        MainClass.task = $"Applying delta patch for {channelTitle}";
        foreach (var (app, patch) in appFile.Zip(patchFile, (app, patch) => (app, patch)))
        {
            ApplyPatch(File.OpenRead(Path.Join(titleFolder, $"{app}.app")), File.OpenRead(Path.Join(patchFolder, $"{patch}.delta")), File.OpenWrite(Path.Join(tempFolder, $"{app}.app")));
        }

        // Copy the patched files to the unpack folder
        MainClass.task = $"Copying patched files for {channelTitle}";
        try
        {
            MainClass.CopyFolder(tempFolder, titleFolder);
        }
        catch (Exception e)
        {
            MenuClass.ErrorScreen(e.HResult, e.Message);
        }

        // Delete the unpack_patched folder
        Directory.Delete(tempFolder, true);

        // Repack the title into a WAD file
        MainClass.task = $"Repacking the title for {channelTitle}";
        PackWAD(titleFolder, outputWad);

        // Delete the unpack and unpack_patched folders
        Directory.Delete(titleFolder, true);
    }

    // Downloads WC24 channel withouth patching (to get stock channel)
    public static void DownloadWC24Channel(string channelName, string channelTitle, int channelVersion, MainClass.Region? channelRegion, string titleID)
    {
        // Define the necessary paths and filenames
        string titleFolder = Path.Join(MainClass.tempDir, "Unpack");

        // Create WiiLink/WAD folder in current directory if they don't exist
        if (!Directory.Exists(Path.Join("WiiLink", "WAD")))
            Directory.CreateDirectory(Path.Join("WiiLink", "WAD"));

        // Name the output WAD file
        string outputWad;
        if (channelName == "ktv" || channelRegion == null)
            outputWad = Path.Join("WiiLink", "WAD", $"{channelTitle} (WiiLink).wad");
        else
            outputWad = Path.Join("WiiLink", "WAD", $"{channelTitle} [{channelRegion}] (WiiLink).wad");

        // Create unpack and unpack-patched folders
        Directory.CreateDirectory(titleFolder);

        // Extract the necessary files for the channel
        MainClass.task = $"Extracting stuff for {channelTitle}";
        DownloadNUS(titleID, titleFolder, channelVersion.ToString());

        // Rename the extracted files
        MainClass.task = $"Renaming files for {channelTitle}";

        // Move resulting WAD to output folder
        if (File.Exists(outputWad))
            File.Delete(outputWad);
        
        File.Move(Path.Join(titleFolder, $"{titleID}v{channelVersion}.wad"), outputWad);

        // Delete the unpack folder
        Directory.Delete(titleFolder, true);
    }

        public static void DownloadAllPatches()
    {
        MainClass.task = "Downloading patches";

        // Download SPD if English is selected
        if (MainClass.lang != MainClass.Language.Japan)
            DownloadSPD(MainClass.platformType);
        else
        {
            // Create "WiiLink/WAD" folder in current directory if they don't exist
            if (!Directory.Exists(Path.Join("WiiLink", "WAD")))
                Directory.CreateDirectory(Path.Join("WiiLink", "WAD"));
        }

        //// Downloading All Channel Patches ////

        if (MainClass.installRegionalChannels)
        {
            // Wii no Ma (Wii Room) //
            DownloadPatch("WiinoMa", $"WiinoMa_0_Universal.delta", $"WiinoMa_0_Universal.delta", "Wii no Ma");

            bool notRussianOrPortuguese = MainClass.wiiRoomLang != MainClass.Language.Russian && MainClass.wiiRoomLang != MainClass.Language.Portuguese;
            if (notRussianOrPortuguese)
                DownloadPatch("WiinoMa", $"WiinoMa_1_Universal.delta", $"WiinoMa_1_Universal.delta", "Wii no Ma");
            else
                DownloadPatch("WiinoMa", $"WiinoMa_1_{MainClass.wiiRoomLang}.delta", $"WiinoMa_1_{MainClass.wiiRoomLang}.delta", "Wii no Ma");

            // For all languages, including Japanese, use their respective patches for 2
            DownloadPatch("WiinoMa", $"WiinoMa_2_{MainClass.wiiRoomLang}.delta", $"WiinoMa_2_{MainClass.wiiRoomLang}.delta", "Wii no Ma");

            // Special handling for Portuguese, (patches 3, 4, and D)
            if (MainClass.wiiRoomLang == MainClass.Language.Portuguese)
            {
                DownloadPatch("WiinoMa", $"WiinoMa_3_{MainClass.Language.Portuguese}.delta", $"WiinoMa_3_{MainClass.Language.Portuguese}.delta", "Wii no Ma");
                DownloadPatch("WiinoMa", $"WiinoMa_4_{MainClass.Language.Portuguese}.delta", $"WiinoMa_4_{MainClass.Language.Portuguese}.delta", "Wii no Ma");
                DownloadPatch("WiinoMa", $"WiinoMa_D_{MainClass.Language.Portuguese}.delta", $"WiinoMa_D_{MainClass.Language.Portuguese}.delta", "Wii no Ma");
            }

            // Special handling for Russian, (patches 3, 4, 9, C, D and E)
            if (MainClass.wiiRoomLang == MainClass.Language.Russian)
            {
                DownloadPatch("WiinoMa", $"WiinoMa_3_{MainClass.Language.Russian}.delta", $"WiinoMa_3_{MainClass.Language.Russian}.delta", "Wii no Ma");
                DownloadPatch("WiinoMa", $"WiinoMa_4_{MainClass.Language.Russian}.delta", $"WiinoMa_4_{MainClass.Language.Russian}.delta", "Wii no Ma");
                DownloadPatch("WiinoMa", $"WiinoMa_9_{MainClass.Language.Russian}.delta", $"WiinoMa_9_{MainClass.Language.Russian}.delta", "Wii no Ma");
                DownloadPatch("WiinoMa", $"WiinoMa_C_{MainClass.Language.Russian}.delta", $"WiinoMa_C_{MainClass.Language.Russian}.delta", "Wii no Ma");
                DownloadPatch("WiinoMa", $"WiinoMa_D_{MainClass.Language.Russian}.delta", $"WiinoMa_D_{MainClass.Language.Russian}.delta", "Wii no Ma");
                DownloadPatch("WiinoMa", $"WiinoMa_E_{MainClass.Language.Russian}.delta", $"WiinoMa_E_{MainClass.Language.Russian}.delta", "Wii no Ma");
            }

            // Photo Prints Channel / Digicam Print Channel
            if (MainClass.lang == MainClass.Language.English)
                DownloadPatch("Digicam", $"Digicam_0_{MainClass.lang}.delta", $"Digicam_0_{MainClass.lang}.delta", "Digicam Print Channel");
            DownloadPatch("Digicam", $"Digicam_1_{MainClass.lang}.delta", $"Digicam_1_{MainClass.lang}.delta", "Digicam Print Channel");
            if (MainClass.lang == MainClass.Language.English)
                DownloadPatch("Digicam", $"Digicam_2_{MainClass.lang}.delta", $"Digicam_2_{MainClass.lang}.delta", "Digicam Print Channel");

            // Demae Channel
            switch (MainClass.demaeVersion)
            {
                case MainClass.DemaeVersion.Standard:
                    if (MainClass.lang == MainClass.Language.English)
                        DownloadPatch("Demae", $"Demae_0_{MainClass.lang}.delta", $"Demae_0_{MainClass.lang}.delta", "Food Channel (Standard)");
                    DownloadPatch("Demae", $"Demae_1_{MainClass.lang}.delta", $"Demae_1_{MainClass.lang}.delta", "Food Channel (Standard)");
                    if (MainClass.lang == MainClass.Language.English)
                        DownloadPatch("Demae", $"Demae_2_{MainClass.lang}.delta", $"Demae_2_{MainClass.lang}.delta", "Food Channel (Standard)");
                    break;
                case MainClass.DemaeVersion.Dominos:
                    DownloadPatch("Dominos", $"Dominos_0.delta", "Dominos_0.delta", "Food Channel (Dominos)");
                    DownloadPatch("Dominos", $"Dominos_1.delta", "Dominos_1.delta", "Food Channel (Dominos)");
                    DownloadPatch("Dominos", $"Dominos_2.delta", "Dominos_2.delta", "Food Channel (Dominos)");
                    DownloadOSCApp("wiilink-account-linker");
                    break;
            }

            // Kirby TV Channel
            DownloadPatch("ktv", $"ktv_2.delta", "KirbyTV_2.delta", "Kirby TV Channel");
        }

        if (MainClass.platformType != MainClass.Platform.Dolphin)
        {
            // Download yawmME from OSC for installing WADs on the Wii
            DownloadOSCApp("yawmME");
        }

        if (MainClass.platformType == MainClass.Platform.Wii)
        {
            // Download sntp from OSC for Syncing the Clock on the Wii
            DownloadOSCApp("sntp");
        }

        // Download WC24 patches
        // Nintendo Channel
        DownloadPatch("nc", $"NC_1_{MainClass.wc24_reg}.delta", $"NC_1_{MainClass.wc24_reg}.delta", "Nintendo Channel");

        // Forecast Channel
        DownloadPatch("forecast", $"Forecast_1.delta", "Forecast_1.delta", "Forecast Channel");
        DownloadPatch("forecast", $"Forecast_5.delta", "Forecast_5.delta", "Forecast Channel");

        // News Channel
        DownloadPatch("news", $"News_1.delta", $"News_1.delta", "News Channel");

        // Download AnyGlobe_Changer from OSC for use with the Forecast Channel
        DownloadAGC();

        // Everybody Votes Channel and Region Select Channel
        DownloadPatch("evc", $"EVC_1_{MainClass.wc24_reg}.delta", $"EVC_1_{MainClass.wc24_reg}.delta", "Everybody Votes Channel");
        DownloadPatch("RegSel", $"RegSel_1.delta", "RegSel_1.delta", "Region Select");

        // Check Mii Out/Mii Contest Channel
        DownloadPatch("cmoc", $"CMOC_1_{MainClass.wc24_reg}.delta", $"CMOC_1_{MainClass.wc24_reg}.delta", "Check Mii Out Channel");

        // Download ww-43db-patcher for vWii if applicable
        if (MainClass.platformType == MainClass.Platform.vWii)
        {
            // DownloadOSCApp("ww-43db-patcher");

            // Also download EULA for vWii users
            string EULATitleID = MainClass.wc24_reg switch
            {
                MainClass.Region.USA => "0001000848414b45",
                MainClass.Region.PAL => "0001000848414b50",
                MainClass.Region.Japan => "0001000848414b4a",
                _ => throw new NotImplementedException()
            };

            DownloadWC24Channel("EULA", "EULA", 3, MainClass.wc24_reg, EULATitleID);

        }

        // Install the WiiLink Mail Patcher
        DownloadOSCApp("Mail-Patcher");

        // Downloading stuff is finished!
        MainClass.patchingProgress_express["downloading"] = "done";
        MainClass.patchingProgress_express["nc"] = "in_progress";
    }

        // Download respective patches for selected core and WiiConnect24 channels (and SPD if English is selected for WiiLink channels)
    public static void DownloadCustomPatches(List<string> channelSelection)
    {
        MainClass.task = "Downloading selected patches";

        // Download SPD if any of the following channels are selected
        if (MainClass.wiiLinkChannels_selection.Any(channel => channel.Contains("food_us") || channel.Contains("food_eu") || channel.Contains("food_dominos") || channel.Contains("digicam_en") || channel.Contains("wiiroom_en") || channel.Contains("wiiroom_es") || channel.Contains("wiiroom_fr") || channel.Contains("wiiroom_de") || channel.Contains("wiiroom_it") || channel.Contains("wiiroom_du") || channel.Contains("wiiroom_ptbr") || channel.Contains("wiiroom_ru")))
            DownloadSPD(MainClass.platformType_custom);
        else
            // Create WiiLink/WAD folders in current directory if they don't exist
            if (!Directory.Exists(Path.Join("WiiLink", "WAD")))
                Directory.CreateDirectory(Path.Join("WiiLink", "WAD"));

        // Download ww-43db-patcher for vWii if applicable
        if (MainClass.platformType_custom == MainClass.Platform.vWii)
        {
            // DownloadOSCApp("ww-43db-patcher");

            // Download the below if any WiiConnect24 channels are selected
            if (MainClass.wiiConnect24Channels_selection.Any())
            {
                // Create a dictionary mapping EULA title IDs to their respective regions
                Dictionary<string, MainClass.Region> EULATitleIDs = new()
                {
                    { "0001000848414b45", MainClass.Region.USA },
                    { "0001000848414b50", MainClass.Region.PAL },
                    { "0001000848414b4a", MainClass.Region.Japan }
                };

                // Iterate over the dictionary
                foreach ((string titleID, MainClass.Region region) in EULATitleIDs)
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
                    MainClass.task = "Downloading Wii Room (English)";
                    DownloadPatch("WiinoMa", $"WiinoMa_0_Universal.delta", "WiinoMa_0_Universal.delta", "Wii Room");
                    DownloadPatch("WiinoMa", $"WiinoMa_1_Universal.delta", "WiinoMa_1_Universal.delta", "Wii Room");
                    DownloadPatch("WiinoMa", $"WiinoMa_2_English.delta", "WiinoMa_2_English.delta", "Wii Room");
                    break;
                case "wiiroom_es":
                    MainClass.task = "Downloading Wii Room (Español)";
                    DownloadPatch("WiinoMa", $"WiinoMa_0_Universal.delta", "WiinoMa_0_Universal.delta", "Wii Room");
                    DownloadPatch("WiinoMa", $"WiinoMa_1_Universal.delta", "WiinoMa_1_Universal.delta", "Wii Room");
                    DownloadPatch("WiinoMa", $"WiinoMa_2_Spanish.delta", "WiinoMa_2_Spanish.delta", "Wii Room");
                    break;
                case "wiiroom_fr":
                    MainClass.task = "Downloading Wii Room (Français)";
                    DownloadPatch("WiinoMa", $"WiinoMa_0_Universal.delta", "WiinoMa_0_Universal.delta", "Wii Room");
                    DownloadPatch("WiinoMa", $"WiinoMa_1_Universal.delta", "WiinoMa_1_Universal.delta", "Wii Room");
                    DownloadPatch("WiinoMa", $"WiinoMa_2_French.delta", "WiinoMa_2_French.delta", "Wii Room");
                    break;
                case "wiiroom_de":
                    MainClass.task = "Downloading Wii Room (Deutsch)";
                    DownloadPatch("WiinoMa", $"WiinoMa_0_Universal.delta", "WiinoMa_0_Universal.delta", "Wii Room");
                    DownloadPatch("WiinoMa", $"WiinoMa_1_Universal.delta", "WiinoMa_1_Universal.delta", "Wii Room");
                    DownloadPatch("WiinoMa", $"WiinoMa_2_German.delta", "WiinoMa_2_German.delta", "Wii Room");
                    break;
                case "wiiroom_it":
                    MainClass.task = "Downloading Wii Room (Italiano)";
                    DownloadPatch("WiinoMa", $"WiinoMa_0_Universal.delta", "WiinoMa_0_Universal.delta", "Wii Room");
                    DownloadPatch("WiinoMa", $"WiinoMa_1_Universal.delta", "WiinoMa_1_Universal.delta", "Wii Room");
                    DownloadPatch("WiinoMa", $"WiinoMa_2_Italian.delta", "WiinoMa_2_Italian.delta", "Wii Room");
                    break;
                case "wiiroom_du":
                    MainClass.task = "Downloading Wii Room (Nederlands)";
                    DownloadPatch("WiinoMa", $"WiinoMa_0_Universal.delta", "WiinoMa_0_Universal.delta", "Wii Room");
                    DownloadPatch("WiinoMa", $"WiinoMa_1_Universal.delta", "WiinoMa_1_Universal.delta", "Wii Room");
                    DownloadPatch("WiinoMa", $"WiinoMa_2_Dutch.delta", "WiinoMa_2_Dutch.delta", "Wii Room");
                    break;
                case "wiiroom_ptbr":
                    MainClass.task = "Downloading Wii Room (Português-Brasil)";
                    DownloadPatch("WiinoMa", $"WiinoMa_0_Universal.delta", "WiinoMa_0_Universal.delta", "Wii Room");
                    DownloadPatch("WiinoMa", $"WiinoMa_1_Portuguese.delta", "WiinoMa_1_Portuguese.delta", "Wii Room");
                    DownloadPatch("WiinoMa", $"WiinoMa_2_Portuguese.delta", "WiinoMa_2_Portuguese.delta", "Wii Room");
                    DownloadPatch("WiinoMa", $"WiinoMa_3_Portuguese.delta", "WiinoMa_3_Portuguese.delta", "Wii Room");
                    DownloadPatch("WiinoMa", $"WiinoMa_4_Portuguese.delta", "WiinoMa_4_Portuguese.delta", "Wii Room");
                    DownloadPatch("WiinoMa", $"WiinoMa_D_Portuguese.delta", "WiinoMa_D_Portuguese.delta", "Wii Room");
                    break;
                case "wiiroom_ru":
                    MainClass.task = "Downloading Wii Room (Русский)";
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
                    MainClass.task = "Downloading Wii no Ma (Japan)";
                    DownloadPatch("WiinoMa", $"WiinoMa_0_Universal.delta", "WiinoMa_0_Universal.delta", "Wii no Ma");
                    DownloadPatch("WiinoMa", $"WiinoMa_1_Universal.delta", "WiinoMa_1_Universal.delta", "Wii no Ma");
                    DownloadPatch("WiinoMa", $"WiinoMa_2_Japan.delta", "WiinoMa_2_Japan.delta", "Wii no Ma");
                    break;
                case "digicam_en":
                    MainClass.task = "Downloading Photo Prints Channel (English)";
                    DownloadPatch("Digicam", $"Digicam_0_English.delta", "Digicam_0_English.delta", "Photo Prints Channel");
                    DownloadPatch("Digicam", $"Digicam_1_English.delta", "Digicam_1_English.delta", "Photo Prints Channel");
                    DownloadPatch("Digicam", $"Digicam_2_English.delta", "Digicam_2_English.delta", "Photo Prints Channel");
                    break;
                case "digicam_jp":
                    MainClass.task = "Downloading Digicam Print Channel (Japan)";
                    DownloadPatch("Digicam", $"Digicam_1_Japan.delta", "Digicam_1_Japan.delta", "Digicam Print Channel");
                    break;
                case "food_us":
                    MainClass.task = "Downloading Food Channel (English)";
                    DownloadPatch("Demae", $"Demae_0_English.delta", "Demae_0_English.delta", "Food Channel (Standard)");
                    DownloadPatch("Demae", $"Demae_1_English.delta", "Demae_1_English.delta", "Food Channel (Standard)");
                    DownloadPatch("Demae", $"Demae_2_English.delta", "Demae_2_English.delta", "Food Channel (Standard)");
                    break;
                case "food_eu":
                    MainClass.task = "Downloading Food Channel (English)";
                    DownloadPatch("Demae", $"Demae_0_English.delta", "Demae_0_English.delta", "Food Channel (Standard)");
                    DownloadPatch("Demae", $"Demae_1_English.delta", "Demae_1_English.delta", "Food Channel (Standard)");
                    DownloadPatch("Demae", $"Demae_2_English.delta", "Demae_2_English.delta", "Food Channel (Standard)");
                    break;
                case "demae_jp":
                    MainClass.task = "Downloading Demae Channel (Japan)";
                    DownloadPatch("Demae", $"Demae_1_Japan.delta", "Demae_1_Japan.delta", "Demae Channel");
                    break;
                case "food_dominos":
                    MainClass.task = "Downloading Food Channel (Domino's)";
                    DownloadPatch("Dominos", $"Dominos_0.delta", "Dominos_0.delta", "Food Channel (Domino's)");
                    DownloadPatch("Dominos", $"Dominos_1.delta", "Dominos_1.delta", "Food Channel (Domino's)");
                    DownloadPatch("Dominos", $"Dominos_2.delta", "Dominos_2.delta", "Food Channel (Domino's)");
                    DownloadOSCApp("wiilink-account-linker");
                    break;
                case "nc_us":
                    MainClass.task = "Downloading Nintendo Channel (USA)";
                    DownloadPatch("nc", $"NC_1_USA.delta", "NC_1_USA.delta", "Nintendo Channel");
                    break;
                case "mnnc_jp":
                    MainClass.task = "Downloading Nintendo Channel (Japan)";
                    DownloadPatch("nc", $"NC_1_Japan.delta", "NC_1_Japan.delta", "Nintendo Channel");
                    break;
                case "nc_eu":
                    MainClass.task = "Downloading Nintendo Channel (Europe)";
                    DownloadPatch("nc", $"NC_1_PAL.delta", "NC_1_PAL.delta", "Nintendo Channel");
                    break;
                case "forecast_us": // Forecast Patch works for all regions now
                case "forecast_jp":
                case "forecast_eu":
                    MainClass.task = "Downloading Forecast Channel";
                    DownloadPatch("forecast", $"Forecast_1.delta", "Forecast_1.delta", "Forecast Channel");
                    DownloadPatch("forecast", $"Forecast_5.delta", "Forecast_5.delta", "Forecast Channel");
                    DownloadAGC(); // Download AnyGlobe_Changer from OSC for use with the Forecast Channel
                    break;
                case "news_us":
                case "news_eu":
                case "news_jp":
                    MainClass.task = "Downloading News Channel";
                    DownloadPatch("news", $"News_1.delta", $"News_1.delta", "News Channel");
                    break;
                case "evc_us":
                    MainClass.task = $"Downloading Everybody Votes Channel (USA)";
                    DownloadPatch("evc", $"EVC_1_USA.delta", "EVC_1_USA.delta", "Everybody Votes Channel");
                    DownloadPatch("RegSel", "RegSel_1.delta", "RegSel_1.delta", "Region Select");
                    break;
                case "evc_eu":
                    MainClass.task = $"Downloading Everybody Votes Channel (PAL)";
                    DownloadPatch("evc", $"EVC_1_PAL.delta", "EVC_1_PAL.delta", "Everybody Votes Channel");
                    DownloadPatch("RegSel", "RegSel_1.delta", "RegSel_1.delta", "Region Select");
                    break;
                case "evc_jp":
                    MainClass.task = $"Downloading Everybody Votes Channel (Japan)";
                    DownloadPatch("evc", $"EVC_1_Japan.delta", "EVC_1_Japan.delta", "Everybody Votes Channel");
                    DownloadPatch("RegSel", "RegSel_1.delta", "RegSel_1.delta", "Region Select");
                    break;
                case "cmoc_us":
                    MainClass.task = $"Downloading Check Mii Out Channel (USA)";
                    DownloadPatch("cmoc", $"CMOC_1_USA.delta", "CMOC_1_USA.delta", "Check Mii Out Channel");
                    break;
                case "cmoc_eu":
                    MainClass.task = $"Downloading Mii Contest Channel (Europe)";
                    DownloadPatch("cmoc", $"CMOC_1_PAL.delta", "CMOC_1_PAL.delta", "Mii Contest Channel");
                    break;
                case "cmoc_jp":
                    MainClass.task = $"Downloading Mii Contest Channel (Japan)";
                    DownloadPatch("cmoc", $"CMOC_1_Japan.delta", "CMOC_1_Japan.delta", "Mii Contest Channel");
                    break;
                case "kirbytv":
                    MainClass.task = "Downloading Kirby TV Channel";
                    DownloadPatch("ktv", $"ktv_2.delta", "KirbyTV_2.delta", "Kirby TV Channel");
                    break;
                case "ws_us":
                    MainClass.task = $"Downloading Wii Speak Channel (USA)";
                    DownloadPatch("ws", $"WS_0_USA.delta", "WS_0_USA.delta", "Wii Speak Channel");
                    DownloadPatch("ws", $"WS_1_USA.delta", "WS_1_USA.delta", "Wii Speak Channel");
                    break;
                case "ws_eu":
                    MainClass.task = $"Downloading Wii Speak Channel (Europe)";
                    DownloadPatch("ws", $"WS_0_PAL.delta", "WS_0_PAL.delta", "Wii Speak Channel");
                    DownloadPatch("ws", $"WS_1_PAL.delta", "WS_1_PAL.delta", "Wii Speak Channel");
                    break;
                case "ws_jp":
                    MainClass.task = $"Downloading Wii Speak Channel (Japan)";
                    DownloadPatch("ws", $"WS_0_Japan.delta", "WS_0_Japan.delta", "Wii Speak Channel");
                    DownloadPatch("ws", $"WS_1_Japan.delta", "WS_1_Japan.delta", "Wii Speak Channel");
                    break;
            }
        }

        if (MainClass.platformType_custom != MainClass.Platform.Dolphin)
        {
            // Downloading yawmME from OSC
            DownloadOSCApp("yawmME");
            // Downloading sntp from OSC
            DownloadOSCApp("sntp");
        }            

        // Install the WiiLink Mail Patcher
        DownloadOSCApp("Mail-Patcher");
    }

    // Patching Wii no Ma
    public static void WiiRoom_Patch(MainClass.Language language)
    {
        MainClass.task = "Patching Wii no Ma";

        // Patches 00 and 01 are universal (except 01 for Russian and Portuguese), 02 has language-specific patches
        // Russian has patches 01, 02, 03, 04, 09, 0C, 0D, and 0E
        // Portuguese has patches 01, 02, 03, 04, and 0D

        bool notRussianOrPortuguese = language != MainClass.Language.Russian && language != MainClass.Language.Portuguese;

        // Generate patch list for Wii Room
        var wiiRoomPatchList = new List<KeyValuePair<string, string>>
        {
            new("WiinoMa_0_Universal", "00000000"),
            new($"WiinoMa_1_{(notRussianOrPortuguese ? "Universal" : language)}", "00000001"),
            new($"WiinoMa_2_{language}", "00000002")
        };

        switch (language)
        {
            case MainClass.Language.Russian:
                wiiRoomPatchList.AddRange(
                [
                    new KeyValuePair<string, string>("WiinoMa_3_Russian", "00000003"),
                    new KeyValuePair<string, string>("WiinoMa_4_Russian", "00000004"),
                    new KeyValuePair<string, string>("WiinoMa_9_Russian", "00000009"),
                    new KeyValuePair<string, string>("WiinoMa_C_Russian", "0000000c"),
                    new KeyValuePair<string, string>("WiinoMa_D_Russian", "0000000d"),
                    new KeyValuePair<string, string>("WiinoMa_E_Russian", "0000000e")
                ]);
                break;
            case MainClass.Language.Portuguese:
                wiiRoomPatchList.AddRange(
                [
                    new KeyValuePair<string, string>("WiinoMa_3_Portuguese", "00000003"),
                    new KeyValuePair<string, string>("WiinoMa_4_Portuguese", "00000004"),
                    new KeyValuePair<string, string>("WiinoMa_D_Portuguese", "0000000d")
                ]);
                break;
        }

        // Name the channel based on the language chosen
        string channelTitle = language switch
        {
            MainClass.Language.Japan => "Wii no Ma",
            _ => "Wii Room"
        };

        PatchRegionalChannel("WiinoMa", channelTitle, "000100014843494a", wiiRoomPatchList, lang: language);

        // Finished patching Wii no Ma
        MainClass.patchingProgress_express["wiiroom"] = "done";
        MainClass.patchingProgress_express["digicam"] = "in_progress";
    }

    // Patching Digicam Print Channel
    public static void Digicam_Patch(MainClass.Language language)
    {
        MainClass.task = "Patching Digicam Print Channel";

        // Dictionary for which files to patch
        var digicamPatchList = new List<KeyValuePair<string, string>>()
        {
            new($"Digicam_0_{language}", "00000000"),
            new($"Digicam_1_{language}", "00000001"),
            new($"Digicam_2_{language}", "00000002")
        };

        string channelTitle = language switch
        {
            MainClass.Language.English => "Photo Prints Channel",
            _ => "Digicam Print Channel"
        };

        PatchRegionalChannel("Digicam", channelTitle, "000100014843444a", digicamPatchList, lang: language);

        // Finished patching Digicam Print Channel
        MainClass.patchingProgress_express["digicam"] = "done";
        MainClass.patchingProgress_express["demae"] = "in_progress";
    }

    // Patching Demae Channel
    public static void Demae_Patch(MainClass.Language language, MainClass.DemaeVersion demaeVersion, MainClass.Region region)
    {
        // Assign channel title based on language chosen
        string channelTitle = language switch
        {
            MainClass.Language.English => "Food Channel",
            _ => "Demae Channel"
        };

        MainClass.task = $"Patching {channelTitle}";

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
        var demaeData = new Dictionary<MainClass.DemaeVersion, (List<KeyValuePair<string, string>>, string)>
        {
            [MainClass.DemaeVersion.Standard] = (GeneratePatchList("Demae", true), "Demae"),
            [MainClass.DemaeVersion.Dominos] = (GeneratePatchList("Dominos", false), "Dominos")
        };

        // Get patch list and folder name for the current version
        var (demaePatchList, folderName) = demaeData[demaeVersion];

        PatchRegionalChannel(folderName, $"{channelTitle} ({demaeVersion})", "000100014843484a", demaePatchList, lang: language);

        if (demaeVersion == MainClass.DemaeVersion.Dominos)
        {
            string channelID = region switch
            {
                MainClass.Region.USA => "0001000148414445",
                MainClass.Region.PAL => "0001000148414450",
                MainClass.Region.Japan => "000100014841444a",
                _ => throw new NotImplementedException(),
            };
            DownloadWC24Channel("ic", "Internet Channel", 1024, region, channelID);
        }

        // Finished patching Demae Channel
        MainClass.patchingProgress_express["demae"] = "done";
        MainClass.patchingProgress_express["kirbytv"] = "in_progress";
    }

    // Patching Kirby TV Channel (if applicable)
    public static void KirbyTV_Patch()
    {
        MainClass.task = "Patching Kirby TV Channel";

        List<string> patches = ["KirbyTV_2"];
        List<string> appNums = ["0000000e"];

        PatchWC24Channel("ktv", $"Kirby TV Channel", 257, null, "0001000148434d50", patches, appNums);

        // Finished patching Kirby TV Channel
        MainClass.patchingProgress_express["kirbytv"] = "done";
        MainClass.patchingProgress_express["finishing"] = "in_progress";
    }


    // Patching Nintendo Channel
    public static void NC_Patch(MainClass.Region region)
    {
        MainClass.task = "Patching Nintendo Channel";

        // Define a dictionary to map Region to channelID, appNum, and channel_title
        Dictionary<MainClass.Region, (string channelID, string appNum, string channel_title)> regionData = new()
        {
            { MainClass.Region.USA, ("0001000148415445", "0000002c", "Nintendo Channel") },
            { MainClass.Region.PAL, ("0001000148415450", "0000002d", "Nintendo Channel") },
            { MainClass.Region.Japan, ("000100014841544a", "0000003e", "Minna no Nintendo Channel") },
        };

        // Get the data for the current region
        var (channelID, appNum, channel_title) = regionData[region];

        List<string> patches = [$"NC_1_{region}"];
        List<string> appNums = [appNum];

        PatchWC24Channel("nc", $"{channel_title}", 1792, region, channelID, patches, appNums);

        // Finished patching Nintendo Channel
        MainClass.patchingProgress_express["nc"] = "done";
        MainClass.patchingProgress_express["forecast"] = "in_progress";
    }

    // Patching Forecast Channel
    public static void Forecast_Patch(MainClass.Region region)
    {
        MainClass.task = "Patching Forecast Channel";

        // Properly set Forecast Channel titleID
        string channelID = region switch
        {
            MainClass.Region.USA => "0001000248414645",
            MainClass.Region.PAL => "0001000248414650",
            MainClass.Region.Japan => "000100024841464a",
            _ => throw new NotImplementedException(),
        };

        List<string> patches = ["Forecast_1", "Forecast_5"];
        List<string> appNums = ["0000000d", "0000000f"];

        PatchWC24Channel("forecast", $"Forecast Channel", 7, region, channelID, patches, appNums);

        // Finished patching Forecast Channel
        MainClass.patchingProgress_express["forecast"] = "done";
        MainClass.patchingProgress_express["news"] = "in_progress";
    }

    // Patching News Channel
    public static void News_Patch(MainClass.Region region)
    {
        MainClass.task = "Patching News Channel";

        // Properly set News Channel titleID
        string channelID = region switch
        {
            MainClass.Region.USA => "0001000248414745",
            MainClass.Region.PAL => "0001000248414750",
            MainClass.Region.Japan => "000100024841474a",
            _ => throw new NotImplementedException(),
        };

        List<string> patches = ["News_1"];
        List<string> appNums = ["0000000b"];

        PatchWC24Channel("news", $"News Channel", 7, region, channelID, patches, appNums);

        // Finished patching News Channel
        MainClass.patchingProgress_express["news"] = "done";
        MainClass.patchingProgress_express["evc"] = "in_progress";
    }

    // Patching Everybody Votes Channel
    public static void EVC_Patch(MainClass.Region region)
    {

        //// Patching Everybody Votes Channel
        MainClass.task = "Patching Everybody Votes Channel";


        // Properly set Everybody Votes Channel titleID and appNum based on region
        string channelID = region switch
        {
            MainClass.Region.USA => "0001000148414a45",
            MainClass.Region.PAL => "0001000148414a50",
            MainClass.Region.Japan => "0001000148414a4a",
            _ => throw new NotImplementedException(),
        };
        string appNum = region switch
        {
            MainClass.Region.Japan => "00000018",
            _ => "00000019",
        };
        
        List<string> patches = [$"EVC_1_{region}"];
        List<string> appNums = new List<string> { appNum };

        PatchWC24Channel("evc", $"Everybody Votes Channel", 512, region, channelID, patches, appNums);

        //// Patching Region Select for Everybody Votes Channel
        RegSel_Patch(region);

        // Finished patching Everybody Votes Channel
        MainClass.patchingProgress_express["evc"] = "done";
        MainClass.patchingProgress_express["cmoc"] = "in_progress";
    }

    // Patching Check Mii Out Channel
    public static void CheckMiiOut_Patch(MainClass.Region region)
    {
        MainClass.task = "Patching Check Mii Out Channel";

        // Properly set Check Mii Out Channel titleID based on region
        string channelID = region switch
        {
            MainClass.Region.USA => "0001000148415045",
            MainClass.Region.PAL => "0001000148415050",
            MainClass.Region.Japan => "000100014841504a",
            _ => throw new NotImplementedException(),
        };

        // Set Check Mii Out Channel title based on region
        string channelTitle = region switch
        {
            MainClass.Region.USA => "Check Mii Out Channel",
            _ => "Mii Contest Channel",
        };

        List<string> patches = [$"CMOC_1_{region}"];
        List<string> appNums = ["0000000c"];

        PatchWC24Channel("cmoc", $"{channelTitle}", 512, region, channelID, patches, appNums);

        // Finished patching Check Mii Out Channel
        MainClass.patchingProgress_express["cmoc"] = "done";
        MainClass.patchingProgress_express["wiiroom"] = "in_progress";
    }

    // Patching Region Select
    public static void RegSel_Patch(MainClass.Region regSel_reg)
    {
        MainClass.task = "Patching Region Select";

        // Properly set Region Select titleID based on region
        string channelID = regSel_reg switch
        {
            MainClass.Region.USA => "0001000848414c45",
            MainClass.Region.PAL => "0001000848414c50",
            MainClass.Region.Japan => "0001000848414c4a",
            _ => throw new NotImplementedException(),
        };

        List<string> patches = ["RegSel_1"];
        List<string> appNums = ["00000009"];

        PatchWC24Channel("RegSel", $"Region Select", 2, regSel_reg, channelID, patches, appNums);
    }

    // Downloading Today and Tomorrow Channel
    public static void TodayTomorrow_Download(MainClass.Region todayTomorrow_reg)
    {
        MainClass.task = "Downloading Today and Tomorrow Channel";
        
        switch(todayTomorrow_reg)
        {
            case MainClass.Region.PAL:
                string titleFolder = Path.Join(MainClass.tempDir, "Unpack");
                string outputWad = Path.Join("WiiLink", "WAD", "Today and Tomorrow Channel [Europe] (WiiLink).wad");
                
                // Create WiiLink/WAD folder in current directory if they don't exist
                if (!Directory.Exists(Path.Join("WiiLink", "WAD")))
                    Directory.CreateDirectory(Path.Join("WiiLink", "WAD"));

                Directory.CreateDirectory(titleFolder);
    
                string baseId = "0001000148415650";
                string fileURL = $"{MainClass.wiiLinkPatcherUrl}/tatc/{baseId}";
                Dictionary<string, string> files = new()
                {
                    [".cert"] = Path.Join(titleFolder, $"{baseId}.cert"),
                    [".tmd"] = Path.Join(titleFolder, "tmd.512"),
                    [".tik"] = Path.Join(titleFolder, "cetk")
                };
    
                MainClass.task = "Downloading necessary files for Today and Tomorrow Channel";
                Parallel.ForEach(files, file =>
                {
                    try
                    {
                        DownloadFile($"{fileURL}{file.Key}", file.Value, 
                            $"Today and Tomorrow Channel {file.Key}", noError: true);
                    }
                    catch { } // File doesn't exist, move on
                });
    
                MainClass.task = "Extracting stuff for Today and Tomorrow Channel";
                DownloadNUS(baseId, titleFolder, "512", true);
    
                MainClass.task = "Renaming files for Today and Tomorrow Channel";
                File.Move(Path.Join(titleFolder, "tmd.512"), Path.Join(titleFolder, $"{baseId}.tmd"));
                File.Move(Path.Join(titleFolder, "cetk"), Path.Join(titleFolder, $"{baseId}.tik"));
    
                MainClass.task = "Repacking the title for Today and Tomorrow Channel";
                PackWAD(titleFolder, outputWad);
                
                Directory.Delete(titleFolder, true);
                break;
            case MainClass.Region.Japan:
                DownloadWC24Channel("tatc", "Today and Tomorrow Channel", 512, todayTomorrow_reg, "000100014841564a");
                break;
            default:
                throw new NotImplementedException();
        }
    }

    // Patching Wii Speak
    public static void WiiSpeak_Patch(MainClass.Region region)
    {
        MainClass.task = "Patching Wii Speak Channel";

        // Properly set Wii Speak Channel titleID based on region
        string channelID = region switch
        {
            MainClass.Region.USA => "0001000148434645",
            MainClass.Region.PAL => "0001000148434650",
            MainClass.Region.Japan => "000100014843464a",
            _ => throw new NotImplementedException(),
        };

        List<string> patches = [$"WS_0_{region}",$"WS_1_{region}"];
        List<string>appNums = region switch
        {
            MainClass.Region.USA => ["00000012","00000013"],
            MainClass.Region.PAL => ["00000009","0000000a"],
            MainClass.Region.Japan => ["00000014","00000012"],
            _ => throw new NotImplementedException(),
        };

        PatchWC24Channel("ws", "Wii Speak Channel", 512, region, channelID, patches, appNums);
    }

    // Downloading Photo Channel 1.1
    public static void PhotoChannel_Download()
    {
        MainClass.task = "Downloading Photo Channel 1.1";
        
        string titleFolder = Path.Join(MainClass.tempDir, "Unpack");
        string outputWad = Path.Join("WiiLink", "WAD", "Photo Channel 1.1 (WiiLink).wad");
                
        // Create WiiLink/WAD folder in current directory if they don't exist
        if (!Directory.Exists(Path.Join("WiiLink", "WAD")))
            Directory.CreateDirectory(Path.Join("WiiLink", "WAD"));
        
        Directory.CreateDirectory(titleFolder);
    
        string baseId = "0001000248415941";
        string fileURL = $"{MainClass.wiiLinkPatcherUrl}/pc/{baseId}";
    
        MainClass.task = "Extracting stuff for Photo Channel 1.1";
        DownloadNUS(baseId, titleFolder, "3", true);

        Dictionary<string, string> files = new()
        {
            [".cert"] = Path.Join(titleFolder, $"{baseId}.cert"),
            [".tmd"] = Path.Join(titleFolder, "tmd.3"),
            [".tik"] = Path.Join(titleFolder, "cetk")
        };
    
        MainClass.task = "Downloading necessary files for Photo Channel 1.1";
        Parallel.ForEach(files, file =>
        {
            try
            {
                DownloadFile($"{fileURL}{file.Key}", file.Value, 
                    $"Photo Channel 1.1 {file.Key}", noError: true);
            }
            catch { } // File doesn't exist, move on
        });
    
        MainClass.task = "Renaming files for Photo Channel 1.1";
        File.Move(Path.Join(titleFolder, "tmd.3"), Path.Join(titleFolder, $"{baseId}.tmd"));
        File.Move(Path.Join(titleFolder, "cetk"), Path.Join(titleFolder, $"{baseId}.tik"));
    
        MainClass.task = "Repacking the title for Photo Channel 1.1";
        PackWAD(titleFolder, outputWad);
                
        Directory.Delete(titleFolder, true);
    }
}

[JsonSerializable(typeof(List<GitHubRelease>))]
internal partial class GitHubJsonContext : JsonSerializerContext {}

public class GitHubRelease
{
    public List<GitHubAsset> assets { get; set; } = new();
}

public class GitHubAsset
{
    public string browser_download_url { get; set; } = "";
}
