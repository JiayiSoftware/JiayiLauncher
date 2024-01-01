using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Timers;
using JiayiLauncher.Features.Launch;
using JiayiLauncher.Features.Mods;
using JiayiLauncher.Features.Stats;
using JiayiLauncher.Settings;
using JiayiLauncher.Utils;

namespace JiayiLauncher.Features.Game;

public static class Minecraft
{
	private static readonly Timer _timer = new(1000);
	private static bool _callbackSet;

	public static List<Mod> ModsLoaded { get; } = new();

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

	public static async Task Open()
	{
		var minecraftApp = await PackageData.GetPackage();
		if (minecraftApp == null) return;
		await minecraftApp.LaunchAsync();

		Process = Process.GetProcessesByName("Minecraft.Windows")[0];
	}

	public static void StartUpdate()
	{
		if (_timer.Enabled) return;

		if (_callbackSet)
		{
			_timer.Start();
			return;
		}
		
		_timer.Elapsed += (_, _) =>
		{
			if (!IsOpen)
			{
				_timer.Stop();
				ModsLoaded.Clear();
			}
			else
			{
				JiayiStats.Instance!.TotalPlayTime += TimeSpan.FromSeconds(1);

				foreach (var mod in ModsLoaded)
				{
					mod.PlayTime += TimeSpan.FromSeconds(1);
					
					var path = mod.RealPath ?? mod.Path;
					
					bool external;
					if (path.EndsWith(".exe")) external = true;
					else if (path.EndsWith(".dll")) external = false;
					else
					{
						Log.Write(nameof(Minecraft), 
							$"Loaded mod {mod.Name} mysteriously disappeared. Removing from list.", Log.LogLevel.Warning);
						ModsLoaded.Remove(mod);
						break; // throws an exception if we don't break
					}
					
					// check if the mod is still loaded
					if (external)
					{
						if (Process.GetProcessesByName(Path.GetFileNameWithoutExtension(path)).Length != 0) continue;
						
						ModsLoaded.Remove(mod);
						Log.Write(nameof(Minecraft), $"{mod.Name} is no longer loaded");
						break;
					}

					// internal mod
					if (Injector.IsInjected(path)) continue;
						
					ModsLoaded.Remove(mod);
					Log.Write(nameof(Minecraft), $"{mod.Name} is no longer loaded");
					break;
				}
			}
			
			JiayiStats.Save();
			ModCollection.Current!.Save();
		};
		
		_callbackSet = true;
		_timer.Start();
	}

	public static async Task WaitForModules()
	{
		await Task.Run(() =>
		{
			while (true)
			{
				Process.Refresh();

				try
				{
					if (JiayiSettings.Instance.OverrideModuleRequirement 
					    && Process.Modules.Count > JiayiSettings.Instance.ModuleRequirement[2])
						break;
					
					if (!IsOpen) break;
					if (Process.Modules.Count > 165) break;
					
					if (JiayiSettings.Instance.AccelerateGameLoading) AccelerateGameLoading();
				}
				catch (Win32Exception e)
				{
					// the operation completed successfully ????
					if (e.NativeErrorCode != 0)
					{
						// check if the game is still open
						if (!IsOpen) break;

						throw; // it is still open ☹
					}
				}

				// wait for a bit
				Task.Delay(100).Wait();
			}
		});
	}

	private static void AccelerateGameLoading()
	{
		var brokers = Process.GetProcessesByName("RuntimeBroker");
		if (brokers.Length <= 0) return;
		
		foreach (var broker in brokers)
		{
			broker.Kill();
		}
	}

	public static async Task<bool> ModSupported(Mod mod)
	{
		var version = await PackageData.GetVersion();
		Log.Write(nameof(Minecraft), $"Current game version is {version} and mod supports {string.Join(", ", mod.SupportedVersions)}");
		return mod.SupportedVersions.Contains(version) 
		       || mod.SupportedVersions.Contains("Any version")
		       || mod.SupportedVersions.Contains("any version"); // for legacy support
	}
}