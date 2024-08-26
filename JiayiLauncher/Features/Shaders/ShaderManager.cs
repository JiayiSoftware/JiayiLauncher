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

public class ShaderManager
{
	public List<string> Shaders { get; } = new();
	public string AppliedShader { get; set; } = string.Empty;
	public List<string> AvailableShaders => Shaders.Where(x => x != AppliedShader).ToList();

	private readonly string[] _blockedFolders = ["iOS", "Android"];
	
	private readonly Log _log = Singletons.Get<Log>();
	private readonly PackageData _packageData = Singletons.Get<PackageData>();

	public async Task BackupVanillaShaders()
	{
		var info = await _packageData.GetPackage();
		if (info == null) return;
		
		var installPath = info.AppInfo.Package.InstalledPath;
		var path = Path.Combine(installPath, "data", "renderer", "materials");
		
		// if the shaders folder doesn't exist, there's nothing to back up
		if (!Directory.Exists(path)) return;

		if (JiayiSettings.Instance.ShadersPath == string.Empty)
		{
			JiayiSettings.Instance.ShadersPath =
				Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
					"JiayiLauncher", "Shaders");
			
			JiayiSettings.Instance.Save();
		}
		
		var backupPath = Path.Combine(JiayiSettings.Instance.ShadersPath, "Vanilla", await _packageData.GetVersion());
		Directory.CreateDirectory(backupPath);
		
		foreach (var file in Directory.GetFiles(path))
		{
			var fileName = Path.GetFileName(file);
			File.Copy(file, Path.Combine(backupPath, fileName), true);
		}
		
