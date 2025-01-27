// Models/CharsetInfo.cs
namespace CKPEConfig.Models;

public class CharsetInfo(string name, int value)
{
    public string Name { get; set; } = name;
    public int Value { get; set; } = value;
}