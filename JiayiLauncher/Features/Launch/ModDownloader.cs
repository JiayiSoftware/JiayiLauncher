using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using JiayiLauncher.Features.Mods;
using JiayiLauncher.Utils;

namespace JiayiLauncher.Features.Launch;

public static class ModDownloader
{
	// a lot of using statements here, (not) sorry if that pisses you off
    public static async Task<string> DownloadMod(Mod mod)
    {
	    if (InternetManager.OfflineMode) return string.Empty;
	    
	    var log = Singletons.Get<Log>();
	    
        var fileName = string.Empty;
        if (mod.Path.EndsWith(".dll"))
            fileName = $"{mod.Name}.dll";
        if (mod.Path.EndsWith(".exe"))
            fileName = $"{mod.Name}.exe";
		
        log.Write(nameof(ModDownloader), $"Downloading {mod.Name}");

        using var response = await InternetManager.Client.GetAsync(mod.Path, HttpCompletionOption.ResponseHeadersRead);
		log.Write(nameof(ModDownloader), $"Server responded with {response.StatusCode}",
			response.IsSuccessStatusCode ? Log.LogLevel.Info : Log.LogLevel.Error);
		if (!response.IsSuccessStatusCode) return string.Empty;

		var hasFileName = true;
        if (response.Content.Headers.ContentDisposition is not { FileName: not null })
        {
            log.Write(nameof(ModDownloader), "Server did not provide a file name", Log.LogLevel.Warning);
            hasFileName = false;
        }

        var path = Path.Combine(ModCollection.Current!.BasePath, hasFileName ?
							   	response.Content.Headers.ContentDisposition.FileName.Replace("\"", "")
								: fileName.Replace("\"", ""));

        // check if the file already exists, and if so, compare the hashes
        if (File.Exists(path))
		{
			log.Write(nameof(ModDownloader), "File already exists, comparing hashes");
			using var hasher = MD5.Create();
			await using var stream = File.OpenRead(path);
			var hash = await hasher.ComputeHashAsync(stream);
			
			var otherHash = response.Content.Headers.ContentMD5;
			if (otherHash is not null && hash == otherHash)
			{
				log.Write(nameof(ModDownloader), "Hashes match, skipping download");
				return path;
			}
		}
		
		await using var file = File.Create(path);
		await using var content = await response.Content.ReadAsStreamAsync();
		
		log.Write(nameof(ModDownloader), "Writing contents to file");
		await content.CopyToAsync(file);
		log.Write(nameof(ModDownloader), $"Downloaded {mod.Name} to {path}");
		
		return path;
	}
}
