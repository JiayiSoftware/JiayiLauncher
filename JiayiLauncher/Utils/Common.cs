using System;
using System.Threading.Tasks;
using System.Timers;

namespace JiayiLauncher.Utils;

public static class Common
{
	public static Timer SetTimeout(Action callback, int timeout)
	{
		var timer = new Timer(timeout);
		timer.AutoReset = false;
		timer.Elapsed += (_, _) =>
		{
			callback();
		};
		timer.Start();
		return timer;
	}
	
	// with async function
	public static Timer SetTimeoutAsync(Func<Task> callback, int timeout)
	{
		var timer = new Timer(timeout);
		timer.AutoReset = false;
		timer.Elapsed += async (_, _) =>
		{
			await callback();
		};
		timer.Start();
		return timer;
	}
}