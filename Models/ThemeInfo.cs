// Models/ThemeInfo.cs
namespace CKPEConfig.Models;

public class ThemeInfo
{
    public string Name { get; set; }
    public int Value { get; set; }

    public ThemeInfo(string name, int value)
    {
        Name = name;
        Value = value;
    }
}