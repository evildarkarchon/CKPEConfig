// Models/ConfigSection.cs

using System.Collections.Generic;

namespace CKPEConfig.Models;

public class ConfigSection
{
    public string Name { get; set; }
    public string Tooltip { get; set; }
    public int? LineNumber { get; set; }
    public List<ConfigEntry> Entries { get; set; }

    public ConfigSection(string name, string tooltip = "", int? lineNumber = null)
    {
        Name = name;
        Tooltip = tooltip;
        LineNumber = lineNumber;
        Entries = new List<ConfigEntry>();
    }
}