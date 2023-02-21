using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

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

	public static Mod? LoadFromMetadata(string path)
	{
		if (!File.Exists(path)) return null;

		try
		{
			var lines = File.ReadAllLines(path);
			var name = lines[0];
			var modPath = lines[1].Replace("at ", string.Empty);
			var supportedVersions = lines[2].Replace("Works on ", string.Empty).Split(", ").ToList();
			
			return new Mod(name, modPath, supportedVersions);
		}
		catch
		{
			Debug.WriteLine("Failed to load mod metadata.");
			return null;
		}
	}
}