using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;

using static JiayiLauncher.Utils.Imports;

namespace JiayiLauncher;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App
{
	private Mutex? _mutex;
		
	protected override void OnStartup(StartupEventArgs e)
	{
		_mutex = new Mutex(true, "Global\\JiayiLauncher", out var createdNew);

		if (createdNew) return;
		var args = Environment.GetCommandLineArgs().ToList();
		args.RemoveAt(0);
		var argString = string.Join(" ", args);
			
		// allocate memory for the string
		var ptr = Marshal.StringToHGlobalUni(argString);
			
		var hWnd = FindWindowW(null, "Jiayi Launcher");
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
		
		_mutex = null;
		Shutdown();
	}
	
	protected override void OnExit(ExitEventArgs e)
	{
		_mutex?.ReleaseMutex();
		base.OnExit(e);
	}
}