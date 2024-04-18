using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Octokit;
using Application = Microsoft.Maui.Controls.Application;

namespace JiayiLauncher.Utils;

public class Updater
{
    private const string INSTALLER_URL = "https://jiyayi.mcpeplayground.com/static/JiayiInstaller.exe";

    private readonly GitHubClient _gh = new(new ProductHeaderValue("JiayiLauncher"));
    public event EventHandler? UpdateDownloaded;
    public event EventHandler? UpdateInstalled;

    public async Task<bool> IsUpdateAvailable()
    {
        if (Debugger.IsAttached)
        {
            Log.Write(nameof(Updater), "Skipping update check");
            return false;
        }

        if ((await _gh.RateLimit.GetRateLimits()).Resources.Core.Remaining <= 0)
        {
            Log.Write(nameof(Updater), "Rate limit exceeded", Log.LogLevel.Warning);
            return false;
        }

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
            UpdateInstalled?.Invoke(null, EventArgs.Empty);
        }

        return newerVersion;
    }

    public async Task DownloadLatest()
    {
        Log.Write(this, "Downloading latest version");

        await using var response = await InternetManager.Client.GetStreamAsync(INSTALLER_URL);
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
        Application.Current!.Quit();
    }
}