using System;
using System.Drawing;
using System.IO;
using JiayiLauncher.Settings;
using JiayiLauncher.Utils;

namespace JiayiLauncher.Appearance;

public static class ThemeManager
{
	private static readonly string _themePath = Path.Combine(Environment.CurrentDirectory, "wwwroot", "css", "theme.css");
	
	public static void ApplyTheme()
	{
		if (JiayiSettings.Instance == null) return;
		
		var styles = new CssBuilder(":root")
			.AddProperty("--background-primary", GetColor(JiayiSettings.Instance.PrimaryBackgroundColor))
			.AddProperty("--background-secondary", GetColor(JiayiSettings.Instance.SecondaryBackgroundColor))
			.AddProperty("--accent", GetColor(JiayiSettings.Instance.AccentColor))
			.AddProperty("--text-primary", GetColor(JiayiSettings.Instance.TextColor))
			.AddProperty("--text-grayed", GetColor(JiayiSettings.Instance.GrayTextColor))
			.AddProperty("--shadow",
				$"{JiayiSettings.Instance.ShadowDistance[2]}px {JiayiSettings.Instance.ShadowDistance[2]}px rgba(0, 0, 0, 0.4)")
			.AddProperty("--transition-speed", $"{JiayiSettings.Instance.MovementSpeed[2]}s")
			
			// finally
			.Build();
		
		// add this comment at the top of the file
		styles = $"/* your launcher theme - edit in settings or yourself if you know the bare minimum about css */\n\n{styles}";
		
		File.WriteAllText(_themePath, styles);
		
		Log.Write(nameof(ThemeManager), "Applied theme.");
	}
	
	private static string GetColor(Color color) => $"#{color.R:X2}{color.G:X2}{color.B:X2}";
}