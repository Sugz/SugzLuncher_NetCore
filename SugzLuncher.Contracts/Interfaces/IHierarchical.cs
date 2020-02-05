using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;

namespace SugzLuncher.Contracts
{
    public interface IHierarchical
    {
        ViewModelBase Parent { get; set; }
        ObservableCollection<ViewModelBase> Children { get; set; }

    }
}
