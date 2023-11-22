using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace JiayiLauncher.Appearance;

public class CSSProperty {
	public string Prop;
	public string Value;

	public CSSProperty(string property, string value) { 
		Prop = property;
		Value = value;
	}

	public override string ToString()
	{
		return $"{Prop}: {Value};";
	}
}

public class CssBuilder
{
    private readonly List<CSSProperty> _properties = new();
    private readonly string _selector;

    public CssBuilder(string selector)
    {
        _selector = selector;

    }

    public static CssBuilder FromFile(string path, string selector)
    {
	    var styles = File.ReadAllText(path);
	    
	    return FromText(styles, selector);
    }

    public static CssBuilder FromText(string css, string selector)
    {
        if (!css.Contains(selector))
            throw new ArgumentException($"Selector '{selector}' was not found in this file.");

        // i hate windows newlines
        var lines = css.Split('\n');

        if (lines.Any(x => x.Contains('\r')))
            lines = css.Split("\r\n");

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
            builder.AddProperty(new CSSProperty(property, value));
        }

        return builder;
    }

    public CssBuilder AddProperty(CSSProperty prop)
    {
        if (GetPropertyValue(prop) != "") return SetProperty(prop);

        _properties.Add(prop);
        return this;
    }
    public CssBuilder SetProperty(CSSProperty prop)
    {
        var idx = _properties.FindIndex(x => x.Prop == prop.Prop);
        if (idx >= 0)
        {
            _properties[idx] = prop;
        } else
        {
            AddProperty(prop);
        }
        return this;
    }
    public CssBuilder AddProperty(string property, string value)
    {
        var prop = new CSSProperty(property, value);
        return AddProperty(prop);
    }
    public CssBuilder SetProperty(string property, string value)
    {
        var prop = new CSSProperty(property, value);
        return SetProperty(prop);
    }

    public CSSProperty? GetProperty(string property)
    {
        var idx = _properties.FindIndex(x => x.Prop == property);
        if (idx >= 0)
        {
            return _properties[idx];
        }
        return null;
    }
    public string GetPropertyValue(CSSProperty property)
    {
	    foreach (var prop in _properties.Where(prop => prop.Prop == property.Prop))
	    {
			return prop.Value;
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