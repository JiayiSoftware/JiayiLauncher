using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Management.Deployment;
using Windows.System;
using JiayiLauncher.Settings;

namespace JiayiLauncher.Features.Game;

public static class PackageData
{
	public static PackageManager PackageManager { get; } = new();
	
	public static async Task<AppDiagnosticInfo?> GetPackage()
	{
		var info = 
			await AppDiagnosticInfo.RequestInfoForPackageAsync("Microsoft.MinecraftUWP_8wekyb3d8bbwe");
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

	public static string GetGameDataPath()
	{
		// i thought i could just use the package for this but naw gotta hardcode it
		return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
			"Packages", "Microsoft.MinecraftUWP_8wekyb3d8bbwe");
	}

	public static async Task<InstallLocation> GetInstallLocation()
	{
		var package = await GetPackage();
		if (package == null) return InstallLocation.Unknown;
		
		if (!package.AppInfo.Package.IsDevelopmentMode) return InstallLocation.MicrosoftStore;
		
		var installPath = package.AppInfo.Package.InstalledLocation.Path;

		// inverting this if statement looks confusing, i'm not taking your suggestion rider
		if (JiayiSettings.Instance!.VersionsPath == string.Empty)
		{
			JiayiSettings.Instance.VersionsPath =
				Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
					"JiayiLauncher", "Versions");
			JiayiSettings.Instance.Save();
		}
		
		return installPath.Contains(JiayiSettings.Instance.VersionsPath) ? 
			InstallLocation.FromJiayi : InstallLocation.OtherVersionManager;
	}
}