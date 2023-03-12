using System;
using System.Threading.Tasks;
using Windows.System;

namespace JiayiLauncher.Features.Bridge;

public static class Minecraft
{
	private static AppDiagnosticInfo? _minecraftApp;

	public static async Task<string> GetVersion()
	{
		if (_minecraftApp == null)
		{
			var info = await AppDiagnosticInfo.RequestInfoForPackageAsync("Microsoft.MinecraftUWP_8wekyb3d8bbwe");
			if (info == null || info.Count == 0) return "Minecraft not found";
			_minecraftApp = info[0];
		}
		
		var version = _minecraftApp.AppInfo.Package.Id.Version;
		return $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
	}
}