// Models/ThemeInfo.cs
namespace CKPEConfig.Models;

public class ThemeInfo(string name, int value)
{
    public string Name { get; set; } = name;
    public int Value { get; set; } = value;
}