using JiayiLauncher.Settings;
using JiayiLauncher.Shared;
using Microsoft.AspNetCore.Components;
using System;
using System.IO;
using System.Reflection;

namespace JiayiLauncher.Appearance;


public class ThemeState
{
    public static ThemeState Instance { get; set; }

    public CssBuilder ThemeCSS { get; set; }
    public event Action? OnChange;

    public static readonly string WWWRootPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "wwwroot");

    public static string ThemePath => Path.Combine(WWWRootPath, $"themes", JiayiSettings.Instance?.Theme ?? ".local/default", "theme.css");

    // If no CssBuilder, it sets it to an empty CssBuilder, which then ends up defaulting all the values in JiayiSettings.cs
    public ThemeState(CssBuilder? themeCSS = null) { ThemeCSS = themeCSS ?? new CssBuilder(); }


    public void ApplyTheme(CssBuilder css)
    {
        ThemeCSS = css;
        Refresh();
    }

    public void UpdateTheme(string property, string value)
    {
        ThemeCSS.UpdateProperty(":root", new CssProperty(property, value));
        Refresh();
    }

    public void Refresh()
    {
        OnChange?.Invoke();
    }
}
