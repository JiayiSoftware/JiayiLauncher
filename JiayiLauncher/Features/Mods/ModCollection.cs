using JiayiLauncher.Settings;
using JiayiLauncher.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Windows;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace JiayiLauncher.Features.Mods;

[Serializable]
public class ModCollection
{
    // Help
    /*[CascadingParameter]
    public IModalService ModalService { get; set; } = default!;*/

    [System.Text.Json.Serialization.JsonIgnore] public string BasePath { get; private set; } = string.Empty;
    public List<Mod> Mods { get; } = new();

    [System.Text.Json.Serialization.JsonIgnore] public static ModCollection? Current { get; private set; }
    
    private static JsonSerializerOptions? _options;
    
    // serialization thing
    public ModCollection() { }

    private ModCollection(string basePath) // nobody should be calling this
    {
		BasePath = basePath;
    }

    public static ModCollection Create(string path)
    {
        // path can either be an empty directory or one with mods in it already
        // if it's empty, we'll create a new mod collection

        Directory.CreateDirectory(path);

        var collection = new ModCollection(path);

        var folders = Directory.GetDirectories(path);
        var fileList = Directory.GetFiles(path, "*.dll", SearchOption.AllDirectories).ToList();
        fileList.AddRange(Directory.GetFiles(path, "*.exe", SearchOption.AllDirectories));
        var files = fileList.ToList();

        if (folders.Length == 0 && files.Count == 0)
        {
            // empty directory
            return collection;
        }

        // add mods
        foreach (var mod in 
                 from file in files 
                 let name = Path.GetFileNameWithoutExtension(file) 
                 select new Mod(name, file))
        {
	        collection.Add(mod);
	        Log.Write("ModCollection", $"Added mod {mod.Name} at {mod.Path}");
        }

        // set path in settings
        JiayiSettings.Instance!.ModCollectionPath = path;
        JiayiSettings.Instance.Save();

        Log.Write("ModCollection", $"Created mod collection at {path}");

        collection.Save();
        return collection;
    }

    public void Save()
    {
	    var indexPath = Path.Combine(BasePath, "index.json");
	    File.WriteAllText(indexPath, string.Empty);
	    
	    _options ??= new JsonSerializerOptions { WriteIndented = true };
	    
	    using var stream = File.OpenWrite(indexPath);
	    JsonSerializer.Serialize(stream, this, _options);
	    
	    #if DEBUG
	    Log.Write(this, "Saved mod collection.");
	    #endif
    }

    public static void Load(string path)
    {
	    // an artifact from the old mod collection system
	    var oldPath = Path.Combine(path, ".jiayi");
	    if (Directory.Exists(oldPath))
	    {
		    Directory.Delete(oldPath, true);
		    Current = Create(path);
		    return;
	    }
	    
	    var indexPath = Path.Combine(path, "index.json");
        if (!File.Exists(indexPath))
		{
			Current = Create(path);
			return;
		}
        
        var json = File.ReadAllText(indexPath);
        
        try
		{
	        // since standard library's json returns an empty collection
	        var collection = JsonConvert.DeserializeObject<ModCollection>(json);
	        
	        if (collection == null)
	        {
		        Log.Write("ModCollection", $"Failed to load mod collection at {path}: index.json is invalid.");
		        Current = Create(path);
		        return;
			}
	        
	        collection.BasePath = path;
	        Current = collection;
	    }
        catch (Exception e)
		{
	        Log.Write("ModCollection", $"Failed to load mod collection at {path}: {e.Message}");
	        Current = Create(path);
		}
        
        Log.Write("ModCollection", $"Loaded mod collection at {path}");
    }

    // path is a folder here, the file name will be the collection name + .jiayi
    public void Export(string path)
    {
        ZipFile.CreateFromDirectory(BasePath, path);

        Log.Write("ModCollection", $"Exported mod collection to {path}");
    }

