using System.IO;
using JiayiLauncher.Features.Game;
using JiayiLauncher.Utils;

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
	
	public static Profile Create(string name, ProfileCollection collection)
	{
		var fullPath = System.IO.Path.Combine(collection.BasePath, name);
		Directory.CreateDirectory(fullPath);
		
		// profiles are created as a clone of (portions) of the user's game data
		// specifically, the LocalState and RoamingState folders, for actual game data and mod settings, like configs, respectively
		
		// first, check if the user actually has a game data folder
		if (!Directory.Exists(PackageData.GetGameDataPath()))
		{
			return new Profile(name, fullPath);
		}

		var localState = System.IO.Path.Combine(PackageData.GetGameDataPath(), "LocalState");
		var roamingState = System.IO.Path.Combine(PackageData.GetGameDataPath(), "RoamingState");
		
		Directory.CreateDirectory(System.IO.Path.Combine(fullPath, "LocalState"));
		Directory.CreateDirectory(System.IO.Path.Combine(fullPath, "RoamingState"));
		
		var localStateFiles = Directory.GetFiles(localState, "*.*", SearchOption.AllDirectories);
		var roamingStateFiles = Directory.GetFiles(roamingState, "*.*", SearchOption.AllDirectories);
		
		// turn all of these paths into relative paths
		for (var i = 0; i < localStateFiles.Length; i++)
		{
			localStateFiles[i] = localStateFiles[i][localState.Length..];
			
			// remove backslash at the start to not confuse Path.Combine
			if (localStateFiles[i][0] == '\\')
			{
				localStateFiles[i] = localStateFiles[i][1..];
			}
		}
		
		for (var i = 0; i < roamingStateFiles.Length; i++)
		{
			roamingStateFiles[i] = roamingStateFiles[i][roamingState.Length..];
			
			if (roamingStateFiles[i][0] == '\\')
			{
				roamingStateFiles[i] = roamingStateFiles[i][1..];
			}
		}
		
		// create any missing directories
		foreach (var file in localStateFiles)
		{
			var dir = System.IO.Path.GetDirectoryName(file);
			if (dir != null)
			{
				Directory.CreateDirectory(System.IO.Path.Combine(fullPath, "LocalState", dir));
			}
		}
		
		foreach (var file in roamingStateFiles)
		{
			var dir = System.IO.Path.GetDirectoryName(file);
			if (dir != null)
			{
				Directory.CreateDirectory(System.IO.Path.Combine(fullPath, "RoamingState", dir));
			}
		}
		
		// copy the files
		foreach (var file in localStateFiles)
		{
			File.Copy(System.IO.Path.Combine(localState, file), 
				System.IO.Path.Combine(fullPath, "LocalState", file));
		}
		
		foreach (var file in roamingStateFiles)
		{
			File.Copy(System.IO.Path.Combine(roamingState, file), 
				System.IO.Path.Combine(fullPath, "RoamingState", file));
		}
		
		// create a README.txt
		File.WriteAllText(System.IO.Path.Combine(fullPath, "README.txt"), 
			"This folder contains the game data for this profile. Don't mess with it unless you know what you're doing.");
		
		Log.Write("Profile.Create", $"Created profile {name} at {fullPath}");
		
		return new Profile(name, fullPath);
	}

	public ProfileInfo GetInfo()
	{
		// LocalState/games/com.mojang
		// from there count the number of folders in behavior_packs, resource_packs, and minecraftWorlds
		
		var mojang = System.IO.Path.Combine(Path, "LocalState", "games", "com.mojang");
		var behaviorPacks = System.IO.Path.Combine(mojang, "behavior_packs");
		var resourcePacks = System.IO.Path.Combine(mojang, "resource_packs");
		var minecraftWorlds = System.IO.Path.Combine(mojang, "minecraftWorlds");
		
		var behaviorPackCount = Directory.Exists(behaviorPacks) ? Directory.GetDirectories(behaviorPacks).Length : 0;
		var resourcePackCount = Directory.Exists(resourcePacks) ? Directory.GetDirectories(resourcePacks).Length : 0;
		var minecraftWorldCount = Directory.Exists(minecraftWorlds) ? Directory.GetDirectories(minecraftWorlds).Length : 0;

		return new ProfileInfo
		{
			BehaviorPacks = behaviorPackCount,
			ResourcePacks = resourcePackCount,
			Worlds = minecraftWorldCount
		};
	}

	public void Apply()
	{
		var localState = System.IO.Path.Combine(PackageData.GetGameDataPath(), "LocalState");
		var roamingState = System.IO.Path.Combine(PackageData.GetGameDataPath(), "RoamingState");
		
		// delete the existing game data
		Directory.Delete(localState, true);
		Directory.Delete(roamingState, true);
		
		var profileLocalState = System.IO.Path.Combine(Path, "LocalState");
		var profileRoamingState = System.IO.Path.Combine(Path, "RoamingState");
		
		// copy the profile's game data
		var localStateFiles = Directory.GetFiles(profileLocalState, "*.*", SearchOption.AllDirectories);
		var roamingStateFiles = Directory.GetFiles(profileRoamingState, "*.*", SearchOption.AllDirectories);
		
		for (var i = 0; i < localStateFiles.Length; i++)
		{
			localStateFiles[i] = localStateFiles[i][profileLocalState.Length..];
			
			if (localStateFiles[i][0] == '\\')
			{
				localStateFiles[i] = localStateFiles[i][1..];
			}
		}
		
		for (var i = 0; i < roamingStateFiles.Length; i++)
		{
			roamingStateFiles[i] = roamingStateFiles[i][profileRoamingState.Length..];
			
			if (roamingStateFiles[i][0] == '\\')
			{
				roamingStateFiles[i] = roamingStateFiles[i][1..];
			}
		}
		
		foreach (var file in localStateFiles)
		{
			var dir = System.IO.Path.GetDirectoryName(file);
			if (dir != null)
			{
				Directory.CreateDirectory(System.IO.Path.Combine(localState, dir));
			}
		}
		
		foreach (var file in roamingStateFiles)
		{
			var dir = System.IO.Path.GetDirectoryName(file);
			if (dir != null)
			{
				Directory.CreateDirectory(System.IO.Path.Combine(roamingState, dir));
			}
		}
		
		foreach (var file in localStateFiles)
		{
			File.Copy(System.IO.Path.Combine(profileLocalState, file), 
				System.IO.Path.Combine(localState, file));
		}
		
		foreach (var file in roamingStateFiles)
		{
			File.Copy(System.IO.Path.Combine(profileRoamingState, file), 
				System.IO.Path.Combine(roamingState, file));
		}
	}

	public void Delete()
	{
		Directory.Delete(Path, true);
		ProfileCollection.Current!.Profiles.Remove(this);
	}
}