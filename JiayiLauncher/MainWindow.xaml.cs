using System;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using Blazored.Modal;
using JiayiLauncher.Features.Discord;
using JiayiLauncher.Features.Mods;
using JiayiLauncher.Settings;
using JiayiLauncher.Utils;
using Microsoft.AspNetCore.Components.WebView;
using Microsoft.Extensions.DependencyInjection;

using static JiayiLauncher.Utils.Imports;

namespace JiayiLauncher;

public partial class MainWindow
{
	public MainWindow()
	{
		var __ = new Mutex(true, "JiayiLauncher", out var createdNew);

		if (!createdNew)
		{
			var args = Environment.GetCommandLineArgs().ToList();
			args.RemoveAt(0);
			var argString = string.Join(" ", args);
			
			// allocate memory for the string
			var ptr = Marshal.StringToHGlobalUni(argString);
			
			var hWnd = FindWindow(null, "Jiayi Launcher");
			if (hWnd != nint.Zero)
			{
				if (argString != string.Empty)
				{
					CopyData cds;
					cds.dwData = 1;
					cds.cbData = (uint)((argString.Length + 1) * 2);
					cds.lpData = ptr;

					SendMessage(hWnd, 0x004A, nint.Zero, ref cds);
				}
			}
			Environment.Exit(0);
		}
		
		InitializeComponent();
		Log.CreateLog();
		
		SourceInitialized += NativeInit;
		
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
#if DEBUG
		services.AddBlazorWebViewDeveloperTools();
#endif
		Resources.Add("services", services.BuildServiceProvider());
		
		// startup stuff
		JiayiSettings.Load();
		if (JiayiSettings.Instance!.ModCollectionPath != string.Empty)
		{
			ModCollection.Load(JiayiSettings.Instance.ModCollectionPath);
		}
		
		RichPresence.Initialize();
	}

	private void NativeInit(object? sender, EventArgs e)
	{
		var windowHelper = new WindowInteropHelper(this);
		var value = true;
		var result = DwmSetWindowAttribute(windowHelper.Handle, 20, ref value, Marshal.SizeOf(value));
		if (result != 0) Log.Write(this, $"Failed to set dark titlebar. Error code: {result}", Log.LogLevel.Warning);
		
		var source = HwndSource.FromHwnd(windowHelper.Handle);
		source?.AddHook(WndProc);
	}

	private nint WndProc(nint hWnd, int msg, nint wParam, nint lParam, ref bool handled)
	{
		if (msg != 0x004A) return 0;
		handled = true;
		
		var data = Marshal.PtrToStructure<CopyData>(lParam);
		var args = Marshal.PtrToStringUni(data.lpData);
		if (args != null)
		{
			Log.Write(this, $"Received args: {args}");
		}
		return 0;
	}

	// ReSharper disable once UnusedMember.Local
	private void ChangeColor(object? unused, BlazorWebViewInitializedEventArgs e)
	{
		e.WebView.DefaultBackgroundColor = Color.FromArgb(15, 15, 15);
	}
}