using Blazored.Modal;
using Blazored.Toast;
using Microsoft.Extensions.Logging;

namespace JiayiLauncher;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();

        builder.Services.AddMauiBlazorWebView();
        builder.Services.AddBlazoredModal();
        builder.Services.AddBlazoredToast();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}