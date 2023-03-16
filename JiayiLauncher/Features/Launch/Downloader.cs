using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using JiayiLauncher.Features.Mods;

namespace JiayiLauncher.Features.Launch;

public static class Downloader
{
	private static readonly HttpClient _client = new();
	
	// a lot of using statements here, (not) sorry if that pisses you off
	public static async Task<string> DownloadMod(Mod mod)
	{
		using var response = await _client.GetAsync(mod.Path, HttpCompletionOption.ResponseHeadersRead);
		if (!response.IsSuccessStatusCode) return string.Empty;

		if (response.Content.Headers.ContentDisposition is not { FileName: { } }) return string.Empty;
		var path = Path.Combine(ModCollection.Current!.BasePath,
			response.Content.Headers.ContentDisposition.FileName);

		// check if the file already exists, and if so, compare the hashes
		if (File.Exists(path))
		{
			using var hasher = MD5.Create();
			await using var stream = File.OpenRead(path);
			var hash = await hasher.ComputeHashAsync(stream);
			
			var otherHash = response.Content.Headers.ContentMD5;
			if (otherHash is not null && hash == otherHash) return path;
		}
		
		await using var file = File.Create(path);
		await using var content = await response.Content.ReadAsStreamAsync();
		await content.CopyToAsync(file);
		return path;
	}
}