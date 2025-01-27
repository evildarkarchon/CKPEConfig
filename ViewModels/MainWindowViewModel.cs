using System;
using System.Collections.Generic;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CKPEConfig.Models;
using CKPEConfig.Services;
using System.IO;
using System.Diagnostics;
using System.Linq;
using Avalonia.Threading;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace CKPEConfig.ViewModels;

public class MainWindowViewModel : ReactiveObject
{
    private readonly IConfigService _configService;
    private bool _isEditorVisible;
    private string? _currentFile;
    private List<string> _originalLines = new();
    
    public ObservableCollection<ConfigSectionViewModel> Sections { get; }
    public ReactiveCommand<Unit, Unit> LoadIniCommand { get; }
    public ReactiveCommand<Unit, Unit> SaveIniCommand { get; }
    
    public bool IsEditorVisible
    {
        get => _isEditorVisible;
        private set => this.RaiseAndSetIfChanged(ref _isEditorVisible, value);
    }

    public MainWindowViewModel()
    {
        _configService = new ConfigService();
        Sections = new ObservableCollection<ConfigSectionViewModel>();

        LoadIniCommand = ReactiveCommand.CreateFromTask(LoadIniAsync);
        SaveIniCommand = ReactiveCommand.CreateFromTask(SaveIniAsync);

        // Handle command errors
        LoadIniCommand.ThrownExceptions.Subscribe(error => 
            Debug.WriteLine($"Load command error: {error}"));
        SaveIniCommand.ThrownExceptions.Subscribe(error => 
            Debug.WriteLine($"Save command error: {error}"));
    }

    private async Task LoadIniAsync()
    {
        try
        {
            // Get the top level window
            var parentWindow = TopLevel.GetTopLevel(App.MainWindow);
            if (parentWindow == null) return;

            // Create file picker options
            var options = new FilePickerOpenOptions
            {
                Title = "Open INI file",
                AllowMultiple = false,
                FileTypeFilter = new[] 
                { 
                    new FilePickerFileType("INI Files") { Patterns = new[] { "*.ini" } }
                }
            };

            // Show the file picker
            var result = await parentWindow.StorageProvider.OpenFilePickerAsync(options);
            if (!result.Any()) return;

            var file = result[0];
            if (!await VerifyFilename(file.Path.LocalPath, "load"))
                return;

            var (sections, lines) = await _configService.ParseIniWithCommentsAsync(file.Path.LocalPath);
            _currentFile = file.Path.LocalPath;
            _originalLines = lines;

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                Sections.Clear();
                foreach (var section in sections)
                {
                    Sections.Add(new ConfigSectionViewModel(section));
                }
                IsEditorVisible = true;
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in LoadIniAsync: {ex}");
            throw;
        }
    }

    private async Task SaveIniAsync()
    {
        try
        {
            var parentWindow = TopLevel.GetTopLevel(App.MainWindow);
            if (parentWindow == null) return;

            if (_currentFile == null)
            {
                var options = new FilePickerSaveOptions
                {
                    Title = "Save INI file",
                    DefaultExtension = "ini",
                    SuggestedFileName = "CreationKitPlatformExtended.ini",
                    FileTypeChoices = new[] 
                    { 
                        new FilePickerFileType("INI Files") { Patterns = new[] { "*.ini" } }
                    }
                };

                var file = await parentWindow.StorageProvider.SaveFilePickerAsync(options);
                if (file == null) return;

                if (!await VerifyFilename(file.Path.LocalPath, "save"))
                    return;

                _currentFile = file.Path.LocalPath;
            }

            var sections = await Dispatcher.UIThread.InvokeAsync(() => 
                Sections.Select(vm => vm.ToModel()).ToList());

            await _configService.SaveIniAsync(_currentFile, sections, _originalLines);

            // Show success dialog
            if (TopLevel.GetTopLevel(App.MainWindow) is { } parentWindow2)
            {
                await MessageDialog.ShowAsync(parentWindow2, "Success", "File saved successfully!");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in SaveIniAsync: {ex}");
            throw;
        }
    }

    private async Task<bool> VerifyFilename(string filepath, string operation)
    {
        const string expectedName = "CreationKitPlatformExtended.ini";
        var actualName = Path.GetFileName(filepath);

        if (actualName != expectedName)
        {
            var parentWindow = TopLevel.GetTopLevel(App.MainWindow);
            if (parentWindow != null)
            {
                await MessageDialog.ShowAsync(
                    parentWindow,
                    "Invalid Filename",
                    $"The {operation} filename must be '{expectedName}'\nSelected file: '{actualName}'");
            }
            return false;
        }
        return true;
    }
}