namespace CKPEConfig;

public class ConfigEntry(string name, string value, string? tooltip = null, int? lineNumber = null)
{
    public string Name
    {
        get;
    } = name;

    public string Value
    {
        get; set;
    } = value;

    public string? Tooltip
    {
        get;
    } = tooltip;

    public int? LineNumber
    {
        get;
    } = lineNumber;

    public string? InlineComment
    {
        get; set;
    }
}