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

    // If no CssBuilder, it sets it to an empty CssBuilder, which then ends up defaulting all the values in JiayiSettings.cs
    public ThemeState(CssBuilder? themeCSS = null) { ThemeCSS = themeCSS ?? new CssBuilder(":root"); }

    public static readonly string WWWRootPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "wwwroot");

    public static readonly string ThemePath = Path.Combine(WWWRootPath, "css", "theme.css");

    public void ApplyTheme(CssBuilder css)
    {
        ThemeCSS = css;
        OnChange?.Invoke();
    }

    public void UpdateTheme(string property, string value)
    {
        ThemeCSS.SetProperty(property, value);
        OnChange?.Invoke();
    }
}
