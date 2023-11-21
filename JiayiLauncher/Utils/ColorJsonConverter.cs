using JiayiLauncher.Appearance;
using System;
using System.Drawing;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

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

		return TranslatableColor.FromHex(color);
	}
	
	public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
	{
		writer.WriteStringValue($"#{value.R:X2}{value.G:X2}{value.B:X2}");
	}
}