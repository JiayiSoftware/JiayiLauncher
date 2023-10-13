using Fluxor;
using JiayiLauncher.Features.Game;
using JiayiLauncher.Features.Profiles;
using JiayiLauncher.Features.Stats;
using JiayiLauncher.State.Dashboard.Actions;

namespace JiayiLauncher.State.Dashboard;

public static class DashboardReducers
{
	[ReducerMethod(typeof(LoadDashboardAction))]
	public static DashboardState ReduceLoadDashboardAction(DashboardState state)
	{
		var stats = JiayiStats.Instance!;
		var profiles = ProfileCollection.Current; // can be null
		var currentVersion = PackageData.GetVersion().GetAwaiter().GetResult();

		return new DashboardState(stats, profiles, currentVersion);
	}
}