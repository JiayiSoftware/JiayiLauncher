using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using JiayiLauncher.Features.Mods;
using JiayiLauncher.Utils;

namespace JiayiLauncher.Features.Launch;

public static class Downloader
{
	private static readonly HttpClient _client = new();
	
	// a lot of using statements here, (not) sorry if that pisses you off
	public static async Task<string> DownloadMod(Mod mod)
	{
		Log.Write(nameof(Downloader), $"Downloading {mod.Name}");
		
		using var response = await _client.GetAsync(mod.Path, HttpCompletionOption.ResponseHeadersRead);
		Log.Write(nameof(Downloader), $"Server responded with {response.StatusCode}",
			response.IsSuccessStatusCode ? Log.LogLevel.Info : Log.LogLevel.Error);
		if (!response.IsSuccessStatusCode) return string.Empty;

		if (response.Content.Headers.ContentDisposition is not { FileName: not null })
		{
			Log.Write(nameof(Downloader), "Server did not provide a file name", Log.LogLevel.Error);
			return string.Empty;
		}

		var path = Path.Combine(ModCollection.Current!.BasePath,
			response.Content.Headers.ContentDisposition.FileName.Replace("\"", ""));

		// check if the file already exists, and if so, compare the hashes
		if (File.Exists(path))
		{
			Log.Write(nameof(Downloader), "File already exists, comparing hashes");
			using var hasher = MD5.Create();
			await using var stream = File.OpenRead(path);
			var hash = await hasher.ComputeHashAsync(stream);
			
			var otherHash = response.Content.Headers.ContentMD5;
			if (otherHash is not null && hash == otherHash)
			{
				Log.Write(nameof(Downloader), "Hashes match, skipping download");
				return path;
			}
		}
		
		await using var file = File.Create(path);
		await using var content = await response.Content.ReadAsStreamAsync();
		
		Log.Write(nameof(Downloader), "Writing contents to file");
		await content.CopyToAsync(file);
		Log.Write(nameof(Downloader), $"Downloaded {mod.Name} to {path}");
		
		return path;
	}
}
