using Fluxor;
using JiayiLauncher.Features.Mods;
using System.Collections.Generic;

namespace JiayiLauncher.State.Mods;

[FeatureState]
public class ModsState
{
	public string SearchQuery { get; }
	public List<Mod> FilteredMods { get; }
	public bool ShowLocalMods { get; }
	public bool ShowWebMods { get; }
	public ModCollection? ModCollection { get; }
	public bool ModCollectionCreated { get; }
	
	// initial state
	public ModsState()
	{
		SearchQuery = string.Empty;
		FilteredMods = new List<Mod>();
	}
	
	// set everything
	public ModsState(string searchQuery, List<Mod> filteredMods, bool showLocalMods, bool showWebMods,
		ModCollection? modCollection, bool modCollectionCreated)
	{
		SearchQuery = searchQuery;
		FilteredMods = filteredMods;
		ShowLocalMods = showLocalMods;
		ShowWebMods = showWebMods;
		ModCollection = modCollection;
		ModCollectionCreated = modCollectionCreated;
	}
}