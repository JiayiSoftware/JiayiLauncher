using System;
using System.Drawing;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Color = System.Drawing.Color;

namespace JiayiLauncher.Utils;

public class ColorJsonConverter : JsonConverter<Color>
{
	// because json is very odd
	public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var color = reader.GetString();
		if (color is null)
		{
			return Color.Empty;
		}

		var bytes = new byte[3];
		for (var i = 0; i < 3; i++)
		{
			bytes[i] = byte.Parse(color.Substring(i * 2 + 1, 2), NumberStyles.HexNumber);
		}
        
		return Color.FromArgb(bytes[0], bytes[1], bytes[2]);
	}
	
	public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
	{
		writer.WriteStringValue($"#{value.R:X2}{value.G:X2}{value.B:X2}");
	}
}