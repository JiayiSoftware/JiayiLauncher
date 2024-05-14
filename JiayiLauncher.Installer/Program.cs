using System.Diagnostics;
using JiayiLauncher.Installer;
using static JiayiLauncher.Installer.Util;

Console.ForegroundColor = ConsoleColor.Red;
Console.Title = "Jiayi Installer";
Console.WindowHeight = 35;

var arg = new Args();

// flags
var uninstall = arg.GetFlag("uninstall");
var desktop   = arg.GetFlag("desktop");
var dotnet    = arg.GetFlag("install-dotnet");
var open	  = arg.GetFlag("open-immediately");

// commands
var version = arg.GetCommand("version");
var path    = arg.GetCommand("path");

// default values for these commands
if (string.IsNullOrEmpty(version)) version = "latest";
if (string.IsNullOrEmpty(path)) path = 
	Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "JiayiLauncher", "App");

if (!IsAdministrator())
{
	Console.WriteLine("Please run this installer as an administrator.");
	Console.WriteLine("Press any key to exit...");
	Console.ReadKey();
	return;
}

if (!arg.Empty) goto install;
	
// actual installer code
Console.Clear();
const string jiayi = """
................................................................................
................................................................................
.............................................(%%%%%%(...........................
.............................................%%%%%%*............................
................#############################%%%%%#############################,
...............,%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%*
................***************************#%%%%%(*****************************.
...........................................%%%%%%*..............................
..................(%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%*......
..................%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%/,......
................................................................................
.....................(%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%#.............
.....................%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%*.............
....................#%%%%#**********************************#%%%%#*.............
....................%%%%%*..................................%%%%%*..............
...................#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%(,..............
...................%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%*...............
............................%%%%%%*.............%%%%%(*.........................
............................(%%%%%/...........(%%%%#*...........................
.....%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%*....
....#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%*.....
....................%%%%%%*.....................................................
...................#%%%%%(,.................%%%%%%%%%%%%%%%%%%%%%%%%#...........
......,%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%*....#%%%%%%%%%%%%%%%%%%%%%%%%/,..........
......#%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%*.....%%%%%**************/%%%%%*...........
................/%%%%%%*.......%%%%%/,....%%%%%/,.............%%%%%/............
..............#%%%%%%/*.......%%%%%#*.....%%%%%*.............*%%%%#*............
.........*#%%%%%%%%/*........(%%%%%*.....%%%%%%%#############%%%%%/.............
.%%%%%%%%%%%%%%%/*....////(#%%%%%%*.....,%%%%%%%%%%%%%%%%%%%%%%%%#*.............
.%%%%%%%%%%#**,.......(%%%%%%%%%/*......%%%%%/**************%%%%%/..............
.%##/***,............../////***........*#####*..................................
................................................................................

""";

foreach (var line in jiayi.Split('\n'))
{
	Console.WriteLine(line);
	Thread.Sleep(25);
}

var str = uninstall ? "un" : "";

Console.WriteLine($"Press any key to start the {str}installation...");
Console.ReadKey(true);
Console.Clear();

if (uninstall)
{
	if (!Directory.Exists(path))
	{
		Console.WriteLine(!string.IsNullOrEmpty(arg.GetCommand("path"))
			? "This path does not lead to a valid installation of Jiayi Launcher."
			: "Jiayi Launcher is not installed on this computer.");
		
		Console.WriteLine("Press any key to exit...");
		Console.ReadKey();
		return;
	}
	
	Console.WriteLine("Are you sure you want to uninstall Jiayi Launcher?");
	Console.WriteLine("Press any key to continue, or close this window to cancel.");
	Console.ReadKey(true);
	Console.Clear();
	
	Console.WriteLine("Uninstalling Jiayi Launcher...");
	Uninstaller.Uninstall(path);
	
	// ask if they want to delete their data
	Console.WriteLine("Would you like to delete your Jiayi Launcher data as well?");
	Console.WriteLine("This includes your settings, installed Minecraft versions, and game data.");
	Console.WriteLine("Press any key to delete your data, or close this window to keep it.");
	Console.ReadKey(true);
	
	Console.WriteLine("Deleting Jiayi Launcher data...");
	var dataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "JiayiLauncher");
	if (Directory.Exists(dataPath)) Directory.Delete(dataPath, true);
	
	Console.WriteLine("Jiayi Launcher has been uninstalled.");
	Console.WriteLine("Press any key to exit...");
	Console.ReadKey();
	return;
}

// path
Console.WriteLine("[1/4] Path\n");
Console.WriteLine("By default, Jiayi Launcher will be installed to:");
Console.WriteLine(path);
Console.WriteLine("\nIf you'd like to install to a different location, enter it now.");
Console.WriteLine("Otherwise, press enter to continue.");
Console.Write("> ");
var newPath = Console.ReadLine();
if (!string.IsNullOrEmpty(newPath)) path = newPath;
Console.Clear();

