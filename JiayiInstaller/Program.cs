using System.Diagnostics;
using JiayiInstaller;
using static JiayiInstaller.Util;

Console.ForegroundColor = ConsoleColor.Red;
Console.Title = "Jiayi Installer";
Console.WindowHeight = 35;

var arg = new Args();

// flags
var desktop = arg.GetFlag("desktop");
var dotnet  = arg.GetFlag("install-dotnet");
var open	= arg.GetFlag("open-immediately");

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
var jiayi = """
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
	Thread.Sleep(50);
}

Console.WriteLine("Press any key to start the installation...");
Console.ReadKey(true);
Console.Clear();

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
Console.WriteLine("Jiayi Launcher requires .NET 7 to run.");
var dotnetInstalled = Environment.GetEnvironmentVariable("PATH")?.Contains("dotnet") ?? false;
if (dotnetInstalled)
{
	Console.WriteLine("It looks like .NET is already installed on your system.");
	Console.WriteLine("If you'd like to install it anyway, type \"install\".");
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
ExtractAndDelete(downloadPath, path);

if (dotnet)
{
	Console.WriteLine("Installing .NET...");
	const string dotnetDownloadUrl = 
		"https://download.visualstudio.microsoft.com/download/pr/342ba160-3776-4ffa-91dd-e3cd9dc0f817/ba649d6b80b27ca164d80bd488cdb51f/windowsdesktop-runtime-7.0.7-win-x64.exe";
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