using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Layout;
using ReactiveUI;

namespace CKPEConfig.Services;

public static class MessageDialog
{
    /// Displays a message dialog with a specified title and message.
    /// <param name="parent">
    /// The top-level window that will act as the owner of the dialog.
    /// Should be of type `Window`.
    /// </param>
    /// <param name="title">
    /// The title text displayed on the dialog window.
    /// </param>
    /// <param name="message">
    /// The message text displayed inside the dialog.
    /// </param>
    /// <returns>
    /// A `Task` representing the asynchronous operation of showing the dialog.
    /// </returns>
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