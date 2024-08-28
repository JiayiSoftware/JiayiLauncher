using System.Diagnostics;
using JiayiLauncher.Settings;

namespace JiayiLauncher.Utils;

public class Log
{
	public enum LogLevel
	{
		Info,
		Warning,
		Error
	}

	public static readonly string LogPath = Path.Combine(
		Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "JiayiLauncher", "Logs");
	
	private FileStream? _logStream;
	private bool _noLogFile;

	public Log()
	{
		Directory.CreateDirectory(Path.Combine(LogPath, "Previous"));

		if (!File.Exists(Path.Combine(LogPath, "Current.log"))) return;

		try
		{
			var name = $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.log";
			var previous = File.ReadAllText(Path.Combine(LogPath, "Current.log"));
			if (previous.Contains("Exception"))
				name = $"[CRASH] {name}";

			File.Move(Path.Combine(LogPath, "Current.log"), Path.Combine(LogPath, "Previous", name));
		}
		catch
		{
			_noLogFile = true;
			Write(this, "Log file is not available, logging to console only", LogLevel.Warning);
		}
	}

	public void Write(object sender, string message, LogLevel level = LogLevel.Info)
	{
		var logText = $"[{DateTime.Now:HH:mm:ss}]";
		
		if (sender is string senderString)
			logText += $" [{senderString} - {level}]: {message}";
		else
			logText += $" [{sender.GetType().Name} - {level}]: {message}";

#if DEBUG
		Debug.WriteLine(logText);
		if (_noLogFile) return;
#endif
		
		// if not debug
		if (_noLogFile)
		{
			Console.WriteLine(logText);
			return;
		}

		if (JiayiSettings.Instance != null && JiayiSettings.Instance.AnonymizeLogs)
			logText = logText.Replace(Environment.UserName, "User");

		_logStream ??= File.Open(Path.Combine(LogPath, "Current.log"), 
			FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
		
		_logStream.Write(System.Text.Encoding.UTF8.GetBytes(logText + Environment.NewLine));
		_logStream.Flush();
	}
}