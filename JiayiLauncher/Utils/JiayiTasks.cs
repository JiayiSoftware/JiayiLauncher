using Blazored.Toast;
using Blazored.Toast.Services;
using JiayiLauncher.Shared.Components.Toasts;
using Microsoft.AspNetCore.Components;

namespace JiayiLauncher.Utils;

public class JiayiTask
{
	public Task Task { get; set; }
	public string Name { get; set; }
	public string FromPage { get; set; }
	
	private readonly CancellationTokenSource _cts = new();
	private readonly TaskFactory _factory;
	
	public JiayiTask(Task task, string name, string fromPage)
	{
		Task = task;
		FromPage = fromPage;
		_factory = new TaskFactory(_cts.Token);
	}
	
	public void Start()
	{
		_factory.StartNew(() => Task.Start());
	}
	
	public void Cancel()
	{
		_cts.Cancel();
	}
}

public class JiayiTasks
{
	private readonly List<JiayiTask> _tasks = new();

	private readonly Log _log = Singletons.Get<Log>();

	public JiayiTasks()
	{
		var mainPage = (MainPage?)Application.Current!.MainPage;
		if (mainPage == null)
		{
			_log.Write(nameof(BlazorBridge), "MainPage was null", Log.LogLevel.Error);
			return;
		}

		var dispatched = mainPage.BlazorWebView.TryDispatchAsync(sp =>
		{
			var navigation = sp.GetRequiredService<NavigationManager>();
			navigation.LocationChanged += (_, e) =>
			{
				foreach (var task in _tasks)
				{
					if (e.Location.Contains(task.FromPage))
					{
						var bridge = Singletons.Get<BlazorBridge>();

						var toastParams = new ToastParameters()
							.Add(nameof(JiayiToast.Level), ToastLevel.Info)
							.Add(nameof(JiayiToast.Title), task.Name)
							.Add(nameof(JiayiToast.Content), new RenderFragment(builder =>
							{
								builder.OpenElement(0, "p");
								builder.AddContent(1, "This task is running in the background.");
								builder.CloseElement();
							}));
						
						bridge.ShowToast(toastParams, settings =>
						{
							settings.Timeout = 3;
							settings.ShowCloseButton = false;
						});
					}
				}
			};
		}).Result;

		if (!dispatched)
			_log.Write(nameof(JiayiTasks), "Some background tasks may not work properly.", Log.LogLevel.Warning);
	}

	public JiayiTask AddTask(Task task, string name, string fromPage)
	{
		var jiayiTask = new JiayiTask(task, name, fromPage);
		_tasks.Add(jiayiTask);
		
		// remove task from list when it's done
		task.ContinueWith(_ => _tasks.Remove(jiayiTask));
		
		return jiayiTask;
	}
}