namespace CKPEConfig;

public class ConfigSection(string name, string? tooltip = null, int? lineNumber = null)
{
    public string Name
    {
        get;
    } = name;

    public string? Tooltip
    {
        get;
    } = tooltip;

    public int? LineNumber
    {
        get;
    } = lineNumber;

    public List<ConfigEntry> Entries { get; } = [];
}