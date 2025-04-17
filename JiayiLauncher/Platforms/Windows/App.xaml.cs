using System.Diagnostics;
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

    private Exception? _lastException;

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
            // exception handler taken from https://gist.github.com/mattjohnsonpint/7b385b7a2da7059c4a16562bc5ddb3b7
            
            AppDomain.CurrentDomain.FirstChanceException += (_, ex) =>
            {
                _lastException = ex.Exception;
            };

            Microsoft.UI.Xaml.Application.Current.UnhandledException += (_, ex) =>
            {
                var exception = ex.Exception;
                if (exception.StackTrace == null)
                {
                    exception = _lastException!;
                }

                log.Write(exception, exception.ToString(), Log.LogLevel.Error);

                var blazor = Singletons.TryGet<BlazorBridge>();
                if (blazor == null) return;

                var parameters = new ToastParameters()
                    .Add(nameof(JiayiToast.Level), ToastLevel.Warning)
                    .Add(nameof(JiayiToast.Title), "Internal error")
                    .Add(nameof(JiayiToast.Content), new RenderFragment(builder =>
                    {
                        builder.OpenElement(0, "p");
                        builder.AddContent(1,
                            "This should not affect the launcher. View your log file for more information.");
                        builder.CloseElement();
                    }));
                blazor.ShowToast(parameters, settings =>
                {
                    settings.Timeout = 5;
                });
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

            SendMessage(hWnd, 0x004A, 0, pCds);
        }
		
        Marshal.FreeHGlobal(ptr);
        Environment.Exit(0);
    }

    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
