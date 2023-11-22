using JiayiLauncher.Shared;
using Microsoft.AspNetCore.Components;
using System;
using System.IO;
using System.Reflection;

namespace JiayiLauncher.Appearance;

public interface IThemeState
{
    CssBuilder ThemeCSS { get; set; }
    event Action? OnChange;

    static readonly string WWWRootPath;
    static readonly string ThemePath;

    void ApplyTheme(CssBuilder css);
    void UpdateTheme(string property, string value);
}

public class ThemeState : IThemeState
{
    public CssBuilder ThemeCSS { get; set; }
    public event Action? OnChange;

    public ThemeState() { ThemeCSS = CssBuilder.FromFile(ThemePath, ":root"); }

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
