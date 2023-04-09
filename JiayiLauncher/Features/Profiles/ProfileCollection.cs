using System.Collections.Generic;

namespace JiayiLauncher.Features.Profiles;

public class ProfileCollection
{
	public string BasePath { get; private init; } = string.Empty;
	public List<Profile> Profiles { get; } = new();
	
	public static ProfileCollection? Current { get; private set; }
	
	private ProfileCollection() { }
	
	public static ProfileCollection Create(string basePath)
	{
		var collection = new ProfileCollection
		{
			BasePath = basePath
		};
		
		Current = collection;
		
		return collection;
	}
	
	public void Add(Profile profile)
	{
		Profiles.Add(profile);
	}
}