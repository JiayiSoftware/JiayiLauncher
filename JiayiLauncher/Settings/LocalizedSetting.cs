namespace JiayiLauncher.Settings;

public struct LocalizedSetting
{
    public string Name { get; set; }
    public string Category { get; set; }
    public string Description { get; set; }
    public string Tooltip { get; set; }
    
    public LocalizedSetting(string name, string category, string description, string tooltip)
    {
        Name = name;
        Category = category;
        Description = description;
        Tooltip = tooltip;
    }
}