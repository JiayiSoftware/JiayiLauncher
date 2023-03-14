using System;
using System.Diagnostics;
using System.IO;

namespace JiayiLauncher.Utils;

public static class Log
{
	public enum LogLevel
	{
		Info,
		Warning,
		Error
	}
	
	private static string _logPath = Path.Combine(
		Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "JiayiLauncher", "Logs");

	public static void CreateLog()
	{
		Directory.CreateDirectory(Path.Combine(_logPath, "Previous"));
		
		if (File.Exists(Path.Combine(_logPath, "Current.log")))
			File.Move(Path.Combine(_logPath, "Current.log"), Path.Combine(_logPath, "Previous", $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.log"));
	}

	public static void Write(object sender, string message, LogLevel level = LogLevel.Info)
	{
		var logText = $"[{DateTime.Now:HH:mm:ss}]";
		
		if (sender is string senderString)
			logText += $" [{senderString} - {level}]: {message}";
		else
			logText += $" [{sender.GetType().Name} - {level}]: {message}";

#if DEBUG
		Debug.WriteLine(logText);
#endif
		
		File.AppendAllText(Path.Combine(_logPath, "Current.log"), logText + Environment.NewLine);
	}
}