using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json.Serialization;
using JiayiLauncher.Features.Stats;

namespace JiayiLauncher.Features.Mods;

[Serializable]
public class Mod
{
	public string Name { get; set; }
	public string Path { get; set; }
	public List<string> SupportedVersions { get; set; }
	public TimeSpan PlayTime { get; set; } = TimeSpan.Zero;
	[JsonIgnore] public bool FromInternet => Path.StartsWith("http");
	
	// for serialization
	public Mod() { }

	public Mod(string name, string path, List<string>? supportedVersions = null)
	{
		Name = name;
		Path = path;
		SupportedVersions = supportedVersions ?? new List<string> { "any version" };
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

	public void Delete()
	{
		if (!FromInternet && File.Exists(Path))
			File.Delete(Path);
		
		ModCollection.Current?.Mods.Remove(this);
		ModCollection.Current?.Save();
		
		JiayiStats.Save();
	}
}