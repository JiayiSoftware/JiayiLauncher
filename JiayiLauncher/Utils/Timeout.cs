using System;
using System.Timers;

namespace JiayiLauncher.Utils;

public static class Timeout
{
	public static Timer Set(Action callback, int timeout)
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
}