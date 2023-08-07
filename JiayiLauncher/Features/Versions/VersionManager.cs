using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Management.Core;
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

	public static bool VersionInstalled(string ver)
	{
		Directory.CreateDirectory(JiayiSettings.Instance!.VersionsPath);
		var folders = Directory.GetDirectories(JiayiSettings.Instance.VersionsPath);
		return folders.Any(x => x.Contains(ver));
	}

	public static List<string> GetCustomVersions()
	{
		Directory.CreateDirectory(JiayiSettings.Instance!.VersionsPath);
		var folders = Directory.GetDirectories(JiayiSettings.Instance!.VersionsPath);
		var versions = VersionList.GetVersionList().GetAwaiter().GetResult();

		return folders.Select(Path.GetFileName).Where(name => versions.All(x => x != name)).ToList()!;
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
		
		// backup shaders i guess
		await ShaderManager.BackupVanillaShaders();
	}

	public static bool IsValidPackage(string path)
	{
		using var archive = ZipFile.OpenRead(path);
		return archive.Entries.Any(x => x.Name == "AppxManifest.xml");
	}

	public static async Task DownloadVersion(MinecraftVersion version)
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
		
		DownloadProgress = 0;

		while (true)
		{
			var read = await stream.ReadAsync(buffer);
			if (read == 0) break;
			
			await fileStream.WriteAsync(buffer.AsMemory(0, read));
			totalRead += read;
			
			var oldProgress = DownloadProgress;
			DownloadProgress = (int)(totalRead * 100 / contentLength)!;
			if (DownloadProgress != oldProgress) SwitchProgressChanged?.Invoke(null, EventArgs.Empty);
		}
		
		DownloadProgress = 100;
		
		fileStream.Close();
		stream.Close();
		
		var folder = Path.Combine(JiayiSettings.Instance.VersionsPath, version.Version);
		Directory.CreateDirectory(folder);
		await Task.Run(() => ZipFile.ExtractToDirectory(filePath, folder));
		File.Delete(filePath);
		
		// DELETE AppxSignature.p7x so that the game can be installed with developer mode
		var signature = Path.Combine(folder, "AppxSignature.p7x");
		if (File.Exists(signature)) File.Delete(signature);
		
		// copy shaders
		await ShaderManager.BackupVanillaShaders();
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

		await ShaderManager.DeleteBackupShaders();
	}
	
	// my favorite part of this class
	public static async Task<SwitchResult> Switch(string version)
	{
		Log.Write(nameof(VersionManager), $"Switching to version {version}");
		
		var folders = Directory.GetDirectories(JiayiSettings.Instance!.VersionsPath);
		var folder = folders.FirstOrDefault(x => x.Contains(version));
		if (folder == null) return SwitchResult.VersionNotFound;

		if (!WinRegistry.DeveloperModeEnabled())
		{
			Log.Write(nameof(VersionManager), "Developer mode is disabled, asking user to enable", Log.LogLevel.Warning);
			return SwitchResult.DeveloperModeDisabled;
		}
		
		var packages = PackageData.PackageManager.FindPackages("Microsoft.MinecraftUWP_8wekyb3d8bbwe");
		foreach (var package in packages)
		{
			if (package.InstalledPath.Contains(version))
			{
				Log.Write(nameof(VersionManager), "Version already installed");
				return SwitchResult.Succeeded;
			}
			
			if (package.IsDevelopmentMode)
				await PackageData.PackageManager.RemovePackageAsync(package.Id.FullName, RemovalOptions.PreserveApplicationData);
			else
			{
				var backupPath = Path.Combine(JiayiSettings.Instance.VersionsPath, "Microsoft.MinecraftUWP_8wekyb3d8bbwe");
				if (Directory.Exists(backupPath))
				{
					Log.Write(nameof(VersionManager),
						"Backup found, this might mean the launcher failed to switch versions last time",
						Log.LogLevel.Warning);
				}
				else
				{
					Log.Write(nameof(VersionManager), "Backing up game data");
					await PackageData.BackupGameData(backupPath);
				}

				Log.Write(nameof(VersionManager), "Removing old game data");
				Directory.Delete(PackageData.GetGameDataPath(), true);
				
				await PackageData.PackageManager.RemovePackageAsync(package.Id.FullName, 0);
			}
		}
		
		Log.Write(nameof(VersionManager), "Registering package");
		
		var manifest = Path.Combine(folder, "AppxManifest.xml");

		try
		{
			var result = await PackageData.PackageManager.RegisterPackageAsync(new Uri(manifest), null,
				DeploymentOptions.DevelopmentMode);

			if (result.IsRegistered)
			{
				Log.Write(nameof(VersionManager), "Package registered");
			
				var path = Path.Combine(JiayiSettings.Instance.VersionsPath, "Microsoft.MinecraftUWP_8wekyb3d8bbwe");
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
				Log.Write(nameof(VersionManager), "Developer mode is disabled, asking user to enable", Log.LogLevel.Warning);
				return SwitchResult.DeveloperModeDisabled;
			}
			
			Log.Write(nameof(VersionManager), $"Unknown error: {e}");
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