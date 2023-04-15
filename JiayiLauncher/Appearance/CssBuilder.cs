using System.Collections.Generic;

namespace JiayiLauncher.Appearance;

public class CssBuilder
{
	private readonly List<string> _properties = new();
	private string _css = string.Empty;
	
	public CssBuilder(string selector)
	{
		_css += $"{selector} {{\n";
	}
	
	public CssBuilder AddProperty(string property, string value)
	{
		_properties.Add($"{property}: {value};");
		return this;
	}
	
	public string Build()
	{
		foreach (var property in _properties)
		{
			_css += $"\t{property}\n";
		}
		
		_css += "}\n";
		return _css;
	}

	public override string ToString() => Build();
}