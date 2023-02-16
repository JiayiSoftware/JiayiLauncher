using System;
using System.Collections.Generic;

namespace JiayiLauncher.Features.Mods;

public class ModCollection
{
	public string BasePath { get; set; } = string.Empty;
	public List<Mod> Mods { get; set; } = new();

	private ModCollection() // nobody should be calling this
	{
		
	}

	public static ModCollection Create(string path)
	{
		throw new NotImplementedException();
	}
}