namespace JiayiLauncher.Utils;

public static class ActionDebounce
{
	public static Action<T> Debounce<T>(this Action<T> func, int milliseconds = 300)
	{
		CancellationTokenSource? cancelTokenSource = null;

		return arg =>
		{
			cancelTokenSource?.Cancel();
			cancelTokenSource = new CancellationTokenSource();

			Task.Delay(milliseconds, cancelTokenSource.Token)
				.ContinueWith(t =>
				{
					if (t.IsCompletedSuccessfully)
					{
						func(arg);
					}
				}, TaskScheduler.Default);
		};
	}
}