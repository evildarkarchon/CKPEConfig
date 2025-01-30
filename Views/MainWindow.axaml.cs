using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using Avalonia.VisualTree;
using CKPEConfig.ViewModels;
using Serilog;

namespace CKPEConfig.Views;
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        
        DataContext = new MainWindowViewModel();
        
        // Dynamically handle whenever the DataContext is assigned
        DataContextChanged += MainWindow_DataContextChanged;


        // Subscribe to an event to adjust tabs after loading (defined in ViewModel)
        if (DataContext is MainWindowViewModel viewModel)
        {
            viewModel.RequestAdjustWindowWidth += async () => { await AdjustTabsAfterLayout(); };
        }
    }

    /// Handles the DataContextChanged event of the MainWindow.
    /// This method ensures that the necessary subscriptions are updated based on the new DataContext.
    /// <param name="sender">The source of the event, typically the MainWindow instance.</param>
    /// <param name="e">Event arguments containing information about the DataContext change.</param>
    private void MainWindow_DataContextChanged(object? sender, EventArgs e)
    {
        // Unsubscribe from previous DataContext if necessary
        if (_viewModel != null)
        {
            _viewModel.RequestAdjustWindowWidth -= OnAdjustWindowWidthRequested;
        }

        // Subscribe to the new DataContext if it's the correct type
        if (DataContext is MainWindowViewModel viewModel)
        {
            _viewModel = viewModel;
            _viewModel.RequestAdjustWindowWidth += OnAdjustWindowWidthRequested;
        }
        else
        {
            Log.Warning("DataContext is not MainWindowViewModel. Skipping subscription.");
        }
    }
    // Event handler for the actual event
    private async void OnAdjustWindowWidthRequested()
    {
        await AdjustTabsAfterLayout();
    }

    private MainWindowViewModel? _viewModel; // Cache current view model to prevent duplicate subscriptions


    /// Adjusts the tab layout after the UI layout process completes.
    /// This method ensures the TabControl headers are properly measured
    /// and adjusts the window width if necessary to accommodate the tabs.
    private async Task AdjustTabsAfterLayout()
    {
        // Find TabControl by its name
        var tabControl = this.FindControl<TabControl>("MainTabControl");
        if (tabControl != null)
        {
            // Wait for the layout process to complete
            await Dispatcher.UIThread.InvokeAsync(() => { AdjustWindowWidth(tabControl); });
        }
    }

    /// Adjusts the width of the window based on the total width of the TabControl's tabs.
    /// This method measures all the tab headers within the provided TabControl and, if necessary,
    /// increases the window width to ensure all tabs are fully visible.
    /// <param name="tabControl">The TabControl containing the tabs whose headers are to be measured.</param>
    private void AdjustWindowWidth(TabControl tabControl)
    {
        double totalTabHeaderWidth = 0;

        // Find TabStrip (container for TabItem headers)
        var tabHeadersContainer = tabControl.GetVisualChildren()
            .OfType<TabStrip>()
            .FirstOrDefault();

        // Measure all TabHeaders
        if (tabHeadersContainer != null)
        {
            foreach (var tabHeader in tabHeadersContainer.GetVisualChildren())
            {
                if (tabHeader is not Control headerControl) continue;
                headerControl.Measure(Size.Infinity);
                totalTabHeaderWidth += headerControl.DesiredSize.Width;
            }
        }

        // Adjust the window size if the tabs overflow
        if (totalTabHeaderWidth > Bounds.Width)
        {
            Width = totalTabHeaderWidth + 40; // Add some padding
        }
    }
}