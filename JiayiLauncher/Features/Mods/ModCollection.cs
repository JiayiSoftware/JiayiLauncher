using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace JiayiLauncher.Features.Mods;

public class ModCollection
{
	public string BasePath { get; set; } = string.Empty;
	public List<Mod> Mods { get; set; } = new();
	
	public static ModCollection? Current { get; set; }

	private ModCollection() // nobody should be calling this
	{
		
	}

	public static ModCollection Create(string path)
	{
		// path can either be an empty directory or one with mods in it already
		// if it's empty, we'll create a new mod collection
		
		Directory.CreateDirectory(path);
		Directory.CreateDirectory(Path.Combine(path, ".jiayi"));
		File.WriteAllText(Path.Combine(path, ".jiayi", "README.txt"), 
			"This folder contains metadata for all mods in this collection. Don't mess with it unless you know what you're doing.");
		
		var collection = new ModCollection
		{
			BasePath = path
		};

		var folders = Directory.GetDirectories(path);
		var files = Directory.GetFiles(path);
		if (folders.Length == 1 && folders[0].EndsWith(".jiayi") && files.Length == 0)
		{
			// empty directory
			return collection;
		}
		
		// create metadata for all mods in the directory
		foreach (var folder in folders)
		{
			var files2 = Directory.GetFiles(folder);
			if (files2.Length == 0) continue;

			foreach (var file in files2)
			{
				var mod = new Mod(Path.GetFileName(folder), file);
				// the user can give it a good name and supported versions later
				mod.SaveMetadata(collection);
				collection.Mods.Add(mod);
				Debug.WriteLine($"Added mod {mod.Name} at {mod.Path}");
			}
		}
		
		return collection;
	}

	public static void Load(string path)
	{
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
		
		foreach (var folder in folders)
		{
			var files2 = Directory.GetFiles(folder);
			if (files2.Length == 0) continue;

			foreach (var file in files2)
			{
				if (!file.EndsWith(".mod")) continue;
				var mod = Mod.LoadFromMetadata(file);
				collection.Mods.Add(mod);
				Debug.WriteLine($"Added mod {mod.Name} at {mod.Path}");
			}
		}
		
		Current = collection;
		Debug.WriteLine($"Loaded mod collection at {path}");
	}
}