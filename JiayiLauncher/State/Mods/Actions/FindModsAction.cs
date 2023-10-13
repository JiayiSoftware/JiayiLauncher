namespace JiayiLauncher.State.Mods.Actions;

public class FindModsAction
{
	public string SearchQuery { get; }
	public bool ShowLocalMods { get; }
	public bool ShowWebMods { get; }
	
	public FindModsAction(string searchQuery, bool showLocalMods, bool showWebMods)
	{
		SearchQuery = searchQuery;
		ShowLocalMods = showLocalMods;
		ShowWebMods = showWebMods;
	}
}