using System;

namespace JiayiLauncher.Settings.Special;

public struct SliderSetting
{
	public Range Range { get; }
	public int Value { get; }
	
	public SliderSetting(int min, int max, int value)
	{
		Range = new Range(min, max);
		Value = value;
	}
}