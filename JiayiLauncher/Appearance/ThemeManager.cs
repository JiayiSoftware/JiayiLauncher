using System;
using System.Drawing;
using System.IO;
using JiayiLauncher.Settings;

namespace JiayiLauncher.Appearance;

public static class ThemeManager
{
	private static readonly string _themePath = Path.Combine(Environment.CurrentDirectory, "wwwroot", "css", "theme.css");
	
	public static void ApplyTheme()
	{
		if (JiayiSettings.Instance == null) return;
		
		var styles = new CssBuilder(":root")
			// non-customizable properties
			.AddProperty("font-size", "16px")
			.AddProperty("font-family", "Montserrat, sans-serif")

			// customizable properties
			.AddProperty("--background-primary", GetColor(JiayiSettings.Instance.PrimaryBackgroundColor))
			.AddProperty("--background-secondary", GetColor(JiayiSettings.Instance.SecondaryBackgroundColor))
			.AddProperty("--accent", GetColor(JiayiSettings.Instance.AccentColor))
			.AddProperty("--accent-darkened", GetColor(JiayiSettings.Instance.DarkAccentColor))
			.AddProperty("--accent-brightened", GetColor(JiayiSettings.Instance.BrightAccentColor))
			.AddProperty("--text-primary", GetColor(JiayiSettings.Instance.TextColor))
			.AddProperty("--text-grayed", GetColor(JiayiSettings.Instance.GrayTextColor))
			.AddProperty("--shadow",
				$"{JiayiSettings.Instance.ShadowDistance[3]} {JiayiSettings.Instance.ShadowDistance[3]} rgba(0, 0, 0, 0.4)")
			.AddProperty("-transition-speed", $"{JiayiSettings.Instance.MovementSpeed}s")
			
			// finally
			.Build();
		
		// add this comment at the top of the file
		styles = $"/* your launcher theme - edit in settings or yourself if you know the bare minimum about css */\n\n{styles}";
		
		File.WriteAllText(_themePath, styles);
	}
	
	private static string GetColor(Color color) => $"#{color.R:X2}{color.G:X2}{color.B:X2}";
}