using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace JiayiLauncher.Features.Mods;

public class ModCollection
{
	public string BasePath { get; private init; } = string.Empty;
	public List<Mod> Mods { get; } = new();
	
	public static ModCollection? Current { get; private set; }

	private ModCollection() // nobody should be calling this
	{
		
	}

	public static ModCollection Create(string path)
	{
		// path can either be an empty directory or one with mods in it already
		// if it's empty, we'll create a new mod collection
		
		Directory.CreateDirectory(path);
		var jiayiFolder = Directory.CreateDirectory(Path.Combine(path, ".jiayi"));
		jiayiFolder.Attributes |= FileAttributes.Hidden;
		File.WriteAllText(Path.Combine(path, ".jiayi", "README.txt"), 
			"This folder contains metadata for all mods in this collection. Don't mess with it unless you know what you're doing.");
		
		var collection = new ModCollection
		{
			BasePath = path
		};

		var folders = Directory.GetDirectories(path);
		var fileList = Directory.GetFiles(path, "*.dll", SearchOption.AllDirectories).ToList();
		fileList.AddRange(Directory.GetFiles(path, "*.exe", SearchOption.AllDirectories));
		var files = fileList.ToArray();
		
		if (folders.Length == 1 && folders[0].EndsWith(".jiayi") && files.Length == 0)
		{
			// empty directory
			return collection;
		}
		
		// create metadata for all mods in the directory
		foreach (var file in files)
		{
			// the user can give it a good name and supported versions later
			var name = Path.GetFileName(file)[..^4];
			var mod = new Mod(name, file);
			collection.Add(mod);
			Debug.WriteLine($"Added mod {mod.Name} at {mod.Path}");
		}

		return collection;
	}

	public static void Load(string path)
	{
		Directory.CreateDirectory(path);
		var folders = Directory.GetDirectories(path);
		
		if (folders.Length == 0)
		{
			// empty directory
			Current = Create(path);
			return;
		}
		
		var collection = new ModCollection
		{
			BasePath = path
		};
		
		var files = Directory.GetFiles(Path.Combine(path, ".jiayi"), "*.jmod", SearchOption.AllDirectories);

		foreach (var file in files)
		{
			var mod = Mod.LoadFromMetadata(file);
			if (mod == null) continue;
			collection.Mods.Add(mod);
			Debug.WriteLine($"Added mod {mod.Name} at {mod.Path}");
		}

		Current = collection;
		Debug.WriteLine($"Loaded mod collection at {path}");
	}

	public void Add(Mod mod)
	{
		Mods.Add(mod);
		
		// copy the mod to the collection directory if it's not already there
		if (!mod.Path.StartsWith(BasePath))
		{
			var filename = Path.GetFileName(mod.Path);
			var newPath = Path.Combine(BasePath, filename);
			File.Copy(mod.Path, newPath);
			mod.Path = newPath;
		}
		
		mod.SaveMetadata(this);
	}
}