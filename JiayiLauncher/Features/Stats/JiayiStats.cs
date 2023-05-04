using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using JiayiLauncher.Features.Mods;
using JiayiLauncher.Utils;

namespace JiayiLauncher.Features.Stats;

[Serializable]
public class JiayiStats
{
	public TimeSpan TotalPlayTime { get; set; } = TimeSpan.Zero;
	public Mod? MostPlayedMod { get; set; }
	public Mod? MostRecentMod { get; set; }

	private static string _statsPath = Path.Combine(
		Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "JiayiLauncher", "stats.json");
	private static JsonSerializerOptions? _options;
	
	public static JiayiStats? Instance { get; set; }
	
	public static void Save()
	{
		if (Instance == null) Load();
		if (ModCollection.Current == null) return; // this func called after the attempt to load mods
		
		// crunch some numbers
		var mostPlayed = new List<Mod>(ModCollection.Current.Mods)
			.OrderByDescending(mod => mod.PlayTime)
			.Where(mod => mod.PlayTime != TimeSpan.Zero)
			.ToList();

		Instance!.MostPlayedMod = mostPlayed.Count > 0 ? mostPlayed[0] : null;
		if (ModCollection.Current.Mods.Count == 0)
		{
			Instance.MostPlayedMod = null;
			Instance.MostRecentMod = null;
		}
		
		File.WriteAllText(_statsPath, string.Empty);
		
		_options ??= new JsonSerializerOptions { WriteIndented = true };
		
		using var stream = File.OpenWrite(_statsPath);
		JsonSerializer.Serialize(stream, Instance, _options);
	}

	public static void Load()
	{
		if (!File.Exists(_statsPath))
		{
			Instance = new JiayiStats();
			Save();
			Log.Write(nameof(JiayiStats), "Created new stats file.");
			return;
		}
		
		using var stream = File.OpenRead(_statsPath);

		try
		{
			_options ??= new JsonSerializerOptions { WriteIndented = true };
			
			var stats = JsonSerializer.Deserialize<JiayiStats>(stream, _options);
			if (stats == null)
			{
				Instance = new JiayiStats();
				Save();
				Log.Write(nameof(JiayiStats), "Stats file was corrupted or invalid. Created new stats file.");
				return;
			}
			
			Instance = stats;
			Log.Write(nameof(JiayiStats), "Loaded stats.");
		}
		catch (Exception e)
		{
			stream.Close();
			Instance = new JiayiStats();
			Save();
			Log.Write(nameof(JiayiStats), $"Stats file was corrupted or invalid. Created new stats file. Error: {e}");
		}
	}
}
