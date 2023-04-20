using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace JiayiLauncher.Features.Versions;

public static class VersionList
{
	private static Dictionary<string, MinecraftVersion> _versionDict = new();
	private static readonly List<string> _versions = new();

	private static async Task UpdateVersions()
	{
		using var client = new HttpClient();

		var response =
			await client.GetAsync("https://raw.githubusercontent.com/MinecraftBedrockArchiver/Metadata/master/w10_meta.json");
		
		if (!response.IsSuccessStatusCode) return;
		
		var content = await response.Content.ReadAsStringAsync();
		var json = JsonConvert.DeserializeObject<Dictionary<string, MinecraftVersion>>(content);
		
		if (json != null)
		{
			// 2 numbers at the end means that version is a beta version that somehow got into the releases
			_versionDict = json
				.Where(version => version.Key.Split('.').Last().Length < 2)
				.Reverse()
				.ToDictionary(x => x.Key, x => x.Value);

			_versions.Clear();
			_versions.AddRange(_versionDict.Keys);
		}
	}

	public static async Task<List<string>> GetVersionList()
	{
		if (_versions.Count > 0) return _versions;

		await UpdateVersions();
		return _versions;
	}

	public static async Task<Dictionary<string, MinecraftVersion>> GetFullVersionList()
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
}