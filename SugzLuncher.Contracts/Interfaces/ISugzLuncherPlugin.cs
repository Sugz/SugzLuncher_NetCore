using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;

namespace SugzLuncher.Contracts
{
    public interface ISugzLuncherPlugin
    {
        Type ViewModelType { get; set; }
        Type ViewType { get; set; }
    }
}
