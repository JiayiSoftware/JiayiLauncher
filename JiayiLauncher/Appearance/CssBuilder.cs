using JiayiLauncher.Utils;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Controls.Primitives;
using System.Xml.Linq;

namespace JiayiLauncher.Appearance;



public class CssProperty
{
    public string Property { get; set; }
    public string? Value { get; set; }

    public CssProperty(string property, string? value)
    {
        Property = property;
        Value = value;
    }

    public override string ToString()
    {
        return $"{Property}: {Value};";
    }
}

public class CssSelector
{
    public string Selector { get; set; }
    public List<CssProperty> Properties { get; set; }

    public CssSelector(string selector, List<CssProperty> properties)
    {
        Selector = selector;
        Properties = properties;
    }

    public CssSelector UpdateProperty(CssProperty prop)
    {
        var idx = Properties.FindIndex(x => x.Property == prop.Property);
        if (idx >= 0)
        {
            Properties[idx] = prop;
        }
        else
        {
            Properties.Add(prop);
        }
        return this;
    }

    public CssProperty? GetProperty(string prop)
    {
        var idx = Properties.FindIndex(x => x.Property == prop);
        if (idx >= 0)
        {
            return Properties[idx];
        }
        else
        {
            return null;
        }
    }
    public string? GetPropertyValue(string prop)
    {
        return GetProperty(prop)?.Value;
    }

    public string ToStringNoSelector()
    {
        return string.Join("\n", Properties.Select(prop => "\t" + prop.ToString()));
    }

    public override string ToString()
    {
        return $"{Selector} {{\n{ToStringNoSelector()}\n}}";
    }
}

public class CssBuilder
{
    private List<CssSelector> _selectors;

    public CssBuilder(List<CssSelector>? selectors = null)
    {
        _selectors = selectors ?? new List<CssSelector>();
    }

    public CssSelector? GetSelector(string selector)
    {
        var idx = _selectors.FindIndex(x => x.Selector == selector);
        if (idx >= 0)
        {
            return _selectors[idx];
        }
        else
        {
            return null;
        }
    }

    public CssProperty? GetProperty(string selector, string prop)
    {
        return GetSelector(selector)?.GetProperty(prop);
    }

    public CssBuilder UpdateProperty(string selector, CssProperty prop)
    {
        GetSelector(selector)?.UpdateProperty(prop);

        return this;
    }

    public static CssBuilder FromFile(string path)
    {
        var css = File.ReadAllText(path);

        return Parse(css);
    }

    public static CssBuilder Parse(string cssString)
    {
        cssString = RemoveCssComments(cssString);

        List<CssSelector> result = new List<CssSelector>();

        string pattern = @"(?<selector>[^{]+)\s*{\s*(?<properties>[^}]+)\s*}";
        Regex regex = new Regex(pattern, RegexOptions.IgnorePatternWhitespace);

        MatchCollection matches = regex.Matches(cssString);

        foreach (Match match in matches)
        {
            string selector = match.Groups["selector"].Value.Trim();
            string propertyString = match.Groups["properties"].Value;

            List<CssProperty> properties = ParseProperties(propertyString);

            result.Add(new CssSelector(selector, properties));
        }

        return new CssBuilder(result);
    }

    private static List<CssProperty> ParseProperties(string propertyString)
    {
        List<CssProperty> properties = new List<CssProperty>();

        string propertyPattern = @"(?<property>[^:]+)\s*:\s*(?<value>[^;]+);+";
        Regex propertyRegex = new Regex(propertyPattern, RegexOptions.IgnorePatternWhitespace);

        MatchCollection propertyMatches = propertyRegex.Matches(propertyString);

        foreach (Match propertyMatch in propertyMatches)
        {
            string property = propertyMatch.Groups["property"].Value.Trim();
            string value = propertyMatch.Groups["value"].Value.Trim();

            properties.Add(new CssProperty(property, value));
        }

        return properties;
    }

    private static string RemoveCssComments(string cssString)
    {
        // Use regular expression to remove CSS comments
        string commentPattern = @"/\*.*?\*/";
        return Regex.Replace(cssString, commentPattern, string.Empty, RegexOptions.Singleline);
    }
    public override string ToString()
    {
        return string.Join("\n\n", _selectors.Select(selector => selector.ToString()));
    }
}