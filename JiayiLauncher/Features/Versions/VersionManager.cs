using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Management.Deployment;
using JiayiLauncher.Features.Game;
using JiayiLauncher.Features.Shaders;
using JiayiLauncher.Settings;
using JiayiLauncher.Utils;

namespace JiayiLauncher.Features.Versions;

public static class VersionManager
{
	public enum SwitchResult
	{
		Succeeded,
		VersionNotFound,
		DeveloperModeDisabled,
		BackupFailed,
		UnknownError
	}

	public static int DownloadProgress { get; private set; }
	public static event EventHandler? SwitchProgressChanged;
	public static long BytesRead { get; private set; }
  
	private static readonly Log _log = Singletons.Get<Log>();

	public static bool VersionInstalled(string ver)
	{
		Directory.CreateDirectory(JiayiSettings.Instance!.VersionsPath);
		var folders = Directory.GetDirectories(JiayiSettings.Instance.VersionsPath);
		return folders.Any(x => x.Contains(ver));
	}

	public static List<string> GetCustomVersions()
	{
		Directory.CreateDirectory(JiayiSettings.Instance!.VersionsPath);
		var folders = Directory.GetDirectories(JiayiSettings.Instance.VersionsPath);
		var versions = VersionList.GetVersionList().GetAwaiter().GetResult();
		
		for (int i = 0; i < folders.Length; i++)
		{
			if (File.Exists(Path.Combine(folders[i], "AppxManifest.xml")))
			{
				continue; 
			}
			
			folders = folders.Where((source, index) => index != i).ToArray();
			i--;
		}

		return folders.Select(Path.GetFileName).Where(name => versions.All(x => x != name) && name != ".backup")
			.ToList()!;
	}

	public static bool IsCustomVersion(string ver)
	{
		var folders = Directory.GetDirectories(JiayiSettings.Instance!.VersionsPath);
		var versions = VersionList.GetVersionList().GetAwaiter().GetResult();

		return folders.Any(x => x.Contains(ver)) && versions.All(x => x != ver);
	}

	public static async Task AddCustomVersion(string path)
	{
		// appx files are just zipped so extract it like we've just downloaded an official version
		var folder = Path.Combine(JiayiSettings.Instance!.VersionsPath, Path.GetFileNameWithoutExtension(path));
		Directory.CreateDirectory(folder);
		await Task.Run(() => ZipFile.ExtractToDirectory(path, folder));

		// delete signature (most likely doesn't exist anyway)
		var signature = Path.Combine(folder, "AppxSignature.p7x");
		if (File.Exists(signature)) File.Delete(signature);
	}

	public static bool IsValidPackage(string path)
	{
		using var archive = ZipFile.OpenRead(path);
		return archive.Entries.Any(x => x.Name == "AppxManifest.xml");
	}

	public static async Task DownloadVersion(MinecraftVersion version)
	{
		var updateId = version.UpdateId;
		var url = await RequestFactory.GetDownloadUrl(updateId);
		var fileName = version.FileName;
		var filePath = Path.Combine(JiayiSettings.Instance!.VersionsPath, fileName);

		if (File.Exists(filePath)) File.Delete(filePath);

		using var response = await InternetManager.Client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);

		if (!response.IsSuccessStatusCode) return;

		var contentLength = response.Content.Headers.ContentLength ?? 0;
		var bufferSize = 1048576;
		var numberOfChunks = 8;
		var chunkSize = contentLength / numberOfChunks;

		var tasks = new List<Task>();
		long totalRead = 0;
		var progressLock = new object();

		DownloadProgress = 0;
		BytesRead = 0;
		
		SwitchProgressChanged?.Invoke(null, EventArgs.Empty);

		for (int i = 0; i < numberOfChunks; i++)
		{
			var start = i * chunkSize;
			var end = (i == numberOfChunks - 1) ? contentLength - 1 : start + chunkSize - 1;
			tasks.Add(DownloadChunk(url, filePath, start, end, bufferSize, totalRead, contentLength, progressLock));
		}

		while (tasks.Any(x => !x.IsCompleted))
		{
			var oldProgress = DownloadProgress;
			DownloadProgress = (int)(BytesRead * 100 / contentLength);
			if (DownloadProgress != oldProgress) SwitchProgressChanged?.Invoke(null, EventArgs.Empty);
			await Task.Delay(1);
		}

		DownloadProgress = 100;
		
		SwitchProgressChanged?.Invoke(null, EventArgs.Empty);

		var folder = Path.Combine(JiayiSettings.Instance.VersionsPath, version.Version);
		Directory.CreateDirectory(folder);
		await Task.Run(() => ZipFile.ExtractToDirectory(filePath, folder));
		File.Delete(filePath);

