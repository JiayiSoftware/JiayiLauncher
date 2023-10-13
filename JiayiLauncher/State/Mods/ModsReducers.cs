using System;
using System.Collections.Generic;
using System.Linq;
using Fluxor;
using JiayiLauncher.Features.Mods;
using JiayiLauncher.Settings;
using JiayiLauncher.State.Mods.Actions;

namespace JiayiLauncher.State.Mods;

public static class ModsReducers
{
	[ReducerMethod(typeof(LoadModsAction))]
	public static ModsState ReduceLoadModsAction(ModsState state)
	{
		var collectionCreated = JiayiSettings.Instance!.ModCollectionPath != string.Empty;
		var collection = collectionCreated ? ModCollection.Current : null;
		var filteredMods = collectionCreated ? collection!.Mods : new List<Mod>();

		return new ModsState(string.Empty, filteredMods, true, true, collection, collectionCreated);
	}

	[ReducerMethod]
	public static ModsState ReduceCreateModsAction(ModsState state, CreateModCollectionAction action)
	{
		ModCollection.Load(action.ModCollectionPath);
		var collection = ModCollection.Current;
		
		// reuse the previous state
		return new ModsState(state.SearchQuery, state.FilteredMods, state.ShowLocalMods, state.ShowWebMods,
			collection, true);
	}

	[ReducerMethod]
	public static ModsState ReduceFindModsAction(ModsState state, FindModsAction action)
	{
		var filteredMods = (
			from mod in state.ModCollection?.Mods 
			where action is not { ShowLocalMods: true, ShowWebMods: false } || !mod.FromInternet 
			where action is not { ShowLocalMods: false, ShowWebMods: true } || mod.FromInternet 
			where mod.Name.Contains(action.SearchQuery, StringComparison.OrdinalIgnoreCase) 
			select mod
		).ToList();

		return new ModsState(action.SearchQuery, filteredMods, action.ShowLocalMods, action.ShowWebMods,
			state.ModCollection, state.ModCollectionCreated);
	}
}