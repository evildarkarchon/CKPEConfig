using CKPEConfig.Models;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Linq;

namespace CKPEConfig.ViewModels;

public class ConfigSectionViewModel : ReactiveObject
{
    private readonly ConfigSection _section;

    public string Name => _section.Name;
    public string Tooltip => _section.Tooltip;
    public ObservableCollection<ConfigEntryViewModel> Entries { get; }

    /// Represents the view model for a configuration section in the application.
    /// Encapsulates the properties and associated entries of a `ConfigSection` model
    /// while providing reactive state management for UI interaction.
    public ConfigSectionViewModel(ConfigSection section)
    {
        _section = section;
        Entries = new ObservableCollection<ConfigEntryViewModel>(
            section.Entries.Select(e => new ConfigEntryViewModel(e, section)));
    }

    /// Converts the current `ConfigSectionViewModel` instance into a `ConfigSection` model.
    /// <returns>
    /// A `ConfigSection` instance that represents the current view model
    /// and encapsulates its properties and associated entries as models.
    /// </returns>
    public ConfigSection ToModel()
    {
        var section = new ConfigSection(_section.Name, _section.Tooltip, _section.LineNumber);
        section.Entries.AddRange(Entries.Select(vm => vm.ToModel()));
        return section;
    }
}