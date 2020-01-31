using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace SugzLuncher.Interfaces
{
    public interface IHierarchical
    {

        ViewModelBase Parent { get; set; }
        ObservableCollection<ViewModelBase> Children { get; set; }

    }
}
