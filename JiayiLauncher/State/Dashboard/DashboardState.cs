using Fluxor;
using JiayiLauncher.Features.Profiles;
using JiayiLauncher.Features.Stats;

namespace JiayiLauncher.State.Dashboard;

[FeatureState]
public class DashboardState
{
	public JiayiStats? Stats { get; }
	public ProfileCollection? Profiles { get; }
	public string? CurrentVersion { get; }

	public DashboardState() { }

	public DashboardState(JiayiStats stats, ProfileCollection? profiles, string currentVersion)
	{
		Stats = stats;
		Profiles = profiles;
		CurrentVersion = currentVersion;
	}
}