		// DELETE AppxSignature.p7x so that the game can be installed with developer mode
		var signature = Path.Combine(folder, "AppxSignature.p7x");
		if (File.Exists(signature)) File.Delete(signature);
	}
	
	private static async Task DownloadChunk(string url, string filePath, long start, long end, int bufferSize, long totalRead, long contentLength, object progressLock)
	{
		var request = new HttpRequestMessage(HttpMethod.Get, url);
		request.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(start, end);

		using var response = await InternetManager.Client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
		response.EnsureSuccessStatusCode();

		await using var stream = await response.Content.ReadAsStreamAsync();
		using var fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Write);

		fileStream.Seek(start, SeekOrigin.Begin);

		var buffer = new byte[bufferSize];
		int bytesRead;

		while ((bytesRead = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length))) > 0)
		{
			await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead));
			BytesRead += bytesRead;

			lock (progressLock)
			{
				totalRead += bytesRead;
			}
		}
	}


	public static async Task RemoveVersion(string ver)
	{
		var package = await PackageData.GetPackage();
		if (package == null) return;

		// in case a shader is applied we don't want to lose the user's shaders
		if (ShaderManager.AppliedShader != string.Empty)
		{
			ShaderManager.DisableShader(ShaderManager.AppliedShader);
			await ShaderManager.RestoreVanillaShaders();
		}

		if (package.AppInfo.Package.InstalledPath == GetVersionPath(ver))
		{
			// remove package
			await PackageData.PackageManager.RemovePackageAsync(package.AppInfo.Package.Id.FullName, 0);
		}

		await Task.Run(() =>
		{
			var folders = Directory.GetDirectories(JiayiSettings.Instance!.VersionsPath);
			var folder = folders.FirstOrDefault(x => x.Contains(ver));
			if (folder == null) return;

			Directory.Delete(folder, true);
		});

		await ShaderManager.DeleteBackupShaders();
	}

	// my favorite part of this class
	public static async Task<SwitchResult> Switch(string version)
	{
		_log.Write(nameof(VersionManager), $"Switching to version {version}");

		if (ShaderManager.AppliedShader != string.Empty)
		{
			ShaderManager.DisableShader(ShaderManager.AppliedShader);
			await ShaderManager.RestoreVanillaShaders();
		}

		var folders = Directory.GetDirectories(JiayiSettings.Instance!.VersionsPath);
		var folder = folders.FirstOrDefault(x => x.Contains(version));
		if (folder == null) return SwitchResult.VersionNotFound;

		if (!WinRegistry.DeveloperModeEnabled())
		{
			_log.Write(nameof(VersionManager), "Developer mode is disabled, asking user to enable", Log.LogLevel.Warning);
			return SwitchResult.DeveloperModeDisabled;
		}

		var packages = PackageData.PackageManager.FindPackages("Microsoft.MinecraftUWP_8wekyb3d8bbwe");
		foreach (var package in packages)
		{
			if (package.InstalledPath.Contains(version))
			{
				_log.Write(nameof(VersionManager), "Version already installed");
				return SwitchResult.Succeeded;
			}

			if (package.IsDevelopmentMode)
				await PackageData.PackageManager.RemovePackageAsync(package.Id.FullName,
					RemovalOptions.PreserveApplicationData);
			else
			{
				var backupPath = Path.Combine(JiayiSettings.Instance.VersionsPath, ".backup");
				if (Directory.Exists(backupPath))
				{
					_log.Write(nameof(VersionManager),
						"Backup found, this might mean the launcher failed to switch versions last time",
						Log.LogLevel.Warning);
				}
				else
				{
					_log.Write(nameof(VersionManager), "Backing up game data");
					await PackageData.BackupGameData(backupPath);
				}

				_log.Write(nameof(VersionManager), "Removing old game data");
				Directory.Delete(PackageData.GetGameDataPath(), true);

				await PackageData.PackageManager.RemovePackageAsync(package.Id.FullName, 0);
			}
		}

		_log.Write(nameof(VersionManager), "Registering package");

		var manifest = Path.Combine(folder, "AppxManifest.xml");

		try
		{
			var result = await PackageData.PackageManager.RegisterPackageAsync(new Uri(manifest), null,
				DeploymentOptions.DevelopmentMode);

			if (result.IsRegistered)
			{

				_log.Write(nameof(VersionManager), "Package registered");
			
				var path = Path.Combine(JiayiSettings.Instance.VersionsPath, ".backup");
				if (Directory.Exists(path))
				{
					await PackageData.ReplaceGameData(path);
					Directory.Delete(path, true);
				}

				return SwitchResult.Succeeded;
			}
		}
		catch (Exception e)
		{
			if (e.ToString().Contains("sideload"))
			{
				_log.Write(nameof(VersionManager), "Developer mode is disabled, asking user to enable", Log.LogLevel.Warning);
				return SwitchResult.DeveloperModeDisabled;
			}
			
			_log.Write(nameof(VersionManager), $"Unknown error: {e}");
		}

		return SwitchResult.UnknownError;
	}

	public static string? GetVersionPath(string ver)
	{
		var folders = Directory.GetDirectories(JiayiSettings.Instance!.VersionsPath);
		var folder = folders.FirstOrDefault(x => x == ver);
		return folder;
	}
}
