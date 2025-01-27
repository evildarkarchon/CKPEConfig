// Models/CharsetInfo.cs
namespace CKPEConfig.Models;

public class CharsetInfo
{
    public string Name { get; set; }
    public int Value { get; set; }

    public CharsetInfo(string name, int value)
    {
        Name = name;
        Value = value;
    }
}