using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.System;
using JiayiLauncher.Features.Mods;

namespace JiayiLauncher.Features.Bridge;

public static class Minecraft
{
	private static List<string> _versions = new();
	
	public static List<Mod> ModsLoaded { get; } = new();
	public static Process Process { get; private set; } = null!;

	public static async Task<List<string>> GetVersionList()
	{
		if (_versions.Count > 0) return _versions;

		using var client = new HttpClient();

		var response =
			await client.GetAsync("https://raw.githubusercontent.com/BionicBen/ProjectStarFiles/main/MinecraftVersions.txt");
		
		if (!response.IsSuccessStatusCode) return _versions;
		
		var content = await response.Content.ReadAsStringAsync();
		var lines = content.Split('\n');
		foreach (var line in lines)
		{
			if (line == string.Empty || line.Contains('\n')) continue;
			_versions.Add(line.Trim());
		}

		return _versions;
	}

	private static async Task<AppDiagnosticInfo?> GetPackage()
	{
		var info = await AppDiagnosticInfo.RequestInfoForPackageAsync("Microsoft.MinecraftUWP_8wekyb3d8bbwe");
		if (info == null || info.Count == 0) return null;
		return info[0];
	}
	
	public static async Task<string> GetVersion()
	{
		var minecraftApp = await GetPackage();
		if (minecraftApp == null) return "Unknown";
		var version = minecraftApp.AppInfo.Package.Id.Version;
		return $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
	}

	public static async Task Open()
	{
		var minecraftApp = await GetPackage();
		if (minecraftApp == null) return;
		await minecraftApp.LaunchAsync();
		
		Process = Process.GetProcessesByName("Minecraft.Windows")[0];
	}

	public static bool IsOpen()
	{
		var processes = Process.GetProcessesByName("Minecraft.Windows");
		if (processes.Length == 0) return false;
		Process = processes[0];
		return true;
	}

	public static async Task WaitForModules()
	{
		await Task.Run(() =>
		{
			while (true)
			{
				Process.Refresh();
				if (Process.Modules.Count > 160) break;

				// wait for a bit
				Task.Delay(4000).Wait();
			}
		});
	}

	public static async Task<bool> ModSupported(Mod mod)
	{
		var version = await GetVersion();
		return mod.SupportedVersions.Contains(version) || mod.SupportedVersions.Contains("any version");
	}
}