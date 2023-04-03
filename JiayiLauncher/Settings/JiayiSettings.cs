using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using JiayiLauncher.Features.Mods;
using JiayiLauncher.Utils;
using Microsoft.Win32;

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

	// note these types of settings should probably be ignored by the json serializer
	[Setting("Export collection", "General",
		"Export your mod collection to a file. You can share this with other people.")]
	[JsonIgnore] public (string, Action) ExportCollection { get; set; } = ("Export", () =>
	{
		var dialog = new SaveFileDialog
		{
			DefaultExt = "jiayi",
			Filter = "Jiayi mod collection (*.jiayi)|*.jiayi",
			InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
			Title = "Export mod collection"
		};
		if (dialog.ShowDialog() != true) return; // this is an action why do i have to return a bool

		var path = dialog.FileName;

		if (ModCollection.Current != null) ModCollection.Current.Export(path);
	});
	
	[Setting("Profile folder path", "General", "The path to the folder containing your profiles.")]
	public string ProfileCollectionPath { get; set; } = string.Empty;

	// discord settings
	[Setting("Enable rich presence", "Discord", "Show what you're doing in Jiayi on Discord.")]
	public bool RichPresence { get; set; } = true;
	
	// i can just use empty objects to display text in the settings window
	[Setting("Formatting strings", "Discord", 
		"Strings that can be used in custom status text:\n\n" +
		"%mod_name% - the name of the mod you're playing\n" +
		"%game_version% - the game version you're playing\n" +
		"%mod_count% - the number of mods you have in your collection", "RichPresence")]
	private object DiscordFormattingStrings { get; set; } = new();
	
	[Setting("Top text", "Discord", "The top-most status text.", "RichPresence")]
	public string DiscordDetails { get; set; } = "Playing with %mod_name%";
	
	[Setting("Bottom text", "Discord", "The bottom-most status text.", "RichPresence")]
	public string DiscordState { get; set; } = "on %game_version%";
	
	[Setting("Show elapsed time", "Discord", "Show how long you've been playing for.", "RichPresence")]
	public bool DiscordShowElapsedTime { get; set; } = true;
	
	[Setting("Discord app ID", "Discord", "The Discord app ID to use for rich presence. Leave this blank to use the default Jiayi app ID.", "RichPresence")]
	public string DiscordAppId { get; set; } = string.Empty;
	
	[Setting("Large image key", "Discord", "The large image key to use for rich presence. Leave this blank to use the default Jiayi image.", "RichPresence")]
	public string DiscordLargeImageKey { get; set; } = string.Empty;
	
	[Setting("Small image key", "Discord", "The small image key to use for rich presence. Leave this blank to use the default Jiayi image.", "RichPresence")]
	public string DiscordSmallImageKey { get; set; } = string.Empty;
	
	[Setting("Large image text", "Discord", "The large image text to use for rich presence.", "RichPresence")]
	public string DiscordLargeImageText { get; set; } = "Jiayi Launcher";

	[Setting("Small image text", "Discord", "The small image text to use for rich presence.", "RichPresence")]
	public string DiscordSmallImageText { get; set; } = "Minecraft for Windows";
	
	// launch settings
	[Setting("Use injection delay", "Launch", "Wait for a set amount of time instead of waiting for the game to load before injecting.")]
	public bool UseInjectionDelay { get; set; } = false;

	[Setting("Injection delay", "Launch", "The amount of time to wait before injecting, in seconds.",
		"UseInjectionDelay")]
	public int[] InjectionDelay { get; set; } = { 0, 30, 5 };
	
	[Setting("Override module requirement", "Launch",
		"Override the amount of loaded modules needed before Jiayi will inject.\n\n" +
		"If the launcher injects too early or too late, try changing this value.")]
	public bool OverrideModuleRequirement { get; set; } = false;
	
	[Setting("Module requirement", "Launch",
		"The amount of loaded modules needed before Jiayi will inject.", "OverrideModuleRequirement")]
	public int[] ModuleRequirement { get; set; } = { 150, 180, 160 };
	
	[Setting("Accelerate game loading", "Launch",
		"Speed up loading times by terminating unnecessary processes. Beware of jank.")]
	public bool AccelerateGameLoading { get; set; } = false;

	public void Save()
	{
		Directory.CreateDirectory(Path.GetDirectoryName(_settingsPath)!);
		
		// wipe first
		File.WriteAllText(_settingsPath, string.Empty);
		
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

		try
		{
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
		catch (Exception e)
		{
			stream.Dispose();
			Instance = new JiayiSettings();
			Instance.Save();
			Log.Write(Instance, $"Settings file was corrupted or invalid. Created new settings file. Error: {e}");
		}
	}
	
	public List<PropertyInfo> GetSettings() => GetType().GetProperties().Where(p => p.GetCustomAttribute<SettingAttribute>() != null).ToList();
	
	public PropertyInfo? GetSetting(string name) => GetSettings().FirstOrDefault(p => p.Name == name);
	
	public List<string> GetCategories() => GetSettings().Select(p => p.GetCustomAttribute<SettingAttribute>()!.Category).Distinct().ToList();
	
	public List<PropertyInfo> GetSettingsInCategory(string category) => GetSettings().Where(p => p.GetCustomAttribute<SettingAttribute>()!.Category == category).ToList();
}