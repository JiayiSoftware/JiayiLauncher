using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using JiayiLauncher.Settings.Special;
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
	[Setting("Mod folder path", "General", "The path to the folder containing your mods.")]
	public string ModCollectionPath { get; set; } = string.Empty;

	// discord settings
	[Setting("Enable rich presence", "Discord", "Show what you're doing in Jiayi on Discord.")]
	public bool RichPresence { get; set; } = true;
	
	[Setting("Show mod name", "Discord", "Show the name of the mod you're playing in Discord.", "RichPresence")]
	public bool DiscordShowModName { get; set; } = true;
	
	[Setting("Show game version", "Discord", "Show the game version you're playing in Discord.", "RichPresence")]
	public bool DiscordShowGameVersion { get; set; } = true;
	
	[Setting("Custom status text", "Discord", "Use your own status text instead of the default ones.", "RichPresence")]
	public bool DiscordCustomStatus { get; set; } = false;
	
	// i can just use empty objects to display text in the settings window
	[Setting("Formatting strings", "Discord", 
		"Strings that can be used in custom status text:\n" +
		"%mod_name% - the name of the mod you're playing\n" +
		"%game_version% - the game version you're playing\n" +
		"%mod_count% - the number of mods you have in your collection", "DiscordCustomStatus")]
	private object DiscordFormattingStrings { get; set; } = new();
	
	[Setting("Top text", "Discord", "The top-most status text.", "DiscordCustomStatus")]
	public string DiscordDetails { get; set; } = "Playing with %mod_name%";
	
	[Setting("Bottom text", "Discord", "The bottom-most status text.", "DiscordCustomStatus")]
	public string DiscordState { get; set; } = "on %game_version%";
	
	[Setting("Discord app ID", "Discord", "The Discord app ID to use for rich presence. Leave this blank to use the default Jiayi app ID.", "RichPresence")]
	public string DiscordAppId { get; set; } = string.Empty;
	
	[Setting("Large image key", "Discord", "The large image key to use for rich presence. Leave this blank to use the default Jiayi image.", "RichPresence")]
	public string DiscordLargeImageKey { get; set; } = string.Empty;
	
	[Setting("Large image text", "Discord", "The large image text to use for rich presence. Leave this blank to use the default Jiayi text.", "RichPresence")]
	public string DiscordLargeImageText { get; set; } = string.Empty;
	
	[Setting("Small image key", "Discord", "The small image key to use for rich presence. Leave this blank to use the default Jiayi image.", "RichPresence")]
	public string DiscordSmallImageKey { get; set; } = string.Empty;
	
	[Setting("Small image text", "Discord", "The small image text to use for rich presence. Leave this blank to use the default Jiayi text.", "RichPresence")]
	public string DiscordSmallImageText { get; set; } = string.Empty;
	
	// injection settings
	[Setting("Use injection delay", "Injection", "Wait for a set amount of time instead of waiting for the game to load before injecting.")]
	public bool UseInjectionDelay { get; set; } = false;
	
	[Setting("Injection delay", "Injection", "The amount of time to wait before injecting, in seconds.", "UseInjectionDelay")]
	public SliderSetting InjectionDelay { get; set; } = new(0, 30, 5);

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
	
	public List<PropertyInfo> GetSettings() => GetType().GetProperties().Where(p => p.GetCustomAttribute<SettingAttribute>() != null).ToList();
	
	public PropertyInfo? GetSetting(string name) => GetSettings().FirstOrDefault(p => p.Name == name);
	
	public List<string> GetCategories() => GetSettings().Select(p => p.GetCustomAttribute<SettingAttribute>()!.Category).Distinct().ToList();
	
	public List<PropertyInfo> GetSettingsInCategory(string category) => GetSettings().Where(p => p.GetCustomAttribute<SettingAttribute>()!.Category == category).ToList();
}