using System.Collections.Generic;
using System.IO;

namespace JiayiLauncher.Features.Mods;

public class Mod
{
	public string Name { get; set; }
	public string Path { get; set; }
	public List<string> SupportedVersions { get; set; }

	public Mod(string name, string path, List<string>? supportedVersions = null)
	{
		Name = name;
		Path = path;
		SupportedVersions = supportedVersions ?? new List<string> { "Any" };
	}

	public void SaveMetadata(ModCollection collection)
	{
		var filename = System.IO.Path.GetFileName(Path);
		var modRelativePath = Path.Replace(collection.BasePath, string.Empty);
		var metadataPath = System.IO.Path.Combine(collection.BasePath, ".jiayi", modRelativePath, filename + ".mod");
		File.WriteAllText(metadataPath, $"{Name}\nat {Path}\nWorks on {string.Join(", ", SupportedVersions)}");
	}
}