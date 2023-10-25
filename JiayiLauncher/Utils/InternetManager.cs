using System.Net.Http;
using JiayiLauncher.Settings;

namespace JiayiLauncher.Utils;

public static class InternetManager
{
	private const string IP = "https://jiayi.software";
	
	public static bool OfflineMode { get; set; }
	public static HttpClient Client { get; } = new();

	public static void CheckOnline()
	{
		if (JiayiSettings.Instance!.OfflineMode)
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
}