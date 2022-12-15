using System;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using Blazored.Modal;
using Microsoft.Extensions.DependencyInjection;

namespace JiayiLauncher;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
	[DllImport("dwmapi.dll", PreserveSig = true)]
	private static extern int DwmSetWindowAttribute(nint hWnd, int attr, ref bool attrValue, int attrSize);
	
	public MainWindow()
	{
		InitializeComponent();
		
		SourceInitialized += DarkTitlebar;

		var services = new ServiceCollection();
		services.AddWpfBlazorWebView();
		services.AddBlazoredModal();
#if DEBUG
		services.AddBlazorWebViewDeveloperTools();
#endif
		Resources.Add("services", services.BuildServiceProvider());
	}

	private void DarkTitlebar(object? sender, EventArgs e)
	{
		var windowHelper = new WindowInteropHelper(this);
		var value = true;
		DwmSetWindowAttribute(windowHelper.Handle, 20, ref value, Marshal.SizeOf(value));
	}
}