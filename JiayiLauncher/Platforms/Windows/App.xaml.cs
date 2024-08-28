using System.Reflection;
using System.Runtime.InteropServices;
using Blazored.Toast;
using Blazored.Toast.Services;
using JiayiLauncher.Shared.Components.Toasts;
using JiayiLauncher.Utils;
using Microsoft.AspNetCore.Components;
using static JiayiLauncher.Utils.Imports;

namespace JiayiLauncher.Platforms.Windows;

public partial class App : MauiWinUIApplication
{
    // mutex is out here to prevent it from being garbage collected
    private Mutex _mutex;

    public App()
    {
        InitializeComponent();
        
        // the first singletons
        var log = Singletons.Add<Log>();
        var arguments = Singletons.Add<Arguments>();
        
        // log current version
        var version = Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0, 0, 0);
        log.Write("JiayiLauncher", $"Running version {version.Major}.{version.Minor}.{version.Build}");
        
        _mutex = new Mutex(true, "Global\\JiayiLauncher", out var createdNew);

        if (createdNew) // only one instance of the launcher
        {
            // list of suppressed exceptions
            var suppressed = new List<string>
            {
                "TimeoutException",
                "Object name: 'System.Net.Sockets.NetworkStream'.",
                "System.Net.Sockets.SocketException (995): The I/O operation has been aborted because of either a thread exit or an application request."
            };
			
            AppDomain.CurrentDomain.FirstChanceException += (_, ex) =>
            {
                var exception = ex.Exception.ToString();
                if (suppressed.Any(exception.Contains)) return;
                
                log.Write(ex.Exception, exception, Log.LogLevel.Error);
                
                // TODO: show error dialog
            };
			
            return;
        }
        
        // send arguments to the running instance
        var ptr = Marshal.StringToHGlobalUni(arguments.Get());
		
        var hWnd = FindWindowW(null, "Jiayi Launcher");
        if (hWnd != nint.Zero)
        {
            CopyData cds;
            cds.dwData = 1;
            cds.cbData = (uint)((arguments.Get().Length + 1) * 2);
            cds.lpData = ptr;
			
            var pCds = Marshal.AllocHGlobal(Marshal.SizeOf<CopyData>());
            Marshal.StructureToPtr(cds, pCds, false);

            SendMessage(hWnd, 0x004A, 0, pCds.ToInt64());
        }
		
        Marshal.FreeHGlobal(ptr);
        Environment.Exit(0);
    }

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}