using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using JiayiLauncher.Utils;

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
	
	// discord settings
	public Setting<bool> RichPresence { get; set; } = new("Enable rich presence", "Discord", 
		"Show what you're doing in Jiayi on Discord.", true);
	
	public Setting<bool> DiscordShowModName { get; set; } = new("Show mod name", "Discord", 
		"Show the name of the mod you're playing in Discord.", true, "RichPresence");
	
	public Setting<bool> DiscordShowGameVersion { get; set; } = new("Show game version", "Discord", 
		"Show the game version you're playing in Discord.", true, "RichPresence");
	
	public Setting<bool> DiscordCustomStatus { get; set; } = new("Custom status text", "Discord",
		"Use your own status text instead of the default ones.", false, "RichPresence");
	
	public Setting<string> DiscordDetails { get; set; } = new("Top text", "Discord",
		"The top-most status text.", "Playing with {modName}", "DiscordCustomStatus");
	
	public Setting<string> DiscordState { get; set; } = new("Bottom text", "Discord",
		"The bottom-most status text.", "on {gameVersion}", "DiscordCustomStatus");
	
	public Setting<string> DiscordAppId { get; set; } = new("Discord app ID", "Discord",
		"The Discord app ID to use for rich presence. Leave this blank to use the default Jiayi app ID.", string.Empty);
	
	public Setting<string> DiscordLargeImageKey { get; set; } = new("Large image key", "Discord",
		"The large image key to use for rich presence. Leave this blank to use the default Jiayi image.", string.Empty);
	
	public Setting<string> DiscordLargeImageText { get; set; } = new("Large image text", "Discord",
		"The large image text to use for rich presence. Leave this blank to use the default Jiayi text.", string.Empty);
	
	public Setting<string> DiscordSmallImageKey { get; set; } = new("Small image key", "Discord",
		"The small image key to use for rich presence. Leave this blank to use the default Jiayi image.", string.Empty);
	
	public Setting<string> DiscordSmallImageText { get; set; } = new("Small image text", "Discord",
		"The small image text to use for rich presence. Leave this blank to use the default Jiayi text.", string.Empty);

	public void Save()
	{
		Directory.CreateDirectory(Path.GetDirectoryName(_settingsPath)!);
		using var stream = File.OpenWrite(_settingsPath);
		JsonSerializer.Serialize(stream, this);
		Log.Write(this, "Saved settings.");
	}

	public static void Load()
	{
		var path = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "JiayiLauncher", "settings.json");

		if (!File.Exists(path))
		{
			Instance = new JiayiSettings();
			Instance.Save();
			Log.Write(Instance, "Created new settings file.");
			return;
		}

		using var stream = File.OpenRead(path);
		var settings = JsonSerializer.Deserialize<JiayiSettings>(stream);
		if (settings == null)
		{
			Instance = new JiayiSettings();
			Instance.Save();
			// TODO: show a notification
			Log.Write(Instance, "Settings file was corrupted or invalid. Created new settings file.");
			return;
		}
		
		Instance = settings;
		Log.Write(Instance, "Loaded settings.");
	}
	
	public Setting<T> GetSetting<T>(string name)
	{
		var properties = GetType().GetProperties();
		foreach (var property in properties)
		{
			if (property.PropertyType != typeof(Setting<T>)) continue;
			var setting = property.GetValue(this);
			if (setting is not Setting<T> s) continue;
			if (s.Name == name) return s;
		}

		throw new ArgumentException($"Setting {name} not found.");
	}
}