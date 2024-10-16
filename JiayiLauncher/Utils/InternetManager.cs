using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using JiayiLauncher.Settings;

namespace JiayiLauncher.Utils;

public class InternetManager
{
	private const string IP = "https://phased.tech";
	
	public bool OfflineMode { get; private set; }
	public HttpClient Client { get; } = new();
	
	public InternetManager()
	{
		Client.Timeout = TimeSpan.FromSeconds(5);
		CheckOnline();
	}

	private void CheckOnline()
	{
		if (JiayiSettings.Instance.OfflineMode)
		{
			OfflineMode = true;
			return;
		}
		
		try
		{
			var request = new HttpRequestMessage(HttpMethod.Head, IP);
			Client.Send(request);
		}
		catch
		{
			OfflineMode = true;
		}
	}

	public async Task DownloadFile(Uri url, string path)
	{
		await using var s = await Client.GetStreamAsync(url);
		await using var fs = new FileStream(Path.Combine(path, Path.GetFileName(url.AbsoluteUri)), FileMode.CreateNew);
		await s.CopyToAsync(fs);
	}
}