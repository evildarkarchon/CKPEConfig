namespace CKPEConfig;

public class ConfigSection
{
    public string Name
    {
        get;
    }
    public string? Tooltip
    {
        get;
    }
    public int? LineNumber
    {
        get;
    }
    public List<ConfigEntry> Entries { get; } = new();

    public ConfigSection(string name, string? tooltip = null, int? lineNumber = null)
    {
        Name = name;
        Tooltip = tooltip;
        LineNumber = lineNumber;
    }
}