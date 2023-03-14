using System;
using System.Threading.Tasks;
using Windows.System;

namespace JiayiLauncher.Features.Bridge;

public static class Minecraft
{
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