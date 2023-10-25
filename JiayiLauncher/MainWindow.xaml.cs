using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using Blazored.Modal;
using Blazored.Toast;
using Fluxor;
using JiayiLauncher.Appearance;
using JiayiLauncher.Features.Discord;
using JiayiLauncher.Features.Mods;
using JiayiLauncher.Features.Profiles;
using JiayiLauncher.Features.Stats;
using JiayiLauncher.Features.Versions;
using JiayiLauncher.Settings;
using JiayiLauncher.Utils;
using Microsoft.AspNetCore.Components.WebView;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Web.WebView2.Wpf;
using static JiayiLauncher.Utils.Imports;

namespace JiayiLauncher;

public partial class MainWindow
{
	private static WebView2? _webView;

	public MainWindow()
	{
		InitializeComponent();
		Log.CreateLog();
		
		AppDomain.CurrentDomain.UnhandledException += (_, args) =>
		{
			Log.Write(args.ExceptionObject, ((Exception)args.ExceptionObject).ToString(), Log.LogLevel.Error);
			MessageBox.Show(
				"Jiayi has ran into a problem and needs to close. Please send your log file to the nearest developer.",
				"Crash", MessageBoxButton.OK, MessageBoxImage.Error
			);
		};

		var services = new ServiceCollection();
		services.AddWpfBlazorWebView();
		services.AddBlazoredModal();
		services.AddBlazoredToast();
		services.AddFluxor(options =>
		{
			options.ScanAssemblies(Assembly.GetExecutingAssembly());
		});
#if DEBUG
		services.AddBlazorWebViewDeveloperTools();
#endif
		Resources.Add("services", services.BuildServiceProvider());
		
		// startup stuff
		
		// user should be admin at this point but just in case
		using var identity = WindowsIdentity.GetCurrent();
		var principal = new WindowsPrincipal(identity);
		if (!principal.IsInRole(WindowsBuiltInRole.Administrator)) return;	
		
		// registry stuff
		WinRegistry.SetFileAssociation("Jiayi Mod Collection", ".jiayi");
		WinRegistry.RegisterUrlProtocol();
		
		JiayiSettings.Load();
		InternetManager.CheckOnline();
		
		if (JiayiSettings.Instance!.ModCollectionPath != string.Empty)
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

		Task.Run(VersionList.UpdateVersions);
		RichPresence.Initialize();
		ThemeManager.LoadTheme();
		JiayiStats.Save();
	}

	protected override void OnSourceInitialized(EventArgs e)
	{
		var windowHelper = new WindowInteropHelper(this);
		var value = true;
		var result = DwmSetWindowAttribute(windowHelper.Handle, 20, ref value, Marshal.SizeOf(value));
		if (result != 0) Log.Write(this, $"Failed to set dark titlebar. Error code: {result}", Log.LogLevel.Warning);

		var source = HwndSource.FromHwnd(windowHelper.Handle);
		source?.AddHook(WndProc);
		
		base.OnSourceInitialized(e);
	}

	private nint WndProc(nint hWnd, int msg, nint wParam, nint lParam, ref bool handled)
	{
		if (msg != 0x004A)
		{
			handled = false;
			return 0; // WM_COPYDATA
		}

		handled = true;
		
		var data = Marshal.PtrToStructure<CopyData>(lParam);
		var args = Marshal.PtrToStringUni(data.lpData);
		if (args != null)
		{
			Log.Write(this, $"Received args: {args}");
			Arguments.Set(args);
			
			// bring window to front
			Activate();
		}
		
		return 0;
	}

	// ReSharper disable once UnusedMember.Local
	// ReSharper disable once UnusedParameter.Local
	private void ModifyWebView(object? _, BlazorWebViewInitializedEventArgs e)
	{
		_webView = e.WebView;
		_webView.DefaultBackgroundColor = Color.FromArgb(15, 15, 15);
	}
}