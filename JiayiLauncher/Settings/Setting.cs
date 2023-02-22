using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace JiayiLauncher.Settings;

public class Setting<T>
{
	[JsonIgnore] public string Name { get; }
	[JsonIgnore] public string Category { get; }
	[JsonIgnore] public string Description { get; }
	
	public T Value { get; set; }
	
	public Setting(string name, string category, string description, T value)
	{
		Name = name;
		Category = category;
		Description = description;
		Value = value;
	}
	
	public static bool operator ==(Setting<T> setting, T value)
	{
		return setting.Value!.Equals(value);
	}
	
	public static bool operator !=(Setting<T> setting, T value)
	{
		return !setting.Value!.Equals(value);
	}
	
	protected bool Equals(Setting<T> other)
	{
		return EqualityComparer<T>.Default.Equals(Value, other.Value);
	}

	public override bool Equals(object? obj)
	{
		if (ReferenceEquals(null, obj)) return false;
		if (ReferenceEquals(this, obj)) return true;
		return obj.GetType() == GetType() && Equals((Setting<T>)obj);
	}

	public override int GetHashCode()
	{
		return EqualityComparer<T>.Default.GetHashCode(Value);
	}
	
	// few extra notes:
	// - settings displayed as ranges (e.g. injection delay) are stored as arrays, like { min, max, default }
	// - colors are stored as BYTE arrays specifically, and supports alpha values
	//   - so { r, g, b } and { r, g, b, a } are both valid
}