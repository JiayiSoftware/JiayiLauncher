using JiayiLauncher.Settings;
using System;
using System.IO;
using System.Reflection;

namespace JiayiLauncher.Appearance;

public class ThemeState
{
    public static ThemeState Instance { get; set; }

    public CssBuilder ThemeStyles { get; private set; }
    public event Action? OnChange;

    public static readonly string RootPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "wwwroot");

    public static string ThemePath => Path.Combine(RootPath, "themes", JiayiSettings.Instance?.Theme ?? ".local/default", "theme.css");

    // If no CssBuilder, it sets it to an empty CssBuilder, which then ends up defaulting all the values in JiayiSettings.cs
    public ThemeState(CssBuilder? themeStyles = null) { ThemeStyles = themeStyles ?? new CssBuilder(); }

    public void ApplyTheme(CssBuilder css)
    {
        ThemeStyles = css;
        Refresh();
    }

    public void UpdateTheme(string property, string value)
    {
        ThemeStyles.UpdateProperty(":root", new CssProperty(property, value));
        Refresh();
    }

    public void Refresh()
    {
        OnChange?.Invoke();
    }
}
