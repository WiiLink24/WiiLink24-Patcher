using System.Diagnostics;
using System.Text;
using System.Runtime.InteropServices;
using Spectre.Console;

class WiiLink_Patcher
{
    /*###### Build Info ######*/
    static readonly string version = "1.1.0";
    static readonly string copyrightYear = "2023";
    static readonly string lastBuild = "March 20th, 2023";
    static readonly string at = "2:12 PM";
    static string? sdcard = DetectSDCard();

    static readonly string wiiLinkPatcherUrl = "https://patcher.wiilink24.com";
    static readonly string PabloURL = "http://pabloscorner.akawah.net/WL24-Patcher";
    /*########################*/

    /*###### Setup Info ######*/

    // Core Channel variables
    static public string reg = "";
    static public string lang = "";
    static public string demae_version = "";

    // WiiConnect24 variables
    static public string nc_reg = "";
    static public string forecast_reg = "";

    // Misc. setup variables
    static public string platformType = "";
    static public bool sd_connected = false;

    static public string task = "";
    static public string curCmd = "";
    static public int exitCode = -1;
    static readonly string curDir = Directory.GetCurrentDirectory();
    static string[] patching_progress = new string[7];

    // Get current console window size
    static int console_width = 0;
    static int console_height = 0;

    static void PrintHeader()
    {
        // Clear console
        Console.Clear();

        string borderChar = "=";
        string borderLine = "";
        int columns = Console.WindowWidth;

        AnsiConsole.MarkupLine($"[bold]WiiLink Patcher v{version} - (c) {copyrightYear} WiiLink[/] (Updated on {lastBuild} at {at} EST)");

        for (int i = 0; i < columns; i++)
        {
            borderLine += borderChar;
        }

        AnsiConsole.WriteLine(borderLine);
        AnsiConsole.WriteLine();
    }

    // GitHub Announcement
    static void PrintAnnouncement()
    {
        string markupTitle = "[bold green]( Announcement )[/]";
        var markupText = new Markup("[bold]If you have any issues with the patcher or services offered by WiiLink, please report them here:[/]\n[link bold green]https://discord.gg/WiiLink[/] - Thank you.");

        var panel = new Panel(markupText)
        {
            Header = new PanelHeader(markupTitle, Justify.Center),
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(Color.Green),
            Expand = true,
        };
        AnsiConsole.Write(panel);
        Console.WriteLine();
    }

