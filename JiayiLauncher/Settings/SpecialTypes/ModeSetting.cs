using System.Text.Json.Serialization;

namespace JiayiLauncher.Settings.SpecialTypes;

[Serializable]
public class ModeSetting
{
	public int Mode { get; set; }
	
	[JsonIgnore]
	public List<string> AvailableModes { get; set; }

	public ModeSetting()
	{
		Mode = 0;
		AvailableModes = new();
	}
	
	public ModeSetting(int defaultMode, List<string> availableModes)
	{
		Mode = defaultMode;
		AvailableModes = availableModes;
	}
	
	// equality
	public static bool operator ==(ModeSetting a, ModeSetting b) => a.Mode == b.Mode;
	public static bool operator !=(ModeSetting a, ModeSetting b) => !(a == b);
	
	protected bool Equals(ModeSetting other)
	{
		return Mode == other.Mode;
	}

	public override bool Equals(object? obj)
	{
		if (obj is null) return false;
		if (ReferenceEquals(this, obj)) return true;
		if (obj.GetType() != GetType()) return false;
		return Equals((ModeSetting)obj);
	}

	public override int GetHashCode()
	{
		return Mode;
	}
}