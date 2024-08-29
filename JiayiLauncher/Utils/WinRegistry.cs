using Microsoft.Win32;
using System;

using static JiayiLauncher.Utils.Imports;

namespace JiayiLauncher.Utils;

public class WinRegistry
{
	public void EnableDeveloperMode()
	{
		var appModelUnlockKey = Registry.LocalMachine.OpenSubKey(
			@"SOFTWARE\Microsoft\Windows\CurrentVersion\AppModelUnlock", true);
		
		appModelUnlockKey?.SetValue("AllowDevelopmentWithoutDevLicense", 1, RegistryValueKind.DWord);
	}

	public bool DeveloperModeEnabled()
	{
		var appModelUnlockKey = Registry.LocalMachine.OpenSubKey(
			@"SOFTWARE\Microsoft\Windows\CurrentVersion\AppModelUnlock", false);

		var val = (int?)appModelUnlockKey?.GetValue("AllowDevelopmentWithoutDevLicense");
		return val == 1;
	}
}