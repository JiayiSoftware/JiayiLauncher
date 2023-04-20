using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using JiayiLauncher.Settings;
using JiayiLauncher.Utils;

namespace JiayiLauncher.Features.Versions;

public static class VersionManager
{
	public static event EventHandler<EventArgs>? DownloadFinished; 

	public static bool VersionInstalled(string ver)
	{
		Directory.CreateDirectory(JiayiSettings.Instance!.VersionsPath);
		var folders = Directory.GetDirectories(JiayiSettings.Instance!.VersionsPath);
		return folders.Any(x => x.Contains(ver));
	}

	public static async Task DownloadVersion(MinecraftVersion version, IProgress<int> progress)
	{
		var updateId = version.Archs.x64!.UpdateIds[0];
		var url = await RequestFactory.GetDownloadUrl(updateId);
		var fileName = version.Archs.x64.FileName;
		var filePath = Path.Combine(JiayiSettings.Instance!.VersionsPath, fileName);

		if (File.Exists(filePath)) File.Delete(filePath);

		using var client = new HttpClient();
		using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
		
		if (!response.IsSuccessStatusCode) return;
		
		var contentLength = response.Content.Headers.ContentLength;
		var totalRead = 0L;
		var buffer = new byte[1048576]; // 1 MB buffer
		
		await using var stream = await response.Content.ReadAsStreamAsync();
		await using var fileStream = new FileStream(filePath, FileMode.Create);
		
		progress.Report(0);

		while (true)
		{
			var read = await stream.ReadAsync(buffer);
			if (read == 0) break;
			
			await fileStream.WriteAsync(buffer.AsMemory(0, read));
			totalRead += read;
			progress.Report((int)(totalRead * 100 / contentLength)!);
		}
		
		progress.Report(100);
		
		fileStream.Close();
		stream.Close();
		
		var folder = Path.Combine(JiayiSettings.Instance.VersionsPath, version.Version);
		Directory.CreateDirectory(folder);
		ZipFile.ExtractToDirectory(filePath, folder);
		File.Delete(filePath);
		
		DownloadFinished?.Invoke(null, EventArgs.Empty);
	}

	public static async Task RemoveVersion(string ver)
	{
		await Task.Run(() =>
		{
			var folders = Directory.GetDirectories(JiayiSettings.Instance!.VersionsPath);
			var folder = folders.FirstOrDefault(x => x.Contains(ver));
			if (folder == null) return;

			Directory.Delete(folder, true);
		});
	}
}