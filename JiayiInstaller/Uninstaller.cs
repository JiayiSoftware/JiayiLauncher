using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace JiayiInstaller;

public static partial class Uninstaller
{
	[LibraryImport("shell32.dll", SetLastError = true)]
	private static partial void SHChangeNotify(uint eventId, uint flags, nint item1, nint item2);
	
	public static void Uninstall(string path)
	{
		// obvious first step
		Directory.Delete(path, true);
		
		// remove file associations
		Registry.ClassesRoot.DeleteSubKey("jiayi", false);
		Registry.ClassesRoot.DeleteSubKey(".jiayi", false);
		Registry.ClassesRoot.DeleteSubKey("Jiayi_Mod_Collection", false);
		Registry.CurrentUser.DeleteSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\FileExts\\.jiayi", false);
		
		// remove shortcuts
		var startMenuShortcut = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Jiayi Launcher.lnk");
		var desktopShortcut = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Jiayi Launcher.lnk");
		if (File.Exists(startMenuShortcut)) File.Delete(startMenuShortcut);
		if (File.Exists(desktopShortcut)) File.Delete(desktopShortcut);
		
		// notify shell
		SHChangeNotify(0x08000000, 0x0000, nint.Zero, nint.Zero);
	}
}