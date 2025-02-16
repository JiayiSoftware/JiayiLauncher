namespace JiayiLauncher.Appearance;

using System;
using System.Drawing;
using System.Text.RegularExpressions;

public interface IColorModel
{
    void UpdateFromRGB(byte r, byte g, byte b);
    (byte r, byte g, byte b) ToRGB();

}

public class RGBColor : IColorModel
{
    // Byte [0,255]
    public byte R { get; set; }
    public byte G { get; set; }
    public byte B { get; set; }

    public RGBColor(byte r, byte g, byte b) => UpdateFromRGB(r, g, b);

    public void UpdateFromRGB(byte r, byte g, byte b)
    {
        (R, G, B) = (r, g, b);
    }
    public (byte r, byte g, byte b) ToRGB() => (R, G, B);
}

// https://en.wikipedia.org/wiki/HSL_and_HSV
public class HSLColor : IColorModel
{
    private double _hue, _saturation, _lightness;
    public double Hue
    {
        get => _hue;
        set
        {
            _hue = ClampHue(value);
        }
    }
    public double Saturation
    {
        get => _saturation;
        set
        {
            _saturation = Math.Clamp(value, 0, 1);
        }
    }
    public double Lightness
    {
        get => _lightness;
        set
        {
            _lightness = Math.Clamp(value, 0, 1);
        }
    }

    public HSLColor(byte r, byte g, byte b) => UpdateFromRGB(r, g, b);

    public void UpdateFromRGB(byte r, byte g, byte b)
    {
        (Hue, Saturation, Lightness) = ConvertToHSL(r, g, b);
    }
    public void Set(double h, double s, double l)
    {
        (Hue, Saturation, Lightness) = (h, s, l);
    }

    public (byte r, byte g, byte b) ToRGB() => ConvertFromHSL(Hue, Saturation, Lightness);


    // Converters
    private static (double hue, double saturation, double lightness) ConvertToHSL(byte r, byte g, byte b)
    {
        double R = r / 255.0,
            G = g / 255.0,
            B = b / 255.0;

        double min = Math.Min(Math.Min(R, G), B);
        double max = Math.Max(Math.Max(R, G), B);
        double delta = max - min;

        double h = 0,
            s = 0,
            l = (max + min) / 2;

        if (delta != 0)
        {
            // Saturation
            s = l > 0.5 ? delta / (2.0 - max - min) : delta / (max + min);

            // Hue
            if (max == R) h = ((G - B) / delta) % 6;
            else if (max == G) h = (B - R) / delta + 2;
            else h = (R - G) / delta + 4;

            h = ClampHue(Math.Round(h * 60));
        }

        return (h, s, l);
    }

    private static (byte r, byte g, byte b) ConvertFromHSL(double hue, double saturation, double lightness)
    {
        double c = (1 - Math.Abs(2 * lightness - 1)) * saturation;
        double x = c * (1 - Math.Abs((hue / 60) % 2 - 1));
        double m = lightness - c / 2;

        double r, g, b;

        switch (hue)
        {
            case < 60:
                (r, g, b) = (c, x, 0);
                break;
            case < 120:
                (r, g, b) = (x, c, 0);
                break;
            case < 180:
                (r, g, b) = (0, c, x);
                break;
            case < 240:
                (r, g, b) = (0, x, c);
                break;
            case < 300:
                (r, g, b) = (x, 0, c);
                break;
            default:
                (r, g, b) = (c, 0, x);
                break;
        }

        return (
            (byte)Math.Clamp((r + m) * 255, 0, 255),
            (byte)Math.Clamp((g + m) * 255, 0, 255),
            (byte)Math.Clamp((b + m) * 255, 0, 255)
        );
    }

    public static double ClampHue(double hue) => (hue % 360 + 360) % 360;
}

// https://en.wikipedia.org/wiki/HSL_and_HSV
public class HSVColor : IColorModel
{
    private double _hue, _saturation, _value;
    public double Hue
    {
        get => _hue;
        set
        {
            _hue = HSLColor.ClampHue(value);
        }
    }
    public double Saturation
    {
        get => _saturation;
        set
        {
            _saturation = Math.Clamp(value, 0, 1);
        }
    }
    public double Value
    {
        get => _value;
        set
        {
            _value = Math.Clamp(value, 0, 1);
        }
    }

    public HSVColor(byte r, byte g, byte b) => UpdateFromRGB(r, g, b);

    public void UpdateFromRGB(byte r, byte g, byte b)
    {
        (Hue, Saturation, Value) = ConvertToHSV(r, g, b);
    }
    public void Set(double h, double s, double v)
    {
        (Hue, Saturation, Value) = (h, s, v);
    }

    public (byte r, byte g, byte b) ToRGB() => ConvertFromHSV(Hue, Saturation, Value);


    // Converters
    private static (double hue, double saturation, double value) ConvertToHSV(byte r, byte g, byte b)
    {
        double R = r / 255.0, G = g / 255.0, B = b / 255.0;
        double min = Math.Min(Math.Min(R, G), B);
        double max = Math.Max(Math.Max(R, G), B);
        double delta = max - min;

        double h = 0, s = 0, v = max;

        if (delta > 0)
        {
            // Saturation
            s = delta / max;

            // Hue Calculation
            if (max == R)
                h = ((G - B) / delta) % 6;
            else if (max == G)
                h = (B - R) / delta + 2;
            else
                h = (R - G) / delta + 4;

            h = HSLColor.ClampHue(Math.Round(h*60));
        }

        return (h, s, v);
    }

