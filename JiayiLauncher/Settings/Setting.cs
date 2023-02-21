namespace JiayiLauncher.Settings;

public class Setting<T>
{
	public string Name { get; set; }
	public string Category { get; set; }
	public string Description { get; set; }
	public T Value { get; set; }
	
	public Setting(string name, string category, string description, T value)
	{
		Name = name;
		Category = category;
		Description = description;
		Value = value;
	}
}