using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using JiayiLauncher.Features.Game;
using JiayiLauncher.Utils;

namespace JiayiLauncher.Features.Mods;

public class Mod
{
	public string Name { get; set; }
	public string Path { get; set; }
	public string MetadataPath { get; }
	public List<string> SupportedVersions { get; set; }
	public bool FromInternet => Path.StartsWith("http");

	public Mod(string name, string path, List<string>? supportedVersions = null, string? metadataPath = null)
	{
		Name = name;
		Path = path;
		SupportedVersions = supportedVersions ?? new List<string> { "any version" };
		MetadataPath = metadataPath ?? string.Empty;
		
		// just in case
		if (SupportedVersions.Contains("Any version"))
		{
			SupportedVersions.Remove("Any version");
			SupportedVersions.Add("any version");
		}
		
		if (!string.IsNullOrEmpty(MetadataPath)) return;

		if (FromInternet)
		{
			var safeName = string.Join("", Name.Split(System.IO.Path.GetInvalidFileNameChars()));
			MetadataPath = System.IO.Path.Combine(ModCollection.Current!.BasePath, ".jiayi", safeName + ".jmod");
		}
		else
		{
			var filename = System.IO.Path.GetFileNameWithoutExtension(Path);
			var directory = System.IO.Path.GetDirectoryName(Path);
			var modRelativePath = directory!.Replace(ModCollection.Current!.BasePath, string.Empty);
			MetadataPath = System.IO.Path.Combine(ModCollection.Current.BasePath, ".jiayi", modRelativePath, filename + ".jmod");
		}
	}

	public void SaveMetadata()
	{
		if (SupportedVersions.Contains("Any version"))
		{
			SupportedVersions.Remove("Any version");
			SupportedVersions.Add("any version");
		}
		
		Directory.CreateDirectory(System.IO.Path.GetDirectoryName(MetadataPath)!);
		File.WriteAllText(MetadataPath, $"{Name}\nat {Path}\nWorks on {string.Join(", ", SupportedVersions)}");
		
		Log.Write("Mod.SaveMetadata()", $"{Name}'s metadata saved to {MetadataPath}");
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

			var mod = new Mod(name, modPath, supportedVersions, path);
			
			return mod;
		}
		catch
		{
			Log.Write("Mod.LoadFromMetadata()", $"Failed to load mod metadata from {path}!", Log.LogLevel.Error);
			return null;
		}
	}
	
	public void Delete(ModCollection collection)
	{
		if (!FromInternet && File.Exists(Path)) File.Delete(Path);
		
		File.Delete(MetadataPath);
		collection.Mods.Remove(this);
		Minecraft.ModsLoaded.Remove(this);
		
		Log.Write("Mod.Delete()", $"Deleted {Name} from the mod collection.");
	}
	
	// for speed
	private HttpResponseMessage? _response;

	public bool IsValid()
	{
		if (_response != null) return _response.IsSuccessStatusCode;

		if (FromInternet)
		{
			// ping
			using var client = new HttpClient();
			using var request = new HttpRequestMessage(HttpMethod.Head, Path);
			
			try
			{
				using var response = client.Send(request);
				_response = response;
				return response.IsSuccessStatusCode;
			}
			catch
			{
				return false;
			}
		}
		
		var exists = File.Exists(Path);
		var isValidFile = Path.EndsWith(".dll") || Path.EndsWith(".exe");
        return exists && isValidFile;
    }
}