using System;
using System.IO;
using System.Threading.Tasks;
using JiayiLauncher.Features.Game;
using JiayiLauncher.Settings;

namespace JiayiLauncher.Features.Shaders;

public static class ShaderManager
{
	public static async Task BackupVanillaShaders()
	{
		var info = await PackageData.GetPackage();
		if (info == null) return;
		
		var installPath = info.AppInfo.Package.InstalledPath;
		var path = Path.Combine(installPath, "data", "renderer", "materials");
		
		// if the shaders folder doesn't exist, there's nothing to back up
		if (!Directory.Exists(path)) return;

		if (JiayiSettings.Instance!.ShadersPath == string.Empty)
		{
			JiayiSettings.Instance.ShadersPath =
				Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
					"JiayiLauncher", "Shaders");
			
			JiayiSettings.Instance.Save();
		}
		
		var backupPath = Path.Combine(JiayiSettings.Instance.ShadersPath, "Vanilla", await PackageData.GetVersion());
		Directory.CreateDirectory(backupPath);
		
		foreach (var file in Directory.GetFiles(path))
		{
			var fileName = Path.GetFileName(file);
			File.Copy(file, Path.Combine(backupPath, fileName), true);
		}
	}
}