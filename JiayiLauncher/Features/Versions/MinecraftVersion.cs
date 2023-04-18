using System.Collections.Generic;

namespace JiayiLauncher.Features.Versions;

public class MinecraftVersion
{
	public string Version { get; set; }
	public Architectures Archs { get; set; }
}

public class Architectures
{
	public Architecture? arm { get; set; }
	public Architecture? x64 { get; set; }
	public Architecture? x86 { get; set; }
}

public class Architecture
{
	public string FileName { get; set; }
	public Hashes Hashes { get; set; }
	public IList<string> UpdateIds { get; set; }
}

public class Hashes
{
	public string? MD5 { get; set; }
	public string? SHA256 { get; set; }
}