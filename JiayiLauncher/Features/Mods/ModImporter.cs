using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Forms;

namespace JiayiLauncher.Features.Mods;

public static class ModImporter
{
	public static Mod ImportFromPath(string path)
	{
		var name = Path.GetFileNameWithoutExtension(path);
		return new Mod(name, path);
	}

	public static async Task<Mod> ImportFromBrowserFile(IBrowserFile file)
	{
		var path = Path.Combine(Path.GetTempPath(), file.Name);
		await using var stream = File.Create(path);
		await file.OpenReadStream(524288000L).CopyToAsync(stream);
		
		return ImportFromPath(path);
	}
	
	public static async Task<List<Mod>> ImportFromBrowserFiles(IEnumerable<IBrowserFile> files)
	{
		var mods = new List<Mod>();
		foreach (var file in files)
		{
			mods.Add(await ImportFromBrowserFile(file));
		}

		return mods;
	}
}