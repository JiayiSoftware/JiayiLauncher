using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace JiayiLauncher.Appearance;

public class CssBuilder
{
    private readonly List<string> _properties = new();
    private string _selector = string.Empty;

    public CssBuilder(string selector)
    {
        _selector = selector;
    }

    public static CssBuilder FromFile(string path, string selector)
    {
	    var styles = File.ReadAllText(path);
	    
	    if (!styles.Contains(selector)) 
		    throw new ArgumentException($"Selector '{selector}' was not found in this file.");
	    
	    var lines = styles.Split('\n');
	    var start = Array.IndexOf(lines, selector + " {");
	    var end = Array.IndexOf(lines, "}", start);
	    
	    var builder = new CssBuilder(selector);
	    for (var i = start + 1; i < end; i++)
	    {
		    var line = lines[i];
		    if (string.IsNullOrWhiteSpace(line)) continue;
		    var property = line.Split(':')[0].Trim();
		    var value = line.Split(':')[1].Trim().TrimEnd(';');
		    builder.AddProperty(property, value);
	    }
	    
	    return builder;
    }

    public CssBuilder AddProperty(string property, string value)
    {
        _properties.Add($"{property}: {value};");
        return this;
    }
    
    public string GetPropertyValue(string property)
    {
	    foreach (var prop in _properties.Where(prop => prop.StartsWith(property)))
	    {
		    return prop.Split(':')[1].Trim().TrimEnd(';');
	    }

	    return string.Empty;
    }

    // i like linq extension methods
    public List<string> GetAllPropertyValues() => _properties.Select(GetPropertyValue).ToList();

    public string Build()
    {
        var css = $"{_selector} {{\n";

        css = _properties.Aggregate(css, (current, property) => current + $"\t{property}\n");

        css += "}\n";
        return css;
    }

    public override string ToString() => Build();
}