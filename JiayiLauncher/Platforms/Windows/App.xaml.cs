using JiayiLauncher.Utils;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
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
                "System.Threading.Tasks.TaskCanceledException",
                "TimeoutException",
                "Object name: 'System.Net.Sockets.NetworkStream'.",
                "at WinRT.ExceptionHelpers.<ThrowExceptionForHR>g__Throw",
                "at Microsoft.AspNetCore.Components.WebView.WebView2.WebView2WebViewManager.SendMessage(String message)",
                "System.Net.Sockets.SocketException (995): The I/O operation has been aborted because of either a thread exit or an application request."
            };

            AppDomain.CurrentDomain.FirstChanceException += async (_, ex) =>
            {
                var exception = ex.Exception.ToString();
                if (suppressed.Any(exception.Contains)) return;

                log.Write(ex.Exception, exception, Log.LogLevel.Error);

                var windowHandle = FindWindowW(null, "Jiayi Launcher");
                var result = MessageBoxW(
                    windowHandle,
                    """
                    Jiayi has ran into an issue and needs to close. Please send your log file to the nearest developer.

                    Would you like to anonymously upload your current log file?
                    """,
                    "Crash",
                    0x00000004 | 0x00000010);

                if (result == 6) // yes
                {
                    string current_log = Path.Join(Log.LogPath, "Current.log");
                    if (File.Exists(current_log))
                    {
                        string upload_bat = Path.Join(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "upload.bat");

                        ProcessStartInfo psi = new ProcessStartInfo
                        {
                            FileName = upload_bat,
                            WindowStyle = ProcessWindowStyle.Minimized,
                            CreateNoWindow = true,
                            UseShellExecute = true
                        };
                        Process.Start(psi);
                    }
                }

                result = MessageBoxW(
                    windowHandle,
                    """
                    Would you like to open the log folder?
                    
                    Your log file is saved as 'Current.log'.
                    """,
                    "Crash",
                    0x00000004 | 0x00000010);

                if (result == 6) // yes
                {
                    Process.Start("explorer.exe", Log.LogPath);
                }



                Environment.Exit(1);
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