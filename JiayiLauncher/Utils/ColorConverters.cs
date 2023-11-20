using System;
using System.Drawing;
using System.Globalization;

namespace JiayiLauncher.Utils;

public static class ColorConverters
{
	public static string ToHex(this Color color)
	{
		return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
	}

	public static float[] ToHsl(this Color color)
	{
		return new[] { color.GetHue(), color.GetSaturation(), color.GetBrightness() };
	}
	
	public static Color FromHex(string hex)
	{
		var bytes = new byte[3];
		for (var i = 0; i < 3; i++)
		{
			bytes[i] = byte.Parse(hex.Substring(i * 2 + 1, 2), NumberStyles.HexNumber);
		}
        
		return Color.FromArgb(bytes[0], bytes[1], bytes[2]);
	}

	public static Color FromHSL(float hue, float saturation, float lightness)
	{
		var c = (1 - Math.Abs(2 * lightness - 1)) * saturation;
		var x = c * (1 - Math.Abs(hue / 60 % 2 - 1));
		var m = lightness - c / 2;
		
		float r, g, b;
		
		switch (hue)
		{
			case < 60:
				r = c;
				g = x;
				b = 0;
				break;
			case < 120:
				r = x;
				g = c;
				b = 0;
				break;
			case < 180:
				r = 0;
				g = c;
				b = x;
				break;
			case < 240:
				r = 0;
				g = x;
				b = c;
				break;
			case < 300:
				r = x;
				g = 0;
				b = c;
				break;
			default:
				r = c;
				g = 0;
				b = x;
				break;
		}

		return Color.FromArgb((int)((r + m) * 255), (int)((g + m) * 255), (int)((b + m) * 255));
	}
}