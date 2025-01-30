// Models/CharsetInfo.cs

namespace CKPEConfig.Models;

/// <summary>
/// Represents charset information with a name and a corresponding integer value.
/// This class is primarily used to define a character set used in configuration settings.
/// </summary>
public class CharsetInfo(string name, int value)
{
    public string Name { get; set; } = name;
    public int Value { get; set; } = value;
}