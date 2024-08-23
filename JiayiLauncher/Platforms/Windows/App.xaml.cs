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
    public App()
    {
        InitializeComponent();
        
        var mutex = new Mutex(true, "Global\\JiayiLauncher", out var createdNew);
        var args = Environment.GetCommandLineArgs().Skip(1).ToList();
		
        // "jiayi://" might be in the args if the user clicked a jiayi: link
        if (args.Any(x => x.StartsWith("jiayi://"))) 
            args = args.Select(x => x.Replace("jiayi://", string.Empty)).ToList();

        if (createdNew)
        {
            Arguments.Set(string.Join(" ", args));
            
            // list of suppressed exceptions
            var suppressed = new List<string>
            {
                "TimeoutException",
                "System.Net.Sockets.SocketException (995): The I/O operation has been aborted because of either a thread exit or an application request."
            };
			
            AppDomain.CurrentDomain.FirstChanceException += (_, ex) =>
            {
                var exception = ex.Exception.ToString();
                if (suppressed.Any(exception.Contains)) return;
                
                var log = Singletons.Get<Log>();
                log.Write(ex.Exception, exception, Log.LogLevel.Error);
                
                // TODO: show error dialog
            };
			
            return;
        }
        var argString = string.Join(" ", args);
			
        var ptr = Marshal.StringToHGlobalUni(argString);
		
        var hWnd = FindWindowW(null, "Jiayi Launcher");
        if (hWnd != nint.Zero)
        {
            CopyData cds;
            cds.dwData = 1;
            cds.cbData = (uint)((argString.Length + 1) * 2);
            cds.lpData = ptr;
			
            var pCds = Marshal.AllocHGlobal(Marshal.SizeOf<CopyData>());
            Marshal.StructureToPtr(cds, pCds, false);

            SendMessage(hWnd, 0x004A, 0, pCds.ToInt64());
        }
		
        mutex.Close();
        Exit();
    }

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}