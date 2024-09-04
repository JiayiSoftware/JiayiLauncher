using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Management.Deployment;
using Windows.System;
using JiayiLauncher.Localization;
using JiayiLauncher.Settings;
using JiayiLauncher.Utils;

namespace JiayiLauncher.Features.Game;

public class PackageData
{
	public const string FAMILY_NAME = "Microsoft.MinecraftUWP_8wekyb3d8bbwe";
	
	public PackageManager PackageManager { get; } = new();
	
	private readonly Log _log = Singletons.Get<Log>();
	
	public async Task<AppDiagnosticInfo?> GetPackage()
	{
		var info = 
			await AppDiagnosticInfo.RequestInfoForPackageAsync(FAMILY_NAME);
		if (info == null || info.Count == 0 || !Directory.Exists(info[0].AppInfo.Package.InstalledPath)) return null;
		return info[0];
	}

	public async Task<string> GetVersion()
	{
		var minecraftApp = await GetPackage();
		if (minecraftApp == null) return Strings.Unknown;
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
			
			// UPDATE: they did it again. i'm not hardcoding this
			
			// when mojang does the silly and makes the build number 1 digit
			// the revision is literally the revision
			revision = version.Revision.ToString()[^1];
		}
		
		return $"{major}.{minor}.{build}.{revision}";
	}

	public async Task<InstallLocation> GetInstallLocation()
	{
		var package = await GetPackage();
		if (package == null) return InstallLocation.Unknown;
		
		if (!package.AppInfo.Package.IsDevelopmentMode) return InstallLocation.MicrosoftStore;
		
		string installPath;

        try
        {
            installPath = package.AppInfo.Package.InstalledLocation.Path;
        } catch (Exception ex)
        {
            _log.Write(nameof(PackageData), $"Failed to locate minecraft (lost path): {ex.Message}", Log.LogLevel.Error);
            return InstallLocation.Unknown;
        }

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

	public string GetGameDataPath()
	{
		// i thought i could just use the package for this but naw gotta hardcode it
		return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
			"Packages", FAMILY_NAME);
	}
	
	public async Task MinimizeFix(bool fix)
	{
		var package = await GetPackage();
		if (package == null) return;
		
		var debugSettings = new PackageDebugSettings();
		if (fix)
			debugSettings.EnableDebugging(package.AppInfo.Package.Id.FullName, null, null);
		else 
			debugSettings.DisableDebugging(package.AppInfo.Package.Id.FullName);
	}

	public async Task MultiInstance(bool enable, string manifestPath)
	{
		const string f1 = "xmlns:build=\"http://schemas.microsoft.com/developer/appx/2015/build\""; // in AppxManifest.xml - Package
		const string r1 = "xmlns:build=\"http://schemas.microsoft.com/developer/appx/2015/build\" xmlns:desktop4=\"http://schemas.microsoft.com/appx/manifest/desktop/windows10/4\"";
		const string f2 = "EntryPoint=\"Minecraft_Win10.App\""; // in AppxManifest.xml - Application
		const string r2 = "EntryPoint=\"Minecraft_Win10.App\" desktop4:SupportsMultipleInstances=\"true\"";
		
		var manifest = await File.ReadAllTextAsync(manifestPath);
		
		if (enable)
		{
			if (manifest.Contains(r1) && manifest.Contains(r2)) return;
			
			manifest = manifest.Replace(f1, r1);
			manifest = manifest.Replace(f2, r2);
		}
		else
		{
			if (!manifest.Contains(r1) && !manifest.Contains(r2)) return;
			
			manifest = manifest.Replace(r1, f1);
			manifest = manifest.Replace(r2, f2);
		}
		
		await File.WriteAllTextAsync(manifestPath, manifest);
		
		// package should be re-registered at this point
	}

	public async Task BackupGameData(string to)
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
                    _log.Write("PackageData", $"Failed to copy file {file}: {ex}");
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
                    _log.Write("PackageData", $"Failed to copy file {file}: {ex}");
                }
            }
        });
        
        _log.Write("PackageData", $"Created backup of game data in {to}");
	}

	public async Task ReplaceGameData(string dataPath)
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