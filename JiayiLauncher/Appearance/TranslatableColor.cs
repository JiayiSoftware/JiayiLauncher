using System.Text.RegularExpressions;
using Color = System.Drawing.Color;

namespace JiayiLauncher.Appearance;

public partial class TranslatableColor
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
        Color = FromHSL(Math.Max(h, 0), Math.Clamp(s, 0, 1), Math.Clamp(l, 0, 1));
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
        get => Color.GetHue();
        set => SetHSLA((int)value, Saturation, Lightness, AlphaHSLA);
    }

    public float Saturation
    {
        get => Color.GetSaturation();
        set => SetHSLA((int)Hue, value, Lightness, AlphaHSLA);
    }

    public float Lightness
    {
        get => Color.GetBrightness();
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
            Color = FromHex(hex);
        }
    }

    public static Color FromHex(string hex)
    {
        return Color.FromArgb(
            hex.Length == 9 ? Convert.ToInt32(hex.Substring(7, 2), 16) : 255,
            Convert.ToInt32(hex.Substring(1, 2), 16),
            Convert.ToInt32(hex.Substring(3, 2), 16),
            Convert.ToInt32(hex.Substring(5, 2), 16)
        );
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
    private static Color FromHSLA(int h, float s, float l, float a)
    {
        Color colorWithoutAlpha = FromHSL(h, s, l);
        return Color.FromArgb((int)(a * 255), colorWithoutAlpha.R, colorWithoutAlpha.G, colorWithoutAlpha.B);
    }

    private static bool IsValidHex(string hex)
    {
        return HexRegex().Match(hex).Success;
    }

    [GeneratedRegex("^#([a-fA-F0-9]{8}|[a-fA-F0-9]{6}|[a-fA-F0-9]{4}|[a-fA-F0-9]{3})$")]
    private static partial Regex HexRegex();
}