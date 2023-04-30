using Microsoft.Win32;
using System;

using static JiayiLauncher.Utils.Imports;

namespace JiayiLauncher.Utils;

public static class WinRegistry
{
	public static void SetFileAssociation(string description, string extension)
	{
		var baseKey = Registry.ClassesRoot.CreateSubKey(extension);
		baseKey.SetValue("", description.Replace(' ', '_'));
		baseKey.Close();
		
		var openWith = Registry.ClassesRoot.CreateSubKey(description.Replace(' ', '_'));
		openWith.SetValue("", description);
		openWith.CreateSubKey("DefaultIcon").SetValue("", $"\"{Environment.ProcessPath}\",0");
		openWith.CreateSubKey("shell").CreateSubKey("open").CreateSubKey("command")
			.SetValue("", $"\"{Environment.ProcessPath}\" \"%1\"");
		openWith.Close();
		
		var userChoice =
			Registry.CurrentUser.OpenSubKey(
				"Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\FileExts\\" + extension, true);
		if (userChoice != null)
		{
			userChoice.DeleteSubKey("UserChoice", false);
			userChoice.SetValue("Progid", description.Replace(' ', '_'));
			userChoice.Close();
		}
		
		SHChangeNotify(0x08000000, 0x0000, nint.Zero, nint.Zero);
	}

	public static void EnableDeveloperMode()
	{
		var appModelUnlockKey = Registry.LocalMachine.OpenSubKey(
			"SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\AppModelUnlock", true);
		
		appModelUnlockKey?.SetValue("AllowDevelopmentWithoutDevLicense", 1, RegistryValueKind.DWord);
	}
}