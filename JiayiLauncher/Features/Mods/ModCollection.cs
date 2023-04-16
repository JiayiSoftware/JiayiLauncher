using JiayiLauncher.Settings;
using JiayiLauncher.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
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
            Log.Write("ModCollection", $"Added mod {mod.Name} at {mod.Path}");
        }

        // set path in settings
        JiayiSettings.Instance!.ModCollectionPath = path;

        Log.Write("ModCollection", $"Created mod collection at {path}");

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

    public static void Import(string path)
    {
        if (Current != null)
        {
            // merge the mods
            var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            Directory.CreateDirectory(tempDir);
            ZipFile.ExtractToDirectory(path, tempDir);

            var files = Directory.GetFiles(tempDir, "*.jmod", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                var mod = Mod.LoadFromMetadata(file);
                if (mod == null) continue;
                Current.Add(mod);
                Log.Write("ModCollection", $"Imported mod {mod.Name} at {mod.Path}");
            }

            Directory.Delete(tempDir, true);
            return;
        }

        // create a new collection
        JiayiSettings.Instance!.ModCollectionPath =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Jiayi Mods");
        JiayiSettings.Instance.Save();
        Load(JiayiSettings.Instance.ModCollectionPath);
        Import(path);
    }

    public void Add(Mod mod)
    {
        // mods get replaced if they already exist
        if (HasMod(mod.Path))
        {
            var idx = Mods.IndexOf(mod);
            Mods[idx] = mod;
        }
        else
        {
            Mods.Add(mod);

            // copy the mod to the collection directory if it's not already there
            if (!mod.FromInternet && !mod.Path.StartsWith(BasePath))
            {
                var filename = Path.GetFileName(mod.Path);
                var newPath = Path.Combine(BasePath, filename);
                File.Copy(mod.Path, newPath);
                mod.Path = newPath;
            }
        }

        mod.SaveMetadata(this);
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

        var files = Directory.GetFiles(tempDir, "*.jmod", SearchOption.AllDirectories);
        var mods = files.Select(Mod.LoadFromMetadata).Where(mod => mod != null).ToList();

        Directory.Delete(tempDir, true);

        return new ModCollectionInfo
        {
            TotalMods = mods.Count,
            InternetMods = mods.Count(mod => mod is { FromInternet: true }),
            LocallyStoredMods = mods.Count(mod => mod is { FromInternet: false })
        };
    }
}