using System.Diagnostics;
using System.IO.Compression;
using System.Security.Principal;

namespace JiayiInstaller;

public static class Util
{
	private static readonly HttpClient _client = new();
	
	public static async Task ExtractAndDelete(string zipPath, string extractPath)
	{
		Directory.CreateDirectory(extractPath);
		
		// extract everything BUT wwwroot/css/theme.css
		using var archive = ZipFile.OpenRead(zipPath);
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
		}
		
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