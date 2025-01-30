using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using CKPEConfig.ViewModels;
using CKPEConfig.Views;
using ReactiveUI;
using System.Reactive.Concurrency;
using Serilog;

namespace CKPEConfig;

[SuppressMessage("ReSharper", "PartialTypeWithSinglePart")]
public partial class App : Application
{
    /// <summary>
    /// Gets the reference to the application's main window instance.
    /// </summary>
    /// <remarks>
    /// This property provides access to the primary <see cref="Window"/> instance of the application
    /// and is initialized during the framework's initialization process. It is intended for use in scenarios
    /// where a reference to the main window is required, such as user interactions or configuring the window's behavior.
    /// </remarks>
    public static Window? MainWindow { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        
        // Set up Serilog for logging
        Log.Logger = new LoggerConfiguration().WriteTo.File("logs/main.txt").CreateLogger();
        Log.Information("Serilog initialized!");
           
        Log.Information("Avalonia app initialized!");


        RxApp.MainThreadScheduler = AvaloniaScheduler.Instance;
        RxApp.TaskpoolScheduler = TaskPoolScheduler.Default;
    }

    /// Called when the framework initialization is completed.
    /// This method is overridden to perform additional configuration specific to the application.
    /// In this case, it sets up the main application window and assigns its data context.
    /// It also assigns the `MainWindow` static property to the instance of the application's main window.
    /// The method checks if the application's lifetime is of type `IClassicDesktopStyleApplicationLifetime`.
    /// If it is, a new `MainWindow` is created and its `DataContext` is initialized with a `MainWindowViewModel`.
    /// The `MainWindow` of the lifetime and the static `MainWindow` property are both assigned to the newly created window.
    /// Always calls the base implementation of `OnFrameworkInitializationCompleted` to ensure proper behavior of the framework.
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel()
            };

            desktop.MainWindow = mainWindow;
            MainWindow = mainWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }
}