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

    // ReSharper disable once MemberCanBePrivate.Global
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

    /// Represents the view model for a configuration entry.
    /// This class provides reactive functionality for handling
    /// and interacting with individual configuration entries,
    /// including related metadata and behavior.
    public ConfigEntryViewModel(ConfigEntry entry, ConfigSection section)
    {
        _entry = entry;
        _section = section;
        _value = entry.Value;

        Charsets =
        [
            new CharsetInfo("ANSI_CHARSET", 0),
            new CharsetInfo("DEFAULT_CHARSET", 1),
            new CharsetInfo("SYMBOL_CHARSET", 2),
            new CharsetInfo("SHIFTJIS_CHARSET", 128),
            new CharsetInfo("HANGEUL_CHARSET", 129),
            new CharsetInfo("GB2312_CHARSET", 134),
            new CharsetInfo("CHINESEBIG5_CHARSET", 136),
            new CharsetInfo("OEM_CHARSET", 255),
            new CharsetInfo("JOHAB_CHARSET", 130),
            new CharsetInfo("HEBREW_CHARSET", 177),
            new CharsetInfo("ARABIC_CHARSET", 178),
            new CharsetInfo("GREEK_CHARSET", 161),
            new CharsetInfo("TURKISH_CHARSET", 162),
            new CharsetInfo("VIETNAMESE_CHARSET", 163),
            new CharsetInfo("THAI_CHARSET", 222),
            new CharsetInfo("EASTEUROPE_CHARSET", 238),
            new CharsetInfo("RUSSIAN_CHARSET", 204),
            new CharsetInfo("MAC_CHARSET", 77),
            new CharsetInfo("BALTIC_CHARSET", 186)
        ];

        Themes =
        [
            new ThemeInfo("Lighter", 0),
            new ThemeInfo("Darker", 1),
            new ThemeInfo("Custom", 2)
        ];

        // Set initial selected items
        if (IsCharset && int.TryParse(_value, out var charsetValue))
        {
            _selectedCharset = Charsets.FirstOrDefault(c => c.Value == charsetValue);
        }

        if (IsTheme && int.TryParse(_value, out var themeValue))
        {
            _selectedTheme = Themes.FirstOrDefault(t => t.Value == themeValue);
        }
    }

    /// Converts the current view model instance into a corresponding
    /// ConfigEntry model instance.
    /// This method extracts the current state of the view model and
    /// encapsulates it into a ConfigEntry object, preserving its
    /// relevant properties and metadata like name, value, tooltip,
    /// line number, and inline comments.
    public ConfigEntry ToModel()
    {
        return new ConfigEntry(_entry.Name, Value, _entry.Tooltip, _entry.LineNumber)
        {
            InlineComment = _entry.InlineComment
        };
    }
}