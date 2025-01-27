using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using CKPEConfig.ViewModels;
using CKPEConfig.Views;
using ReactiveUI;
using System.Reactive.Concurrency;

namespace CKPEConfig;

public partial class App : Application
{
    public static Window? MainWindow { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

        RxApp.MainThreadScheduler = AvaloniaScheduler.Instance;
        RxApp.TaskpoolScheduler = TaskPoolScheduler.Default;
    }

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