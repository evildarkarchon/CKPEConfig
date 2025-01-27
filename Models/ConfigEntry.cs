namespace CKPEConfig.Models;

public class ConfigEntry(string name, string value, string tooltip = "", int? lineNumber = null)
{
    public string Name { get; set; } = name;
    public string Value { get; set; } = value;
    public string Tooltip { get; set; } = tooltip;
    public int? LineNumber { get; set; } = lineNumber;
    public string InlineComment { get; init; } = string.Empty;
}