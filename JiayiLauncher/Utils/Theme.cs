using JiayiLauncher.Appearance;
using JiayiLauncher.Settings;
using JiayiLauncher.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
public class LocalTheme
{
    private static string ThemeRoot = Path.Combine(ThemeState.WWWRootPath, "themes");

    public LocalTheme(string name)
    {
        Name = name;
    }

    public static LocalTheme[] GetAllThemes()
    {
        List<LocalTheme> localThemes = new List<LocalTheme>();

        var directories = Directory.GetDirectories(Path.Combine(ThemeRoot, ".local"));
        foreach (var d in directories)
        {
            var name = new DirectoryInfo(d).Name;
            var theme = new LocalTheme(name);
            localThemes.Add(theme);
        }

        if (localThemes.Count <= 0)
        {
            localThemes.Add(CreateTheme("default"));
        }

        return localThemes.ToArray();
    }

    public static LocalTheme? CreateTheme(string name)
    {
        var path = Path.Combine(ThemeRoot, $".local\\{name}");
        if (Directory.Exists(path))
        {
            return null;
        }

        Directory.CreateDirectory(path);

        var buffer = File.Create(Path.Combine(path, "theme.css"));
        var defaultTheme = new ThemeState().ThemeCSS.ToString();
        byte[] byteArray = Encoding.UTF8.GetBytes(defaultTheme);
        buffer.Write(byteArray, 0, byteArray.Length);
        buffer.Close();

        return new LocalTheme(name);
    }

    public static void SaveCurrentTheme()
    {
        var buffer = File.OpenWrite(Path.Combine(ThemeRoot, JiayiSettings.Instance.Theme, "theme.css"));
        var themeCSS = ThemeState.Instance.ThemeCSS.ToString();
        byte[] byteArray = Encoding.UTF8.GetBytes(themeCSS);
        buffer.Write(byteArray, 0, byteArray.Length);
        buffer.Close();
    }

    public string Name;
}

public class Metadata
{
    [JsonProperty("author")]
    public string Author;
    [JsonProperty("tags")]
    public List<string> Tags;
    [JsonProperty("raw_tags")]
    public List<string> RawTags;

    public static readonly List<string> RAW_TAGS = new() { "Dark", "Light", "Animated" };

}

public class PublicTheme : Metadata
{
    public static PublicTheme[]? GetAllThemes()
    {
        string url = "https://raw.githubusercontent.com/JiayiSoftware/jiayi-themes/main/all_themes.json";
        string response = InternetManager.Client.GetStringAsync(url).Result;
        var data = JsonConvert.DeserializeObject<PublicTheme[]>(response);

        if (data != null)
        {
            return data;
        }
        else
        {
            Log.Write("Theme", $"Failed to retrieve public themes", Log.LogLevel.Error);
            return null;
        }
    }

    [JsonProperty("Name")]
    public string Name;


    [JsonProperty("bg")]
    public Uri Background;
    [JsonProperty("meta")]
    public Uri Metadata;
    [JsonProperty("theme")]
    public Uri Theme;
}

