using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;
using JiayiLauncher.Settings;

namespace JiayiLauncher.Features.Versions;

public static class VersionManager
{
	public static bool VersionInstalled(string ver)
	{
		var folders = Directory.GetDirectories(JiayiSettings.Instance!.VersionsPath);
		return folders.Any(x => x.Contains(ver));
	}

	// haven't finished this code yet pls no touch
	// public static async Task DownloadVersion(MinecraftVersion version)
	// {
	// 	var updateId = version.Archs.x64!.UpdateIds[0];
	// 	var url = await RequestFactory.GetDownloadUrl(updateId);
	// 	var fileName = version.Archs.x64.FileName;
	// 	var filePath = Path.Combine(JiayiSettings.Instance!.VersionsPath, fileName);
	// 	
	// 	var hash = MD5.HashData(await File.ReadAllBytesAsync(filePath));
	// 	var hashString = string.Join("", hash.Select(x => x.ToString("x2")));
	// 	if (hashString == version.Archs.x64.Hashes.MD5) return;
	//
	// 	using var client = new HttpClient();
	// 	using 
	// }
}