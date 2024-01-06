using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Runtime.InteropServices;
using JiayiLauncher.Settings;

namespace JiayiLauncher.Appearance;

public static class PaletteGenerator
{
	private enum ColorChannel
	{
		Red,
		Green,
		Blue
	}
	
	public static void CreatePalette()
	{
		var themeRoot = Path.Combine(ThemeState.RootPath, "themes", JiayiSettings.Instance.Theme);
		var imagePath = Path.Combine(themeRoot, $"background{Path.GetExtension(JiayiSettings.Instance.BackgroundImageUrl)}");
		using var bitmap = new Bitmap(imagePath);
		
		var data = bitmap.LockBits(
			new Rectangle(0, 0, bitmap.Width, bitmap.Height),
			ImageLockMode.ReadOnly,
			PixelFormat.Format32bppArgb
		);
		
		var pixels = new byte[data.Stride * data.Height];
		Marshal.Copy(data.Scan0, pixels, 0, pixels.Length);
		bitmap.UnlockBits(data);

		var colors = new List<Color>();
		for (var i = 0; i < pixels.Length; i += 4)
		{
			var color = Color.FromArgb(pixels[i + 3], pixels[i + 2], pixels[i + 1], pixels[i]);
			if (color.A == 0) continue;
			colors.Add(color);
		}

		var distinct = colors.Distinct().ToList();
		colors.Clear();
		
		// https://en.wikipedia.org/wiki/Median_cut
		const int paletteSize = 6;
		var buckets = new List<List<Color>> {distinct};

		for (var i = 0; i < buckets.Count; i++)
		{
			var bucket = buckets[i];
        				
			if (bucket.Count <= 1) continue;
        				
			var biggestR = bucket.Max(color => color.R);
			var biggestG = bucket.Max(color => color.G);
			var biggestB = bucket.Max(color => color.B);
        				
			if (biggestR == biggestG && biggestG == biggestB)
			{
				var upper = bucket.TakeLast(bucket.Count / 2).ToList();
        				
				buckets.Remove(bucket);
				buckets.Add(upper);
				i--;
				continue;
			}
        
			// I am so sorry.
			var biggestChannel = true switch
			{
				true when biggestR > biggestG && biggestR > biggestB => ColorChannel.Red,
				true when biggestG > biggestR && biggestG > biggestB => ColorChannel.Green,
				true when biggestB > biggestR && biggestB > biggestG => ColorChannel.Blue,
				_ => (ColorChannel)Random.Shared.Next(0, 3)
			};
        				
			bucket.Sort((x, y) =>
			{
				switch (biggestChannel)
				{
					case ColorChannel.Red:
						if (x.R == y.R) return 0;
						if (x.R > y.R) return 1;
						return -1;
					case ColorChannel.Green:
						if (x.G == y.G) return 0;
						if (x.G > y.G) return 1;
						return -1;
					case ColorChannel.Blue:
						if (x.B == y.B) return 0;
						if (x.B > y.B) return 1;
						return -1;
					default:
						throw new Exception("biggest channel is not biggest");
				}
			});
        				
			var lowerHalf = bucket.Take(bucket.Count / 2).ToList();
			var upperHalf = bucket.TakeLast(bucket.Count / 2).ToList();
        				
			buckets.Remove(bucket);
			buckets.Add(lowerHalf);
			buckets.Add(upperHalf);
			
			if (buckets.Count >= paletteSize) break;
		}
		
		var palette = new List<Color>();

		foreach (var bucket in buckets)
		{
			var averageR = bucket.Average(color => color.R);
			var averageG = bucket.Average(color => color.G);
			var averageB = bucket.Average(color => color.B);
			palette.Add(Color.FromArgb((int)averageR, (int)averageG, (int)averageB));
		}
		
		// average of all colors
		var averageColor = Color.FromArgb(
			(int)palette.Average(color => color.R),
			(int)palette.Average(color => color.G),
			(int)palette.Average(color => color.B)
		);
		
		var allChannelsAverage = (averageColor.R + averageColor.G + averageColor.B) / 3;
		var dark = allChannelsAverage < 200; // prefer dark
		
		// values and stuff
		var accent = palette[0];
		Color primaryBackground;
		Color secondaryBackground;
		
		// things we can determine right now
		var accentAverage = (accent.R + accent.G + accent.B) / 3;
		var textOnAccent = accentAverage < 180 ? Color.White : Color.Black;
		
		var text = dark ? Color.White : Color.Black;
		var gray = Color.FromArgb(accentAverage, accentAverage, accentAverage);
		
		if (dark)
		{
			// darken the accent color to get primary background color
			float[] accentHsl = [ accent.GetHue(), accent.GetSaturation(), accent.GetBrightness() ];
			accentHsl[2] = Math.Clamp(accentHsl[2] * 0.05f, 0.03f, 1f);
			primaryBackground = ColorFromHsl(accentHsl);
			
			// brighten the primary background color to get secondary background color
			float[] primaryHsl = [ primaryBackground.GetHue(), primaryBackground.GetSaturation(), primaryBackground.GetBrightness() ];
			primaryHsl[2] *= 1.8f;
			secondaryBackground = ColorFromHsl(primaryHsl);
		}
		else
		{
			// lighten the accent color to get primary background color
			float[] accentHsl = [ accent.GetHue(), accent.GetSaturation(), accent.GetBrightness() ];
			accentHsl[2] = Math.Clamp(accentHsl[2] * 1.8f, 0f, 0.97f);
			primaryBackground = ColorFromHsl(accentHsl);
			
			// darken the primary background color to get secondary background color
			float[] primaryHsl = [ primaryBackground.GetHue(), primaryBackground.GetSaturation(), primaryBackground.GetBrightness() ];
			primaryHsl[2] *= 0.95f;
			secondaryBackground = ColorFromHsl(primaryHsl);
		}
		
		JiayiSettings.Instance.PrimaryBackgroundColor = primaryBackground;
		JiayiSettings.Instance.SecondaryBackgroundColor = secondaryBackground;
		JiayiSettings.Instance.AccentColor = accent;
		JiayiSettings.Instance.TextColor = text;
		JiayiSettings.Instance.AccentTextColor = textOnAccent;
		JiayiSettings.Instance.GrayTextColor = gray;
		
		JiayiSettings.Instance.Save();

		GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
		GC.Collect(); // this function is very memory intensive
	}

	private static Color ColorFromHsl(float[] accentHsl)
	{
		var hue = accentHsl[0];
		var saturation = accentHsl[1];
		var lightness = accentHsl[2];
		
		var chroma = (1 - Math.Abs(2 * lightness - 1)) * saturation;
		var huePrime = hue / 60;
		var x = chroma * (1 - Math.Abs(huePrime % 2 - 1));
		var m = lightness - chroma / 2;
		
		var rgb = huePrime switch
		{
			>= 0 and < 1 => [chroma, x, 0f],
			>= 1 and < 2 => [x, chroma, 0f],
			>= 2 and < 3 => [0f, chroma, x],
			>= 3 and < 4 => [0f, x, chroma],
			>= 4 and < 5 => [x, 0f, chroma],
			>= 5 and < 6 => [chroma, 0f, x],
			_ => new[] { 0f, 0f, 0f }
		};
		
		return Color.FromArgb(
			(int)((rgb[0] + m) * 255),
			(int)((rgb[1] + m) * 255),
			(int)((rgb[2] + m) * 255)
		);
	}
}