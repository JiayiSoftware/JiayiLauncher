using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Security.Principal;
using JiayiLauncher.Appearance;
using JiayiLauncher.Features.Discord;
using JiayiLauncher.Features.Game;
using JiayiLauncher.Features.Launch;
using JiayiLauncher.Features.Mods;
using JiayiLauncher.Features.Profiles;
using JiayiLauncher.Features.Shaders;
using JiayiLauncher.Features.Stats;
using JiayiLauncher.Features.Versions;
using JiayiLauncher.Settings;
using JiayiLauncher.Utils;
using Microsoft.AspNetCore.Components.WebView.Maui;
using Launcher = JiayiLauncher.Features.Launch.Launcher;

namespace JiayiLauncher;

public partial class MainPage : ContentPage
{
    public BlazorWebView BlazorWebView => WebView;
    
    public MainPage()
    {
        // set ui culture for testing
        //CultureInfo.CurrentUICulture = new CultureInfo("es-ES", false);
        
        InitializeComponent();
        
        // new singleton system
        var log = Singletons.Add<Log>();
        var packageData = Singletons.Add<PackageData>();
        Singletons.Add<ShaderManager>();
        Singletons.Add<Injector>();
        Singletons.Add<Minecraft>();
        Singletons.Add<Launcher>();
        Singletons.Add<ModImporter>();
        
        // log current version
        var version = Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0, 0, 0);
        log.Write("JiayiLauncher", $"Running version {version.Major}.{version.Minor}.{version.Build}");

        WebView.BlazorWebViewInitialized += (_, e) =>
        {
            e.WebView.DefaultBackgroundColor = Windows.UI.Color.FromArgb(255, 15, 15, 15);
        };
		
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
        Task.Run(async () => await packageData.MinimizeFix(JiayiSettings.Instance.MinimizeFix));
        
        //RichPresence.Initialize();
        Singletons.Add<RichPresence>();
    }
}