using System.Collections.Generic;

namespace JiayiLauncher.Appearance;

public class CssBuilder
{
    private readonly List<string> _properties = new();
    private string _selector = string.Empty;

    public CssBuilder(string selector)
    {
        _selector = selector;
    }

    public CssBuilder AddProperty(string property, string value)
    {
        _properties.Add($"{property}: {value};");
        return this;
    }

    public string Build()
    {
        var css = $"{_selector} {{\n";

        foreach (var property in _properties)
        {
            css += $"\t{property}\n";
        }

        css += "}\n";
        return css;
    }

    public override string ToString() => Build();
}