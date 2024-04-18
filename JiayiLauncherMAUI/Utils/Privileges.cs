using System.Diagnostics;
using System.Security.Principal;
using System.Reflection;

namespace JiayiLauncher.Utils;

public static class Privileges
{
	public static bool IsAdmin()
	{
		using var identity = WindowsIdentity.GetCurrent();
		var principal = new WindowsPrincipal(identity);
		return principal.IsInRole(WindowsBuiltInRole.Administrator);
	}
	
	public static void Escalate()
	{
		if (IsAdmin()) return;
		
		var exePath = Assembly.GetExecutingAssembly().Location;
		exePath = exePath.Replace("dll", "exe"); // because .NET
		
		var startInfo = new ProcessStartInfo
		{
			FileName = "cmd",
			Arguments = $"/c start \"\" \"{exePath}\"",
			Verb = "runas",
			UseShellExecute = true
		};
		
		Process.Start(startInfo);
		Application.Current!.Quit();
	}
}