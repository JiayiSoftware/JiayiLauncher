using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using Microsoft.AspNetCore.Components.WebView.Wpf;
using Microsoft.Extensions.DependencyInjection;

namespace JiayiLauncher;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
	[DllImport("dwmapi.dll", PreserveSig = true)]
	private static extern int DwmSetWindowAttribute(IntPtr hWnd, int attr, ref bool attrValue, int attrSize);
	
	public MainWindow()
	{
		InitializeComponent();
		
		SourceInitialized += DarkTitlebar;

		var services = new ServiceCollection();
		services.AddWpfBlazorWebView();
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

	private void ShowWebView(object sender, RoutedEventArgs e)
	{
		var webView = (BlazorWebView)sender;
		webView.Visibility = Visibility.Visible;
	}
}