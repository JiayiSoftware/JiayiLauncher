using System;
using System.Text.Json.Serialization;

namespace JiayiLauncher.Settings;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
public class SettingAttribute : Attribute
{
	[JsonIgnore] public string Name { get; set; }
	[JsonIgnore] public string Category { get; set; }
	[JsonIgnore] public string Description { get; set; }
	[JsonIgnore] public string Dependency { get; }
	[JsonIgnore] public string Tooltip { get; set; }
	[JsonIgnore] public bool Confirm { get; }
	[JsonIgnore] public bool CanReset { get; }
	
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