using GalaSoft.MvvmLight;
using SugzLuncher.Helpers;
using SugzLuncher.Interfaces;
using SugzLuncher.Messages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Windows;

namespace SugzLuncher.ViewModels
{
    public abstract class BaseViewModel : ViewModelBase, IDropTarget
    {

        #region Fields

        private BaseViewModel _SelectedItem;
        private BaseViewModel _Parent;

        #endregion Fields



        #region Properties

        public virtual ObservableCollection<BaseViewModel> Children { get; set; } = new ObservableCollection<BaseViewModel>();


        /// <summary>
        /// The parent containing this item
        /// </summary>
        public BaseViewModel Parent
        {
            get => _Parent;
            set => Set(ref _Parent, value);
        }


        /// <summary>
        /// The current hovered item
        /// </summary>
        public BaseViewModel SelectedItem
        {
            get => _SelectedItem;
            set => Set(ref _SelectedItem, value);
        }


        #endregion Properties



        #region Constructor

        internal BaseViewModel()
        {
            Children.CollectionChanged += OnChildrenCollectionChanged;
        }

        #endregion Constructor



        #region Event handlers

        protected virtual void OnChildrenCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems is null)
                return;

            foreach (BaseViewModel child in e.NewItems)
            {
                child.Parent = this;
                SetChild(child);
            }

            MessengerInstance.Send(new HasUpdatedMessage());
        }

        /// <summary>
        /// Virtual method called for each child in Children when the collection changed
        /// </summary>
        /// <param name="child"></param>
        internal virtual void SetChild(BaseViewModel child) { }


        #endregion Event handlers



        #region IDropTarget

        public virtual bool CanDrop => true;

        public abstract void Drop(IDataObject data, int? index = null);

        #endregion IDropTarget

    }
}
