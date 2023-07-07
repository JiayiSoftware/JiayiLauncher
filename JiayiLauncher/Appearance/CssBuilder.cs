using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace JiayiLauncher.Appearance;

public class CssBuilder
{
    private readonly List<string> _properties = new();
    private readonly string _selector;

    public CssBuilder(string selector)
    {
        _selector = selector;
    }

    public static CssBuilder FromFile(string path, string selector)
    {
	    var styles = File.ReadAllText(path);
	    
	    if (!styles.Contains(selector)) 
		    throw new ArgumentException($"Selector '{selector}' was not found in this file.");
	    
	    // i hate windows newlines
	    var lines = styles.Split('\n');
	    
	    if (lines.Any(x => x.Contains('\r')))
		    lines = styles.Split("\r\n");
	    
	    var start = Array.IndexOf(lines, selector + " {");
	    var end = Array.IndexOf(lines, "}", start);
	    
	    var builder = new CssBuilder(selector);
	    for (var i = start + 1; i < end; i++)
	    {
		    var line = lines[i];
		    if (string.IsNullOrWhiteSpace(line)) continue;
		    var property = line.Split(':')[0].Trim();
		    
		    //var value = line.Split(':')[1].Trim().TrimEnd(';');
		    // in case the value here contains a url we need to join the rest of the line
		    var value = string.Join(':', line.Split(':').Skip(1)).Trim().TrimEnd(';');
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
		    return string.Join(':', prop.Split(':').Skip(1)).Trim().TrimEnd(';');
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