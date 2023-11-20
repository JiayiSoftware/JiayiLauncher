using JiayiLauncher.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Navigation;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace JiayiLauncher.Appearance;

/*public class TranslatableColor
{
    private Color Color;

    /// <summary>
    /// Initializes a new instance of the <see cref="TranslatableColor"/> class.
    /// </summary>
    /// <param name="color">The color to initialize with.</param>
    public TranslatableColor(Color color)
    {
        Color = color;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TranslatableColor"/> class with RGB values and optional alpha.
    /// </summary>
    /// <param name="r">The red component (0-255).</param>
    /// <param name="g">The green component (0-255).</param>
    /// <param name="b">The blue component (0-255).</param>
    /// <param name="a">The alpha component (0-1, default is 1).</param>
    public TranslatableColor(int r, int g, int b, float a = 1)
    {
        SetRGBA(r, g, b, a);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TranslatableColor"/> class with HSL values and optional alpha.
    /// </summary>
    /// <param name="h">The hue component (0-360).</param>
    /// <param name="s">The saturation component (0-1).</param>
    /// <param name="l">The lightness component (0-1).</param>
    /// <param name="a">The alpha component (0-1, default is 1).</param>
    public TranslatableColor(int h, float s, float l, float a = 1)
    {
        SetHSLA(h, s, l, a);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TranslatableColor"/> class from a hex color code.
    /// </summary>
    /// <param name="hex">The hex color code (#RRGGBB or #RRGGBBAA).</param>
    public TranslatableColor(string hex)
    {
        SetHex(hex);
    }

    /// <summary>
    /// Sets the internal color using RGB values and optional alpha.
    /// </summary>
    /// <param name="r">The red component (0-255).</param>
    /// <param name="g">The green component (0-255).</param>
    /// <param name="b">The blue component (0-255).</param>
    /// <param name="a">The alpha component (0-1, default is 1).</param>
    /// <returns>This <see cref="TranslatableColor"/> instance.</returns>
    public TranslatableColor SetRGBA(int r, int g, int b, float a = 1)
    {
        Color = Color.FromArgb(
            (int)(Math.Clamp(a * 255, 0, 255)),
            Math.Clamp(r, 0, 255),
            Math.Clamp(g, 0, 255),
            Math.Clamp(b, 0, 255)
        );
        return this;
    }

    /// <summary>
    /// Sets the internal color using HSL values and optional alpha.
    /// </summary>
    /// <param name="h">The hue component (0-360).</param>
    /// <param name="s">The saturation component (0-1).</param>
    /// <param name="l">The lightness component (0-1).</param>
    /// <param name="a">The alpha component (0-1, default is 1).</param>
    /// <returns>This <see cref="TranslatableColor"/> instance.</returns>
    public TranslatableColor SetHSLA(int h, float s, float l, float a = 1)
    {
        Color = FromHSLA(
            Math.Max(h, 0),
            Math.Clamp(s, 0, 1),
            Math.Clamp(l, 0, 1),
            Math.Clamp(a, 0, 1)
        );
        return this;
    }

    /// <summary>
    /// Sets the internal color using a hex color code. Returns without notification if the value is invalid.
    /// </summary>
    /// <param name="hex">The hex color code (#RRGGBB or #RRGGBBAA).</param>
    /// <returns>This <see cref="TranslatableColor"/> instance.</returns>
    public TranslatableColor SetHex(string hex)
    {
        if (!IsValidHex(hex))
            return this;

        Color = HexToColor(hex);
        return this;
    }

    /// <summary>
    /// Gets the internal color in RGBA format.
    /// </summary>
    /// <returns>The internal color as (R, G, B, A).</returns>
    public (int, int, int, float) GetRGBA()
    {
        return (Color.R, Color.G, Color.B, Color.A / 255.0f);
    }

    /// <summary>
    /// Gets the internal color in HSLA format.
    /// </summary>
    /// <returns>The internal color as (H, S, L, A).</returns>
    public (int, float, float, float) GetHSLA()
    {
        float[] hsla = ToHSLA(Color);
        return ((int)hsla[0], hsla[1], hsla[2], hsla[3]);
    }

    /// <summary>
    /// Gets the internal color in hex format.
    /// </summary>
    /// <returns>The internal color as a hex color code.</returns>
    public string GetHex()
    {
        return ColorToHex(Color);
    }

    #region Color Conversion

    /// <summary>
    /// Converts HSLA values to a <see cref="Color"/>.
    /// </summary>
    /// <param name="h">The hue component (0-360).</param>
    /// <param name="s">The saturation component (0-1).</param>
    /// <param name="l">The lightness component (0-1).</param>
    /// <param name="a">The alpha component (0-1).</param>
    /// <returns>The resulting <see cref="Color"/>.</returns>
    private static Color FromHSLA(int h, float s, float l, float a)
    {
        var hue = h % 360 / 360.0f;

        float r, g, b;

        if (s == 0)
        {
            r = g = b = l; // achromatic 
        }
        else
        {
            var q = l < 0.5 ? l * (1 + s) : l + s - l * s;

            var p = 2 * l - q;
            r = hue2rgb(p, q, hue + 1f / 3f);
            g = hue2rgb(p, q, hue);
            b = hue2rgb(p, q, hue - 1f / 3f);
        }

        return Color.FromArgb((int)Math.Round(r * 255), (int)Math.Round(g * 255), (int)Math.Round(b * 255));
    }
    private static float hue2rgb(float p, float q, float t)
    {
        if (t < 0) t += 1;
        if (t > 1) t -= 1;
        if (t < 1d / 6) return p + (q - p) * 6f * t;
        if (t < 1d / 2) return q;
        if (t < 2d / 3) return p + (q - p) * (2f / 3f - t) * 6f;
        return p;
    }

    /// <summary>
    /// Converts a <see cref="Color"/> to HSLA values.
    /// </summary>
    /// <param name="color">The input <see cref="Color"/>.</param>
    /// <returns>The resulting HSLA values.</returns>
    private static float[] ToHSLA(Color color)
    {
        float r = color.R / 255.0f;
        float g = color.G / 255.0f;
        float b = color.B / 255.0f;

        float max = Math.Max(r, Math.Max(g, b));
        float min = Math.Min(r, Math.Min(g, b));

        float h, s, l, a;

        l = (max + min) / 2;

        if (max == min)
        {
            h = s = 0; // achromatic
        }
        else
        {
            float d = max - min;
            s = l > 0.5 ? d / (2 - max - min) : d / (max + min);

            if (max == r)
                h = (g - b) / d + (g < b ? 6 : 0);
            else if (max == g)
                h = (b - r) / d + 2;
            else
                h = (r - g) / d + 4;

            h /= 6;
        }

        a = color.A / 255.0f;

        return new float[] { h * 360, s, l, a };
    }

    /// <summary>
    /// Converts a <see cref="Color"/> to a hex color code.
    /// </summary>
    /// <param name="color">The input <see cref="Color"/>.</param>
    /// <returns>The resulting hex color code.</returns>
    private static string ColorToHex(Color color)
    {
        return $"#{color.R:X2}{color.G:X2}{color.B:X2}{color.A:X2}";
    }

    /// <summary>
    /// Converts a hex color code to a <see cref="Color"/>.
    /// </summary>
    /// <param name="hex">The input hex color code.</param>
    /// <returns>The resulting <see cref="Color"/>.</returns>
    private static Color HexToColor(string hex)
    {
        return Color.FromArgb(
            Convert.ToInt32(hex.Substring(1, 2), 16),
            Convert.ToInt32(hex.Substring(3, 2), 16),
            Convert.ToInt32(hex.Substring(5, 2), 16),
            hex.Length == 9 ? Convert.ToInt32(hex.Substring(7, 2), 16) : 255
        );
    }

    /// <summary>
    /// Validates a hex color code.
    /// </summary>
    /// <param name="hex">The hex color code to validate.</param>
    /// <returns><c>true</c> if the hex code is valid; otherwise, <c>false</c>.</returns>
    private static bool IsValidHex(string hex)
    {
        return Regex.Match(hex, "^#([a-fA-F0-9]{8}|[a-fA-F0-9]{6}|[a-fA-F0-9]{4}|[a-fA-F0-9]{3})$").Success;
    }

    #endregion
}*/

