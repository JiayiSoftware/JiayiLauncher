using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JiayiLauncher.Features.Game;
using JiayiLauncher.Settings;
using JiayiLauncher.Utils;

namespace JiayiLauncher.Features.Shaders;

public static class ShaderManager
{
	public static List<string> Shaders { get; } = new();
	public static List<string> AppliedShaders { get; } = new();
	public static List<string> AvailableShaders => Shaders.Except(AppliedShaders).ToList();

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
		
		Log.Write(nameof(ShaderManager), "Backed up vanilla shaders");
	}
	
	public static async Task DeleteBackupShaders()
	{
		var version = await PackageData.GetVersion();
		var path = Path.Combine(JiayiSettings.Instance!.ShadersPath, "Vanilla", version);
		if (Directory.Exists(path)) Directory.Delete(path, true);
		
		Log.Write(nameof(ShaderManager), "Deleted backup shaders");
	}

	public static void UpdateShaders()
	{
		if (!Directory.Exists(JiayiSettings.Instance!.ShadersPath))
		{
			JiayiSettings.Instance.ShadersPath =
				Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
					"JiayiLauncher", "Shaders");
			
			JiayiSettings.Instance.Save();
		}
		
		var files = Directory.GetFiles(JiayiSettings.Instance.ShadersPath, "*.zip");
		Shaders.AddRange(files.Select(Path.GetFileNameWithoutExtension)!);
	}

	public static async Task ApplyShader(string shader)
	{
		
	}
}