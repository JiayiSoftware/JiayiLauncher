﻿using Microsoft.AspNetCore.Components.Forms;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace JiayiLauncher.Features.Mods;

public class ModImporter
{
    public Mod ImportFromPath(string path)
    {
        var name = Path.GetFileNameWithoutExtension(path);
        return new Mod(name, path);
    }
    
    public List<Mod> ImportFromPaths(IEnumerable<string> paths)
    {
        var list = new List<Mod>();
        foreach (var path in paths)
        {
            var mod = ImportFromPath(path);
            list.Add(mod);
            mod.SupportedVersions = new List<string> { "Any version" };
        }

        return list;
    }

    public async Task<Mod> ImportFromBrowserFile(IBrowserFile file)
    {
        // var path = Path.Combine(Path.GetTempPath(), file.Name);
        // await using var stream = File.Create(path);
        // await file.OpenReadStream(524288000L).CopyToAsync(stream);

        return await ImportFromStream(file.OpenReadStream(524288000L), file.Name);
    }

    public async Task<List<Mod>> ImportFromBrowserFiles(IEnumerable<IBrowserFile> files)
    {
        var mods = new List<Mod>();
        foreach (var file in files)
        {
            mods.Add(await ImportFromBrowserFile(file));
        }

        return mods;
    }
    
    public async Task<Mod> ImportFromStream(Stream stream, string name)
    {
        var path = Path.Combine(Path.GetTempPath(), name);
        await using var file = File.Create(path);
        await stream.CopyToAsync(file);

        return ImportFromPath(path);
    }
}
