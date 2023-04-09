using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.System;
using JiayiLauncher.Features.Mods;
using JiayiLauncher.Settings;
using JiayiLauncher.Utils;

namespace JiayiLauncher.Features.Launch;

public static class Minecraft
{
	private static readonly List<string> _versions = new();
	private static readonly List<Mod> _modsLoaded = new();

	public static List<Mod> ModsLoaded
	{
		get
		{
			if (!IsOpen) _modsLoaded.Clear();
			return _modsLoaded;
		}
	}
	
	public static bool IsOpen
	{
		get
		{
			var processes = Process.GetProcessesByName("Minecraft.Windows");
			if (processes.Length == 0) return false;
			Process = processes[0];
			return true;
		}
	}

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
		
		// the game does it weird
		var major = version.Major;
		var minor = version.Minor;
		// take the first two numbers of the build number
		var build = version.Build.ToString()[..2];
		// the last number of the build number is the revision
		var revision = version.Build.ToString()[^1];
		return $"{major}.{minor}.{build}.{revision}";
	}

	public static async Task Open()
	{
		var minecraftApp = await GetPackage();
		if (minecraftApp == null) return;
		await minecraftApp.LaunchAsync();
		
		Process = Process.GetProcessesByName("Minecraft.Windows")[0];
	}

	public static async Task WaitForModules()
	{
		await Task.Run(() =>
		{
			while (true)
			{
				Process.Refresh();
				if (JiayiSettings.Instance!.OverrideModuleRequirement)
					if (Process.Modules.Count > JiayiSettings.Instance.ModuleRequirement[2]) break;
				
				if (Process.Modules.Count > 160) break;

				if (JiayiSettings.Instance.AccelerateGameLoading)
				{
					var brokers = Process.GetProcessesByName("RuntimeBroker");
					if (brokers.Length > 0)
					{
						foreach (var broker in brokers)
						{
							broker.Kill();
						}
					}
				}

				// wait for a bit
				Task.Delay(100).Wait();
			}
		});
	}

	public static async Task<bool> ModSupported(Mod mod)
	{
		var version = await GetVersion();
		Log.Write(nameof(Minecraft), $"Current game version is {version} and mod supports {string.Join(", ", mod.SupportedVersions)}");
		return mod.SupportedVersions.Contains(version) || mod.SupportedVersions.Contains("any version");
	}
}