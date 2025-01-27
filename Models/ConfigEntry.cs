namespace CKPEConfig.Models;

public class ConfigEntry
{
    public string Name { get; set; }
    public string Value { get; set; }
    public string Tooltip { get; set; }
    public int? LineNumber { get; set; }
    public string InlineComment { get; set; }

    public ConfigEntry(string name, string value, string tooltip = "", int? lineNumber = null)
    {
        Name = name;
        Value = value;
        Tooltip = tooltip;
        LineNumber = lineNumber;
        InlineComment = string.Empty;
    }
}