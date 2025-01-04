// MainWindow.xaml.cs
using System.IO;
using System.Windows;
using System.Windows.Controls;
using CKPEConfig.Controls;
using Microsoft.Win32;

namespace CKPEConfig;

public class CharsetItem
{
    public required string Name { get; init; }
    public int Value { get; init; }
}

public partial class MainWindow
{
    private List<ConfigSection> _sections = [];
    private readonly Dictionary<(string SectionName, string EntryName), Control> _widgets = new();
    private string[] _originalLines = [];
    private string? _currentFile;
    private TabControl? _tabControl;
    private UIElement? _brandingContent;

    public MainWindow()
    {
        InitializeComponent();
        InitializeBrandingContent();
        ShowBrandingContent();

        // Handle window closing
        Closing += MainWindow_Closing;
    }

    private void InitializeBrandingContent()
    {
        var brandingPanel = new StackPanel
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            Background = System.Windows.Media.Brushes.Transparent  // Allow background to show through
        };

        var heading = new TextBlock
        {
            Text = "Creation Kit Platform Extended",
            FontSize = 24,
            FontWeight = FontWeights.Bold,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 10),
            Foreground = System.Windows.Media.Brushes.White
        };

        var subheading = new TextBlock
        {
            Text = "INI Configuration Editor",
            FontSize = 16,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 5),
            Foreground = System.Windows.Media.Brushes.White
        };

        var version = new TextBlock
        {
            Text = "v0.1.0",
            FontSize = 12,
            HorizontalAlignment = HorizontalAlignment.Center,
            Foreground = System.Windows.Media.Brushes.White
        };

        brandingPanel.Children.Add(heading);
        brandingPanel.Children.Add(subheading);
        brandingPanel.Children.Add(version);

        _brandingContent = brandingPanel;
    }

    private void ShowBrandingContent()
    {
        MainContent.Children.Clear();
        if (_brandingContent != null)
        {
            MainContent.Children.Add(_brandingContent);
        }
    }

    private void ShowEditorContent()
    {
        MainContent.Children.Clear();
        if (_tabControl != null)
        {
            MainContent.Children.Add(_tabControl);
        }
    }

    private void LoadIni_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "INI file (CreationKitPlatformExtended.ini)|CreationKitPlatformExtended.ini",
            Title = "Open INI file"
        };

        if (dialog.ShowDialog() != true)
        {
            return;
        }

        if (!VerifyFilename(dialog.FileName, "selected"))
        {
            return;
        }

        (_sections, _originalLines) = ConfigParser.ParseIniWithComments(dialog.FileName);
        _currentFile = dialog.FileName;
        RefreshUi();
    }

    private bool _isSaving;  // Add this field at class level

    private void SaveIni_Click(object sender, RoutedEventArgs e)
    {
        if (_isSaving)
        {
            return;  // Prevent multiple simultaneous saves
        }

        try
        {
            _isSaving = true;

            if (_currentFile == null)
            {
                var dialog = new SaveFileDialog
                {
                    FileName = "CreationKitPlatformExtended.ini",
                    Filter = "INI files (CreationKitPlatformExtended.ini)|CreationKitPlatformExtended.ini"
                };

                if (dialog.ShowDialog() != true)
                {
                    return;
                }

                if (!VerifyFilename(dialog.FileName, "save"))
                {
                    return;
                }

                _currentFile = dialog.FileName;
            }

            var newLines = new List<string>(_originalLines);

            foreach (var section in _sections)
            {
                foreach (var entry in section.Entries)
                {
                    var widget = _widgets[(section.Name, entry.Name)];
                    var value = GetWidgetValue(widget);

                    var newLine = entry.InlineComment != null
                        ? $"{entry.Name}={value}\t\t\t; {entry.InlineComment}"
                        : $"{entry.Name}={value}";

                    if (entry.LineNumber.HasValue)
                    {
                        var leadingSpace = _originalLines[entry.LineNumber.Value].Length -
                                         _originalLines[entry.LineNumber.Value].TrimStart().Length;
                        newLines[entry.LineNumber.Value] = new string(' ', leadingSpace) + newLine;
                    }
                    else
                    {
                        newLines.Add(newLine);
                    }
                }
            }

            File.WriteAllLines(_currentFile, newLines);
            MessageBox.Show(
                "Settings saved successfully.",
                "Save Complete",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Unable to save the file:\n\n{ex.Message}",
                "Save Failed",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally
        {
            _isSaving = false;
        }
    }

    private string GetWidgetValue(Control widget) => widget switch
    {
        CheckBox cb => cb.IsChecked?.ToString().ToLower() ?? "false",
        TextBox tb => tb.Text,
        ComboBox cb => ((int?)((cb.SelectedItem as ComboBoxItem)?.Tag) ?? 1).ToString(),
        NumericUpDownControl nud => nud.Value.ToString(),
        _ => ""
    };

    private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        // If there are unsaved changes, prompt the user
        if (HasUnsavedChanges())
        {
            var result = MessageBox.Show(
                "Do you want to save changes before closing?",
                "Unsaved Changes",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question);

            switch (result)
            {
                case MessageBoxResult.Yes:
                    SaveIni_Click(this, new RoutedEventArgs());
                    break;
                case MessageBoxResult.Cancel:
                    e.Cancel = true;
                    return;
            }
        }

        // Let the window close naturally
    }

    private bool HasUnsavedChanges()
    {
        // TODO: Implement change tracking if needed
        // For now, return false to allow closing without prompting
        return false;
    }

    private bool VerifyFilename(string filepath, string operation)
    {
        const string expectedName = "CreationKitPlatformExtended.ini";
        var actualName = Path.GetFileName(filepath);

        if (actualName != expectedName)
        {
            MessageBox.Show(
                $"The {operation} filename must be '{expectedName}'\nSelected file: '{actualName}'",
                "Invalid Filename",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return false;
        }
        return true;
    }

    private void RefreshUi()
    {
        _widgets.Clear();
        _tabControl = new TabControl();

        foreach (var section in _sections)
        {
            var scrollViewer = new ScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
            };

            var container = CreateSectionWidget(section);
            scrollViewer.Content = container;

            var tabItem = new TabItem
            {
                Header = section.Name,
                Content = scrollViewer
            };

            if (!string.IsNullOrEmpty(section.Tooltip))
            {
                tabItem.ToolTip = section.Tooltip;
            }

            _tabControl.Items.Add(tabItem);
        }

        ShowEditorContent();
    }

    private UIElement CreateSectionWidget(ConfigSection section)
    {
        var container = new StackPanel
        {
            Margin = new Thickness(10)
        };

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        for (var i = 0; i < section.Entries.Count; i++)
        {
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        }

        for (var i = 0; i < section.Entries.Count; i++)
        {
            var entry = section.Entries[i];

            var label = new TextBlock
            {
                Text = entry.Name,
                Margin = new Thickness(0, 0, 10, 5),
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = System.Windows.Media.Brushes.White
            };

            if (!string.IsNullOrEmpty(entry.Tooltip))
            {
                label.ToolTip = entry.Tooltip;
            }

            var widget = CreateWidgetForValue(entry.Value, entry.Name, section.Name);

            Grid.SetRow(label, i);
            Grid.SetColumn(label, 0);
            Grid.SetRow(widget, i);
            Grid.SetColumn(widget, 1);

            grid.Children.Add(label);
            grid.Children.Add(widget);

            _widgets[(section.Name, entry.Name)] = widget;
        }

        container.Children.Add(grid);
        return container;
    }

    private static readonly List<CharsetItem> Charsets =
    [
        new CharsetItem { Name = "ANSI_CHARSET", Value = 0 },
        new CharsetItem { Name = "DEFAULT_CHARSET", Value = 1 },
        new CharsetItem { Name = "SYMBOL_CHARSET", Value = 2 },
        new CharsetItem { Name = "MAC_CHARSET", Value = 77 },
        new CharsetItem { Name = "SHIFTJIS_CHARSET", Value = 128 },
        new CharsetItem { Name = "HANGEUL_CHARSET", Value = 129 },
        new CharsetItem { Name = "JOHAB_CHARSET", Value = 130 },
        new CharsetItem { Name = "GB2312_CHARSET", Value = 134 },
        new CharsetItem { Name = "CHINESEBIG5_CHARSET", Value = 136 },
        new CharsetItem { Name = "GREEK_CHARSET", Value = 161 },
        new CharsetItem { Name = "TURKISH_CHARSET", Value = 162 },
        new CharsetItem { Name = "VIETNAMESE_CHARSET", Value = 163 },
        new CharsetItem { Name = "HEBREW_CHARSET", Value = 177 },
        new CharsetItem { Name = "ARABIC_CHARSET", Value = 178 },
        new CharsetItem { Name = "BALTIC_CHARSET", Value = 186 },
        new CharsetItem { Name = "RUSSIAN_CHARSET", Value = 204 },
        new CharsetItem { Name = "THAI_CHARSET", Value = 222 },
        new CharsetItem { Name = "EASTEUROPE_CHARSET", Value = 238 },
        new CharsetItem { Name = "OEM_CHARSET", Value = 255 }
    ];

    private Control CreateWidgetForValue(string value, string entryName, string sectionName)
    {
        if (sectionName == "Hotkeys" || entryName == "uTintMaskResolution" || sectionName == "Log")
        {
            return new TextBox { Text = value };
        }

        if (entryName == "nCharset")
        {
            var comboBox = new ComboBox();
            foreach (var charset in Charsets)
            {
                var item = new ComboBoxItem
                {
                    Content = charset.Name,
                    Tag = charset.Value
                };
                comboBox.Items.Add(item);
            }

            if (int.TryParse(value, out var charsetValue))
            {
                var item = comboBox.Items.Cast<ComboBoxItem>()
                    .FirstOrDefault(item => (int)item.Tag == charsetValue);
                if (item != null)
                {
                    comboBox.SelectedItem = item;
                }
                else
                {
                    comboBox.SelectedIndex = 1; // Default to DEFAULT_CHARSET (1)
                }
            }
            else
            {
                comboBox.SelectedIndex = 1; // Default to DEFAULT_CHARSET (1)
            }

            return comboBox;
        }

        if (value.ToLower() is "true" or "false")
        {
            return new CheckBox { IsChecked = bool.Parse(value) };
        }

        if (int.TryParse(value, out var numericValue))
        {
            if (entryName == "bUIDarkThemeID" || entryName == "nGenerationVersion")
            {
                return new NumericUpDownControl
                {
                    Value = numericValue,
                    Minimum = 0,
                    Maximum = 2,
                    Step = 1
                };
            }
            return new NumericUpDownControl
            {
                Value = numericValue,
                Minimum = 0,
                Maximum = 999999,
                Step = 1
            };
        }

        return new TextBox { Text = value };
    }
}