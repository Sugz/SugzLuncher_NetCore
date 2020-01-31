using System.Windows;

namespace SugzLuncher.Interfaces
{
    public interface IDropTarget
    {
        //bool CanDrop { get; }
        bool CanDrop(object source, IDataObject data);
        void Drop(IDataObject data, int? index = null);

    }
}
