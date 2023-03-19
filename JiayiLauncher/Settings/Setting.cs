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
	[JsonIgnore] public string InternalName { get; set; }
	
	public SettingAttribute(string name, string category, string description, string dependency = "")
	{
		Name = name;
		Category = category;
		Description = description;
		Dependency = dependency;
	}
}