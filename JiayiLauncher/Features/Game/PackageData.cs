using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.System;

namespace JiayiLauncher.Features.Game;

public static class PackageData
{
	public static async Task<AppDiagnosticInfo?> GetPackage()
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

	public static string GetGameDataPath()
	{
		// i thought i could just use the package for this but naw gotta hardcode it
		return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Packages",
			"Microsoft.MinecraftUWP_8wekyb3d8bbwe");
	}
}