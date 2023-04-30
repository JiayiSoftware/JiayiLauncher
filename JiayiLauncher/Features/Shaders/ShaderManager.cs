using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using JiayiLauncher.Features.Game;
using JiayiLauncher.Settings;
using JiayiLauncher.Utils;
using Microsoft.AspNetCore.Components.Forms;

namespace JiayiLauncher.Features.Shaders;

public static class ShaderManager
{
	public static List<string> Shaders { get; } = new();
	public static string AppliedShader { get; set; } = string.Empty;
	public static List<string> AvailableShaders => Shaders.Where(x => x != AppliedShader).ToList();

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
		
		Log.Write(nameof(ShaderManager), $"Backed up vanilla shaders for version {await PackageData.GetVersion()}");
	}
	
	public static async Task DeleteBackupShaders()
	{
		var version = await PackageData.GetVersion();
		var path = Path.Combine(JiayiSettings.Instance!.ShadersPath, "Vanilla", version);
		if (Directory.Exists(path)) Directory.Delete(path, true);
		
		Log.Write(nameof(ShaderManager), $"Deleted backup shaders for version {version}");
	}

	public static async Task RestoreVanillaShaders()
	{
		var info = await PackageData.GetPackage();
		if (info == null) return;
		
		var installPath = info.AppInfo.Package.InstalledPath;
		var path = Path.Combine(installPath, "data", "renderer", "materials");
		if (!Directory.Exists(path)) return;
		
		var version = await PackageData.GetVersion();
		var backupPath = Path.Combine(JiayiSettings.Instance!.ShadersPath, "Vanilla", version);
		if (!Directory.Exists(backupPath)) return;
		
		foreach (var file in Directory.GetFiles(backupPath))
		{
			var fileName = Path.GetFileName(file);
			File.Copy(file, Path.Combine(path, fileName), true);
		}

		Log.Write(nameof(ShaderManager), $"Restored vanilla shaders for version {version}");
	}

	public static void UpdateShaders()
	{
		Shaders.Clear();
		
		if (!Directory.Exists(JiayiSettings.Instance!.ShadersPath) 
		    || !Directory.Exists(Path.Combine(JiayiSettings.Instance.ShadersPath, "Applied")))
		{
			var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
				"JiayiLauncher", "Shaders");
			
			JiayiSettings.Instance.ShadersPath = path;
			Directory.CreateDirectory(path);
			Directory.CreateDirectory(Path.Combine(path, "Applied"));
			
			JiayiSettings.Instance.Save();
		}

		var files = Directory.GetFiles(JiayiSettings.Instance.ShadersPath, "*.zip");
		Shaders.AddRange(files.Select(Path.GetFileNameWithoutExtension)!);
		
		var applied = Directory.GetFiles(Path.Combine(JiayiSettings.Instance.ShadersPath, "Applied"), "*.zip");
		AppliedShader = Path.GetFileNameWithoutExtension(applied.FirstOrDefault() ?? string.Empty);
		
		Log.Write(nameof(ShaderManager), 
			$"Updated shaders list. Found {Shaders.Count} shaders. {AppliedShader} is currently applied.");
	}

	public static async Task AddShader(IBrowserFile file)
	{
		var path = Path.Combine(JiayiSettings.Instance!.ShadersPath, file.Name);
		if (!path.EndsWith(".zip")) return;
		
		await using var stream = File.Create(path);
		await file.OpenReadStream(524288000L).CopyToAsync(stream);
		
		Log.Write(nameof(ShaderManager), $"Added shader {file.Name}");
		UpdateShaders();
	}

	public static void EnableShader(string shader)
	{
		if (AppliedShader != string.Empty) DisableShader(AppliedShader);
		
		var path = Path.Combine(JiayiSettings.Instance!.ShadersPath, shader + ".zip");
		if (!File.Exists(path)) return;
		
		File.Move(path, Path.Combine(JiayiSettings.Instance.ShadersPath, "Applied", shader + ".zip"));
		
		Log.Write(nameof(ShaderManager), $"Enabled shader {shader}");
		UpdateShaders();
	}
	
	public static void DisableShader(string shader)
	{
		var path = Path.Combine(JiayiSettings.Instance!.ShadersPath, "Applied", shader + ".zip");
		if (!File.Exists(path)) return;
		
		File.Move(path, Path.Combine(JiayiSettings.Instance.ShadersPath, shader + ".zip"));
		
		Log.Write(nameof(ShaderManager), $"Disabled shader {shader}");
		UpdateShaders();
	}
	
	public static void DeleteShader(string shader)
	{
		var path = Path.Combine(JiayiSettings.Instance!.ShadersPath, shader + ".zip");
		if (!File.Exists(path)) return;
		
		File.Delete(path);
		
		Log.Write(nameof(ShaderManager), $"Deleted shader {shader}");
		UpdateShaders();
	}

	public static async Task ApplyShader()
	{
		if (AppliedShader == string.Empty) return;
		
		var path = Path.Combine(JiayiSettings.Instance!.ShadersPath, "Applied", AppliedShader + ".zip");
		if (!File.Exists(path)) return;
		
		var version = await PackageData.GetVersion();
		var backupPath = Path.Combine(JiayiSettings.Instance.ShadersPath, "Vanilla", version);
		if (!Directory.Exists(backupPath)) await BackupVanillaShaders();

		await RestoreVanillaShaders();
		
		var info = await PackageData.GetPackage();
		if (info == null) return;
		
		var installPath = info.AppInfo.Package.InstalledPath;
		var shaderPath = Path.Combine(installPath, "data", "renderer", "materials");
		if (!Directory.Exists(shaderPath)) return;
		
		var shaderFiles = Directory.GetFiles(shaderPath);
		
		using var zip = ZipFile.OpenRead(path);
		
		foreach (var file in shaderFiles)
		{
			var fileName = Path.GetFileName(file);
			if (zip.Entries.Any(x => x.Name == fileName)) File.Delete(file);
		}

		zip.ExtractToDirectory(shaderPath);
		Log.Write(nameof(ShaderManager), $"Applied shader {AppliedShader}");
	}
}