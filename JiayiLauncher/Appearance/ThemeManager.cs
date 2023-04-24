using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using JiayiLauncher.Settings;
using JiayiLauncher.Utils;

namespace JiayiLauncher.Appearance;

public static class ThemeManager
{
	public static readonly string ThemePath = Path.Combine(Environment.CurrentDirectory, "wwwroot", "css", "theme.css");

	public static void LoadTheme()
	{
		if (JiayiSettings.Instance == null) return;
		
		var builder = CssBuilder.FromFile(ThemePath, ":root");
		var styles = builder.GetAllPropertyValues();
		
		JiayiSettings.Instance.PrimaryBackgroundColor = GetColorFromHex(styles[0]);
		JiayiSettings.Instance.SecondaryBackgroundColor = GetColorFromHex(styles[1]);
		JiayiSettings.Instance.AccentColor = GetColorFromHex(styles[2]);
		JiayiSettings.Instance.TextColor = GetColorFromHex(styles[3]);
		JiayiSettings.Instance.GrayTextColor = GetColorFromHex(styles[4]);
		
		var shadow = styles[5].Split(' ');
		JiayiSettings.Instance.ShadowDistance[2] = int.Parse(shadow[0].Trim('p', 'x'));
		
		JiayiSettings.Instance.MovementSpeed[2] = float.Parse(styles[6].Trim('s'));
		
		JiayiSettings.Instance.Save();
		ApplyTheme();
	}
	
	public static void ApplyTheme()
	{
		if (JiayiSettings.Instance == null) return;
		
		var styles = new CssBuilder(":root")
			.AddProperty("--background-primary", GetHexForColor(JiayiSettings.Instance.PrimaryBackgroundColor))
			.AddProperty("--background-secondary", GetHexForColor(JiayiSettings.Instance.SecondaryBackgroundColor))
			.AddProperty("--accent", GetHexForColor(JiayiSettings.Instance.AccentColor))
			.AddProperty("--text-primary", GetHexForColor(JiayiSettings.Instance.TextColor))
			.AddProperty("--text-grayed", GetHexForColor(JiayiSettings.Instance.GrayTextColor))
			.AddProperty("--shadow",
				$"{JiayiSettings.Instance.ShadowDistance[2]}px {JiayiSettings.Instance.ShadowDistance[2]}px rgba(0, 0, 0, 0.4)")
			.AddProperty("--transition-speed", $"{JiayiSettings.Instance.MovementSpeed[2]}s")
			
			// finally
			.Build();
		
		// add this comment at the top of the file
		styles = $"/* your launcher theme - edit in settings or yourself if you know the bare minimum about css */\n\n{styles}";
		
		File.WriteAllText(ThemePath, styles);
		
		Log.Write(nameof(ThemeManager), "Applied theme.");
	}
	
	private static string GetHexForColor(Color color) => $"#{color.R:X2}{color.G:X2}{color.B:X2}";

	private static Color GetColorFromHex(string hex)
	{
		var bytes = new byte[3];
		for (var i = 0; i < 3; i++)
		{
			bytes[i] = byte.Parse(hex.Substring(i * 2 + 1, 2), NumberStyles.HexNumber);
		}
		
		return Color.FromArgb(bytes[0], bytes[1], bytes[2]);
	}
}