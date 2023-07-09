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
		
		if (!File.Exists(ThemePath)) ApplyTheme(); // create theme file if it doesn't exist
		
		var builder = CssBuilder.FromFile(ThemePath, ":root");
		var styles = builder.GetAllPropertyValues();
		
		JiayiSettings.Instance.PrimaryBackgroundColor = GetColorFromHex(styles[0]);
		JiayiSettings.Instance.SecondaryBackgroundColor = GetColorFromHex(styles[1]);
		JiayiSettings.Instance.AccentColor = GetColorFromHex(styles[2]);
		JiayiSettings.Instance.TextColor = GetColorFromHex(styles[3]);
		JiayiSettings.Instance.AccentTextColor = GetColorFromHex(styles[4]);
		JiayiSettings.Instance.GrayTextColor = GetColorFromHex(styles[5]);
			
		var shadow = styles[6].Split(' ');
		JiayiSettings.Instance.ShadowDistance[2] = int.Parse(shadow[0].Trim('p', 'x'));
			
		JiayiSettings.Instance.MovementSpeed[2] = float.Parse(styles[7].Trim('s'), CultureInfo.InvariantCulture);
		
		// if the length of styles is 8 then stop here (old theme)
		if (styles.Count == 8)
		{
			JiayiSettings.Instance.Save();
			ApplyTheme(); // writes missing properties to theme file
			return;
		}

		if (styles[8] == "none")
		{
			JiayiSettings.Instance.BackgroundImageUrl = "";
			JiayiSettings.Instance.BackgroundBlur[2] = 0;
			JiayiSettings.Instance.BackgroundBrightness[2] = 0;
			JiayiSettings.Instance.UseBackgroundImage = false;
		}
		else
		{
			JiayiSettings.Instance.UseBackgroundImage = true;
			// extract url from "url("...")"
			JiayiSettings.Instance.BackgroundImageUrl = JiayiSettings.Instance.BackgroundImageUrl != ""
				? styles[8].Replace("url(\"", "").Replace("\")", "")
				: "";
			JiayiSettings.Instance.BackgroundBlur[2] = int.Parse(styles[9].Trim('p', 'x'));
			JiayiSettings.Instance.BackgroundBrightness[2] = int.Parse(styles[10].Trim('%'));
		}

		JiayiSettings.Instance.Rounding[2] = int.Parse(styles[11].Trim('p', 'x'));
		JiayiSettings.Instance.BorderColor = GetColorFromHex(styles[12]);
		JiayiSettings.Instance.AccentBorderColor = GetColorFromHex(styles[13]);
		JiayiSettings.Instance.BorderThickness[2] = int.Parse(styles[14].Trim('p', 'x'));
		
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
			.AddProperty("--text-accent", GetHexForColor(JiayiSettings.Instance.AccentTextColor))
			.AddProperty("--text-grayed", GetHexForColor(JiayiSettings.Instance.GrayTextColor))
			.AddProperty("--shadow",
				$"{JiayiSettings.Instance.ShadowDistance[2]}px {JiayiSettings.Instance.ShadowDistance[2]}px rgba(0, 0, 0, 0.4)")
			.AddProperty("--transition-speed", $"{JiayiSettings.Instance.MovementSpeed[2]}s")
			.AddProperty("--background-image",
				JiayiSettings.Instance.UseBackgroundImage
					? $"url(\"{JiayiSettings.Instance.BackgroundImageUrl}\")"
					: "none")
			.AddProperty("--background-blur", $"{JiayiSettings.Instance.BackgroundBlur[2]}px")
			.AddProperty("--background-brightness", $"{JiayiSettings.Instance.BackgroundBrightness[2]}%")
			.AddProperty("--rounding", $"{JiayiSettings.Instance.Rounding[2]}px")
			.AddProperty("--border-primary", GetHexForColor(JiayiSettings.Instance.BorderColor))
			.AddProperty("--border-accent", GetHexForColor(JiayiSettings.Instance.AccentBorderColor))
			.AddProperty("--border-thickness", $"{JiayiSettings.Instance.BorderThickness[2]}px")
			
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