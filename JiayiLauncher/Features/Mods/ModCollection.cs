using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using JiayiLauncher.Settings;
using JiayiLauncher.Utils;

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
			Log.Write("ModCollection", $"Added mod {mod.Name} at {mod.Path}");
		}
		
		// set path in settings
		JiayiSettings.Instance!.ModCollectionPath = path;

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
			Log.Write("ModCollection", $"Loaded mod {mod.Name} at {mod.Path}");
		}

		Current = collection;
		Log.Write("ModCollection", $"Loaded mod collection at {path}");
	}

	// path is a folder here, the file name will be the collection name + .jiayi
	public void Export(string path)
	{
		ZipFile.CreateFromDirectory(BasePath, path);
		
		Log.Write("ModCollection", $"Exported mod collection to {path}");
	}

	public void Add(Mod mod)
	{
		// mods get replaced if they already exist
		if (HasMod(mod.Path))
		{
			var oldMod = Mods.First(m => m.Path == mod.Path);
			Mods.Remove(oldMod);
			oldMod.Delete(this);
		}
		
		Mods.Add(mod);
		
		// copy the mod to the collection directory if it's not already there
		if (!mod.FromInternet && !mod.Path.StartsWith(BasePath))
		{
			var filename = Path.GetFileName(mod.Path);
			var newPath = Path.Combine(BasePath, filename);
			File.Copy(mod.Path, newPath);
			mod.Path = newPath;
		}
		
		mod.SaveMetadata(this);
	}
	
	public bool HasMod(string path)
	{
		return Mods.Any(mod => mod.Path == path);
	}
}