using System;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Management.Deployment;
using Windows.System;
using JiayiLauncher.Settings;
using JiayiLauncher.Utils;

namespace JiayiLauncher.Features.Game;

public static class PackageData
{
	public static PackageManager PackageManager { get; } = new();
	
	public static async Task<AppDiagnosticInfo?> GetPackage()
	{
		var info = 
			await AppDiagnosticInfo.RequestInfoForPackageAsync("Microsoft.MinecraftUWP_8wekyb3d8bbwe");
		if (info == null || info.Count == 0 || !File.Exists(info[0].AppInfo.Package.InstalledPath)) return null;
		return info[0];
	}

	public static async Task<string> GetVersion()
	{
		var minecraftApp = await GetPackage();
		if (minecraftApp == null) return "Unknown";
		var version = minecraftApp.AppInfo.Package.Id.Version;
		
		// the game does it weird
		var major = version.Major;
		var minor = version.Minor;
		
		// weird edge case here
		var build = version.Build.ToString();
		char revision;
		if (build.Length != 1)
		{
			build = build[..(version.Build.ToString().Length - 2)];
			// the last number of the build number is the revision
			revision = version.Build.ToString()[^1];
		}
		else
		{
			// for some reason 1.20.0.1's build and revision are swapped (1.20.1.0)
			// i sure hope mojang just changed how versions work and this isn't a one time thing
			if (version == new PackageVersion(1, 20, 1, 0))
				return "1.20.0.1";
			
			// when mojang does the silly and makes the build number 1 digit
			// the revision is literally the revision
			revision = version.Revision.ToString()[^1];
		}
		
		return $"{major}.{minor}.{build}.{revision}";
	}

	public static async Task<InstallLocation> GetInstallLocation()
	{
		var package = await GetPackage();
		if (package == null) return InstallLocation.Unknown;
		
		if (!package.AppInfo.Package.IsDevelopmentMode) return InstallLocation.MicrosoftStore;
		
		var installPath = package.AppInfo.Package.InstalledLocation.Path;

		// inverting this if statement looks confusing, i'm not taking your suggestion rider
		if (JiayiSettings.Instance!.VersionsPath == string.Empty)
		{
			JiayiSettings.Instance.VersionsPath =
				Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
					"JiayiLauncher", "Versions");
			JiayiSettings.Instance.Save();
		}
		
		return installPath.Contains(JiayiSettings.Instance.VersionsPath) ? 
			InstallLocation.FromJiayi : InstallLocation.OtherVersionManager;
	}

	public static string GetGameDataPath()
	{
		// i thought i could just use the package for this but naw gotta hardcode it
		return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
			"Packages", "Microsoft.MinecraftUWP_8wekyb3d8bbwe");
	}

	public static async Task BackupGameData(string to)
	{
        Directory.CreateDirectory(to);

        var localState = Path.Combine(GetGameDataPath(), "LocalState");
        var roamingState = Path.Combine(GetGameDataPath(), "RoamingState");

        Directory.CreateDirectory(Path.Combine(to, "LocalState"));
        Directory.CreateDirectory(Path.Combine(to, "RoamingState"));

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
            var dir = Path.GetDirectoryName(file);
            if (dir != null)
            {
                Directory.CreateDirectory(Path.Combine(to, "LocalState", dir));
            }
        }

        foreach (var file in roamingStateFiles)
        {
            var dir = Path.GetDirectoryName(file);
            if (dir != null)
            {
                Directory.CreateDirectory(Path.Combine(to, "RoamingState", dir));
            }
        }

        await Task.Run(() =>
        {
	        // copy the files
            foreach (var file in localStateFiles)
            {
                try
                {
                    // use a stream to move the file contents (File.Copy has issues because of file owner)
                    var read = File.OpenRead(Path.Combine(localState, file));
                    var write = File.OpenWrite(Path.Combine(to, "LocalState", file));
                    read.CopyTo(write);
                    
                    read.Close();
                    write.Close();
                }
                catch (Exception ex)
                {
                    Log.Write("PackageData", $"Failed to copy file {file}: {ex}");
                }
            }

            foreach (var file in roamingStateFiles)
            {
                try
                {
                    var read = File.OpenRead(Path.Combine(roamingState, file));
					var write = File.OpenWrite(Path.Combine(to, "RoamingState", file));
					read.CopyTo(write);
					
					read.Close();
					write.Close();
                }
                catch (Exception ex)
                {
                    Log.Write("PackageData", $"Failed to copy file {file}: {ex}");
                }
            }
        });
        
        Log.Write("PackageData", $"Created backup of game data in {to}");
	}

	public static async Task ReplaceGameData(string dataPath)
	{
		var localState = Path.Combine(GetGameDataPath(), "LocalState");
        var roamingState = Path.Combine(GetGameDataPath(), "RoamingState");

        // delete the existing game data
        Directory.Delete(localState, true);
        Directory.Delete(roamingState, true);

        var backupLocalState = Path.Combine(dataPath, "LocalState");
        var backupRoamingState = Path.Combine(dataPath, "RoamingState");

        // copy the backup's game data
        var localStateFiles = Directory.GetFiles(backupLocalState, "*.*", SearchOption.AllDirectories);
        var roamingStateFiles = Directory.GetFiles(backupRoamingState, "*.*", SearchOption.AllDirectories);

        for (var i = 0; i < localStateFiles.Length; i++)
        {
            localStateFiles[i] = localStateFiles[i][backupLocalState.Length..];

            if (localStateFiles[i][0] == '\\')
            {
                localStateFiles[i] = localStateFiles[i][1..];
            }
        }

        for (var i = 0; i < roamingStateFiles.Length; i++)
        {
            roamingStateFiles[i] = roamingStateFiles[i][backupRoamingState.Length..];

            if (roamingStateFiles[i][0] == '\\')
            {
                roamingStateFiles[i] = roamingStateFiles[i][1..];
            }
        }

        foreach (var file in localStateFiles)
        {
            var dir = Path.GetDirectoryName(file);
            if (dir != null)
            {
                Directory.CreateDirectory(Path.Combine(localState, dir));
            }
        }

        foreach (var file in roamingStateFiles)
        {
            var dir = Path.GetDirectoryName(file);
            if (dir != null)
            {
                Directory.CreateDirectory(Path.Combine(roamingState, dir));
            }
        }

        await Task.Run(() =>
        {
	        foreach (var file in localStateFiles)
	        {
		        var read = File.OpenRead(Path.Combine(backupLocalState, file));
		        var write = File.OpenWrite(Path.Combine(localState, file));
		        read.CopyTo(write);

		        read.Close();
		        write.Close();
	        }

	        foreach (var file in roamingStateFiles)
	        {
		        var read = File.OpenRead(Path.Combine(backupRoamingState, file));
		        var write = File.OpenWrite(Path.Combine(roamingState, file));
		        read.CopyTo(write);

		        read.Close();
		        write.Close();
	        }
        });
	}
}