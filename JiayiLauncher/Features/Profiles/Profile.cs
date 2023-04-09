namespace JiayiLauncher.Features.Profiles;

public class Profile
{
	public string Name { get; set; }
	public string Path { get; set; }
	
	public Profile(string name, string path)
	{
		Name = name;
		Path = path;
	}
}