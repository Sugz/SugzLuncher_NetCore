using System.Windows;

namespace SugzLuncher.Interfaces
{
    public interface IDropTarget
    {
        //TODO: bool CanAccept(object source, IDataObject data);

        bool CanDrop { get; }
        
        void Drop(IDataObject data, int? index = null);
    }
}
