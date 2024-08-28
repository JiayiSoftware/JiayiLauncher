using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using JiayiLauncher.Features.Mods;
using JiayiLauncher.Utils;

namespace JiayiLauncher.Features.Stats;

[Serializable]
public class JiayiStats
{
	public TimeSpan TotalPlayTime { get; set; } = TimeSpan.Zero;
	public Mod? MostPlayedMod { get; set; }
	public Mod? MostRecentMod { get; set; }

	[JsonIgnore]
	private string _statsPath = Path.Combine(
		Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "JiayiLauncher", "stats.json");
	
	[JsonIgnore]
	private JsonSerializerOptions? _options;
	
	[JsonIgnore]
	private bool _loaded;

	public JiayiStats()
	{
		Save();
	}

	public void Save()
	{
		if (!_loaded) Load();
		if (ModCollection.Current == null) return; // this func called after the attempt to load mods
		
		// crunch some numbers
		var mostPlayed = new List<Mod>(ModCollection.Current.Mods)
			.OrderByDescending(mod => mod.PlayTime)
			.Where(mod => mod.PlayTime != TimeSpan.Zero)
			.Where(mod => ModCollection.Current.HasMod(mod.Path))
			.ToList();

		MostPlayedMod = mostPlayed.Count > 0 ? mostPlayed[0] : null;
		
		if (MostRecentMod != null && !ModCollection.Current.HasMod(MostRecentMod.Path))
			MostRecentMod = null;
		
		if (ModCollection.Current.Mods.Count == 0)
		{
			MostPlayedMod = null;
			MostRecentMod = null;
		}
		
		File.WriteAllText(_statsPath, string.Empty);
		
		_options ??= new JsonSerializerOptions { WriteIndented = true };
		
		using var stream = File.OpenWrite(_statsPath);
		JsonSerializer.Serialize(stream, this, _options);
	}

	public void Load()
	{
		var log = Singletons.Get<Log>();
		
		if (!File.Exists(_statsPath))
		{
			_loaded = true;
			Save();
			log.Write(nameof(JiayiStats), "Created new stats file.");
			return;
		}
		
		using var stream = File.OpenRead(_statsPath);

		try
		{
			_options ??= new JsonSerializerOptions { WriteIndented = true };
			
			var stats = JsonSerializer.Deserialize<JiayiStats>(stream, _options);
			if (stats == null)
			{
				Save();
				log.Write(nameof(JiayiStats), "Stats file was corrupted or invalid. Created new stats file.");
				return;
			}
			
			TotalPlayTime = stats.TotalPlayTime;
			MostPlayedMod = stats.MostPlayedMod;
			MostRecentMod = stats.MostRecentMod;
			_loaded = true;
			
			log.Write(nameof(JiayiStats), "Loaded stats.");
		}
		catch (Exception e)
		{
			stream.Close();
			Save();
			log.Write(nameof(JiayiStats), $"Stats file was corrupted or invalid. Created new stats file. Error: {e}");
		}
	}
}
