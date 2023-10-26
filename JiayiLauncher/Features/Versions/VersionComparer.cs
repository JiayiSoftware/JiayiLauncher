using System.Collections.Generic;

namespace JiayiLauncher.Features.Versions;

public class VersionComparer : IComparer<string>
{
	public int Compare(string? x, string? y)
	{
		if (x == null && y == null) return 0;
		if (x == null) return 1;
		if (y == null) return -1;
		
		var leftVersion = x.Split('.');
		var rightVersion = y.Split('.');

		for (var i = 0; i < leftVersion.Length; i++)
		{
			if (int.Parse(leftVersion[i]) > int.Parse(rightVersion[i])) return -1;
			if (int.Parse(leftVersion[i]) < int.Parse(rightVersion[i])) return 1;
		}

		return 0;
	}
}