public class TranslatableColor
{
    public Color Color;

    public TranslatableColor(Color color)
    {
        Color = color;
    }

    public TranslatableColor(int r, int g, int b)
    {
        Color = Color.FromArgb(255, Math.Clamp(r, 0, 255), Math.Clamp(g, 0, 255), Math.Clamp(b, 0, 255));
    }

    public TranslatableColor(int h, float s, float l)
    {
        Color = ColorConverters.FromHSL(Math.Max(h, 0), Math.Clamp(s, 0, 1), Math.Clamp(l, 0, 1));
    }

    public TranslatableColor(string hex)
    {
        SetHex(hex);
    }

    public int Red
    {
        get => Color.R;
        set => Color = Color.FromArgb((int)(AlphaRGBA * 255), value, Green, Blue);
    }

    public int Green
    {
        get => Color.G;
        set => Color = Color.FromArgb((int)(AlphaRGBA * 255), Red, value, Blue);
    }

    public int Blue
    {
        get => Color.B;
        set => Color = Color.FromArgb((int)(AlphaRGBA * 255), Red, Green, value);
    }

    public float Hue
    {
        get => ColorConverters.ToHsl(Color)[0];
        set => SetHSLA((int)value, Saturation, Lightness, AlphaHSLA);
    }

    public float Saturation
    {
        get => ColorConverters.ToHsl(Color)[1];
        set => SetHSLA((int)Hue, value, Lightness, AlphaHSLA);
    }