		_log.Write(nameof(ShaderManager), $"Backed up vanilla shaders for version {await _packageData.GetVersion()}");
	}
	
	public async Task DeleteBackupShaders()
	{
		var version = await _packageData.GetVersion();
		var path = Path.Combine(JiayiSettings.Instance.ShadersPath, "Vanilla", version);
		if (Directory.Exists(path)) Directory.Delete(path, true);
		
		_log.Write(nameof(ShaderManager), $"Deleted backup shaders for version {version}");
	}

	public async Task RestoreVanillaShaders()
	{
		var info = await _packageData.GetPackage();
		if (info == null) return;
		
		var installPath = info.AppInfo.Package.InstalledPath;
		var path = Path.Combine(installPath, "data", "renderer", "materials");
		if (!Directory.Exists(path)) return;
		
		var version = await _packageData.GetVersion();
		var backupPath = Path.Combine(JiayiSettings.Instance.ShadersPath, "Vanilla", version);
		if (!Directory.Exists(backupPath)) return;
		
		foreach (var file in Directory.GetFiles(backupPath))
		{
			var fileName = Path.GetFileName(file);
			File.Copy(file, Path.Combine(path, fileName), true);
		}

		_log.Write(nameof(ShaderManager), $"Restored vanilla shaders for version {version}");
	}

	public void UpdateShaders()
	{
		Shaders.Clear();
		
		if (!Directory.Exists(JiayiSettings.Instance.ShadersPath) 
		    || !Directory.Exists(Path.Combine(JiayiSettings.Instance.ShadersPath, "Applied")))
		{
			var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
				"JiayiLauncher", "Shaders");
			
			JiayiSettings.Instance.ShadersPath = path;
			Directory.CreateDirectory(path);
			Directory.CreateDirectory(Path.Combine(path, "Applied"));
			
			JiayiSettings.Instance.Save();
		}
		
		var folders = Directory.GetDirectories(JiayiSettings.Instance.ShadersPath).Where(x => !x.EndsWith("Applied") && !x.EndsWith("Vanilla"));
		Shaders.AddRange(folders.Select(Path.GetFileName)!);
		
		var applied = Directory.GetDirectories(Path.Combine(JiayiSettings.Instance.ShadersPath, "Applied"));
		AppliedShader = Path.GetFileName(applied.FirstOrDefault() ?? string.Empty);
		
		_log.Write(nameof(ShaderManager), AppliedShader == string.Empty
			? $"Updated shaders list. Found {Shaders.Count} shaders. {AppliedShader} is currently applied."
			: $"Updated shaders list. Found {Shaders.Count} shaders. No shader is currently applied.");
	}

	public async Task AddShader(IBrowserFile file)
	{
		var tempPath = Path.Combine(JiayiSettings.Instance.ShadersPath, "Temp");
		if (Directory.Exists(tempPath)) Directory.Delete(tempPath, true);
		Directory.CreateDirectory(tempPath);
		
		var path = Path.Combine(JiayiSettings.Instance.ShadersPath, file.Name);
		if (!path.EndsWith(".zip") && !path.EndsWith(".mcpack")) return;
		
		var stream = file.OpenReadStream(524288000L);
		var zipPath = Path.Combine(tempPath, file.Name);
		var fs = File.Create(zipPath);
		await stream.CopyToAsync(fs);
		stream.Close();
		fs.Close();
		
		await Task.Run(() => ZipFile.ExtractToDirectory(zipPath, tempPath));
		File.Delete(zipPath);
		
		var materialsFolder = FindMaterials(tempPath);
		if (materialsFolder == null) return;
		
		Directory.Move(materialsFolder, Path.Combine(JiayiSettings.Instance.ShadersPath, Path.GetFileNameWithoutExtension(file.Name)));
		Directory.Delete(tempPath, true);
		
		_log.Write(nameof(ShaderManager), $"Added shader {file.Name}");
		UpdateShaders();
	}

	private string? FindMaterials(string path)
	{
		var directories = Directory.GetDirectories(path, "*", SearchOption.AllDirectories).ToList();
		for (var i = 0; i < directories.Count; i++)
		{
			var directory = directories[i];
			// skip over any folders that are blocked
			if (_blockedFolders.Any(blockedFolder => directory.Contains(blockedFolder)))
			{
				directories.Remove(directory);
				i--;
				continue;
			}

			var files = Directory.GetFiles(directory);
			if (files.Any(x => x.EndsWith(".material.bin"))) continue; // folder has shaders
			
			directories.Remove(directory);
			i--;
		}

		// there should only be one folder left
		return directories.ElementAtOrDefault(0); // returns null if there are no folders left
	}
	
	public void RenameShader(string shader, string newName) {
		var path = Path.Combine(JiayiSettings.Instance.ShadersPath, shader);
		if (!Directory.Exists(path)) return;
		
		Directory.Move(path, Path.Combine(JiayiSettings.Instance.ShadersPath, newName));
		
		_log.Write(nameof(ShaderManager), $"Renamed shader {shader} to {newName}");
		UpdateShaders();
	}

	public void EnableShader(string shader)
	{
		if (AppliedShader != string.Empty) DisableShader(AppliedShader);
		
		var path = Path.Combine(JiayiSettings.Instance.ShadersPath, shader);
		if (!Directory.Exists(path)) return;
		
		Directory.Move(path, Path.Combine(JiayiSettings.Instance.ShadersPath, "Applied", shader));
		
		_log.Write(nameof(ShaderManager), $"Enabled shader {shader}");
		UpdateShaders();
	}
	
	public void DisableShader(string shader)
	{
		var path = Path.Combine(JiayiSettings.Instance.ShadersPath, "Applied", shader);
		if (!Directory.Exists(path)) return;
		
		Directory.Move(path, Path.Combine(JiayiSettings.Instance.ShadersPath, shader));
		
		_log.Write(nameof(ShaderManager), $"Disabled shader {shader}");
		UpdateShaders();
	}
	
	public void DeleteShader(string shader)
	{
		var path = Path.Combine(JiayiSettings.Instance.ShadersPath, shader);
		if (!Directory.Exists(path)) return;
		
		Directory.Delete(path, true);
		
		_log.Write(nameof(ShaderManager), $"Deleted shader {shader}");
		UpdateShaders();
	}

	public async Task ApplyShader()
	{
		if (AppliedShader == string.Empty) return;
		
		var path = Path.Combine(JiayiSettings.Instance.ShadersPath, "Applied", AppliedShader);
		if (!Directory.Exists(path)) return;
		
		var version = await _packageData.GetVersion();
		var backupPath = Path.Combine(JiayiSettings.Instance.ShadersPath, "Vanilla", version);
		if (!Directory.Exists(backupPath)) await BackupVanillaShaders();

		await RestoreVanillaShaders();
		
		var info = await _packageData.GetPackage();
		if (info == null) return;
		
		var installPath = info.AppInfo.Package.InstalledPath;
		var shaderPath = Path.Combine(installPath, "data", "renderer", "materials");
		if (!Directory.Exists(shaderPath)) return;
		
		var vanillaShaders = Directory.GetFiles(shaderPath);
		var shaders = Directory.GetFiles(path);

		foreach (var shader in shaders)
		{
			var fileName = Path.GetFileName(shader);
			if (vanillaShaders.Any(x => Path.GetFileName(x) == fileName))
			{
				File.Delete(Path.Combine(shaderPath, fileName));
			}
			
			File.Copy(shader, Path.Combine(shaderPath, fileName));
		}

		_log.Write(nameof(ShaderManager), $"Applied shader {AppliedShader}");
	}

	public List<string> GetMaterialDiff(string shader)
	{
		var path = Path.Combine(JiayiSettings.Instance.ShadersPath, shader);
		if (shader == AppliedShader) path = Path.Combine(JiayiSettings.Instance.ShadersPath, "Applied", shader);
		
		if (!Directory.Exists(path)) return [];
		
		var shaderFiles = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
		return shaderFiles.Select(Path.GetFileNameWithoutExtension)
			.Select(x => x?.Replace(".material", "")).ToList()!;
	}
}