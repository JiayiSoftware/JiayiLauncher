using System.IO;
using System.Threading.Tasks;
using JiayiLauncher.Features.Game;
using JiayiLauncher.Utils;

namespace JiayiLauncher.Features.Profiles;

public class Profile
{
    public string Name { get; set; }
    public string Path { get; set; }
    
    private static readonly Log _log = Singletons.Get<Log>();
    private static readonly PackageData _packageData = Singletons.Get<PackageData>();

    public Profile(string name, string path)
    {
        Name = name;
        Path = path;
    }

    public static async Task<Profile> Create(string name, ProfileCollection collection)
    {
	    var fullPath = System.IO.Path.Combine(collection.BasePath, name);
        await _packageData.BackupGameData(fullPath);

        // create a README.txt
        await File.WriteAllTextAsync(System.IO.Path.Combine(fullPath, "README.txt"),
	        "This folder contains the game data for this profile. Don't mess with it unless you know what you're doing.");

        _log.Write("Profile.Create", $"Created profile {name} at {fullPath}");

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
	    await _packageData.ReplaceGameData(Path);
	    _log.Write("Profile.Apply", $"Applied profile {Name}");
    }

    public void Delete()
    {
        if (Directory.Exists(Path))
            Directory.Delete(Path, true);

        ProfileCollection.Current!.Profiles.Remove(this);
        _log.Write("Profile.Delete", $"Deleted profile {Name}");
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