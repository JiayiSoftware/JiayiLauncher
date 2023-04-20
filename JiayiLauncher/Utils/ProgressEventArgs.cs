using System;

namespace JiayiLauncher.Utils;

public class ProgressEventArgs : EventArgs
{
	public int Percentage { get; set; }
	
	public ProgressEventArgs(int percentage)
	{
		Percentage = percentage;
	}
}