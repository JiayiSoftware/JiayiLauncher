using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using JiayiLauncher.Features.Mods;
using JiayiLauncher.Utils;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace JiayiLauncher.Features.Stats;

[Serializable]
public class JiayiStats
{
	public TimeSpan TotalPlayTime { get; set; } = TimeSpan.Zero;
	public long? MostPlayedMod { get; set; }
	public long? MostRecentMod { get; set; }

	[System.Text.Json.Serialization.JsonIgnore]
	private string _statsPath = Path.Combine(
		Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "JiayiLauncher", "stats.json");
	
	[System.Text.Json.Serialization.JsonIgnore]
	private JsonSerializerOptions? _options;
	
	[System.Text.Json.Serialization.JsonIgnore]
	private bool _loaded;

	public JiayiStats()
	{
	}

	public void Save()
	{
		if (!_loaded) Load();
		if (ModCollection.Current == null) return; // this func called after the attempt to load mods
		
		// crunch some numbers
		var mostPlayed = new List<Mod>(ModCollection.Current.Mods)
			.OrderByDescending(mod => mod.PlayTime)
			.Where(mod => mod.PlayTime != TimeSpan.Zero)
			.Where(mod => ModCollection.Current.HasMod(mod.Id.Value))
			.ToList();

		MostPlayedMod = mostPlayed.Count > 0 ? mostPlayed[0].Id : null;
		
		if (MostRecentMod != null && !ModCollection.Current.HasMod(MostRecentMod.Value))
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
		
		var json = File.ReadAllText(_statsPath);

		try
		{
			var stats = JsonConvert.DeserializeObject<JiayiStats>(json);
			if (stats == null)
			{
				_loaded = true;
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
			_loaded = true;
			Save();
			log.Write(nameof(JiayiStats), $"Stats file was corrupted or invalid. Created new stats file. Error: {e}");
		}
	}
}
