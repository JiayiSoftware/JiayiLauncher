using System;
using System.Text.Json.Serialization;

namespace JiayiLauncher.Settings;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
public class SettingAttribute : Attribute
{
	[JsonIgnore] public string Name { get; }
	[JsonIgnore] public string Category { get; }
	[JsonIgnore] public string Description { get; }
	[JsonIgnore] public string Dependency { get; }
	[JsonIgnore] public string Tooltip { get; set; }
	[JsonIgnore] public bool Confirm { get; set; }
	[JsonIgnore] public bool CanReset { get; set; }
	
	public SettingAttribute(string name, string category, string description,
		// optionals
		string dependency = "", string tooltip = "", bool confirm = false, bool canReset = true)
	{
		Name = name;
		Category = category;
		Description = description;
		Dependency = dependency;
		Tooltip = tooltip;
		Confirm = confirm;
		CanReset = canReset;
	}
}