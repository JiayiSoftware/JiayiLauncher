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
	public bool ToastShown { get; set; }
	
	private readonly CancellationTokenSource _cts = new();
	private readonly TaskFactory _factory;
	
	public JiayiTask(Task task, string name, string fromPage)
	{
		Task = task;
		Name = name;
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

	public JiayiTasks(NavigationManager navigation, IToastService toastService)
	{
		navigation.LocationChanged += (_, e) =>
		{
			foreach (var task in _tasks)
			{
				if (!e.Location.Contains(task.FromPage))
				{
					if (task.ToastShown) continue;
					
					var toastParams = new ToastParameters()
						.Add(nameof(JiayiToast.Level), ToastLevel.Info)
						.Add(nameof(JiayiToast.Title), task.Name)
						.Add(nameof(JiayiToast.Content), new RenderFragment(builder =>
						{
							builder.OpenElement(0, "p");
							builder.AddContent(1, "This task is running in the background.");
							builder.CloseElement();
						}));
						
					toastService.ShowToast<JiayiToast>(toastParams, settings =>
					{
						settings.Timeout = 3;
					});
					
					task.ToastShown = true;
				}
				else
				{
					task.ToastShown = false;
				}
			}
		};
	}

	public JiayiTask AddTask(Task task, string name, string fromPage)
	{
		var jiayiTask = new JiayiTask(task, name, fromPage);
		
		// remove task from list when it's done
		//task.ContinueWith(_ => _tasks.Remove(jiayiTask));
		_tasks.Add(jiayiTask);
		
		return jiayiTask;
	}
}