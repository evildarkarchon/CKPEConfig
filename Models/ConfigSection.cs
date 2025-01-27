// Models/ConfigSection.cs

using System.Collections.Generic;

namespace CKPEConfig.Models;

public class ConfigSection(string name, string tooltip = "", int? lineNumber = null)
{
    public string Name { get; set; } = name;
    public string Tooltip { get; set; } = tooltip;
    public int? LineNumber { get; set; } = lineNumber;
    public List<ConfigEntry> Entries { get; set; } = new();
}