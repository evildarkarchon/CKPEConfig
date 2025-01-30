using Avalonia;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;

namespace CKPEConfig;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    [SuppressMessage("ReSharper", "HeapView.ObjectAllocation.Possible")]
    public static void Main(string[] args)
    {
        AppContext.SetData("APP_CONTEXT_BASE_DIRECTORY", AppDomain.CurrentDomain.BaseDirectory);
        AppDomain.CurrentDomain.AssemblyResolve += (_, resolveEventArgs) =>
        {
            var assemblyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "libs",
                new AssemblyName(resolveEventArgs.Name).Name + ".dll");
            return File.Exists(assemblyPath) ? Assembly.LoadFrom(assemblyPath) : null;
        };

        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    private static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}