using System;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading.Tasks;
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

    public static async Task<Profile> Create(string name, ProfileCollection collection)
    {
	    var fullPath = System.IO.Path.Combine(collection.BasePath, name);
        await PackageData.BackupGameData(fullPath);

        // create a README.txt
        await File.WriteAllTextAsync(System.IO.Path.Combine(fullPath, "README.txt"),
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

    public async Task Apply()
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

        await Task.Run(() =>
        {
	        foreach (var file in localStateFiles)
	        {
		        var read = File.OpenRead(System.IO.Path.Combine(profileLocalState, file));
		        var write = File.OpenWrite(System.IO.Path.Combine(localState, file));
		        read.CopyTo(write);

		        read.Close();
		        write.Close();
	        }

	        foreach (var file in roamingStateFiles)
	        {
		        var read = File.OpenRead(System.IO.Path.Combine(profileRoamingState, file));
		        var write = File.OpenWrite(System.IO.Path.Combine(roamingState, file));
		        read.CopyTo(write);

		        read.Close();
		        write.Close();
	        }
        });
        
        Log.Write("Profile.Apply", $"Applied profile {Name}");
    }

    public void Delete()
    {
        if (Directory.Exists(Path))
            Directory.Delete(Path, true);

        ProfileCollection.Current!.Profiles.Remove(this);
        Log.Write("Profile.Delete", $"Deleted profile {Name}");
    }

    public bool IsValid()
    {
        var dirExists = Directory.Exists(Path);

        var profileLocalState = System.IO.Path.Combine(Path, "LocalState");
        var profileRoamingState = System.IO.Path.Combine(Path, "RoamingState");
        var localStateExists = Directory.Exists(profileLocalState);
        var roamingStateExists = Directory.Exists(profileRoamingState);

        return dirExists && localStateExists && roamingStateExists;
    }
}