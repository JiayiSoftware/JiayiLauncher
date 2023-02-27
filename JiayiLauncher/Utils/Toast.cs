using System;
using System.Timers;

namespace JiayiLauncher.Utils;

public class Toast
{
	public event EventHandler? Finished;
	public event EventHandler? Clicked;
	
	public string Title { get; set; }
	public string Message { get; set; }
	public int Timeout { get; set; } // if unset then it will be determined by the length of the message
	
	private readonly Timer _timer = new();
	
	public static Toast Create(string title, string message, int timeout = 0)
	{
		var toast = new Toast
		{
			Title = title,
			Message = message,
			Timeout = timeout
		};
		return toast;
	}
}