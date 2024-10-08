﻿using Blazored.Modal;
using Blazored.Modal.Services;
using Blazored.Toast;
using Blazored.Toast.Configuration;
using Blazored.Toast.Services;
using JiayiLauncher.Shared.Components.Toasts;
using Microsoft.AspNetCore.Components;

namespace JiayiLauncher.Utils;

public class BlazorBridge
{
	private readonly Log _log = Singletons.Get<Log>();
	
	public void ShowToast(ToastParameters parameters, Action<ToastSettings> settings)
	{
		var mainPage = (MainPage?)Application.Current!.MainPage;
		if (mainPage == null)
		{
			_log.Write(nameof(BlazorBridge), "MainPage was null", Log.LogLevel.Error);
			return;
		}
		
		var dispatched = mainPage.BlazorWebView.TryDispatchAsync(sp =>
		{
			var toastService = sp.GetRequiredService<IToastService>();
			toastService.ShowToast<JiayiToast>(parameters, settings);
		}).Result;
		
		if (!dispatched) 
			_log.Write(nameof(BlazorBridge), "Failed to dispatch toast.", Log.LogLevel.Error);
	}
	
	public async Task<ModalResult> ShowModal<T>(string title) where T : IComponent
	{
		var mainPage = (MainPage?)Application.Current!.MainPage;
		if (mainPage == null)
		{
			_log.Write(nameof(BlazorBridge), "MainPage was null", Log.LogLevel.Error);
			return ModalResult.Cancel();
		}
		
		ModalResult? result = null;
		
		var dispatched = await mainPage.BlazorWebView.TryDispatchAsync(async sp =>
		{
			var modalService = sp.GetRequiredService<IModalService>();
			var modal = modalService.Show<T>(title);
			result = await modal.Result;
		});

		if (dispatched && result != null) return result;
		
		_log.Write(nameof(BlazorBridge), "Failed to dispatch modal.", Log.LogLevel.Error);
		return ModalResult.Cancel();
	}

	public async Task<ModalResult> ShowModal<T>(string title, ModalParameters parameters) where T : IComponent
	{
		var mainPage = (MainPage?)Application.Current!.MainPage;
		if (mainPage == null)
		{
			_log.Write(nameof(BlazorBridge), "MainPage was null", Log.LogLevel.Error);
			return ModalResult.Cancel();
		}
		
		ModalResult? result = null;
		
		var dispatched = await mainPage.BlazorWebView.TryDispatchAsync(async sp =>
        {
            var modalService = sp.GetRequiredService<IModalService>();
            var modal = modalService.Show<T>(title, parameters);
            result = await modal.Result;
        });

		if (dispatched && result != null) return result;
		
		_log.Write(nameof(BlazorBridge), "Failed to dispatch modal.", Log.LogLevel.Error);
		return ModalResult.Cancel();
	}
}