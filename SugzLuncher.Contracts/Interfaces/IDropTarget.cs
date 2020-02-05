using System.Windows;

namespace SugzLuncher.Contracts
{
    public interface IDropTarget
    {
        //bool CanDrop { get; }
        bool CanDrop(object source, IDataObject data);
        void Drop(IDataObject data, int? index = null);

    }
}
