using System;

namespace JiayiLauncher.Settings;

[Serializable]
public class JiayiSettings
{
	// general settings
	public Setting<string> ModCollectionPath { get; set; } = new("Mod Collection Path", "General", "The path to your mod collection.", string.Empty);
}