    static string? DetectSDCard()
    {
        // Check for drive on Windows, and check mounted drives on Linux/MacOS
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            DriveInfo[] drives = DriveInfo.GetDrives();
            foreach (DriveInfo drive in drives)
            {
                if (drive.DriveType == DriveType.Removable && Directory.Exists(Path.Combine(drive.RootDirectory.FullName, "apps")))
                {
                    return drive.RootDirectory.FullName;
                }
            }
            return null;
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            string[] mountedDrives = Directory.GetDirectories($"/media/{Environment.UserName}");
            foreach (string drive in mountedDrives)
            {
                if (Directory.Exists(Path.Combine(drive, "apps")))
                {
                    return drive;
                }
            }
            return null;
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            string[] mountedDrives = Directory.GetDirectories("/Volumes");
            foreach (string drive in mountedDrives)
            {
                if (Directory.Exists(Path.Combine(drive, "apps")))
                {
                    return drive;
                }
            }
            return null;
        }
        else
        {
            return null;
        }
    }

    // User choice
    static int UserChoose(string choices)
    {
        ConsoleKeyInfo keyPressed;
        do
        {
            Console.Write("Choose: ");
            keyPressed = Console.ReadKey(intercept: true);
            if (choices.Contains(keyPressed.KeyChar))
            {
                Console.WriteLine();
                return choices.IndexOf(keyPressed.KeyChar) + 1;
            }
            else
            {
                return -1;
            }
        } while (keyPressed.Key != ConsoleKey.Escape);
    }

    // Credits function
    static void Credits()
    {
        {
            PrintHeader();
            Console.WriteLine("\u001b[1;32mCredits\u001b[0m:\n");
            Console.WriteLine("  - \u001b[1mSketch:\u001b[0m WiiLink Founder\n");
            Console.WriteLine("  - \u001b[1mPablosCorner:\u001b[0m WiiLink Patcher Maintainer\n");
            Console.WriteLine("  - \u001b[1mTheShadowEevee:\u001b[0m Sharpii-NetCore\n");
            Console.WriteLine("  - \u001b[1mJoshua MacDonald:\u001b[0m Xdelta3\n");
            Console.WriteLine("  - \u001b[1mperson66, and leathl:\u001b[0m Original Sharpii, and libWiiSharp developers\n");
            Console.WriteLine("\u001b[1;32mWiiLink\u001b[0m \u001b[1;32mwebsite:\u001b[0m https://wiilink24.com\n");
            Console.WriteLine("\u001b[1mPress any key to go back to the main menu\u001b[0m");
            Console.ReadKey();
        }
    }

    // If on macOS or Linux, check if the user has the required dependencies installed (xdelta3). Use AnsiConsole for better formatting.
    static void CheckDependency(string commandName, string? packageName = null)
    {
        if (string.IsNullOrEmpty(packageName))
        {
            // Expect that the package name is the same as the command being searched for.
            packageName = commandName;
        }

        // Use "which" to check if the command exists.

        ProcessStartInfo startInfo = new ProcessStartInfo();
        startInfo.FileName = "which";
        startInfo.Arguments = commandName;
        startInfo.RedirectStandardOutput = true;
        startInfo.UseShellExecute = false;
        startInfo.CreateNoWindow = true;

        Process process = new Process();
        process.StartInfo = startInfo;
        process.Start();

        string output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        if (string.IsNullOrEmpty(output))
        {
            PrintHeader();

            AnsiConsole.MarkupLine("[bold red]Dependency Error:[/]\n");
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                AnsiConsole.MarkupLine($"Cannot find the command [bold]{commandName}[/]. You can use [bold]brew install {packageName}[/] to get this required package.");
                AnsiConsole.MarkupLine("If you don't have Homebrew installed, please install at [link bold green]https://brew.sh/[/]");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                AnsiConsole.MarkupLine($"Cannot find the command [bold]{commandName}[/]. Please install [bold]{packageName}[/] using your package manager.");
            }
            Console.WriteLine();

            Console.Write("Press any key to exit...");
            Console.ReadKey();
            Console.Clear();
            ExitApp();
        }
    }

    public static void CheckDependencies()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            CheckDependency("xdelta3", "xdelta");
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            CheckDependency("xdelta3");
        }
    }

    static void DownloadFile(string URL, string dest, string name)
    {
        while (true)
        {
            task = $"Downloading {name}";
            curCmd = $"DownloadFile({URL}, {dest}, {name})";
            try
            {
                using (var client = new HttpClient())
                {
                    var response = client.GetAsync(URL).Result;
                    if (response.IsSuccessStatusCode)
                    {
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
                        ErrorScreen(statusCode, $"Failed to download {name} from {URL} to {dest}");
                    }
                }
            }
            catch (Exception e)
            {
                AnsiConsole.MarkupLine($"[bold red]ERROR:[/] {e.Message}");
            }
        }
    }

    static void DownloadPatch(string folderName, string patchInput, string patchOutput, string channelName)
    {
        string patchUrl = $"{wiiLinkPatcherUrl}/{folderName.ToLower()}/{patchInput}";
        string patchDestinationPath = Path.Join("WiiLink_Patcher", folderName, patchOutput);

        // If WiiLink_Patcher/{folderName} doesn't exist, make it
        if (!Directory.Exists(Path.Join("WiiLink_Patcher", folderName)))
        {
            Directory.CreateDirectory(Path.Join("WiiLink_Patcher", folderName));
        }

        DownloadFile(patchUrl, patchDestinationPath, channelName);
    }

    static void DownloadSPD()
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
                spdDestinationPath = Path.Join("WAD", "WiiLink_SPD (Wii).wad");
                break;
            case "vWii":
                spdUrl = $"{wiiLinkPatcherUrl}/spd/SPD_vWii.wad";
                spdDestinationPath = Path.Join("WAD", "WiiLink_SPD (vWii).wad");
                break;
        }

        DownloadFile(spdUrl, spdDestinationPath, "SPD");
    }


    static void PatchCoreChannel(string channelName, string channelTitle, string titleID, string[] patchFiles, string[] appFiles)
    {
        string titleFolder = Path.Join("unpack");
        string tempFolder = Path.Join("unpack-patched");
        string patchFolder = Path.Join("WiiLink_Patcher", channelName);
        string outputWad = Path.Join(titleFolder, $"{titleID}.wad");
        string outputChannel = Path.Join("WAD", $"{channelTitle} ({lang}).wad");
        string urlSubdir = channelName.ToLower();

        string sharpiiPath = "";
        string xdeltaPath = "";

        // Create unpack and unpack-patched folders
        Directory.CreateDirectory(titleFolder);
        Directory.CreateDirectory(tempFolder);

        // Set path to Sharpii and Xdelta3 depending on platform
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            sharpiiPath = Path.Join("WiiLink_Patcher", "Sharpii.exe");
            xdeltaPath = Path.Join("WiiLink_Patcher", "xdelta3.exe");
        }
        else
        {
            sharpiiPath = Path.Join("WiiLink_Patcher", "Sharpii");
            xdeltaPath = "xdelta3";
        }

        task = $"Downloading and extracting stuff for {channelTitle}";
        ExecuteProcess(sharpiiPath, "nusd", "-id", titleID, "-o", outputWad, "-wad", "-q");
        ExecuteProcess(sharpiiPath, "wad", "-u", outputWad, titleFolder, "-q");

        // Download patched TMD file and rename to title_id.tmd using HttpClient
        task = $"Downloading patched TMD file for {channelTitle}";
        DownloadFile($"{wiiLinkPatcherUrl}/{urlSubdir}/{channelName}.tmd", Path.Join(titleFolder, $"{titleID}.tmd"), channelTitle);

        task = $"Applying delta patches for {channelTitle}";
        // First delta patch
        if (reg == "EN" || channelName == "Dominos")
            ExecuteProcess(xdeltaPath, "-q", "-f", "-d", "-s", Path.Join(titleFolder, appFiles[0] + ".app"), Path.Join(patchFolder, patchFiles[0] + ".delta"), Path.Join(tempFolder, appFiles[0] + ".app"));

        // Second delta patch
        ExecuteProcess(xdeltaPath, "-q", "-f", "-d", "-s", Path.Join(titleFolder, appFiles[1] + ".app"), Path.Join(patchFolder, patchFiles[1] + ".delta"), Path.Join(tempFolder, appFiles[1] + ".app"));

        // Third delta patch
        if (reg == "EN" || channelName == "Dominos" || channelName == "WiinoMa")
            ExecuteProcess(xdeltaPath, "-q", "-f", "-d", "-s", Path.Join(titleFolder, appFiles[2] + ".app"), Path.Join(patchFolder, patchFiles[2] + ".delta"), Path.Join(tempFolder, appFiles[2] + ".app"));

        // Copy patched files to unpack folder
        task = $"Copying patched files for {channelTitle}";
        CopyFolder(tempFolder, titleFolder);

        task = $"Repacking the title for {channelTitle}";
        ExecuteProcess(sharpiiPath, "wad", "-p", titleFolder, $"\"{outputChannel}\"", "-f", "-q");

        // Delete unpack and unpack-patched folders
        Directory.Delete(titleFolder, true);
        Directory.Delete(tempFolder, true);
    }

    static void PatchWC24Channel(string channelName, string channelTitle, int channelVersion, string channelRegion, string titleID, string patchFile, string appFile)
    {
        string titleFolder = Path.Join("unpack");
        string tempFolder = Path.Join("unpack-patched");
        string patchFolder = Path.Join("WiiLink_Patcher", channelName);
        string outputWad = Path.Join("WAD", $"{channelTitle} [{channelRegion}] (WiiLink).wad");

        string sharpiiPath = "";
        string xdeltaPath = "";

        // Create unpack and unpack-patched folders
        Directory.CreateDirectory(titleFolder);
        Directory.CreateDirectory(tempFolder);

        // Set path to Sharpii and Xdelta3 depending on platform
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            sharpiiPath = Path.Join("WiiLink_Patcher", "Sharpii.exe");
            xdeltaPath = Path.Join("WiiLink_Patcher", "xdelta3.exe");
        }
        else
        {
            sharpiiPath = Path.Join("WiiLink_Patcher", "Sharpii");
            xdeltaPath = "xdelta3";
        }


        string urlSubdir = "";

        switch (channelName)
        {
            case "nc":
                urlSubdir = "Nintendo_Channel";
                break;
            case "forecast":
                urlSubdir = "Forecast_Channel";
                break;
        }

        task = $"Downloading necessary files for {channelTitle}";
        DownloadFile($"{PabloURL}/WC24_Patcher/{urlSubdir}/cert/{titleID}.cert", Path.Join(titleFolder, $"{titleID}.cert"), $"{channelTitle} cert");

        // Download tik file just for Nintendo Channel
        if (titleID == "0001000148415450" || titleID == "0001000148415445" || titleID == "000100014841544a")
            DownloadFile($"{PabloURL}/WC24_Patcher/{urlSubdir}/tik/{titleID}.tik", Path.Join(titleFolder, "cetk"), $"{channelTitle} tik");

        task = $"Extracting stuff for {channelTitle}";
        ExecuteProcess(sharpiiPath, "nusd", "-id", titleID, "-o", titleFolder, "-q", "-decrypt");

        task = $"Renaming files for {channelTitle}";
        File.Move(Path.Join(titleFolder, $"tmd.{channelVersion}"), Path.Join(titleFolder, $"{titleID}.tmd"));
        File.Move(Path.Join(titleFolder, "cetk"), Path.Join(titleFolder, $"{titleID}.tik"));

        task = $"Applying delta patch for {channelTitle}";
        ExecuteProcess(xdeltaPath, "-q", "-f", "-d", "-s", Path.Join(titleFolder, $"{appFile}.app"), Path.Join(patchFolder, $"{patchFile}.delta"), Path.Join(tempFolder, $"{appFile}.app"));

        // Copy patched files to unpack folder
        task = $"Copying patched files for {channelTitle}";
        CopyFolder(tempFolder, titleFolder);

        task = $"Repacking the title for {channelTitle}";
        ExecuteProcess(sharpiiPath, "wad", "-p", titleFolder, $"\"{outputWad}\"", "-f", "-q");

        // Delete unpack and unpack-patched folders
        Directory.Delete(titleFolder, true);
        Directory.Delete(tempFolder, true);
    }


    // Install Choose
    static void CoreChannel_LangSetup()
    {
        while (true)
        {
            PrintHeader();

            Console.WriteLine("\u001b[1;32mExpress Install\u001b[0m");
            Console.WriteLine("\nHello {0}! Welcome to the Express Installation of WiiLink!", Environment.UserName);
            Console.WriteLine("The patcher will download any files that are required to run the patcher.");
            Console.WriteLine();
            Console.WriteLine("\u001b[1mStep 1: Choose core channel language\u001b[0m");
            Console.WriteLine();
            Console.WriteLine("For \u001b[1mWii Room\u001b[0m, \u001b[1mDigicam Print Channel\u001b[0m, and \u001b[1mFood Channel\u001b[0m, which language would you like to select?");
            Console.WriteLine();
            Console.WriteLine("1. English Translation");
            Console.WriteLine("2. Japanese");
            Console.WriteLine();
            Console.WriteLine("3. Go Back to Main Menu");
            Console.WriteLine();

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
                    NCSetup();
                    break;
                case 3:
                    // Go back to main menu
                    MainMenu();
                    return;
                default:
                    break;
            }
        }
    }


    // Configure Demae Channel (if English was selected)
    static void DemaeConfiguration()
    {
        while (true)
        {
            PrintHeader();

            Console.WriteLine("\u001b[1;32mExpress Install\u001b[0m");
            Console.WriteLine();
            Console.WriteLine("\u001b[1mStep 1B: Choose Food Channel version\u001b[0m");
            Console.WriteLine();
            Console.WriteLine("For \u001b[1mFood Channel\u001b[0m, which version would you like to install?");
            Console.WriteLine();
            Console.WriteLine("1. Standard \u001b[1m(Fake Ordering)\u001b[0m");
            Console.WriteLine("2. Domino's \u001b[1m(US and Canada only)\u001b[0m");
            Console.WriteLine();
            Console.WriteLine("3. Go Back to Main Menu");
            Console.WriteLine();

            int choice = UserChoose("123");
            switch (choice)
            {
                case 1:
                    demae_version = "standard";
                    NCSetup();
                    break;
                case 2:
                    demae_version = "dominos";
                    NCSetup();
                    break;
                case 3:
                    // Go back to main menu
                    MainMenu();
                    break;
                default:
                    break;
            }
        }
    }

    // Configure Nintendo Channel
    static void NCSetup()
    {
        while (true)
        {
            PrintHeader();

            Console.WriteLine("\u001b[1;32mExpress Install\u001b[0m");
            Console.WriteLine();
            Console.WriteLine("\u001b[1mStep 2: Choose Nintendo Channel region\u001b[0m");
            Console.WriteLine();
            Console.WriteLine("For \u001b[1mNintendo Channel\u001b[0m, which region would you like to install?");
            Console.WriteLine();
            Console.WriteLine("1. North America");
            Console.WriteLine("2. PAL");
            Console.WriteLine("3. Japan");
            Console.WriteLine();
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
                case 4:
                    // Go back to main menu
                    MainMenu();
                    break;
                default:
                    break;
            }
        }
    }


    // Configure Forecast Channel
    static void ForecastSetup()
    {
        while (true)
        {
            PrintHeader();

            Console.WriteLine("\u001b[1;32mExpress Install\u001b[0m");
            Console.WriteLine();
            Console.WriteLine("\u001b[1mStep 3: Choose Forecast Channel region\u001b[0m");
            Console.WriteLine();
            Console.WriteLine("For \u001b[1mForecast Channel\u001b[0m, which region would you like to install?");
            Console.WriteLine();
            Console.WriteLine("1. North America");
            Console.WriteLine("2. PAL");
            Console.WriteLine("3. Japan");
            Console.WriteLine();
            Console.WriteLine("4. Go Back to Main Menu");
            Console.WriteLine();

            int choice = UserChoose("1234");
            switch (choice)
            {
                case 1:
                    forecast_reg = "USA";
                    ChoosePlatform();
                    break;
                case 2:
                    forecast_reg = "PAL";
                    ChoosePlatform();
                    break;
                case 3:
                    forecast_reg = "Japan";
                    ChoosePlatform();
                    break;
                case 4:
                    // Go back to main menu
                    MainMenu();
                    break;
                default:
                    break;
            }
        }
    }


    // Choose console platformType (Wii [Dolphin Emulator] or vWii [Wii U])
    static void ChoosePlatform()
    {
        while (true)
        {
            PrintHeader();

            Console.WriteLine("\u001b[1;32mExpress Install\u001b[0m");
            Console.WriteLine();
            Console.WriteLine("\u001b[1mStep 4: Choose console platform\u001b[0m");
            Console.WriteLine();
            Console.WriteLine("Which Wii version are you installing to?");
            Console.WriteLine();
            Console.WriteLine("1. Wii \u001b[1m(or Dolphin Emulator)\u001b[0m");
            Console.WriteLine("2. vWii \u001b[1m(Wii U)\u001b[0m");
            Console.WriteLine();
            Console.WriteLine("3. Go Back to Main Menu");
            Console.WriteLine();

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
                case 3:
                    // Go back to main menu
                    MainMenu();
                    break;
                default:
                    break;
            }
        }
    }


    // SD card setup
    static void SDSetup()
    {
        string start_btn = "Start without SD card";

        if (sdcard != null)
            start_btn = "\u001b[1mStart\u001b[0m";
        while (true)
        {
            PrintHeader();

            Console.WriteLine("\u001b[1;32mExpress Install\u001b[0m");
            Console.WriteLine();
            Console.WriteLine("\u001b[1mStep 5: Insert SD Card (if applicable)\u001b[0m");
            Console.WriteLine();
            Console.WriteLine("After passing this step, any user interaction won't be needed, so sit back and relax!");
            Console.WriteLine();
            Console.WriteLine("If you have your Wii SD card inserted, everything can download straight to it, but if not, everything will be downloaded to where this patcher is located on your computer.");
            Console.WriteLine();
            Console.WriteLine("1. " + start_btn);
            Console.WriteLine("2. Manually Select SD card");

            Console.WriteLine();

            if (sdcard != null)
            {
                Console.WriteLine("[SD card detected: \u001b[1;32m" + sdcard + "\u001b[0m]");
                Console.WriteLine();
            }

            Console.WriteLine("3. Go Back to Main Menu\n");

            int choice = UserChoose("123");
            switch (choice)
            {
                case 1:
                    // Start
                    WADFolderCheck();
                    break;
                case 2:
                    // Manually select SD card
                    SDCardSelect();
                    break;

                case 3:
                    // Go back to main menu
                    MainMenu();
                    break;
                default:
                    break;
            }
        }
    }


    // WAD folder check
    static void WADFolderCheck()
    {
        // If WAD folder doesn't exist, go to PatchingProgress(), otherwise, ask user if they want to delete it (use AnsiConsole.MarkupLine for formatting)
        if (!Directory.Exists("WAD"))
        {
            PatchingProgress();
        }
        else
        {
            while (true)
            {
                PrintHeader();

                AnsiConsole.MarkupLine("[bold green]Express Install[/]");
                Console.WriteLine();
                AnsiConsole.MarkupLine("[bold]Step 6: WAD folder detected[/]");
                Console.WriteLine();
                AnsiConsole.MarkupLine("A [bold]WAD[/] folder has been detected in the current directory. This folder is used to store the WAD files that are downloaded during the patching process. If you choose to delete this folder, it will be recreated when you start the patching process again.");
                Console.WriteLine();
                AnsiConsole.MarkupLine("1. [bold]Delete[/] WAD folder");
                AnsiConsole.MarkupLine("2. [bold]Keep[/] WAD folder");
                Console.WriteLine();
                AnsiConsole.MarkupLine("3. [bold]Go Back to Main Menu[/]");
                Console.WriteLine();

                int choice = UserChoose("123");
                switch (choice)
                {
                    case 1:
                        // Delete WAD folder in a try catch block
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
                        PatchingProgress();
                        break;
                    case 2:
                        // Keep WAD folder
                        PatchingProgress();
                        break;
                    case 3:
                        // Go back to main menu
                        MainMenu();
                        break;
                    default:
                        break;
                }
            }
        }

    }


    // Patching progress function
    static void PatchingProgress()
    {
        int counter_done = 0;
        int percent = 0;

        string demae_prog_msg = "";

        // Make WiiLink_Patcher folder in current directory if it doesn't exist
        if (!Directory.Exists("WiiLink_Patcher"))
        {
            Directory.CreateDirectory("WiiLink_Patcher");
        }

        // Set patching progress
        patching_progress[0] = "downloading:in_progress";
        patching_progress[1] = "wiiroom:not_started";
        patching_progress[2] = "digicam:not_started";
        patching_progress[3] = "demae:not_started";
        patching_progress[4] = "nc:not_started";
        patching_progress[5] = "forecast:not_started";
        patching_progress[6] = "finishing:not_started";

        // Change Demae progress message depending on version
        switch (demae_version)
        {
            case "standard":
                demae_prog_msg = "(Standard)";
                break;
            case "dominos":
                demae_prog_msg = "(Domino's)";
                break;
        }

        // Progress messages
        string[] progress_messages = new string[]
        {
            "Downloading files\n\n[bold]Patching Core Channels:[/]",
            "Wii Room",
            "Digicam Print Channel",
            $"Food Channel {demae_prog_msg}\n\n[bold]Patching WiiConnect24 Channels:[/]",
            "Nintendo Channel",
            "Forecast Channel\n\n[bold]Post-Patching:[/]",
            "Finishing up!"
        };

        // Setup patching process arrays
        Action[] patching_functions = new Action[]
        {
            DownloadAllPatches,
            WiiRoom_Patch,
            Digicam_Patch,
            Demae_Patch,
            NC_Patch,
            Forecast_Patch,
            FinishSDCopy
        };

        while (patching_progress[6] != "finishing:done")
        {
            // Progress bar and completion display
            switch (percent)
            {
                case 0:
                    counter_done = 1;
                    break;
                case 1:
                    counter_done = 2;
                    break;
                case 2:
                    counter_done = 4;
                    break;
                case 3:
                    counter_done = 6;
                    break;
                case 4:
                    counter_done = 8;
                    break;
                case 5:
                    counter_done = 9;
                    break;
                case 6:
                    counter_done = 10;
                    break;
            }

            PrintHeader();

            AnsiConsole.MarkupLine("[bold][[*]] Patching... this can take some time depending on the processing speed (CPU) of your computer.[/]\n");

            Console.Write("    Progress: ");

            //Progress bar
            switch (counter_done)
            {
                case 1:
                    AnsiConsole.Markup("[[[bold green]■         [/]]]");
                    break;
                case 2:
                    AnsiConsole.Markup("[[[bold green]■■        [/]]]");
                    break;
                case 4:
                    AnsiConsole.Markup("[[[bold green]■■■■      [/]]]");
                    break;
                case 6:
                    AnsiConsole.Markup("[[[bold green]■■■■■■    [/]]]");
                    break;
                case 8:
                    AnsiConsole.Markup("[[[bold green]■■■■■■■■  [/]]]");
                    break;
                case 9:
                    AnsiConsole.Markup("[[[bold green]■■■■■■■■■ [/]]]");
                    break;
                case 10:
                    AnsiConsole.Markup("[[[bold green]■■■■■■■■■■[/]]]");
                    break;
                default:
                    AnsiConsole.Write("[          ]");
                    break;
            }

            // Calculate percentage
            int percent_done = (counter_done * 100 / 11);

            AnsiConsole.Markup($" [bold]{percent_done}% completed[/]\n\n");
            Console.WriteLine("Please wait while the patching process is in progress...\n");

            Console.WriteLine("\u001b[1mPre-Patching:\u001b[0m");
            // Show progress of each channel
            for (int i = 0; i < patching_progress.Length; i++)
            {
                string[] progress = patching_progress[i].Split(':');
                string progress_status = progress[1];

                switch (progress_status)
                {
                    case "not_started":
                        AnsiConsole.MarkupLine($"○ {progress_messages[i]}");
                        break;
                    case "in_progress":
                        AnsiConsole.MarkupLine($"[slowblink yellow]●[/] {progress_messages[i]}");
                        break;
                    case "done":
                        AnsiConsole.MarkupLine($"[green]●[/] {progress_messages[i]}");
                        break;
                }
            }
            Console.WriteLine();

            patching_functions[percent]();

            // Increment percent
            percent++;
        }

        // After all the channels are patched, we're done!
        Finished();
        return;
    }


    static void DownloadAllPatches()
    {
        task = "Downloading patches";

        string SharpiiToDL = "";
        string SharpiiFile = "";

        // Set Sharpii variables by Windows, Linux and macOS (Sharpii.exe, Sharpii(linux-x64), Sharpii(macOS-x64)
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            SharpiiToDL = "Sharpii.exe";
            SharpiiFile = "Sharpii.exe";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            // Check if Linux x64 or ARM64
            if (RuntimeInformation.OSArchitecture == Architecture.X64)
            {
                SharpiiToDL = "sharpii(linux-x64)";
                SharpiiFile = "Sharpii";
            }
            else if (RuntimeInformation.OSArchitecture == Architecture.Arm64)
            {
                SharpiiToDL = "sharpii(linux-arm64)";
                SharpiiFile = "Sharpii";
            }
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            SharpiiToDL = "sharpii(macOS-x64)";
            SharpiiFile = "Sharpii";
        }

        // Downloading Sharpii
        DownloadFile($"{PabloURL}/Sharpii/{SharpiiToDL}", Path.Join("WiiLink_Patcher", SharpiiFile), "Sharpii");

        // Downloading Xdelta3
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            DownloadFile($"{PabloURL}/xdelta/xdelta.exe", Path.Join("WiiLink_Patcher", "xdelta3.exe"), "Xdelta3");

        // If on Linux or macOS, give Sharpii executable permissions
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            // Give Sharpii executable permissions
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "chmod",
                Arguments = $"+x {SharpiiFile}",
                WorkingDirectory = Path.Join("WiiLink_Patcher")
            };
            Process.Start(startInfo)?.WaitForExit();
        }

        // Download SPD if English is selected
        if (reg == "EN")
            DownloadSPD();
        else
            Directory.CreateDirectory("WAD");

        ///// Downloading All Channel Patches /////
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

        // Demae Channel (Standard or Dominos)
        if (demae_version == "standard")
        {
            if (reg == "EN")
                DownloadPatch("Demae", $"Demae_0_{lang}.delta", "Demae_0.delta", "Demae Channel (Standard)");
            DownloadPatch("Demae", $"Demae_1_{lang}.delta", "Demae_1.delta", "Demae Channel (Standard)");
            if (reg == "EN")
                DownloadPatch("Demae", $"Demae_2_{lang}.delta", "Demae_2.delta", "Demae Channel (Standard)");
        }
        else if (demae_version == "dominos")
        {
            DownloadPatch("Dominos", $"Dominos_0.delta", "Dominos_0.delta", "Demae Channel (Dominos)");
            DownloadPatch("Dominos", $"Dominos_1.delta", "Dominos_1.delta", "Demae Channel (Dominos)");
            DownloadPatch("Dominos", $"Dominos_2.delta", "Dominos_2.delta", "Demae Channel (Dominos)");
        }

        // If /apps/WiiModLite folder doesn't exist, create it
        if (!Directory.Exists(Path.Join("apps", "WiiModLite")))
            Directory.CreateDirectory(Path.Join("apps", "WiiModLite"));


        // Downloading Wii Mod Lite
        task = "Downloading Wii Mod Lite";
        DownloadFile($"https://hbb1.oscwii.org/unzipped_apps/WiiModLite/apps/WiiModLite/boot.dol", Path.Join("apps", "WiiModLite", "boot.dol"), "Wii Mod Lite");
        DownloadFile($"https://hbb1.oscwii.org/unzipped_apps/WiiModLite/apps/WiiModLite/meta.xml", Path.Join("apps", "WiiModLite", "meta.xml"), "Wii Mod Lite");
        DownloadFile($"https://hbb1.oscwii.org/hbb/WiiModLite.png", Path.Join("apps", "WiiModLite", "icon.png"), "Wii Mod Lite");

        // Downloading Get Console ID (for Dominos Demae Channel)
        if (demae_version == "dominos")
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
        DownloadPatch("forecast", $"Forecast_1_{platformType}_{forecast_reg}.delta", "Forecast_1.delta", "Forecast Channel");

        // Downloading stuff is finished!
        patching_progress[0] = "downloading:done";
        patching_progress[1] = "wiinoma:in_progress";
    }

    // Patching Wii no Ma
    static void WiiRoom_Patch()
    {
        task = "Patching Wii no Ma";

        string[] wiiroom_patches = { "WiinoMa_0", "WiinoMa_1", "WiinoMa_2" };
        string[] wiiroom_apps = { "00000000", "00000001", "00000002" };

        PatchCoreChannel("WiinoMa", "Wii no Ma", "000100014843494a", wiiroom_patches, wiiroom_apps);

        // Finished patching Wii no Ma
        patching_progress[1] = "wiinoma:done";
        patching_progress[2] = "digicam:in_progress";
    }

    // Patching Digicam Print Channel
    static void Digicam_Patch()
    {
        task = "Patching Digicam Print Channel";

        string[] digicam_patches = { "Digicam_0", "Digicam_1", "Digicam_2" };
        string[] digicam_apps = { "00000000", "00000001", "00000002" };

        PatchCoreChannel("Digicam", "Digicam Print Channel", "000100014843444a", digicam_patches, digicam_apps);

        // Finished patching Digicam Print Channel
        patching_progress[2] = "digicam:done";
        patching_progress[3] = "demae:in_progress";
    }

    // Patching Demae Channel
    static void Demae_Patch()
    {
        task = "Patching Demae Channel";
        string demae_folder = "Demae";
        string demae_ver = "Standard";

        string[] demae_patches = new string[3];
        if (demae_version == "standard")
            demae_patches = new string[] { "Demae_0", "Demae_1", "Demae_2" };
        else if (demae_version == "dominos")
        {
            demae_patches = new string[] { "Dominos_0", "Dominos_1", "Dominos_2" };
            demae_folder = "Dominos";
            demae_ver = "Dominos";
        }

        string[] demae_apps = { "00000000", "00000001", "00000002" };

        PatchCoreChannel(demae_folder, $"Food Channel ({demae_ver})", "000100014843484a", demae_patches, demae_apps);

        // Finished patching Demae Channel
        patching_progress[3] = "demae:done";
        patching_progress[4] = "nc:in_progress";
    }

    // Patching Nintendo Channel
    static void NC_Patch()
    {
        task = "Patching Nintendo Channel";
        string NC_titleID = "";
        string appNum = "";
        switch (nc_reg)
        {
            case "USA":
                NC_titleID = "0001000148415445";
                appNum = "0000002c";
                break;
            case "PAL":
                NC_titleID = "0001000148415450";
                appNum = "0000002d";
                break;
            case "Japan":
                NC_titleID = "000100014841544a";
                appNum = "0000003e";
                break;
        }

        PatchWC24Channel("nc", "Nintendo Channel", 1792, nc_reg, NC_titleID, "NC_1", appNum);

        // Finished patching Nintendo Channel
        patching_progress[4] = "nc:done";
        patching_progress[5] = "forecast:in_progress";
    }

    // Patching Forecast Channel
    static void Forecast_Patch()
    {
        task = "Patching Forecast Channel";
        string Forecast_titleID = "";
        switch (forecast_reg)
        {
            case "USA":
                Forecast_titleID = "0001000248414645";
                break;
            case "PAL":
                Forecast_titleID = "0001000248414650";
                break;
            case "Japan":
                Forecast_titleID = "000100024841464a";
                break;
        }

        PatchWC24Channel("forecast", $"Forecast Channel [{platformType}]", 7, forecast_reg, Forecast_titleID, "Forecast_1", "0000000d");

        // Finished patching Forecast Channel
        patching_progress[5] = "forecast:done";
        patching_progress[6] = "finishing:in_progress";
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
                CopyFolder("apps", Path.Combine(sdcard, "apps"));
                CopyFolder("WAD", Path.Combine(sdcard, "WAD"));
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
        patching_progress[6] = "finishing:done";
    }

    static void Finished()
    {
        while (true)
        {
            PrintHeader();
            AnsiConsole.MarkupLine("[bold slowblink green]Patching Completed![/]\n");

            if (sdcard != null)
            {
                Console.WriteLine("Every file is in it's place on your SD card!\n");
            }
            else
            {
                Console.WriteLine("Please connect your Wii SD card and copy the \u001b[1mWAD\u001b[0m and \u001b[1mapps\u001b[0m folders to the root (main folder) of your SD card.");
                Console.WriteLine($"You can find these folders in the \u001b[1m{curDir}\u001b[0m folder of your computer.\n");
            }

            AnsiConsole.Markup("Please proceed with the tutorial that you can find on [bold green link]https://wii.guide/wiilink[/]\n");

            Console.WriteLine("What would you like to do now?\n");
            if (sdcard != null)
                Console.WriteLine("1. Open the SD card folder");
            else
                Console.WriteLine("1. Open the folder");
            Console.WriteLine("2. Go back to the main menu");
            Console.WriteLine("3. Exit the program\n");

            int choice = UserChoose("123");
            switch (choice)
            {
                case 1:
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        if (sdcard != null)
                            Process.Start(@"explorer.exe", sdcard);
                        else
                            Process.Start(@"explorer.exe", curDir);
                    }
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    {
                        if (sdcard != null)
                            Process.Start("xdg-open", sdcard);
                        else
                            Process.Start("xdg-open", curDir);
                    }
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    {
                        if (sdcard != null)
                            Process.Start("open", sdcard);
                        else
                            Process.Start("open", curDir);
                    }
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

            Console.WriteLine("\u001b[1;32mManually Select SD Card\u001b[0m\n");

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                Console.WriteLine("Please enter the drive letter of your SD card (e.g. E)");
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                Console.WriteLine("Please enter the mount name of your SD card (e.g. /media/username/Wii)");
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                Console.WriteLine("Please enter the volume name of your SD card (e.g. /Volumes/Wii)");

            Console.WriteLine("(Type \u001b[1mEXIT\u001b[0m to go back to the previous menu)\n");

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                Console.Write("New SD card drive: ");
            else
                Console.Write("New SD card volume: ");

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
                    Console.WriteLine("\u001b[1;31mDrive letter must be 1 character!\u001b[0m");
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
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (sdcard_new == "C:\\")
                {
                    Console.WriteLine("\u001b[1;31mYou cannot select your boot drive!\u001b[0m");
                    System.Threading.Thread.Sleep(2000);
                    continue;
                }
            }
            else
            {
                if (sdcard_new == "/")
                {
                    Console.WriteLine("\u001b[1;31mYou cannot select your boot drive!\u001b[0m");
                    System.Threading.Thread.Sleep(2000);
                    continue;
                }
            }

            // Check if new SD card path is the same as the old one
            if (sdcard_new == sdcard)
            {
                Console.WriteLine("\u001b[1;31mYou have already selected this SD card!\u001b[0m");
                System.Threading.Thread.Sleep(2000);
                continue;
            }

            // Check if drive/volume exists
            if (!Directory.Exists(sdcard_new))
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    Console.WriteLine("\u001b[1;31mDrive does not exist!\u001b[0m");
                else
                    Console.WriteLine("\u001b[1;31mVolume does not exist!\u001b[0m");

                System.Threading.Thread.Sleep(2000);
                continue;
            }

            // Check if SD card has /apps folder (using PathCombine)
            if (Directory.Exists(Path.Combine(sdcard_new, "apps")))
            {
                // SD card is valid
                sdcard = sdcard_new;
                break;
            }
            else
            {
                // SD card is invalid
                Console.WriteLine("\n\u001b[1mDrive detected, but no /apps folder found!\u001b[0m");
                Console.WriteLine("Please create it first and then try again.\n");

                // Press any key to continue
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
            }
        }
    }


    // Main Menu function
    static void MainMenu()
    {
        while (true)
        {
            // Call the PrintHeader() method
            PrintHeader();
            PrintAnnouncement();

            AnsiConsole.MarkupLine("[bold]Welcome to the WiiLink Patcher![/]");
            Console.WriteLine();
            Console.WriteLine("1. Start");
            Console.WriteLine("2. Credits");
            Console.WriteLine();
            Console.WriteLine("3. Exit Patcher");
            Console.WriteLine();

            if (sdcard != null)
                AnsiConsole.MarkupLine("[bold green]Detected SD Card:[/] " + sdcard);
            else
                AnsiConsole.MarkupLine("[bold red]Could not detect your SD Card.[/]");

            Console.WriteLine("M. Manually select SD Card path");
            Console.WriteLine();

            // User chooses an option
            int choice = UserChoose("123Mm");
            switch (choice)
            {
                case 1:
                    // Start (1)
                    CoreChannel_LangSetup();
                    break;
                case 2:
                    // Credits (2)
                    Credits();
                    break;
                case 3:
                    // Clear console and Exit app (3)
                    Console.Clear();
                    ExitApp();
                    break;
                case 4:
                    // Manually select SD Card path (O)
                    SDCardSelect();
                    break;
                case 5:
                    // Manually select SD Card path (o)
                    SDCardSelect();
                    break;
                default:
                    break;
            }
        }
    }

    // Check if server is up
    static async System.Threading.Tasks.Task<bool> CheckServerAsync(string serverURL)
    {
        string url = $"{serverURL}/wiinoma/WiinoMa_1_English.delta";
        bool isServerUp = false;
        var httpClient = new HttpClient();

        try
        {
            PrintHeader();
            Console.WriteLine("Checking server status...");

            using (var response = await httpClient.GetAsync(url))
            {
                isServerUp = response.IsSuccessStatusCode;
                Console.WriteLine("\u001b[1;32mServer is up!\u001b[0m");
                System.Threading.Thread.Sleep(1000);
            }
        }
        catch
        {
            isServerUp = false;
            Console.WriteLine("\u001b[1;31mServer is down!\u001b[0m");
            System.Threading.Thread.Sleep(1000);
        }

        return isServerUp;
    }

    static void ServerDown()
    {
        PrintHeader();

        Console.WriteLine("The WiiLink server is currently down!\n");
        Console.WriteLine("It seems that our server is currently down. We're trying to get it back up as soon as possible.\n");
        Console.WriteLine("Stay tuned on our Discord server for updates:");
        AnsiConsole.MarkupLine("[link bold green]https://discord.gg/WiiLink\u001b[/]\n");

        Console.Write("Press any key to exit...");
        Console.ReadKey();
        ExitApp();
    }

    // Error detected!
    static void ErrorScreen(int exitCode, string? msg)
    {
        PrintHeader();

        Console.WriteLine("\u001b[5;31mAn error has occurred.\u001b[0m\n");

        Console.WriteLine("\u001b[1mERROR DETAILS:\u001b[0m");
        Console.WriteLine($" * \u001b[1mTask:\u001b[0m {task}");
        if (msg == null)
            Console.WriteLine($" * \u001b[1mCommand:\u001b[0m {curCmd}");
        else
            Console.WriteLine($" * \u001b[1mMessage:\u001b[0m {msg}");
        Console.WriteLine($" * \u001b[1mExit code:\u001b[0m {exitCode}\n");

        AnsiConsole.MarkupLine("Please open an issue on our GitHub page ([link bold green]https://github.com/WiiLink24/WiiLink24-Patcher/issues[/]) and describe the");
        Console.WriteLine("issue you're having.");

        // Press any key to go back to the main menu
        Console.WriteLine("\n\u001b[1mPress any key to go back to the main menu...\u001b[0m");
        Console.ReadKey();

        // Go back to the main menu
        MainMenu();
    }

    public static void ExecuteProcess(string programName, params string[] args)
    {

        curCmd = $"{programName} {string.Join(" ", args)}";

        Process process = new Process();
        process.StartInfo.FileName = programName;
        process.StartInfo.Arguments = string.Join(" ", args);
        process.Start();

        // Wait for the process to exit before accessing its information
        process.WaitForExit();

        // Get the exit code after the process has exited
        exitCode = process.ExitCode;

        // If the exit code is not 0, then an error has occurred
        if (exitCode != 0)
            ErrorScreen(exitCode, null);
    }

    private static void CopyFolder(string sourceFolder, string destinationFolder)
    {
        if (!Directory.Exists(destinationFolder))
            Directory.CreateDirectory(destinationFolder);

        string[] files = Directory.GetFiles(sourceFolder);

        foreach (string file in files)
        {
            string fileName = Path.GetFileName(file);
            string destFile = Path.Combine(destinationFolder, fileName);
            File.Copy(file, destFile, true);
        }

        string[] subFolders = Directory.GetDirectories(sourceFolder);

        foreach (string subfolder in subFolders)
            CopyFolder(subfolder, Path.Combine(destinationFolder, Path.GetFileName(subfolder)));
    }

    // Exit console app
    static void ExitApp()
    {
        // Restore original console size if not Windows
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            Console.Write($"\u001b[8;{console_height};{console_width}t");

        Environment.Exit(0);
    }

    static async System.Threading.Tasks.Task Main(string[] args)
    {
        // Set console encoding to UTF-8
        Console.OutputEncoding = Encoding.UTF8;

        // Cache current console size to console_width and console_height
        console_width = Console.WindowWidth;
        console_height = Console.WindowHeight;

        // Set console window size to 120x30 on macOS and Linux
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            Console.Write("\u001b[8;30;120t");
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            // If console size is less than 100x25, then resize it
            if (console_width < 100 || console_height < 25)
                Console.Write("\u001b[8;30;120t");

        // Change Windows console title
        Console.Title = $"WiiLink Patcher v{version}";

        // Check dependencies (if not on Windows)
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            CheckDependencies();

        // Check if the server is up
        if (!await CheckServerAsync(wiiLinkPatcherUrl))
            ServerDown();

        // Delete temp folders
        if (Directory.Exists("WiiLink_Patcher"))
            Directory.Delete(Path.Join("WiiLink_Patcher"), true);
        if (Directory.Exists("unpack"))
            Directory.Delete(Path.Join("unpack"), true);
        if (Directory.Exists("unpack-patched"))
            Directory.Delete(Path.Join("unpack-patched"), true);

        // Go to the main menu
        MainMenu();
    }
}