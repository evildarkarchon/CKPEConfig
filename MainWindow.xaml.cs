// MainWindow.xaml.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using CKPEConfig.Controls;
using Microsoft.Win32;

namespace CKPEConfig;

public partial class MainWindow : Window
{
    private List<ConfigSection> sections = new();
    private readonly Dictionary<(string SectionName, string EntryName), Control> widgets = new();
    private string[] originalLines = Array.Empty<string>();
    private string? currentFile;
    private TabControl? tabControl;
    private UIElement? brandingContent;

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

        brandingContent = brandingPanel;
    }

    private void ShowBrandingContent()
    {
        MainContent.Children.Clear();
        if (brandingContent != null)
        {
            MainContent.Children.Add(brandingContent);
        }
    }

    private void ShowEditorContent()
    {
        MainContent.Children.Clear();
        if (tabControl != null)
        {
            MainContent.Children.Add(tabControl);
        }
    }

    private void LoadIni_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "INI file (CreationKitPlatformExtended.ini)|CreationKitPlatformExtended.ini",
            Title = "Open INI file"
        };

        if (dialog.ShowDialog() != true) return;

        if (!VerifyFilename(dialog.FileName, "selected")) return;

        (sections, originalLines) = ConfigParser.ParseIniWithComments(dialog.FileName);
        currentFile = dialog.FileName;
        RefreshUI();
    }

    private bool isSaving = false;  // Add this field at class level

    private void SaveIni_Click(object sender, RoutedEventArgs e)
    {
        if (isSaving) return;  // Prevent multiple simultaneous saves

        try
        {
            isSaving = true;

            if (currentFile == null)
            {
                var dialog = new SaveFileDialog
                {
                    FileName = "CreationKitPlatformExtended.ini",
                    Filter = "INI files (CreationKitPlatformExtended.ini)|CreationKitPlatformExtended.ini"
                };

                if (dialog.ShowDialog() != true) return;

                if (!VerifyFilename(dialog.FileName, "save")) return;

                currentFile = dialog.FileName;
            }

            var newLines = new List<string>(originalLines);

            foreach (var section in sections)
            {
                foreach (var entry in section.Entries)
                {
                    var widget = widgets[(section.Name, entry.Name)];
                    string value = GetWidgetValue(widget);

                    var newLine = entry.InlineComment != null
                        ? $"{entry.Name}={value}\t\t\t; {entry.InlineComment}"
                        : $"{entry.Name}={value}";

                    if (entry.LineNumber.HasValue)
                    {
                        var leadingSpace = originalLines[entry.LineNumber.Value].Length -
                                         originalLines[entry.LineNumber.Value].TrimStart().Length;
                        newLines[entry.LineNumber.Value] = new string(' ', leadingSpace) + newLine;
                    }
                    else
                    {
                        newLines.Add(newLine);
                    }
                }
            }

            File.WriteAllLines(currentFile, newLines);
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
            isSaving = false;
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

    private void RefreshUI()
    {
        widgets.Clear();
        tabControl = new TabControl();

        foreach (var section in sections)
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

            tabControl.Items.Add(tabItem);
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

            widgets[(section.Name, entry.Name)] = widget;
        }

        container.Children.Add(grid);
        return container;
    }

    private Control CreateWidgetForValue(string value, string entryName, string sectionName)
    {
        if (sectionName == "Hotkeys" || entryName == "uTintMaskResolution" || sectionName == "Log")
        {
            return new TextBox { Text = value };
        }

        if (entryName == "nCharset")
        {
            var comboBox = new ComboBox();
            comboBox.Items.Add(new ComboBoxItem { Content = "ANSI_CHARSET", Tag = 0 });
            comboBox.Items.Add(new ComboBoxItem { Content = "DEFAULT_CHARSET", Tag = 1 });
            comboBox.Items.Add(new ComboBoxItem { Content = "SYMBOL_CHARSET", Tag = 2 });
            comboBox.Items.Add(new ComboBoxItem { Content = "MAC_CHARSET", Tag = 77 });
            comboBox.Items.Add(new ComboBoxItem { Content = "SHIFTJIS_CHARSET", Tag = 128 });
            comboBox.Items.Add(new ComboBoxItem { Content = "HANGEUL_CHARSET", Tag = 129 });
            comboBox.Items.Add(new ComboBoxItem { Content = "JOHAB_CHARSET", Tag = 130 });
            comboBox.Items.Add(new ComboBoxItem { Content = "GB2312_CHARSET", Tag = 134 });
            comboBox.Items.Add(new ComboBoxItem { Content = "CHINESEBIG5_CHARSET", Tag = 136 });
            comboBox.Items.Add(new ComboBoxItem { Content = "GREEK_CHARSET", Tag = 161 });
            comboBox.Items.Add(new ComboBoxItem { Content = "TURKISH_CHARSET", Tag = 162 });
            comboBox.Items.Add(new ComboBoxItem { Content = "VIETNAMESE_CHARSET", Tag = 163 });
            comboBox.Items.Add(new ComboBoxItem { Content = "HEBREW_CHARSET", Tag = 177 });
            comboBox.Items.Add(new ComboBoxItem { Content = "ARABIC_CHARSET", Tag = 178 });
            comboBox.Items.Add(new ComboBoxItem { Content = "BALTIC_CHARSET", Tag = 186 });
            comboBox.Items.Add(new ComboBoxItem { Content = "RUSSIAN_CHARSET", Tag = 204 });
            comboBox.Items.Add(new ComboBoxItem { Content = "THAI_CHARSET", Tag = 222 });
            comboBox.Items.Add(new ComboBoxItem { Content = "EASTEUROPE_CHARSET", Tag = 238 });
            comboBox.Items.Add(new ComboBoxItem { Content = "OEM_CHARSET", Tag = 255 });

            if (int.TryParse(value, out var charsetValue))
            {
                var item = comboBox.Items.Cast<ComboBoxItem>()
                    .FirstOrDefault(item => (int)item.Tag == charsetValue);
                if (item != null)
                    comboBox.SelectedItem = item;
                else
                    comboBox.SelectedIndex = 1; // Default to DEFAULT_CHARSET (1)
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