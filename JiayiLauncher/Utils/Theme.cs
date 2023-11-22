using JiayiLauncher.Utils;
using Newtonsoft.Json;
using System;
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

