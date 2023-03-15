using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.System;

namespace JiayiLauncher.Features.Bridge;

public static class Minecraft
{
	private static List<string> _versions = new();

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
	}
}