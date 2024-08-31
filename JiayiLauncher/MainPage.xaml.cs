using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
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
using JiayiLauncher.Localization;
using JiayiLauncher.Settings;
using JiayiLauncher.Utils;
using Microsoft.AspNetCore.Components.WebView.Maui;
using Launcher = JiayiLauncher.Features.Launch.Launcher;

namespace JiayiLauncher;

public partial class MainPage : ContentPage
{
    public BlazorWebView BlazorWebView => WebView;
    
    private static nint _originalWndProc;
    private nint _windowHandle;
    private delegate int WndProc(nint hWnd, uint msg, nuint wParam, nint lParam);
    
    public MainPage()
    {
        // set ui culture for testing
        //CultureInfo.CurrentUICulture = new CultureInfo("es-ES", false);
        
        InitializeComponent();
        Singletons.Add<BlazorBridge>();

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
        
        // load languages
        var rm = new ResourceManager(typeof(Strings));
        var cultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
        var supportedCultures = new List<CultureInfo>();

        foreach (var cultureInfo in cultures)
        {
            try
            {
                if (cultureInfo.Equals(CultureInfo.InvariantCulture)) continue;
                    
                var resourceSet = rm.GetResourceSet(cultureInfo, true, false);
                if (resourceSet != null) supportedCultures.Add(cultureInfo);
            }
            catch (CultureNotFoundException)
            {
                // ignored
            }
        }
            
        // add languages to languages
        JiayiSettings.Instance.Language.AvailableModes.AddRange(supportedCultures.Select(x => x.EnglishName));
        
        // set language
        if (JiayiSettings.Instance.Language.Mode != "Use system language")
        {
            var lang = JiayiSettings.Instance.Language.Mode;
            var culture = supportedCultures.FirstOrDefault(x => x.EnglishName.Contains(lang));
            if (culture != null) CultureInfo.CurrentUICulture = culture;
        }
        
        // add the rest of the singletons
        Singletons.Add<InternetManager>();
        var packageData = Singletons.Add<PackageData>();
        var versionList = Singletons.Add<VersionList>();
        Singletons.Add<Privileges>();
        Singletons.Add<WinRegistry>();
        Singletons.Add<ShaderManager>();
        Singletons.Add<VersionManager>();
        Singletons.Add<Injector>();
        Singletons.Add<JiayiStats>();
        Singletons.Add<Minecraft>();
        Singletons.Add<Launcher>();
        Singletons.Add<ModImporter>();
        Singletons.Add<RichPresence>();
        Singletons.Add(this);

        WebView.BlazorWebViewInitialized += (_, e) =>
        {
            e.WebView.DefaultBackgroundColor = Windows.UI.Color.FromArgb(255, 15, 15, 15);
            
            // technically the window is created when this event is fired so
            
            // set window hook manually because maui doesn't support it
            _windowHandle = Imports.FindWindowW(null, "Jiayi Launcher");
            if (_windowHandle == nint.Zero) return;
		
            _originalWndProc = Imports.SetWindowLongPtrA(_windowHandle, -4, 
                Marshal.GetFunctionPointerForDelegate<WndProc>(WndProcHook));
            
            var log = Singletons.Get<Log>();
            log.Write("MainPage", "Window hook set");
        };
        Task.Run(async () => await versionList.UpdateVersions());
		      
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
        
        Task.Run(async () => await packageData.MinimizeFix(JiayiSettings.Instance.MinimizeFix));
    }
    
    public void ActivateWindow()
    {
        Imports.SetForegroundWindow(_windowHandle);
    }
    
    private static int WndProcHook(nint hWnd, uint msg, nuint wParam, nint lParam)
    {
        if (msg == Imports.WM_COPYDATA)
        {
            var cds = Marshal.PtrToStructure<Imports.CopyData>(lParam);
            var args = Marshal.PtrToStringUni(cds.lpData, (int)cds.cbData / 2);
            Singletons.Get<Arguments>().Set(args);
            
            Singletons.Get<MainPage>().ActivateWindow();
        }
		
        return Imports.CallWindowProcA(_originalWndProc, hWnd, msg, wParam, lParam);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Window.MinimumWidth = 750;
        Window.MinimumHeight = 420; // funny number huh
    }
}