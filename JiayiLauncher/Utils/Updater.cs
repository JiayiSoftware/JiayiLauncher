using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Octokit;
using Application = System.Windows.Application;

namespace JiayiLauncher.Utils;

public class Updater
{
	private const string INSTALLER_URL = "https://dl.jiayi.software/static/JiayiInstaller.exe";
	
	private readonly GitHubClient _gh = new(new ProductHeaderValue("JiayiLauncher"));
	private readonly HttpClient _client = new();

	public event EventHandler? UpdateDownloaded;
	
	public async Task<bool> IsUpdateAvailable()
	{
		var release = await _gh.Repository.Release.GetLatest("JiayiSoftware", "JiayiLauncher");
		var version = new Version(release.TagName.TrimStart('v'));
		var currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
		
		var newerVersion = version > currentVersion;
		var installerPath = Path.Combine(Directory.GetCurrentDirectory(), "JiayiInstaller.exe");
		
		if (File.Exists(installerPath) && newerVersion)
		{
			UpdateDownloaded?.Invoke(null, EventArgs.Empty);
			return false; // so we don't display another update toast
		}

		// up to date
		if (File.Exists(installerPath))
		{
			File.Delete(installerPath);
		}
		
		return newerVersion;
	}

	public async Task DownloadLatest()
	{
		Log.Write(this, "Downloading latest version");
		
		await using var response = await _client.GetStreamAsync(INSTALLER_URL);
		await using var fileStream = File.Create("JiayiInstaller.exe");
		await response.CopyToAsync(fileStream);
		
		Log.Write(this, "Downloaded latest version");
		UpdateDownloaded?.Invoke(null, EventArgs.Empty);
	}

	public void Update()
	{
		var installer = Path.Combine(Directory.GetCurrentDirectory(), "JiayiInstaller.exe");
		if (!File.Exists(installer))
		{
			Log.Write(this, "Failed to find installer", Log.LogLevel.Error);
			return;
		}
		
		Log.Write(this, "Starting installer and exiting");
		Process.Start(new ProcessStartInfo
		{
			FileName = installer,
			Arguments = $"--path \"{Directory.GetCurrentDirectory()}\" --open-immediately"
		});
		Application.Current.Shutdown();
	}
}