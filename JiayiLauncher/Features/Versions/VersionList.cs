using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using JiayiLauncher.Settings;
using JiayiLauncher.Utils;
using Newtonsoft.Json;
using StoreLib.Models;
using StoreLib.Services;

namespace JiayiLauncher.Features.Versions;

public static class VersionList
{
	private const string STORE_ID = "9NBLGGH2JHXJ";
	private const string OLD_VERSIONS_DB = "https://raw.githubusercontent.com/MCMrARM/mc-w10-versiondb/master/versions.txt";
	
	private static readonly List<string> _versions = new();
	private static readonly SortedDictionary<string, MinecraftVersion> _versionDict = new(new VersionComparer());
	private static readonly DisplayCatalogHandler _catalog = DisplayCatalogHandler.ProductionConfig();
	private static readonly string _versionsPath = Path.Combine(JiayiSettings.Instance!.VersionsPath, "versions.json");

	private static bool _loaded;
	
	public static async Task UpdateVersions(bool clear = false)
	{
		// TODO: Show notification
		if (clear)
		{
			_versionDict.Clear();
			_versions.Clear();
			_loaded = false;
		}
		
		if (File.Exists(_versionsPath) && !_loaded)
		{
			var jsonIn = JsonConvert.DeserializeObject<SortedDictionary<string, MinecraftVersion>>(
				await File.ReadAllTextAsync(_versionsPath));

			if (jsonIn != null)
			{
				foreach (var version in jsonIn)
				{
					_versionDict.TryAdd(version.Key, version.Value);
				}
			}
            
			if (_versions.Count == 0) _versions.AddRange(_versionDict.Keys);
		}
		
		_loaded = true;

		if (InternetManager.OfflineMode)
		{
			Log.Write(nameof(VersionList), "Offline mode enabled, skipping version list update.");
			return;
		}

		await _catalog.QueryDCATAsync(STORE_ID);
		if (_catalog.Result == DisplayCatalogResult.Found)
		{
			var packages = await _catalog.GetPackagesForProductAsync();
			foreach (var package in packages)
			{
				if (!package.PackageMoniker.StartsWith("Microsoft.MinecraftUWP_") || !package.PackageMoniker.Contains("x64")) continue;

				var target = package.ApplicabilityBlob.ContentTargetPlatforms[0].PlatformTarget;
				if (target != 0 && target != 3) continue;
			
				var fileName = package.PackageMoniker + ".Appx";
				var updateId = Guid.Parse(package.UpdateId).ToString();
				var version = ParseVersion(fileName);
			
				var mcVersion = new MinecraftVersion(fileName, updateId, version);
				if (_versionDict.TryAdd(version, mcVersion))
				{
					Log.Write(nameof(VersionList), $"Found new version: {version}");
					var jsonOut = JsonConvert.SerializeObject(_versionDict, Formatting.Indented);
					await File.WriteAllTextAsync(_versionsPath, jsonOut);
				}
				else
				{
					var newVersion = _versionDict[version];
					newVersion.FileName = fileName;
					newVersion.UpdateId = updateId;
				
					_versionDict[version] = newVersion;
				}
			}
		}
		
		// fetch old versions regardless of result (works as a fallback)
		var response = await InternetManager.Client.GetAsync(OLD_VERSIONS_DB);
		var mrArmVersions = await response.Content.ReadAsStringAsync();
		
		foreach (var mrArmVersion in mrArmVersions.Split('\n'))
		{
			if (mrArmVersion.Contains("Beta")) break; // end of the release versions
			
			if (mrArmVersion == "" || mrArmVersion.Split(' ').Length < 2) continue;
			
			var updateId = mrArmVersion.Split(' ')[0];
			var fileName = mrArmVersion.Split(' ')[1];
			
			if (!fileName.StartsWith("Microsoft.MinecraftUWP_") || !fileName.Contains("x64")) continue;
			if (fileName.EndsWith("EAppx") || fileName.Contains(".70_")) continue;
			if (!fileName.EndsWith(".Appx")) fileName += ".Appx";
			
			var version = ParseVersion(fileName);
			
			var mcVersion = new MinecraftVersion(fileName, updateId, version);
			if (_versionDict.TryAdd(version, mcVersion))
			{
				Log.Write(nameof(VersionList), $"Found new version: {version}");
				var jsonOut = JsonConvert.SerializeObject(_versionDict, Formatting.Indented);
				await File.WriteAllTextAsync(_versionsPath, jsonOut);
			}
			else
			{
				var newVersion = _versionDict[version];
				newVersion.FileName = fileName;
				newVersion.UpdateId = updateId;
				
				_versionDict[version] = newVersion;
			}
		}

		if (_versions.Count != 0) _versions.Clear();
		_versions.AddRange(_versionDict.Keys);
		
		Log.Write(nameof(VersionList), $"Updated version list. Found {_versions.Count} versions.");
	}

	public static async Task<List<string>> GetVersionList()
	{
		if (_versions.Count > 0) return _versions;

		await UpdateVersions();
		return _versions;
	}

	public static async Task<SortedDictionary<string, MinecraftVersion>> GetFullVersionList()
	{
		if (_versionDict.Count > 0) return _versionDict;
		
		await UpdateVersions();
		return _versionDict;
	}

	public static async Task<MinecraftVersion> GetVersion(string version)
	{
		if (_versionDict.TryGetValue(version, out var mcVersion)) return mcVersion;
		
		await UpdateVersions();

		if (!_versions.Contains(version)) throw new ArgumentException("The version you specified does not exist.");
		return _versionDict[version];
	}
	
	public static bool CompareVersions(string left, string right)
	{
		var leftVersion = left.Split('.');
		var rightVersion = right.Split('.');

		for (var i = 0; i < leftVersion.Length; i++)
		{
			if (int.Parse(leftVersion[i]) > int.Parse(rightVersion[i])) return true;
			if (int.Parse(leftVersion[i]) < int.Parse(rightVersion[i])) return false;
		}

		return false;
	}
	
	private static string ParseVersion(string fileName)
	{
		var name = Path.GetFileNameWithoutExtension(fileName);
		var rawVer = name.Split("_")[1];
		var verParts = rawVer.Split('.');
 
		if (verParts[0] == "0")
		{
			var lastBit = verParts[1][2..].TrimStart('0');
			var firstBit = verParts[1][..2];

			if (lastBit == "")
			{
				lastBit = "0";
			}

			return $"{verParts[0]}.{firstBit}.{lastBit}.{verParts[2]}";
		}
		else
		{
			verParts[2] = verParts[2].PadLeft(2, '0');
			var lastBit = verParts[2][^2..].TrimStart('0');
			var firstBit = verParts[2][..^2];

			if (firstBit == "")
			{
				firstBit = "0";
			}

			if (lastBit == "")
			{
				lastBit = "0";
			}

			return $"{verParts[0]}.{verParts[1]}.{firstBit}.{lastBit}";
		}
	}
}