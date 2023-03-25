using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using JiayiLauncher.Features.Bridge;
using JiayiLauncher.Features.Mods;
using JiayiLauncher.Settings;
using JiayiLauncher.Utils;

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
	
	public static event EventHandler? LaunchProgressChanged;

	// the big method
	public static async Task<LaunchResult> Launch(Mod mod)
	{
		if (Launching) return LaunchResult.AlreadyRunning;
		Launching = true;
		
		LaunchProgress = 0;
		LaunchProgressChanged?.Invoke(null, EventArgs.Empty);
		Log.Write(nameof(Launcher), $"Launching {mod.Name}");
		
		var supported = await Minecraft.ModSupported(mod);
		if (!supported)
		{
			Log.Write(nameof(Launcher), $"{mod.Name} is not supported by the current version of Minecraft");
			return LaunchResult.VersionMismatch;
		}

		LaunchProgress += 10;
		LaunchProgressChanged?.Invoke(null, EventArgs.Empty);
		
		await Minecraft.Open();
		Log.Write(nameof(Launcher), "Opened game, waiting to launch mod...");
		
		LaunchProgress += 5;
		LaunchProgressChanged?.Invoke(null, EventArgs.Empty);

		string path;
		
		// if this is a web mod, download it in the meantime
		if (mod.FromInternet)
		{
			var downloadedPath = await Downloader.DownloadMod(mod);
			if (downloadedPath == string.Empty) return LaunchResult.DownloadFailed;
			path = downloadedPath;
			
			LaunchProgress += 30;
			LaunchProgressChanged?.Invoke(null, EventArgs.Empty);
		}
		else
		{
			path = mod.Path;
			LaunchProgress += 35;
			LaunchProgressChanged?.Invoke(null, EventArgs.Empty);
		}

		// either wait for the game's modules to load
		// or if the user has injection delay enabled, wait for the time they specified
		if (JiayiSettings.Instance!.UseInjectionDelay)
			Task.Delay(JiayiSettings.Instance.InjectionDelay[2] * 1000).Wait();
		else
			await Minecraft.WaitForModules();
		
		LaunchProgress += 25;
		LaunchProgressChanged?.Invoke(null, EventArgs.Empty);
		
		if (!Minecraft.IsOpen()) return LaunchResult.GameNotFound;
		
		// determine whether this is an internal or external mod
		bool external;
		if (path.EndsWith(".exe")) external = true;
		else if (path.EndsWith(".dll")) external = false;
		else return LaunchResult.ModNotFound;
		
		Log.Write(nameof(Launcher), external ? "Detected external mod" : "Detected internal mod");
		
		if (!File.Exists(path)) return LaunchResult.ModNotFound;

		if (external)
		{
			if (Process.GetProcessesByName(Path.GetFileNameWithoutExtension(path)).Length != 0)
				return LaunchResult.AlreadyRunning;

			Process.Start(path);
			LaunchProgress += 30;
			LaunchProgressChanged?.Invoke(null, EventArgs.Empty);
			Launching = false;
			
			Minecraft.ModsLoaded.Add(mod);
			
			return LaunchResult.Success;
		}

		// else
		if (Injector.IsInjected(path)) return LaunchResult.AlreadyRunning;

		var injected = await Injector.Inject(path);
		LaunchProgress += 30;
		LaunchProgressChanged?.Invoke(null, EventArgs.Empty);
		Launching = false;
		
		if (injected) Minecraft.ModsLoaded.Add(mod);
		
		return injected ? LaunchResult.Success : LaunchResult.InjectionFailed;
	}
}