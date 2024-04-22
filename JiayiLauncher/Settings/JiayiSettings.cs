using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Blazored.Toast;
using Blazored.Toast.Services;
using JiayiLauncher.Appearance;
using JiayiLauncher.Features.Mods;
using JiayiLauncher.Features.Versions;
using JiayiLauncher.Localization;
using JiayiLauncher.Shared;
using JiayiLauncher.Shared.Components.Toasts;
using JiayiLauncher.Utils;
using Microsoft.AspNetCore.Components;
using Microsoft.Win32;
using WindowsAPICodePack.Dialogs;
using Color = System.Drawing.Color;

namespace JiayiLauncher.Settings;

[Serializable]
public class JiayiSettings
{
    public static JiayiSettings? Instance;
    public static JiayiSettings Default => new(new ThemeState());
    
    private static string _settingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "JiayiLauncher", "settings.json");
    private static JsonSerializerOptions? _options;
    
    private ThemeState _themeState = ThemeState.Instance;

    [JsonConstructor]
    public JiayiSettings() { }
    
    public JiayiSettings(ThemeState themeState)
    {
        _themeState = themeState;
    }

    private string HexToColor(Color c)
    {
        return "#" + c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
    }
    
    // general settings
    [Setting("Mod folder path", "General", "The path to the folder containing your mods.", canReset: false)]
    public string ModCollectionPath { get; set; } = string.Empty;

    // note these types of settings should probably be ignored by the json serializer
    [JsonIgnore]
    [Setting("Export collection", "General",
        "Export your mod collection to a file. You can share this with other people.")]
    public (string, Action) ExportCollection { get; set; } = (Strings.Export, () =>
    {
        // var dialog = new SaveFileDialog
        // {
        //     DefaultExt = "jiayi",
        //     Filter = "Jiayi mod collection (*.jiayi)|*.jiayi",
        //     InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
        //     Title = "Export mod collection"
        // };
        // if (dialog.ShowDialog() != true) return; // this is an action why do i have to return a bool
        //
        // var path = dialog.FileName;
        //
        // if (ModCollection.Current != null) ModCollection.Current.Export(path);
        
        using var dialog = new CommonSaveFileDialog(Strings.SettingsExportCollection);
        dialog.DefaultExtension = "jiayi";
        dialog.Filters.Add(new CommonFileDialogFilter(Strings.SettingsExportCollectionFilter, "*.jiayi"));
        dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        
        if (dialog.ShowDialog() != CommonFileDialogResult.Ok) return;
        var path = dialog.FileName;
        
        if (ModCollection.Current != null) ModCollection.Current.Export(path);
    });

    [Setting("Profile folder path", "General", "The path to the folder containing your profiles.", canReset: false)]
    public string ProfileCollectionPath { get; set; } = string.Empty;

    [Setting("Version folder path", "General", "The path to the folder containing your game installs.", canReset: false)]
    public string VersionsPath { get; set; } = string.Empty;

    [JsonIgnore]
    [Setting("Update version list", "General", "Update the list of available versions.")]
    public (string, Action) UpdateVersionList { get; set; } = (Strings.Update, () =>
    {
        Task.Run(() => VersionList.UpdateVersions(true));
    });

    [Setting("Shader folder path", "General", "The path to the folder containing your shaders.", canReset: false)]
    public string ShadersPath { get; set; } = string.Empty;

    // appearance settings (my favorite)
    [JsonIgnore]
    [Setting("Primary background color", "Appearance", "The primary background color of the launcher.")]
    public Color PrimaryBackgroundColor
    {
        get => ColorTranslator.FromHtml(_themeState.ThemeStyles.GetProperty(":root", "--background-primary")?.Value ?? "#0f0f0f");
        set => _themeState.UpdateTheme("--background-primary", HexToColor(value));
    }

    [JsonIgnore]
    [Setting("Secondary background color", "Appearance", "The secondary background color of the launcher.")]
    public Color SecondaryBackgroundColor
    {
        get => ColorTranslator.FromHtml(_themeState.ThemeStyles.GetProperty(":root", "--background-secondary")?.Value ?? "#1e1e1e");
        set => _themeState.UpdateTheme("--background-secondary", HexToColor(value));
    }

    [JsonIgnore]
    [Setting("Accent color", "Appearance", "The accent color of the launcher.")]
    public Color AccentColor
    {
        get => ColorTranslator.FromHtml(_themeState.ThemeStyles.GetProperty(":root", "--accent")?.Value ?? "#dc0000");
        set => _themeState.UpdateTheme("--accent", HexToColor(value));
    }

    [JsonIgnore]
    [Setting("Text color", "Appearance", "The color of text seen throughout the launcher.")]
    public Color TextColor
    {
        get => ColorTranslator.FromHtml(_themeState.ThemeStyles.GetProperty(":root", "--text-primary")?.Value ?? "#ffffff");
        set => _themeState.UpdateTheme("--text-primary", HexToColor(value));
    }

    [JsonIgnore]
    [Setting("Text color (on accent)", "Appearance", "The color of text on top of the accent color.")]
    public Color AccentTextColor
    {
        get => ColorTranslator.FromHtml(_themeState.ThemeStyles.GetProperty(":root", "--text-accent")?.Value ?? "#ffffff");
        set => _themeState.UpdateTheme("--text-accent", HexToColor(value));
    }

    [JsonIgnore]
    [Setting("Gray text color", "Appearance", "A gray version of the text color.")]
    public Color GrayTextColor
    {
        get => ColorTranslator.FromHtml(_themeState.ThemeStyles.GetProperty(":root", "--text-grayed")?.Value ?? "#7e7e7e");
        set => _themeState.UpdateTheme("--text-grayed", HexToColor(value));
    }

    [JsonIgnore]
    [Setting("Shadow distance", "Appearance", "The distance of the shadows on UI elements.")]
    public int[] ShadowDistance
    {
        get => [0, 10, int.Parse(Regex.Match(_themeState.ThemeStyles.GetProperty(":root", "--shadow")?.Value ?? "5", 
            @"\d+").Value, CultureInfo.InvariantCulture)];
        set => _themeState.UpdateTheme("--shadow", $"{value[2]}px {value[2]}px rgba(0, 0, 0, 0.4)");
    }

    [JsonIgnore]
    [Setting("UI movement speed", "Appearance", "The speed at which the UI moves.")]
    public float[] MovementSpeed
    {
        get => [0, 0.5f, float.Parse(Regex.Match(_themeState.ThemeStyles.GetProperty(":root", "--transition-speed")?.Value ?? "0.2", 
            @"\d*\.?\d*").Value, CultureInfo.InvariantCulture)];
        set => _themeState.UpdateTheme("--transition-speed", $"{value[2]}s");
    }

    [JsonIgnore]
    [Setting("Use background image", "Appearance", "Use an image as the background of the launcher.")]
    public bool UseBackgroundImage
    {
        get => _themeState.ThemeStyles.GetProperty(":root", "--background-image")?.Value != "none";
        set => _themeState.UpdateTheme("--background-image", value ? "url('')" : "none");
    }
    
    [JsonIgnore]
    [Setting("Select background image", "Appearance", "Select a background image from your computer.", "UseBackgroundImage")]
    public (string, Action) SelectBackground { get; set; } = (Strings.Select, async () =>
    {
        // var dialog = new OpenFileDialog
        // {
        //     DefaultExt = "png",
        //     Filter = "Image files (*.png;*.jpg;*.jpeg;*.gif;*.bmp)|*.png;*.jpg;*.jpeg;*.gif;*.bmp" +
        //              "|Video files (*.mp4;*.mov;*.webm)|*.mp4;*.mov;*.webm",
        //     InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
        //     Title = "Select background image"
        // };
        // if (dialog.ShowDialog() != true) return;

        using var dialog = new CommonOpenFileDialog(Strings.SettingsSelectBackground);
        dialog.Filters.Add(new CommonFileDialogFilter(Strings.SettingsSelectBackgroundImageFilter, "*.png;*.jpg;*.jpeg;*.gif;*.bmp"));
        dialog.Filters.Add(new CommonFileDialogFilter(Strings.SettingsSelectBackgroundVideoFilter, "*.mp4;*.mov;*.webm"));
        dialog.DefaultExtension = "png";
        dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        
        if (dialog.ShowDialog() != CommonFileDialogResult.Ok) return;
        var path = dialog.FileName;

        var themeRoot = Path.Combine(ThemeState.RootPath, "themes", Instance.Theme);
        var dest = Path.Combine(themeRoot, "background" + Path.GetExtension(path));
        
        File.Copy(path, dest, true);
        
        Instance.BackgroundImageUrl = $"./themes/{Instance.Theme}/background{Path.GetExtension(path)}";
        Instance.Save();
    });

    // [Setting("Background image URL", "Appearance", "The URL of the image to use as the background. Videos are also supported.",
    //     "UseBackgroundImage", "The URL should be a link to an image or video on the internet. A path to a local image will not work.")]
    public string BackgroundImageUrl
    {
        get
        {
            if (Instance == null) return string.Empty;
            if (Instance.UseBackgroundImage) 
                return Regex.Match(_themeState.ThemeStyles.GetProperty(":root", "--background-image")?.Value ?? "", 
                    @"url\(\'(?<url>[^']+)\'\)").Groups["url"].Value;
            _themeState.UpdateTheme("--background-image", "none");
            return string.Empty;
        }
        set
        {
            // TODO: better fix for the launcher not loading backgrounds with the same extension
            var needRefresh = Path.GetExtension(BackgroundImageUrl) == Path.GetExtension(value);
            
            value = value.Replace("\\", "/");
            _themeState.UpdateTheme("--background-image", $"url('{value}')");
            
            if (needRefresh) MainLayout.Instance?.Reload();
        }
    }

    [JsonIgnore]
    [Setting("Generate palette from background", "Appearance",
        "Create a color palette based on your background image. Videos are NOT supported.", "UseBackgroundImage",
        "Palette generation is memory intensive and may take a while. Every generated palette is slightly different.")]
    public (string, Action) GeneratePalette { get; set; } = (Strings.Generate, () =>
    {
        if (!Instance!.UseBackgroundImage) return;
        PaletteGenerator.CreatePalette();
    });

    [JsonIgnore]
    [Setting("Background blur", "Appearance", "How much to blur the background by.", "UseBackgroundImage")]
    public int[] BackgroundBlur
    {
        get => [0, 100, int.Parse(Regex.Match(_themeState.ThemeStyles.GetProperty(":root", "--background-blur")?.Value ?? "0", 
            @"\d+").Value, CultureInfo.InvariantCulture)];
        set => _themeState.UpdateTheme("--background-blur", $"{value[2]}px");
    }

    [JsonIgnore]
    [Setting("Background brightness", "Appearance", "How bright the background should be.", "UseBackgroundImage")]
    public int[] BackgroundBrightness
    {
        get => [0, 100, int.Parse(Regex.Match(_themeState.ThemeStyles.GetProperty(":root", "--background-brightness")?.Value ?? "100"
            , @"\d+").Value, CultureInfo.InvariantCulture)];
        set => _themeState.UpdateTheme("--background-brightness", $"{value[2]}%");
    }

    [JsonIgnore]
    [Setting("Rounding", "Appearance", "How much to round the corners of most UI elements.")]
    public int[] Rounding
    {
        get => [0, 10, int.Parse(Regex.Match(_themeState.ThemeStyles.GetProperty(":root", "--rounding")?.Value ?? "0", 
            @"\d+").Value, CultureInfo.InvariantCulture)];
        set => _themeState.UpdateTheme("--rounding", $"{value[2]}px");
    }

    [JsonIgnore]
    [Setting("Border color", "Appearance", "The border color seen throughout the launcher.")]
    public Color BorderColor
    {
        get => ColorTranslator.FromHtml(_themeState.ThemeStyles.GetProperty(":root", "--border-primary")?.Value ?? "#000000");
        set => _themeState.UpdateTheme("--border-primary", HexToColor(value));
    }

    [JsonIgnore]
    [Setting("Border color (on accent)", "Appearance", "The color of the border around the accent color.")]
    public Color AccentBorderColor
    {
        get => ColorTranslator.FromHtml(_themeState.ThemeStyles.GetProperty(":root", "--border-accent")?.Value ?? "#000000");
        set => _themeState.UpdateTheme("--border-accent", HexToColor(value));
    }
    
    [JsonIgnore]
    [Setting("Border thickness", "Appearance", "The thickness of the borders on UI elements.")]
    public int[] BorderThickness
    {
        get => [0, 5, int.Parse(Regex.Match(_themeState.ThemeStyles.GetProperty(":root", "--border-thickness")?.Value ?? "0", 
            @"\d+").Value, CultureInfo.InvariantCulture)];
        set => _themeState.UpdateTheme("--border-thickness", $"{value[2]}px");
    }

    [JsonIgnore]
    [Setting("Show theme", "Appearance",
        "Reveal your theme in File Explorer. You can share this with other people or use other people's themes.")]
    public (string, Action) OpenTheme { get; set; } = (Strings.Open, () =>
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "explorer.exe ",
            Arguments = $"/select, \"{ThemeState.ThemePath}\"",
            UseShellExecute = true
        });
    }
    );

    [JsonIgnore]
    [Setting("Restore default theme", "Appearance", "Go back to Jiayi's default theme.", confirm: true)]
    public (string, Action) RestoreDefaultTheme { get; set; } = (Strings.Restore, () =>
    {
        var appearanceSettings = Default.GetSettingsInCategory("Appearance")
            .Select(setting => typeof(JiayiSettings).GetProperty(setting.Name));

        var defaults = Default;

        foreach (var property in appearanceSettings)
        {
            property?.SetValue(Instance, property.GetValue(defaults));
        }
        Instance.Save();
        MainLayout.Instance?.Reload(); // not sure why, but it isnt detecting this state change, but it does detect ResetToDefault();
    }
    );


    [JsonIgnore]
    [Setting("Themes menu", "Appearance", "Local and downloadable themes created by other users.", canReset: false)]
    public (string, string) ThemesMenu { get; set; } = (Strings.Open, "/themes");
    
    public string Theme { get; set; } = ".local/default";
    
    // discord settings
    [Setting("Enable rich presence", "Discord", "Show what you're doing in Jiayi on Discord.", tooltip: """
	Available formatting strings for rich presence:

	%mod_name% - the name of the mod you're playing with
	%game_version% - your current Minecraft version
	%mod_count% - how many mods you have in your collection
	%shader_name% - the name of the shader you're using
	%profile_count% - how many profiles you have in your collection
	%launcher_version% - the version of Jiayi you're using

	Only 1 formatting string can be used per field.
	""")]
    public bool RichPresence { get; set; } = true;

    [Setting("Top text", "Discord", "The top-most status text.", "RichPresence")]
    public string DiscordDetails { get; set; } = Strings.SettingsDiscordDetailsDefault;

    [Setting("Bottom text", "Discord", "The bottom-most status text.", "RichPresence")]
    public string DiscordState { get; set; } = Strings.SettingsDiscordStateDefault;

    [Setting("Show elapsed time", "Discord", "Show how long you've been playing for.", "RichPresence")]
    public bool DiscordShowElapsedTime { get; set; } = true;

    [Setting("Show download button", "Discord", "Show a button to download Jiayi.", "RichPresence")]
    public bool DiscordShowDownloadButton { get; set; } // = false; (because advertising by default is bad)

    [Setting("Share current mod", "Discord",
        "Show a button for other users to add the mod you're playing with. This only works with web mods.", "RichPresence")]
    public bool DiscordShareCurrentMod { get; set; } // = false;

    [Setting("Large image key", "Discord", "The large image key to use for rich presence.", "RichPresence")]
    public string DiscordLargeImageKey { get; set; } = "logo";

    [Setting("Small image key", "Discord", "The small image key to use for rich presence.", "RichPresence")]
    public string DiscordSmallImageKey { get; set; } = "minecraft";

    [Setting("Large image text", "Discord", "The large image text to use for rich presence.", "RichPresence")]
    public string DiscordLargeImageText { get; set; } = Strings.JiayiLauncher;

    [Setting("Small image text", "Discord", "The small image text to use for rich presence.", "RichPresence")]
    public string DiscordSmallImageText { get; set; } = Strings.MinecraftForWindows;

    // update settings
    [Setting("Enable the updater", "Update", "Allow the launcher to check for and download updates.")]
    public bool EnableUpdater { get; set; } = true;

    [Setting("Automatically download updates", "Update", "Always download updates when they're available.")]
    public bool AutoDownloadUpdates { get; set; } = false;

    // network settings
    [Setting("Offline mode", "Network",
        "Disable access to the internet. Some launcher features may not work. Restart Jiayi for this to take full effect.",
        tooltip: "Features like internet mods, downloading versions, and checking for updates will not work.")]
    public bool OfflineMode { get; set; } = false;

    [Setting("Follow redirects", "Network",
        "Follow redirects when adding web mods. Change this setting if you have trouble adding some mods.")]
    public bool FollowRedirects { get; set; } = false;

    // launch settings
    [Setting("Use injection delay", "Launch", 
        "Wait for a set amount of time instead of waiting for the game to load before injecting.")]
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
    public int[] ModuleRequirement { get; set; } = { 150, 185, 165 };

    [Setting("Accelerate game loading", "Launch",
        "Speed up loading times by terminating unnecessary processes. Beware of jank.",
        tooltip: "May cause issues related to Microsoft Store licensing (determining whether you own the game or not).")]
    public bool AccelerateGameLoading { get; set; } = false;
    
    [Setting("Minimize fix", "Launch", "Prevents the game from suspending itself when minimized. Changes are applied upon launch.")]
    public bool MinimizeFix { get; set; } = true;

    // log settings
    [Setting("Anonymize logs", "Logs",
        "Hides any identifying information from logs. This does not apply retroactively.")]
    public bool AnonymizeLogs { get; set; } = true;

    [JsonIgnore]
    [Setting("Open log folder", "Logs", "Open the log folder.")]
    public (string, Action) OpenLogFolder { get; set; } = (Strings.Open, () =>
    {
        var info = new ProcessStartInfo
        {
            UseShellExecute = true,
            Verb = "open",
            FileName = Log.LogPath
        };

        Process.Start(info);
    }
    );

    [JsonIgnore]
    [Setting("Clear previous logs", "Logs", "Clear all previous log files.", confirm: true)]
    public (string, Action) ClearPreviousLogs { get; set; } = (Strings.Clear, () =>
    {
        var path = Path.Combine(Log.LogPath, "Previous");
        if (!Directory.Exists(path)) return;

        Directory.Delete(path, true);
        Directory.CreateDirectory(path);
    }
    );

    public void Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_settingsPath)!);

        // wipe first
        File.WriteAllText(_settingsPath, string.Empty);

        _options ??= new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new ColorJsonConverter() }
        };

        using var stream = File.OpenWrite(_settingsPath);
        JsonSerializer.Serialize(stream, this, _options);
        LocalTheme.SaveCurrentTheme();
        
        #if DEBUG
        Log.Write(this, "Saved settings.");
        #endif
    }

    public static void Load()
    {
        if (!File.Exists(_settingsPath))
        {
            Instance = Default;
            Instance.Save();
            Log.Write(Instance, "Created new settings file.");
            return;
        }

        using var stream = File.OpenRead(_settingsPath);
        
        // in case of failure
        var toastParams = new ToastParameters()
            .Add(nameof(JiayiToast.Level), ToastLevel.Warning)
            .Add(nameof(JiayiToast.Title), Strings.SettingsMenuName)
            .Add(nameof(JiayiToast.Content), new RenderFragment(builder =>
            {
                builder.OpenElement(0, "p");
                builder.AddContent(1, Strings.SettingsInvalidSettingsText);
                builder.CloseElement();

                builder.OpenElement(2, "p");
                builder.AddContent(3,
                    Strings.SettingsInvalidSettingsText2);
                builder.CloseElement();
            }))
            .Add(nameof(JiayiToast.Buttons), new List<(string, EventCallback)>
            {
                (Strings.Okay, EventCallback.Empty)
            });

        try
        {
            _options ??= new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters = { new ColorJsonConverter() }
            };

            var settings = JsonSerializer.Deserialize<JiayiSettings>(stream, _options);
            if (settings == null)
            {
                Instance = Default;
                Instance.Save();
                
                BlazorBridge.ShowToast(toastParams, toastSettings => toastSettings.DisableTimeout = true);
                
                Log.Write(Instance, "Settings file was corrupted or invalid. Created new settings file.", Log.LogLevel.Warning);
                return;
            }

            Instance = settings;
            Log.Write(Instance, "Loaded settings.");
        }
        catch (Exception e)
        {
            stream.Close();
            Instance = Default;
            Instance.Save();
            
            BlazorBridge.ShowToast(toastParams, toastSettings => toastSettings.DisableTimeout = true);
            
            Log.Write(Instance, $"Settings file was corrupted or invalid. Created new settings file. Error: {e}",
                Log.LogLevel.Warning);
        }
    }

    public List<PropertyInfo> GetSettings() => GetType().GetProperties()
        .Where(p => p.GetCustomAttribute<SettingAttribute>() != null).ToList();

    public PropertyInfo? GetSetting(string name) => GetSettings().FirstOrDefault(p => p.Name == name);

    public List<string> GetCategories() => GetSettings()
        .Select(p => p.GetCustomAttribute<SettingAttribute>()!.Category).Distinct().ToList();

    public List<PropertyInfo> GetSettingsInCategory(string category) => GetSettings()
        .Where(p => p.GetCustomAttribute<SettingAttribute>()!.Category == category).ToList();

    public bool IsDefault(PropertyInfo property)
    {
        var defaultValue = property.GetValue(Default);
        var currentValue = property.GetValue(this);

        // slider settings are a special case
        if (property.PropertyType == typeof(int[]))
        {
            var defaultSlider = (int[])defaultValue!;
            var currentSlider = (int[])currentValue!;
            return defaultSlider[2] == currentSlider[2];
        }

        // floats too
        if (property.PropertyType == typeof(float[]))
        {
            var defaultSlider = (float[])defaultValue!;
            var currentSlider = (float[])currentValue!;
            return defaultSlider[2] == currentSlider[2];
        }

        // Buttons
        if (property.PropertyType == typeof ((string, Action)))
        {
            return true;
        }

        return Equals(defaultValue, currentValue);
    }
    
    public string GetLocalizedCategory(string category)
    {
        var strings = typeof(Strings).GetProperties(BindingFlags.NonPublic | BindingFlags.Static);
        return strings.FirstOrDefault(s => s.Name == "Settings" + category)?.GetValue(null) as string ?? category;
    }
    
    public LocalizedSetting GetLocalizedSetting(PropertyInfo property)
    {
        var strings = typeof(Strings).GetProperties(BindingFlags.NonPublic | BindingFlags.Static);

        var attribute = property.GetCustomAttribute<SettingAttribute>();
        var category = strings.FirstOrDefault(s => s.Name == "Settings" + attribute?.Category);

        var name = strings.FirstOrDefault(s => s.Name.Contains(property.Name));
        var description = strings.FirstOrDefault(s => s.Name.Contains(property.Name + "Desc"));
        var tooltip = strings.FirstOrDefault(s => s.Name.Contains(property.Name + "Tooltip"));

        return new LocalizedSetting(
            name?.GetValue(null) as string ?? attribute?.Name ?? string.Empty,
            category?.GetValue(null) as string ?? attribute?.Category ?? string.Empty,
            description?.GetValue(null) as string ?? attribute?.Description ?? string.Empty,
            tooltip?.GetValue(null) as string ?? attribute?.Tooltip ?? string.Empty
        );
    }

    public void ResetToDefault(PropertyInfo setting)
    {
        setting.SetValue(this, setting.GetValue(Default));
        Save();
    }
}