using System.Collections.Generic;
using System.IO;
using System.Text;
using JiayiLauncher.Settings;

namespace JiayiLauncher.Appearance;

public class LocalTheme
{
	private static readonly string _themeRoot = Path.Combine(ThemeState.RootPath, "themes");
    
	public string Name;

	public LocalTheme(string name)
	{
		Name = name;
	}

	public static LocalTheme[] GetAllThemes()
	{
		var localThemes = new List<LocalTheme>();

		var path = Path.Combine(_themeRoot, ".local");
		Directory.CreateDirectory(path); // Ensure directory exists
		var directories = Directory.GetDirectories(Path.Combine(_themeRoot, ".local"));
		foreach (var d in directories)
		{
			var name = new DirectoryInfo(d).Name;
			var theme = new LocalTheme(name);
			localThemes.Add(theme);
		}

		if (localThemes.Count <= 0)
		{
			var theme = CreateTheme("default");
			if (theme != null) localThemes.Add(theme);
		}

		return localThemes.ToArray();
	}

	public static LocalTheme? CreateTheme(string name)
	{
		var path = Path.Combine(_themeRoot, $".local\\{name}");
		if (Directory.Exists(path))
		{
			return null;
		}

		Directory.CreateDirectory(path);

		var buffer = File.Create(Path.Combine(path, "theme.css"));
		var defaultTheme = new ThemeState().ThemeStyles.ToString();
		var byteArray = Encoding.UTF8.GetBytes(defaultTheme);
		buffer.Write(byteArray, 0, byteArray.Length);
		buffer.Close();

		return new LocalTheme(name);
	}

	public static void SaveCurrentTheme()
	{
		var buffer = File.OpenWrite(Path.Combine(_themeRoot, JiayiSettings.Instance.Theme, "theme.css"));
		var themeStyles = ThemeState.Instance.ThemeStyles.ToString();
		var byteArray = Encoding.UTF8.GetBytes(themeStyles);
		buffer.Write(byteArray, 0, byteArray.Length);
		buffer.Close();
	}
}