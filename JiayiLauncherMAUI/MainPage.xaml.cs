using System.Diagnostics;
using System.Security.Principal;
using JiayiLauncher.Appearance;
using JiayiLauncher.Features.Discord;
using JiayiLauncher.Features.Game;
using JiayiLauncher.Features.Mods;
using JiayiLauncher.Features.Profiles;
using JiayiLauncher.Features.Stats;
using JiayiLauncher.Features.Versions;
using JiayiLauncher.Settings;
using JiayiLauncher.Utils;
using Microsoft.AspNetCore.Components.WebView.Maui;

namespace JiayiLauncher;

public partial class MainPage : ContentPage
{
    public BlazorWebView BlazorWebView => WebView;
    
    public MainPage()
    {
        InitializeComponent();
        Log.CreateLog();

        WebView.BlazorWebViewInitialized += (_, e) =>
        {
            e.WebView.DefaultBackgroundColor = Windows.UI.Color.FromArgb(255, 15, 15, 15);
        };
		
        // TODO: the installer should be doing this
        // WinRegistry.SetFileAssociation("Jiayi Mod Collection", ".jiayi");
        // WinRegistry.RegisterUrlProtocol();
		
        // Theme path is local\default before settings load
        if (!File.Exists(ThemeState.ThemePath))
        {
            var themeParent = Path.GetDirectoryName(ThemeState.ThemePath)!;
            Directory.CreateDirectory(themeParent);
            File.Copy(Path.Combine(ThemeState.RootPath, "css", "theme.css"), ThemeState.ThemePath);
        }
		      
        ThemeState.Instance = new ThemeState(CssBuilder.FromFile(ThemeState.ThemePath));
        JiayiSettings.Load();
		      
        // Path changes after settings load
        if (!File.Exists(ThemeState.ThemePath))
        {
            JiayiSettings.Instance.Theme = ".local/default";
            JiayiSettings.Instance.Save();
        }
		      
        InternetManager.CheckOnline();
        Task.Run(async () => await VersionList.UpdateVersions());
		      
        if (JiayiSettings.Instance.ModCollectionPath != string.Empty)
        {
            ModCollection.Load(JiayiSettings.Instance.ModCollectionPath);
        }
		      
        if (JiayiSettings.Instance.ProfileCollectionPath != string.Empty)
        {
            ProfileCollection.Load(JiayiSettings.Instance.ProfileCollectionPath);
        }
        
        if (JiayiSettings.Instance.VersionsPath == string.Empty)
        {
            JiayiSettings.Instance.VersionsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "JiayiLauncher", "Versions");
            JiayiSettings.Instance.Save();
        }
		      
        JiayiStats.Save();
        Task.Run(async () => await PackageData.MinimizeFix(true));
        RichPresence.Initialize();
    }
}