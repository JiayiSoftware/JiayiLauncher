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
}