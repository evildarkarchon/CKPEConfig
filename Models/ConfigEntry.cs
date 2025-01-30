namespace CKPEConfig.Models;

/// <summary>
/// Represents a configuration entry consisting of a key-value pair, with optional metadata such as
/// a tooltip, line number, and inline comment. This class is typically used for parsing and
/// representing individual entries within configuration files (e.g., INI files).
/// </summary>
public class ConfigEntry(string name, string value, string tooltip = "", int? lineNumber = null)
{
    public string Name { get; set; } = name;
    public string Value { get; set; } = value;
    public string Tooltip { get; set; } = tooltip;
    public int? LineNumber { get; set; } = lineNumber;
    public string InlineComment { get; init; } = string.Empty;
}