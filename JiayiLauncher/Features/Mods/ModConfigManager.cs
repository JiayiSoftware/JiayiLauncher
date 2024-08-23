using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JiayiLauncher.Features.Game;
using JiayiLauncher.Utils;
using Microsoft.AspNetCore.Components.Forms;

namespace JiayiLauncher.Features.Mods;

public class ModConfigManager
{
	public readonly string ConfigPath;
	//private readonly string _configExtension; // maybe in the future
	
	private readonly Log _log = Singletons.Get<Log>();
	
	public ModConfigManager(Mod mod)
	{
		var modDataFolder = Path.Combine(PackageData.GetGameDataPath(), "RoamingState", mod.DataFolderName);
		
		// find config folder (could be Config, config, configs, configurations, etc)
		var configFolder = Directory.GetDirectories(modDataFolder)
			.FirstOrDefault(x => x.Contains("config", StringComparison.OrdinalIgnoreCase));
		
		ConfigPath = configFolder ?? string.Empty;
	}
	
	public async Task AddConfig(IBrowserFile file)
	{
		var fileName = Path.GetFileName(file.Name);
		var filePath = Path.Combine(ConfigPath, fileName);
		
		if (File.Exists(filePath)) File.Delete(filePath);
		
		await using var stream = File.Create(filePath);
		await file.OpenReadStream().CopyToAsync(stream);
		
		_log.Write(this, $"Added config {fileName}");
	}
	
	public void RemoveConfig(string path)
	{
		var configName = Path.GetFileName(path);
		var configPath = Path.Combine(ConfigPath, configName);
		
		if (!File.Exists(configPath)) return;
		
		File.Delete(configPath);
		
		_log.Write(this, $"Removed config {configName}");
	}
	
	public List<string> GetConfigs()
	{
		return Directory.GetFiles(ConfigPath).ToList();
	}

	public void OpenConfig(string path)
	{
		var configName = Path.GetFileName(path);
		var configPath = Path.Combine(ConfigPath, configName);
		
		if (!File.Exists(configPath)) return;
		
		Process.Start(new ProcessStartInfo
		{
			FileName = "notepad.exe",
			Arguments = configPath,
			UseShellExecute = true
		});
	}
}