using Blazored.Toast;
using Blazored.Toast.Configuration;
using Blazored.Toast.Services;
using JiayiLauncher.Shared.Components.Toasts;

namespace JiayiLauncher.Utils;

public static class BlazorBridge
{
	public static void ShowToast(ToastParameters parameters, Action<ToastSettings> settings)
	{
		var mainPage = (MainPage)Application.Current!.MainPage!;
		var dispatched = mainPage.BlazorWebView.TryDispatchAsync(sp =>
		{
			var toastService = sp.GetRequiredService<IToastService>();
			toastService.ShowToast<JiayiToast>(parameters, settings);
		}).Result;
		
		if (!dispatched) 
			Log.Write(nameof(BlazorBridge), "Failed to dispatch toast.", Log.LogLevel.Error);
	} 
}