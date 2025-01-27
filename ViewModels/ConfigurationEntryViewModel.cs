// ViewModels/ConfigEntryViewModel.cs
using CKPEConfig.Models;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Linq;

namespace CKPEConfig.ViewModels;

public class ConfigEntryViewModel : ReactiveObject
{
    private readonly ConfigEntry _entry;
    private readonly ConfigSection _section;
    private string _value;
    private CharsetInfo? _selectedCharset;
    private ThemeInfo? _selectedTheme;
    
    public string Name => _entry.Name;
    public string Tooltip => _entry.Tooltip;
    
    public string Value
    {
        get => _value;
        set => this.RaiseAndSetIfChanged(ref _value, value);
    }

    public CharsetInfo? SelectedCharset
    {
        get => _selectedCharset;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedCharset, value);
            if (value != null)
            {
                Value = value.Value.ToString();
            }
        }
    }

    public ThemeInfo? SelectedTheme
    {
        get => _selectedTheme;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedTheme, value);
            if (value != null)
            {
                Value = value.Value.ToString();
            }
        }
    }

    public bool IsBoolean => bool.TryParse(_entry.Value.ToLower(), out _);
    public bool IsInteger => int.TryParse(_entry.Value, out _);
    public bool IsNormalInteger => IsInteger && !IsCharset && !IsTheme;
    public bool IsCharset => Name == "nCharset";
    public bool IsTheme => Name == "uUIDarkThemeId";
    public bool IsSpecialTextbox => Name == "uTintMaskResolution" || 
                                   _section.Name == "Hotkeys" || 
                                   _section.Name == "Log";
                                   
    public bool IsDefaultTextBox => !IsBoolean && !IsInteger && !IsCharset && !IsTheme;

    public ObservableCollection<CharsetInfo> Charsets { get; }
    public ObservableCollection<ThemeInfo> Themes { get; }

    public ConfigEntryViewModel(ConfigEntry entry, ConfigSection section)
    {
        _entry = entry;
        _section = section;
        _value = entry.Value;

        Charsets = new ObservableCollection<CharsetInfo>
        {
            new("ANSI_CHARSET", 0),
            new("DEFAULT_CHARSET", 1),
            new("SYMBOL_CHARSET", 2),
            new("SHIFTJIS_CHARSET", 128),
            new("HANGEUL_CHARSET", 129),
            new("GB2312_CHARSET", 134),
            new("CHINESEBIG5_CHARSET", 136),
            new("OEM_CHARSET", 255),
            new("JOHAB_CHARSET", 130),
            new("HEBREW_CHARSET", 177),
            new("ARABIC_CHARSET", 178),
            new("GREEK_CHARSET", 161),
            new("TURKISH_CHARSET", 162),
            new("VIETNAMESE_CHARSET", 163),
            new("THAI_CHARSET", 222),
            new("EASTEUROPE_CHARSET", 238),
            new("RUSSIAN_CHARSET", 204),
            new("MAC_CHARSET", 77),
            new("BALTIC_CHARSET", 186)
        };

        Themes = new ObservableCollection<ThemeInfo>
        {
            new("Lighter", 0),
            new("Darker", 1),
            new("Custom", 2)
        };

        // Set initial selected items
        if (IsCharset && int.TryParse(_value, out int charsetValue))
        {
            _selectedCharset = Charsets.FirstOrDefault(c => c.Value == charsetValue);
        }

        if (IsTheme && int.TryParse(_value, out int themeValue))
        {
            _selectedTheme = Themes.FirstOrDefault(t => t.Value == themeValue);
        }
    }

    public ConfigEntry ToModel()
    {
        return new ConfigEntry(_entry.Name, Value, _entry.Tooltip, _entry.LineNumber)
        {
            InlineComment = _entry.InlineComment
        };
    }
}