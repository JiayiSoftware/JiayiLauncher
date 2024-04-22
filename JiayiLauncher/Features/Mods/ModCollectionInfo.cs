namespace JiayiLauncher.Features.Mods;

public struct ModCollectionInfo
{
	public int LocallyStoredMods { get; init; }
	public int InternetMods { get; init; }
	public int TotalMods { get; init; }
}