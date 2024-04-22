using System;

namespace JiayiLauncher.Utils;

public static class Arguments
{
	private static string _args = string.Empty;
	
	public static event EventHandler? Changed;

	public static void Set(string args)
	{
		_args = args;
		Changed?.Invoke(null, EventArgs.Empty);
	}
	
	public static string Get() => _args;
}