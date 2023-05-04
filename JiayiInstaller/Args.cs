namespace JiayiInstaller;

public class Args
{
	private readonly string _args;
	
	public Args()
	{
		var args = Environment.GetCommandLineArgs();
		// separate args with | so we can have spaces in them
		_args = args.Length > 1 ? args.Skip(1).Aggregate((a, b) => $"{a}|{b}") : string.Empty;
	}
	
	public bool Empty => string.IsNullOrEmpty(_args);
	
	public bool GetFlag(string flag)
	{
		return _args.Contains($"--{flag}");
	}
	
	public string GetCommand(string command)
	{
		var fullFlag = $"--{command}";
		var index = _args.IndexOf(fullFlag, StringComparison.Ordinal);
		if (index == -1) return string.Empty;
		
		// start index + length of flag + 1 for the argument separator
		var startIndex = index + fullFlag.Length + 1;
		var endIndex = _args.IndexOf('|', startIndex);
		if (endIndex == -1) endIndex = _args.Length; // probably the last command
		
		return _args[startIndex..endIndex];
	}
}