    private static (byte r, byte g, byte b) ConvertFromHSV(double hue, double saturation, double value)
    {
        double c = value * saturation; // Chroma
        double x = c * (1 - Math.Abs((hue / 60) % 2 - 1));
        double m = value - c;

        double r, g, b;

        switch (hue)
        {
            case < 60:
                (r, g, b) = (c, x, 0);
                break;
            case < 120:
                (r, g, b) = (x, c, 0);
                break;
            case < 180:
                (r, g, b) = (0, c, x);
                break;
            case < 240:
                (r, g, b) = (0, x, c);
                break;
            case < 300:
                (r, g, b) = (x, 0, c);
                break;
            default:
                (r, g, b) = (c, 0, x);
                break;
        }

        return (
         (byte)Math.Clamp((r + m) * 255, 0, 255),
         (byte)Math.Clamp((g + m) * 255, 0, 255),
         (byte)Math.Clamp((b + m) * 255, 0, 255)
     );
    }
}

public class HexColor : IColorModel
{
    private string _hex;
    public string Hex
    {
        get => _hex;
        set => _hex = value;
    }

    public HexColor(byte r, byte g, byte b) => UpdateFromRGB(r, g, b);

    public void UpdateFromRGB(byte r, byte g, byte b)
    {
        Hex = $"#{r:X2}{g:X2}{b:X2}";
    }

    public (byte r, byte g, byte b) ToRGB() => ConvertFromHex(Hex);

    public void Set(string hex)
    {
        Hex = hex;
    }

    // Converters

    private static (byte r, byte g, byte b) ConvertFromHex(string hex)
    {
        hex = hex.TrimStart('#');
        if (hex.Length == 3)
            hex = $"{hex[0]}{hex[0]}{hex[1]}{hex[1]}{hex[2]}{hex[2]}";

        if (hex.Length != 6)
            throw new ArgumentException("Invalid hex format");

        return (
            Convert.ToByte(hex.Substring(0, 2), 16),
            Convert.ToByte(hex.Substring(2, 2), 16),
            Convert.ToByte(hex.Substring(4, 2), 16)
        );
    }

    public static bool IsValidHex(string hex)
    {
        return HexRegex().Match(hex).Success;
    }

    private static Regex HexRegex() => new Regex("^#([a-fA-F0-9]{6}|[a-fA-F0-9]{3})$"); // 3 or 6
}

public class TranslatableColor
{
    private RGBColor _rgb;
    private HSLColor _hsl;
    private HSVColor _hsv;
    private HexColor _hex;

    public TranslatableColor(Color color)
    {
        (byte r, byte g, byte b) = (color.R, color.G, color.B);
        _rgb = new RGBColor(r, g, b);
        _hsl = new HSLColor(r, g, b);
        _hsv = new HSVColor(r, g, b);
        _hex = new HexColor(r, g, b);
    }
    public TranslatableColor(byte r, byte g, byte b)
    {
        _rgb = new RGBColor(r, g, b);
        _hsl = new HSLColor(r, g, b);
        _hsv = new HSVColor(r, g, b);
        _hex = new HexColor(r, g, b);
    }

    public Color Color => Color.FromArgb(255, _rgb.R, _rgb.G, _rgb.B);

    public byte R { get => _rgb.R; set => SetRGB(value, _rgb.G, _rgb.B); }
    public byte G { get => _rgb.G; set => SetRGB(_rgb.R, value, _rgb.B); }
    public byte B { get => _rgb.B; set => SetRGB(_rgb.R, _rgb.G, value); }

    public double Hue { get => _hsl.Hue; set => SetHSL(value, _hsl.Saturation, _hsl.Lightness); }
    public double Saturation { get => _hsl.Saturation; set => SetHSL(_hsl.Hue, value, _hsl.Lightness); }
    public double Lightness { get => _hsl.Lightness; set => SetHSL(_hsl.Hue, _hsl.Saturation, value); }

    public double HSVHue { get => _hsv.Hue; set => SetHSV(value, _hsv.Saturation, _hsv.Value); }
    public double HSVSaturation { get => _hsv.Saturation; set => SetHSV(_hsv.Hue, value, _hsv.Value); }
    public double HSVValue { get => _hsv.Value; set => SetHSV(_hsv.Hue, _hsv.Saturation, value); }

    public string Hex
    {
        get => _hex.Hex;
        set
        {
            if (HexColor.IsValidHex(value))
                SetHex(value);
        }
    }

    private void SetRGB(byte r, byte g, byte b)
    {
        PropogateChanges(r, g, b);
    }

    private void SetHSL(double hue, double saturation, double lightness)
    {
        _hsl.Set(hue, saturation, lightness);

        (byte r, byte g, byte b) = _hsl.ToRGB();

        PropogateChanges(r, g, b);
    }
    private void SetHSV(double hue, double saturation, double value)
    {
        _hsv.Set(hue, saturation, value);

        (byte r, byte g, byte b) = _hsv.ToRGB();

        PropogateChanges(r, g, b);
    }

    private void SetHex(string hex)
    {
        _hex.Set(hex);

        (byte r, byte g, byte b) = _hex.ToRGB();

        PropogateChanges(r, g, b);
    }

    private void PropogateChanges(byte r, byte g, byte b)
    {
        _rgb.UpdateFromRGB(r, g, b);
        _hsl.UpdateFromRGB(r, g, b);
        _hsv.UpdateFromRGB(r, g, b);
        _hex.UpdateFromRGB(r, g, b);

    }
}
