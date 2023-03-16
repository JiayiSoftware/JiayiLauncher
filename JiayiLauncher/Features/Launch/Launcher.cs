using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using JiayiLauncher.Features.Bridge;
using JiayiLauncher.Features.Mods;

namespace JiayiLauncher.Features.Launch;

// maybe i should use dependency injection for this class but it'll be static until i figure out how to do that
public static class Launcher
{
	public enum LaunchResult
	{
		VersionMismatch,
		GameNotFound,
		ModNotFound,
		InjectionFailed,
		DownloadFailed,
		AlreadyRunning,
		Success
	}
	
	public static bool Launching { get; private set; }
	
	public static int LaunchProgress { get; private set; }
	
	private static async Task<bool> CheckVersion(Mod mod)
	{
		var version = await Minecraft.GetVersion();
		return mod.SupportedVersions.Contains(version) || mod.SupportedVersions.Contains("any version");
	}

	private static async Task<LaunchResult> DownloadMod(Mod mod)
	{
		throw new NotImplementedException();
	}
}