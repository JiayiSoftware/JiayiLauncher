using JiayiLauncher.Appearance;
using JiayiLauncher.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
public class LocalTheme
{
    private static string ThemeRoot = Path.Combine(ThemeState.WWWRootPath, "themes");

    public LocalTheme(string name)
    {
        Name = name;
    }

    public static LocalTheme[] GetAllThemes()
    {
        /*
         * <name>
         *   - theme.css
         *   - background.<png|jpg|gif|(other common image types) ; mp4|mov|webm>
         */
        List<LocalTheme> localThemes = new List<LocalTheme>();

        var directories = Directory.GetDirectories(ThemeRoot);
        foreach (var d in directories)
        {
            var name = new DirectoryInfo(d).Name;
            var matches = Regex.Match(name, "^local-(?<name>.+)$");
            if (matches.Groups["name"].Captures.Count <= 0)
            {
                Log.Write(nameof(LocalTheme), $"Skipping loading theme: {name}");
                continue;
            }

            var theme = new LocalTheme(matches.Groups["name"].Value);
            localThemes.Add(theme);
        }

        if (localThemes.Count <= 0) {
            localThemes.Add(CreateTheme("default"));
        }

        return localThemes.ToArray();
    }

    public static LocalTheme? CreateTheme(string name)
    {
        var path = Path.Combine(ThemeRoot, $"local-{name}");
        if (Directory.Exists(path))
        {
            return null;
        }

        Directory.CreateDirectory(path);

        var buffer = File.Create(Path.Combine(path, "theme.css"));
        var defaultTheme = new ThemeState().ThemeCSS.ToString();
        byte[] byteArray = Encoding.UTF8.GetBytes(defaultTheme);
        buffer.Write(byteArray, 0, byteArray.Length);

        return new LocalTheme(name);
    }

    public string Name;
}


public class PublicTheme
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

    [JsonProperty("author")]
    public string Author;
    [JsonProperty("tags")]
    public string[] Tags;
    [JsonProperty("raw_tags")]
    public string[] RawTags;

    [JsonProperty("bg")]
    public Uri Background;
    [JsonProperty("meta")]
    public Uri Metadata;
    [JsonProperty("theme")]
    public Uri Theme;
}

