using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using JiayiLauncher.Features.Game;
using JiayiLauncher.Features.Mods;
using JiayiLauncher.Features.Stats;
using JiayiLauncher.Settings;
using JiayiLauncher.Utils;

namespace JiayiLauncher.Features.Launch;

public static class Launcher
{
	public enum LaunchResult
	{
		VersionMismatch,
		GameNotFound,
		ModNotFound,
		InjectionFailed,
		DownloadFailed,
		AlreadyLoaded,
		AlreadyLaunching,
		Success
	}
	
	public static bool Launching { get; private set; }
	
	public static int LaunchProgress { get; private set; }
	
	public static event EventHandler? LaunchProgressChanged;

	// the big method
	public static async Task<LaunchResult> Launch(Mod mod)
	{
		if (Launching) return LaunchResult.AlreadyLaunching;
		Launching = true;
		
		var log = Singletons.Get<Log>();
		
		LaunchProgress = 0;
		LaunchProgressChanged?.Invoke(null, EventArgs.Empty);
		log.Write(nameof(Launcher), $"Launching {mod.Name}");
		
		// minimize fix !
		await PackageData.MinimizeFix(JiayiSettings.Instance.MinimizeFix);
		
		var supported = await Minecraft.ModSupported(mod);
		if (!supported)
		{
			log.Write(nameof(Launcher), $"{mod.Name} does not support this version of Minecraft");
			Launching = false;
			return LaunchResult.VersionMismatch;
		}

		LaunchProgress += 10;
		LaunchProgressChanged?.Invoke(null, EventArgs.Empty);

		await Minecraft.Open();
		log.Write(nameof(Launcher), "Opened game, waiting to launch mod...");
		
		LaunchProgress += 5;
		LaunchProgressChanged?.Invoke(null, EventArgs.Empty);

		string path;
		
		// if this is a web mod, download it in the meantime
		if (mod.FromInternet)
		{
			var downloadedPath = await ModDownloader.DownloadMod(mod);
			if (downloadedPath == string.Empty)
			{
				Launching = false;
				return LaunchResult.DownloadFailed;
			}

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
		
		mod.RealPath = path;

		// either wait for the game's modules to load
		// or if the user has injection delay enabled, wait for the time they specified
		if (JiayiSettings.Instance.UseInjectionDelay)
			Task.Delay(JiayiSettings.Instance.InjectionDelay[2] * 1000).Wait();
		else
			await Minecraft.WaitForModules();
		
		LaunchProgress += 25;
		LaunchProgressChanged?.Invoke(null, EventArgs.Empty);
		
		if (!Minecraft.IsOpen)
		{
			Launching = false;
			return LaunchResult.GameNotFound;
		}

		// determine whether this is an internal or external mod
		bool external;
		if (path.EndsWith(".exe")) external = true;
		else if (path.EndsWith(".dll")) external = false;
		else
		{
			Launching = false;
			return LaunchResult.ModNotFound;
		}

		log.Write(nameof(Launcher), external ? "Detected external mod" : "Detected internal mod");
		
		if (!File.Exists(path))
		{
			Launching = false;
			return LaunchResult.ModNotFound;
		}

		if (external)
		{
			if (Process.GetProcessesByName(Path.GetFileNameWithoutExtension(path)).Length != 0)
			{
				Launching = false;
				return LaunchResult.AlreadyLoaded;
			}

			var info = new ProcessStartInfo
			{
				FileName = path,
				Arguments = mod.Arguments
			};
			
			Process.Start(info);
			LaunchProgress += 30;
			LaunchProgressChanged?.Invoke(null, EventArgs.Empty);
			Launching = false;
			
			Minecraft.ModsLoaded.Add(mod);
			Minecraft.StartUpdate();
			JiayiStats.Instance!.MostRecentMod = mod;
			
			return LaunchResult.Success;
		}

		// else
		if (Injector.IsInjected(path))
		{
			Launching = false;
			return LaunchResult.AlreadyLoaded;
		}

		var injected = await Injector.Inject(path);
		LaunchProgress += 30;
		LaunchProgressChanged?.Invoke(null, EventArgs.Empty);
		Launching = false;
		
		if (injected)
		{
			Minecraft.ModsLoaded.Add(mod);
			Minecraft.StartUpdate();
			JiayiStats.Instance!.MostRecentMod = mod;
		}

		return injected ? LaunchResult.Success : LaunchResult.InjectionFailed;
	}
}