using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

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
        return idx >= 0 ? Properties[idx] : null;
    }
    public string? GetPropertyValue(string prop)
    {
        return GetProperty(prop)?.Value;
    }

    public string ToStringNoSelector()
    {
        return string.Join("\n", Properties.Select(prop => "\t" + prop));
    }

    public override string ToString()
    {
        return $"{Selector} {{\n{ToStringNoSelector()}\n}}";
    }
}

public class CssBuilder
{
    private readonly List<CssSelector> _selectors;

    public CssBuilder(List<CssSelector>? selectors = null)
    {
        _selectors = selectors ?? new List<CssSelector>();
    }

    public CssSelector? GetSelector(string selector)
    {
        var idx = _selectors.FindIndex(x => x.Selector == selector);
        return idx >= 0 ? _selectors[idx] : null;
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

        var result = new List<CssSelector>();

        const string pattern = @"(?<selector>[^{]+)\s*{\s*(?<properties>[^}]+)\s*}";
        var regex = new Regex(pattern, RegexOptions.IgnorePatternWhitespace);

        var matches = regex.Matches(cssString);

        foreach (Match match in matches)
        {
            var selector = match.Groups["selector"].Value.Trim();
            var propertyString = match.Groups["properties"].Value;

            var properties = ParseProperties(propertyString);

            result.Add(new CssSelector(selector, properties));
        }

        return new CssBuilder(result);
    }

    private static List<CssProperty> ParseProperties(string propertyString)
    {
        var properties = new List<CssProperty>();

        const string propertyPattern = @"(?<property>[^:]+)\s*:\s*(?<value>[^;]+);+";
        var propertyRegex = new Regex(propertyPattern, RegexOptions.IgnorePatternWhitespace);

        var propertyMatches = propertyRegex.Matches(propertyString);

        foreach (Match propertyMatch in propertyMatches)
        {
            var property = propertyMatch.Groups["property"].Value.Trim();
            var value = propertyMatch.Groups["value"].Value.Trim();

            properties.Add(new CssProperty(property, value));
        }

        return properties;
    }

    private static string RemoveCssComments(string cssString)
    {
        // Use regular expression to remove CSS comments
        const string commentPattern = @"/\*.*?\*/";
        return Regex.Replace(cssString, commentPattern, string.Empty, RegexOptions.Singleline);
    }
    public override string ToString()
    {
        return string.Join("\n\n", _selectors.Select(selector => selector.ToString()));
    }
}