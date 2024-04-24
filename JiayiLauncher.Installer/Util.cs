using System.Diagnostics;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Security.Principal;
using Microsoft.Win32;

namespace JiayiLauncher.Installer;

#pragma warning disable CA1416

public static partial class Util
{
	[LibraryImport("shell32.dll", SetLastError = true)]
	public static partial void SHChangeNotify(uint eventId, uint flags, nint item1, nint item2);
	
	private static readonly HttpClient _client = new();
	
	public static async Task ExtractAndDelete(string zipPath, string extractPath)
	{
		Directory.CreateDirectory(extractPath);
		
		// extract everything BUT wwwroot/css/theme.css
		var archive = ZipFile.OpenRead(zipPath);
		foreach (var entry in archive.Entries)
		{
			if (entry.FullName == "wwwroot/css/theme.css") continue;
			
			var path = Path.Combine(extractPath, entry.FullName);
			Directory.CreateDirectory(Path.GetDirectoryName(path)!);
			
			// there is a slight chance that this entry is a folder
			if (entry.Length == 0) continue;
			
			await using var stream = entry.Open();
			await using var fileStream = File.Create(path);
			await stream.CopyToAsync(fileStream);
			
			fileStream.Close();
			stream.Close();
		}
		
		archive.Dispose();
		File.Delete(zipPath);
	}

	public static async Task Download(string url, string path)
	{
		if (File.Exists(path)) File.Delete(path);
		
		await using var stream = await _client.GetStreamAsync(url);
		await using var fileStream = File.Create(path);
		await stream.CopyToAsync(fileStream);
	}
	
	public static bool IsAdministrator()
	{
		using var identity = WindowsIdentity.GetCurrent();
		var principal = new WindowsPrincipal(identity);
		return principal.IsInRole(WindowsBuiltInRole.Administrator);
	}
	
	public static void SetFileAssociation(string description, string extension, string path)
	{
		var baseKey = Registry.ClassesRoot.CreateSubKey(extension);
		baseKey.SetValue("", description.Replace(' ', '_'));
		baseKey.Close();
		
		var openWith = Registry.ClassesRoot.CreateSubKey(description.Replace(' ', '_'));
		openWith.SetValue("", description);
		openWith.CreateSubKey("DefaultIcon").SetValue("", $"\"{path}\",0");
		openWith.CreateSubKey("shell").CreateSubKey("open").CreateSubKey("command")
			.SetValue("", $"\"{path}\" \"%1\"");
		openWith.Close();
		
		var userChoice =
			Registry.CurrentUser.OpenSubKey(
				@"Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\" + extension, true);
		if (userChoice != null)
		{
			userChoice.DeleteSubKey("UserChoice", false);
			userChoice.SetValue("Progid", description.Replace(' ', '_'));
			userChoice.Close();
		}
		
		SHChangeNotify(0x08000000, 0x0000, nint.Zero, nint.Zero);
	}

	public static void RegisterUrlProtocol(string path)
	{
		var protocolKey = Registry.ClassesRoot.CreateSubKey("jiayi"); // same thing as above
		protocolKey.SetValue("", "URL: Jiayi Protocol");
		protocolKey.SetValue("URL Protocol", "");
		
		protocolKey.CreateSubKey("shell").CreateSubKey("open").CreateSubKey("command")
			.SetValue("", $"\"{path}\" \"%1\"");
	}
	
	public static void CreateShortcut(string shortcutPath, string targetPath)
	{
		// fuck COM
		Process.Start("powershell.exe", $"""
			$s = (New-Object -ComObject WScript.Shell).CreateShortcut('{shortcutPath}')
			$s.TargetPath = '{targetPath}'
			$s.WorkingDirectory = '{Path.GetDirectoryName(targetPath)!}'
			$s.Save()
		""").WaitForExit();
	}
}