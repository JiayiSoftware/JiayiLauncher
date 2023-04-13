using System.Collections.Generic;
using System.IO;
using JiayiLauncher.Utils;

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
	
	public static void Load(string basePath)
	{
		var collection = new ProfileCollection
		{
			BasePath = basePath
		};
		
		var profiles = Directory.GetDirectories(basePath);
		foreach (var profile in profiles)
		{
			var name = Path.GetFileName(profile);
			var profileObj = new Profile(name, profile);
			collection.Add(profileObj);
		}
		
		Current = collection;
		Log.Write("ProfileCollection", $"Loaded {profiles.Length} profiles from {basePath}");
	}
	
	public void Add(Profile profile)
	{
		Profiles.Add(profile);
	}
}