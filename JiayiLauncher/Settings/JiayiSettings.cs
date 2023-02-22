using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace JiayiLauncher.Settings;

[Serializable]
public class JiayiSettings
{
	[JsonIgnore] // idk if static fields are serialized but just in case
	public static JiayiSettings? Instance { get; set; }
	
	private string _settingsPath = Path.Combine(
		Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "JiayiLauncher", "settings.json");
	
	// general settings
	public Setting<string> ModCollectionPath { get; set; } = new("Mod folder path", "General", 
	"The path to your mod folder. If there is no folder at this path, Jiayi will create one.", string.Empty);

	// network settings
	public Setting<bool> EnforceHttps { get; set; } = new("Enforce HTTPS", "Network", 
		"Only allow secure connections for mod downloads.", true);

	public void Save()
	{
		Directory.CreateDirectory(Path.GetDirectoryName(_settingsPath)!);
		using var stream = File.OpenWrite(_settingsPath);
		JsonSerializer.Serialize(stream, this);
		Debug.WriteLine("Saved settings.");
	}

	public static void Load()
	{
		var path = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "JiayiLauncher", "settings.json");

		if (!File.Exists(path))
		{
			Instance = new JiayiSettings();
			Instance.Save();
			Debug.WriteLine("Created new settings file.");
			return;
		}

		using var stream = File.OpenRead(path);
		var settings = JsonSerializer.Deserialize<JiayiSettings>(stream);
		if (settings == null)
		{
			Instance = new JiayiSettings();
			Instance.Save();
			// TODO: show a notification
			return;
		}
		
		Instance = settings;
		Debug.WriteLine("Loaded settings.");
	}
}