// dotnet
Console.WriteLine("[2/4] Installing .NET\n");
// check if .NET is on the PATH
Console.WriteLine("Jiayi Launcher requires .NET 8 to run.");
var dotnetInstalled = Environment.GetEnvironmentVariable("PATH")?.Contains("dotnet") ?? false;
if (dotnetInstalled)
{
	// Console.WriteLine("It looks like .NET is already installed on your system.");
	// Console.WriteLine("If you'd like to install it anyway, type \"install\".");
	
	// run dotnet --version for the actual version
	var process = Process.Start(new ProcessStartInfo
	{
		FileName = "dotnet",
		Arguments = "--version",
		RedirectStandardOutput = true,
		UseShellExecute = false
	});
	
	process?.WaitForExit();
	var output = process?.StandardOutput.ReadToEnd();
	if (output != null && output.StartsWith("8."))
	{
		Console.WriteLine("It looks like .NET 8 is already installed on your system.");
		Console.WriteLine("If you'd like to install it anyway, type \"install\".");
	}
	else
	{
		Console.WriteLine("It looks like .NET is already installed on your system, but it's not .NET 8.");
		Console.WriteLine("If you'd like to install it, type \"install\".");
	}
}
else
{
	Console.WriteLine("If you'd like to install it, type \"install\".");
}

Console.WriteLine("Otherwise, press enter to continue.");
Console.Write("> ");
var dotnetInput = Console.ReadLine();
if (dotnetInput == "install") dotnet = true;
Console.Clear();

// extra stuff
Console.WriteLine("[3/4] Extra stuff\n");
Console.WriteLine("The installer will create a start menu shortcut for Jiayi Launcher.");
Console.WriteLine("\nIf you'd like to create a desktop shortcut as well, type \"desktop\".");
Console.WriteLine("Otherwise, press enter to continue.");
Console.Write("> ");
var desktopInput = Console.ReadLine();
if (desktopInput == "desktop") desktop = true;
Console.Clear();

// review and install
Console.WriteLine("[4/4] Review and install\n");
Console.WriteLine("So far, you've chosen the following options:");
Console.WriteLine($"Path: {path}");
Console.WriteLine($"Install .NET: {dotnet}");
Console.WriteLine($"Create desktop shortcut: {desktop}\n");
Console.WriteLine("If you've changed your mind about any of these options, close this window now.");
Console.WriteLine("Otherwise, press enter to install Jiayi Launcher.");
Console.ReadLine();
Console.Clear();

install:
Console.WriteLine("Downloading Jiayi Launcher...");
var downloadUrl = "https://github.com/JiayiSoftware/JiayiLauncher/releases/latest/download/JiayiLauncher.zip";
if (version != "latest")
	downloadUrl = $"https://github.com/JiayiSoftware/JiayiLauncher/releases/download/{version}/JiayiLauncher.zip";

var downloadPath = Path.Combine(Path.GetTempPath(), "JiayiLauncher.zip");
await Download(downloadUrl, downloadPath);

Console.WriteLine("Extracting...");
await ExtractAndDelete(downloadPath, path);

if (dotnet)
{
	Console.WriteLine("Installing .NET...");
	const string dotnetDownloadUrl = 
		"https://download.visualstudio.microsoft.com/download/pr/f18288f6-1732-415b-b577-7fb46510479a/a98239f751a7aed31bc4aa12f348a9bf/windowsdesktop-runtime-8.0.1-win-x64.exe";
	var dotnetDownloadPath = Path.Combine(Path.GetTempPath(), "dotnet.exe");
	await Download(dotnetDownloadUrl, dotnetDownloadPath);
	
	var process = Process.Start(new ProcessStartInfo
	{
		FileName = dotnetDownloadPath,
		Arguments = "/passive /norestart"
	});
	
	process?.WaitForExit();
	File.Delete(dotnetDownloadPath);
}

if (desktop)
{
	Console.WriteLine("Creating desktop shortcut...");
	var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
	var shortcutPath = Path.Combine(desktopPath, "Jiayi Launcher.lnk");
	CreateShortcut(shortcutPath, Path.Combine(path, "JiayiLauncher.exe"));
}

Console.WriteLine("Creating start menu shortcut...");
var startMenuPath = Environment.GetFolderPath(Environment.SpecialFolder.StartMenu);
var startMenuShortcutPath = Path.Combine(startMenuPath, "Jiayi Launcher.lnk");
CreateShortcut(startMenuShortcutPath, Path.Combine(path, "JiayiLauncher.exe"));

Console.WriteLine("Setting file associations...");
SetFileAssociation("Jiayi Mod Collection", ".jiayi", Path.Combine(path, "JiayiLauncher.exe"));
RegisterUrlProtocol(Path.Combine(path, "JiayiLauncher.exe"));

if (!open)
{
	Console.WriteLine("\nJiayi Launcher has been installed.");
	Console.WriteLine("Press enter to open the launcher, or close this window to exit.");
	Console.ReadLine();
}

// make sure to set working directory or the launcher will crash
Process.Start(new ProcessStartInfo
{
	FileName = Path.Combine(path, "JiayiLauncher.exe"),
	WorkingDirectory = path
});
