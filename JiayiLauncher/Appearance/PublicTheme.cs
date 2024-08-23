using System;
using JiayiLauncher.Utils;
using Newtonsoft.Json;

namespace JiayiLauncher.Appearance;

public class PublicTheme : ThemeMetadata
{
    [JsonProperty("Name")]
    public string Name;

    [JsonProperty("bg")]
    public Uri? Background;
    
    [JsonProperty("meta")]
    public Uri Metadata;
    
    [JsonProperty("theme")]
    public Uri Theme;
    
    public static PublicTheme[]? GetAllThemes()
    {
        const string url = "https://raw.githubusercontent.com/JiayiSoftware/jiayi-themes/main/all_themes.json";
        var response = InternetManager.Client.GetStringAsync(url).Result;
        var data = JsonConvert.DeserializeObject<PublicTheme[]>(response);

        if (data != null)
        {
            return data;
        }

        var log = Singletons.Get<Log>();
        log.Write("Theme", "Failed to retrieve public themes", Log.LogLevel.Error);
        return null;
    }
}