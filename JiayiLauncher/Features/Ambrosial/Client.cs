using System;

public class Client {
    public Client(string name, string version, string mc_version, string dll_url)
    {
        Name = name;
        Version = version;
        DllURL = dll_url;
        FormattedName = $"{name} v{version}";

        var mc_ver = mc_version.Split(".");

		var major = mc_ver[0];
		var minor = mc_ver[1];
		var build = mc_ver[2].ToString()[..Math.Min(2, mc_ver[2].ToString().Length)];
        var revision = mc_ver[2].ToString()[^1];

        MCVersion = $"{major}.{minor}.{build}.{revision}";
    }
    public string Name { get; set; }
    public string FormattedName { get; set; }
    public string Version { get; set; }
    public string MCVersion { get; set; }
    public string DllURL { get; set; }
    
}
