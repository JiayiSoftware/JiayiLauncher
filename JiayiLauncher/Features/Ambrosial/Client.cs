using System;

namespace JiayiLauncher.Features.Ambrosial;

public class Client {
	public string Name { get; set; }
	public string FormattedName { get; set; }
	public string Version { get; set; }
	public string SupportedVersion { get; set; }
	public string Url { get; set; }
	
	public Client(string name, string version, string supportedVersion, string url)
	{
		Name = name;
		Version = version;
		Url = url;
		FormattedName = $"{name} v{version}";

		var ver = supportedVersion.Split(".");

		var major = ver[0];
		var minor = ver[1];
		var build = ver[2][..Math.Min(2, ver[2].Length)];
		var revision = ver[2][^1];

		SupportedVersion = $"{major}.{minor}.{build}.{revision}";
	}
}