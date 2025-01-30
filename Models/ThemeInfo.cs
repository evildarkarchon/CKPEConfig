// Models/ThemeInfo.cs

namespace CKPEConfig.Models;

/// <summary>
/// Represents a theme information entity, typically used for configuration or user interface purposes.
/// </summary>
public class ThemeInfo(string name, int value)
{
    public string Name { get; set; } = name;
    public int Value { get; set; } = value;
}