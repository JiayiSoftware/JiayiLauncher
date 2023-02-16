using System.Collections.Generic;

namespace JiayiLauncher.Features.Mods;

public class Mod
{
	public string? Name { get; set; }
	public string? Path { get; set; }
	public List<string> SupportedVersions { get; set; } = new() { "Any" };
}