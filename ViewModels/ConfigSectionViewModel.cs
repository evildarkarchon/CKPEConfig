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

    public ConfigSectionViewModel(ConfigSection section)
    {
        _section = section;
        Entries = new ObservableCollection<ConfigEntryViewModel>(
            section.Entries.Select(e => new ConfigEntryViewModel(e, section)));
    }

    public ConfigSection ToModel()
    {
        var section = new ConfigSection(_section.Name, _section.Tooltip, _section.LineNumber);
        section.Entries.AddRange(Entries.Select(vm => vm.ToModel()));
        return section;
    }
}