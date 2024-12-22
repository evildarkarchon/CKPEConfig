namespace CKPEConfig;

public class ConfigEntry
{
    public string Name
    {
        get;
    }
    public string Value
    {
        get; set;
    }
    public string? Tooltip
    {
        get;
    }
    public int? LineNumber
    {
        get;
    }
    public string? InlineComment
    {
        get; set;
    }

    public ConfigEntry(string name, string value, string? tooltip = null, int? lineNumber = null)
    {
        Name = name;
        Value = value;
        Tooltip = tooltip;
        LineNumber = lineNumber;
    }
}