    public float Lightness
    {
        get => ColorConverters.ToHsl(Color)[2];
        set => SetHSLA((int)Hue, Saturation, value, AlphaHSLA);
    }

    public float AlphaRGBA
    {
        get => Color.A / 255.0f;
        set => Color = Color.FromArgb((int)(value * 255), Red, Green, Blue);
    }

    public float AlphaHSLA
    {
        get => Color.A / 255.0f;
        set => SetHSLA((int)Hue, Saturation, Lightness, value);
    }

    public string Hex
    {
        get => $"#{Red:X2}{Green:X2}{Blue:X2}{(int)(AlphaRGBA * 255):X2}";
        set => SetHex(value);
    }

    public (int, int, int) GetRGB()
    {
        return (Red, Green, Blue);
    }

    public (int, float, float) GetHSL()
    {
        return ((int)Hue, Saturation, Lightness);
    }

    public void SetRGBA(int r, int g, int b, float a = 1.0f)
    {
        Color = Color.FromArgb((int)(a * 255), Math.Clamp(r, 0, 255), Math.Clamp(g, 0, 255), Math.Clamp(b, 0, 255));
    }

    public void SetHSLA(int h, float s, float l, float a = 1.0f)
    {
        Color = FromHSLA(Math.Max(h, 0), Math.Clamp(s, 0, 1), Math.Clamp(l, 0, 1), a);
    }

    private void SetHex(string hex)
    {
        if (IsValidHex(hex))
        {
            Color = HexToColor(hex);
        }
    }

    private static Color HexToColor(string hex)
    {
        return Color.FromArgb(
            hex.Length == 9 ? Convert.ToInt32(hex.Substring(7, 2), 16) : 255,
            Convert.ToInt32(hex.Substring(1, 2), 16),
            Convert.ToInt32(hex.Substring(3, 2), 16),
            Convert.ToInt32(hex.Substring(5, 2), 16)
        );
    }

    private static Color FromHSLA(int h, float s, float l, float a)
    {
        Color colorWithoutAlpha = ColorConverters.FromHSL(h, s, l);
        return Color.FromArgb((int)(a * 255), colorWithoutAlpha.R, colorWithoutAlpha.G, colorWithoutAlpha.B);
    }

    private static bool IsValidHex(string hex)
    {
        return Regex.Match(hex, "^#([a-fA-F0-9]{8}|[a-fA-F0-9]{6}|[a-fA-F0-9]{4}|[a-fA-F0-9]{3})$").Success;
    }
}
