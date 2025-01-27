using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Layout;
using ReactiveUI;

namespace CKPEConfig.Services;

public static class MessageDialog
{
    public static async Task ShowAsync(TopLevel parent, string title, string message)
    {
        if (parent is not Window parentWindow)
        {
            return;
        }

        // Create content first
        var content = new StackPanel
        {
            Margin = new Avalonia.Thickness(20)
        };
        
        var messageText = new TextBlock
        {
            Text = message,
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            Margin = new Avalonia.Thickness(0, 0, 0, 20)
        };
        
        var okButton = new Button
        {
            Content = "OK",
            HorizontalAlignment = HorizontalAlignment.Center
        };

        content.Children.Add(messageText);
        content.Children.Add(okButton);

        // Create dialog window
        var dialogWindow = new Window
        {
            Title = title,
            Width = 300,
            Height = 150,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Content = content
        };

        // Wire up the button command after window is created
        okButton.Command = ReactiveCommand.Create(() => dialogWindow.Close());

        await dialogWindow.ShowDialog(parentWindow);
    }
}