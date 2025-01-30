// Models/ConfigSection.cs

using System.Collections.Generic;

namespace CKPEConfig.Models;

/// <summary>
/// Represents a section within a configuration file (e.g., an INI file), containing metadata
/// such as the name of the section, an optional tooltip or comment, and the line number
/// where the section appears in the file. Also contains a collection of <see cref="ConfigEntry"/>
/// objects that make up the key-value pairs within the section.
/// </summary>
public class ConfigSection(string name, string tooltip = "", int? lineNumber = null)
{
    public string Name { get; set; } = name;
    public string Tooltip { get; set; } = tooltip;
    public int? LineNumber { get; set; } = lineNumber;
    public List<ConfigEntry> Entries { get; set; } = new();
}