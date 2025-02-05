﻿using System;
using System.Collections.Generic;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Reactive;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CKPEConfig.Services;
using System.IO;
using System.Diagnostics;
using System.Linq;
using Avalonia.Threading;
using System.Threading.Tasks;
using Serilog;

namespace CKPEConfig.ViewModels;

public class MainWindowViewModel : ReactiveObject
{
    private readonly IConfigService _configService;
    private bool _isEditorVisible;
    private string? _currentFile;
    private List<string> _originalLines = [];

    public ObservableCollection<ConfigSectionViewModel> Sections { get; }
    public ReactiveCommand<Unit, Unit> LoadIniCommand { get; }
    public ReactiveCommand<Unit, Unit> SaveIniCommand { get; }

    public bool IsEditorVisible
    {
        get => _isEditorVisible;
        private set => this.RaiseAndSetIfChanged(ref _isEditorVisible, value);
    }

    /// <summary>
    /// Represents the primary view model for the main window of the application.
    /// Manages the loading, displaying, and saving of INI file configurations,
    /// including user interactivity through commands and visibility control for the editor.
    /// </summary>
    public MainWindowViewModel()
    {
        _configService = new ConfigService();
        Sections = [];

        LoadIniCommand = ReactiveCommand.CreateFromTask(LoadIniAsync);
        SaveIniCommand = ReactiveCommand.CreateFromTask(SaveIniAsync);

        // Handle command errors
        LoadIniCommand.ThrownExceptions.Subscribe(error =>
            Log.Warning($"Load command error: {error}"));
        SaveIniCommand.ThrownExceptions.Subscribe(error =>
            Log.Warning($"Save command error: {error}"));
    }

    public event Action? RequestAdjustWindowWidth;

    /// <summary>
    /// Loads an INI file asynchronously, parses its contents, and updates the application's state.
    /// Handles file selection, validation, and UI updates, including populating sections
    /// and managing the visibility of the editor.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation of loading and parsing the INI file.</returns>
    private async Task LoadIniAsync()
    {
        try
        {
            // File picker logic remains unchanged
            var parentWindow = TopLevel.GetTopLevel(App.MainWindow);
            if (parentWindow == null) return;

            var options = new FilePickerOpenOptions
            {
                Title = "Open INI file",
                AllowMultiple = false,
                FileTypeFilter =
                [
                    new FilePickerFileType("INI Files") { Patterns = ["*.ini"] }
                ]
            };

            var result = await parentWindow.StorageProvider.OpenFilePickerAsync(options);
            if (!result.Any()) return;

            var file = result[0];
            if (!await VerifyFilename(file.Path.LocalPath, "load"))
                return;

            // Parse file
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
                // Signal that the window adjustment is needed
                RequestAdjustWindowWidth?.Invoke();

            });
        }
        catch (Exception ex)
        {
            Log.Information($"Error in LoadIniAsync: {ex}");
            throw;
        }
    }

    /// <summary>
    /// Saves the current contents of the configuration editor to an INI file.
    /// If no file has been specified, prompts the user to select a file through a save dialog.
    /// </summary>
    /// <returns>Returns a task that completes when the save operation is finished.</returns>
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
                    FileTypeChoices =
                    [
                        new FilePickerFileType("INI Files") { Patterns = ["*.ini"] }
                    ]
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

    /// <summary>
    /// Verifies if the given filename matches the expected filename required for the operation.
    /// </summary>
    /// <param name="filepath">The full path of the file to verify.</param>
    /// <param name="operation">The operation being performed, such as "load" or "save", to include in the validation message.</param>
    /// <returns>Returns a task that resolves to <c>true</c> if the filename matches the expected name; otherwise, <c>false</c>.</returns>
    private async Task<bool> VerifyFilename(string filepath, string operation)
    {
        const string expectedName = "CreationKitPlatformExtended.ini";
        var actualName = Path.GetFileName(filepath);

        if (actualName == expectedName) return true;
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
}