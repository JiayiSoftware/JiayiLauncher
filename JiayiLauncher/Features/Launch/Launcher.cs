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

public class Launcher
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
	
	public bool Launching { get; private set; }
	
	public int LaunchProgress { get; private set; }
	
	public event EventHandler? LaunchProgressChanged;

	// the big method
	public async Task<LaunchResult> Launch(Mod mod)
	{
		if (Launching) return LaunchResult.AlreadyLaunching;
		Launching = true;
		
		var log = Singletons.Get<Log>();
		var minecraft = Singletons.Get<Minecraft>();
		var packageData = Singletons.Get<PackageData>();
		var modDownloader = Singletons.Get<ModDownloader>();
		var stats = Singletons.Get<JiayiStats>();
		
		LaunchProgress = 0;
		LaunchProgressChanged?.Invoke(null, EventArgs.Empty);
		log.Write(nameof(Launcher), $"Launching {mod.Name}");
		
		// minimize fix !
		await packageData.MinimizeFix(JiayiSettings.Instance.MinimizeFix);
		
		var supported = await minecraft.ModSupported(mod);
		if (!supported)
		{
			log.Write(nameof(Launcher), $"{mod.Name} does not support this version of Minecraft");
			Launching = false;
			return LaunchResult.VersionMismatch;
		}

		LaunchProgress += 10;
		LaunchProgressChanged?.Invoke(null, EventArgs.Empty);

		await minecraft.Open();
		log.Write(nameof(Launcher), "Opened game, waiting to launch mod...");
		
		LaunchProgress += 5;
		LaunchProgressChanged?.Invoke(null, EventArgs.Empty);

		string path;
		
		// if this is a web mod, download it in the meantime
		if (mod.FromInternet)
		{
			var downloadedPath = await modDownloader.DownloadMod(mod);
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
			await minecraft.WaitForModules();
		
		LaunchProgress += 25;
		LaunchProgressChanged?.Invoke(null, EventArgs.Empty);
		
		if (!minecraft.IsOpen)
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
			
			minecraft.ModsLoaded.Add(mod);
			minecraft.StartUpdate();
			stats.MostRecentMod = mod.Id;
			
			return LaunchResult.Success;
		}
		
		var injector = Singletons.Get<Injector>();

		// else
		if (injector.IsInjected(path))
		{
			Launching = false;
			return LaunchResult.AlreadyLoaded;
		}

		var injected = await injector.Inject(path);
		LaunchProgress += 30;
		LaunchProgressChanged?.Invoke(null, EventArgs.Empty);
		Launching = false;
		
		if (injected)
		{
			minecraft.ModsLoaded.Add(mod);
			minecraft.StartUpdate();
			stats.MostRecentMod = mod.Id;
		}

		return injected ? LaunchResult.Success : LaunchResult.InjectionFailed;
	}
}