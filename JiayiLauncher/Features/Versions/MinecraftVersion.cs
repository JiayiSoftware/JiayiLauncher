using System.Collections.Generic;

namespace JiayiLauncher.Features.Versions;

public class MinecraftVersion
{
	public string FileName { get; set; }
	public string UpdateId { get; set; }
	public string Version { get; set; }

	public MinecraftVersion(string fileName, string updateId, string version)
	{
		FileName = fileName;
		UpdateId = updateId;
		Version = version;
	}
}