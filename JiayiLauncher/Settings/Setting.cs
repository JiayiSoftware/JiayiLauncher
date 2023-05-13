using System;
using System.Text.Json.Serialization;

namespace JiayiLauncher.Settings;

// you know default constructors would REALLY help here, too bad it's in C# 12
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
public class SettingAttribute : Attribute
{
	[JsonIgnore] public string Name { get; }
	[JsonIgnore] public string Category { get; }
	[JsonIgnore] public string Description { get; }
	[JsonIgnore] public string Dependency { get; }
	[JsonIgnore] public string Tooltip { get; set; }
	[JsonIgnore] public bool Confirm { get; set; }
	
	public SettingAttribute(string name, string category, string description,
		// optionals
		string dependency = "", string tooltip = "", bool confirm = false)
	{
		Name = name;
		Category = category;
		Description = description;
		Dependency = dependency;
		Tooltip = tooltip;
		Confirm = confirm;
	}
}