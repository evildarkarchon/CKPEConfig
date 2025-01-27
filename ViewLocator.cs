using System;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using CKPEConfig.ViewModels;

namespace CKPEConfig;

[SuppressMessage("ReSharper", "UnusedType.Global")]
public class ViewLocator : IDataTemplate
{
    /// <summary>
    /// Builds a corresponding view for the given view model object.
    /// If the view model's type has a matching view type, the view is instantiated.
    /// If no matching view type is found, a <see cref="TextBlock"/> is returned with a "Not Found" message.
    /// </summary>
    /// <param name="param">An object representing the view model for which a view is to be created.</param>
    /// <returns>A <see cref="Control"/> representing the corresponding view if found, or a <see cref="TextBlock"/> with a "Not Found" message if no matching view is available.</returns>
    public Control? Build(object? param)
    {
        if (param is null)
            return null;

        var name = param.GetType().FullName!.Replace("ViewModel", "View", StringComparison.Ordinal);
        var type = Type.GetType(name);

        if (type != null)
        {
            return (Control)Activator.CreateInstance(type)!;
        }

        return new TextBlock { Text = "Not Found: " + name };
    }

    /// <summary>
    /// Determines whether the given data object matches the required type for this template.
    /// </summary>
    /// <param name="data">The data object to check against the required type.</param>
    /// <returns><c>true</c> if the data object is of type <see cref="ViewModelBase"/>; otherwise, <c>false</c>.</returns>
    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}