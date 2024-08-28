using System;
using System.Runtime.InteropServices;

namespace JiayiLauncher.Utils;

public class Arguments
{
	public event EventHandler? Changed;
	
	private string _args = string.Empty;
	private readonly Log _log = Singletons.Get<Log>();

	public Arguments()
	{
		var args = Environment.GetCommandLineArgs().Skip(1).ToList();
		
		// "jiayi://" might be in the args if the user clicked a jiayi: link
		if (args.Any(x => x.StartsWith("jiayi://"))) 
			args = args.Select(x => x.Replace("jiayi://", string.Empty)).ToList();
		
		Set(string.Join(" ", args));
	}

	public void Set(string args)
	{
		_args = args;
		if (string.IsNullOrWhiteSpace(args))
		{
			_log.Write("Arguments", "No arguments received");
			return;
		}
		
		_log.Write("Arguments", $"Received arguments: {args}");
		Changed?.Invoke(null, EventArgs.Empty);
	}
	
	public string Get() => _args;
}