    public static void Import(string path)
    {
        if (Current != null)
        {
            // merge the mods
            var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDir);
            ZipFile.ExtractToDirectory(path, tempDir);

            var index = Path.Combine(tempDir, "index.json");
            if (!File.Exists(index))
			{
				Log.Write("ModCollection", $"Failed to import mod collection from {path}: index.json is missing.");
				Directory.Delete(tempDir, true);
				return;
			}
            
            var json = File.ReadAllText(index);
            
            try
            {
	            var collection = JsonConvert.DeserializeObject<ModCollection>(json);
	            if (collection == null)
	            {
		            Log.Write("ModCollection", $"Failed to load mod collection at {path}: index.json is invalid.");
		            Current = Create(path);
		            return;
	            }
	            
	            foreach (var mod in collection.Mods)
	            {
		            // zero out playtime and fix path
		            mod.PlayTime = TimeSpan.Zero;

		            if (!mod.FromInternet)
		            {
			            mod.Path = Path.Combine(Current.BasePath, Path.GetFileName(mod.Path));
			            File.Copy(Path.Combine(tempDir, Path.GetFileName(mod.Path)), mod.Path, true);
		            }

		            if (Current.HasMod(mod.Path))
		            {
			            var existingIndex = Current.Mods.FindIndex(m => m.Path == mod.Path);
			            Current.Mods[existingIndex] = mod;
		            }
		            else
		            {
			            Current.Add(mod, false);
		            }
	            }
	            
	            Current.Save();
            }
            catch (Exception e)
            {
	            Log.Write("ModCollection", $"Failed to load mod collection at {path}: {e.Message}");
	            Current = Create(path);
            }

            Directory.Delete(tempDir, true);
            return;
        }

        // create a new collection
        JiayiSettings.Instance!.ModCollectionPath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Mods");
        JiayiSettings.Instance.Save();
        Load(JiayiSettings.Instance.ModCollectionPath);
        Import(path);
    }

    public void Add(Mod mod, bool confirm = true)
    {
	    if (HasMod(mod.Path))
	    {
		    var existing = Mods.First(m => m.Path == mod.Path);
		    if (confirm && 
		        MessageBox.Show("This mod already exists in your collection. Do you want to replace it?",
			        "Jiayi Launcher", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
		    {
			    return;
		    }
			    
		    Mods.Remove(existing);
		    if (!existing.FromInternet && File.Exists(existing.Path) && mod.Path != existing.Path)
			    File.Copy(mod.Path, existing.Path, true);
	    }

	    if (!mod.FromInternet && !mod.Path.StartsWith(BasePath))
	    {
		    var newPath = Path.Combine(BasePath, Path.GetFileName(mod.Path));
		    File.Copy(mod.Path, newPath, true);
		    mod.Path = newPath;
	    }

	    Mods.Add(mod);
		Save();
    }

    public bool HasMod(string path)
    {
        return Mods.Any(mod => mod.Path == path);
    }

    public static ModCollectionInfo GetInfo(string path = "")
    {
        if (path == string.Empty)
        {
            // get info for the current collection
            if (Current == null) return new ModCollectionInfo();

            return new ModCollectionInfo
            {
                TotalMods = Current.Mods.Count,
                InternetMods = Current.Mods.Count(mod => mod.FromInternet),
                LocallyStoredMods = Current.Mods.Count(mod => !mod.FromInternet)
            };
        }

        // path should be a .jiayi file
        var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        ZipFile.ExtractToDirectory(path, tempDir);

        var index = Path.Combine(tempDir, "index.json");
        if (!File.Exists(index))
		{
			Log.Write("ModCollection", $"Failed to get info for mod collection at {path}: index.json is missing.");
			Directory.Delete(tempDir, true);
			return new ModCollectionInfo();
		}
        
        var json = File.ReadAllText(index);
        
        try
		{
			var collection = JsonConvert.DeserializeObject<ModCollection>(json);
	        if (collection == null)
	        {
		        Log.Write("ModCollection", $"Failed to get info for mod collection at {path}: index.json is invalid.");
		        Directory.Delete(tempDir, true);
		        return new ModCollectionInfo();
	        }
	        
	        var mods = collection.Mods;
	        Directory.Delete(tempDir, true);
	        
	        return new ModCollectionInfo
	        {
		        TotalMods = mods.Count,
		        InternetMods = mods.Count(mod => mod.FromInternet),
		        LocallyStoredMods = mods.Count(mod => !mod.FromInternet)
	        };
		}
		catch (Exception e)
		{
	        Log.Write("ModCollection", $"Failed to get info for mod collection at {path}: {e.Message}");
	        Directory.Delete(tempDir, true);
	        return new ModCollectionInfo();
		}
    }
}