using System.Collections.Generic;
using Newtonsoft.Json;

namespace JiayiLauncher.Appearance;

public class ThemeMetadata
{
	[JsonProperty("author")]
	public string Author;
	[JsonProperty("tags")]
	public List<string> Tags;
	[JsonProperty("raw_tags")]
	public List<string> RawTags;

	public static readonly List<string> RAW_TAGS = ["Dark", "Light", "Animated"];

}