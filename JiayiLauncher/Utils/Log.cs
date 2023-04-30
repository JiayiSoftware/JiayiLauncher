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

	public static readonly string LogPath = Path.Combine(
		Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "JiayiLauncher", "Logs");

	public static void CreateLog()
	{
		Directory.CreateDirectory(Path.Combine(LogPath, "Previous"));

		if (!File.Exists(Path.Combine(LogPath, "Current.log"))) return;
		
		var name = $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.log";
		var previous = File.ReadAllText(Path.Combine(LogPath, "Current.log"));
		if (previous.Contains("Exception"))
			name = $"[CRASH] {name}";

		File.Move(Path.Combine(LogPath, "Current.log"),
			Path.Combine(LogPath, "Previous", name));
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
		
		File.AppendAllText(Path.Combine(LogPath, "Current.log"), logText + Environment.NewLine);
	}
}