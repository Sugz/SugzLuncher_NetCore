using GalaSoft.MvvmLight;
using SugzLuncher.Helpers;
using SugzLuncher.Interfaces;
using SugzLuncher.Messages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml.Linq;

namespace SugzLuncher.ViewModels
{
    public abstract class BaseLuncherViewModel : ViewModelBase, IHierarchical, IDropTarget
    {

        #region Fields

        private ViewModelBase _Parent;
        private bool _ShowChildrenNames = true;
        private IconSize _ChildrenIconSize = IconSize.Middle;

        #endregion Fields



        #region Properties

        /// <summary>
        /// The parent containing this item
        /// </summary>
        public ViewModelBase Parent
        {
            get => _Parent;
            set => Set(ref _Parent, value);
        }


        public ObservableCollection<ViewModelBase> Children { get; set; } = new ObservableCollection<ViewModelBase>();


        /// <summary>
        /// Wheter or not item show their names
        /// </summary>
        public bool ShowChildrenNames
        {
            get => _ShowChildrenNames;
            set => Set(ref _ShowChildrenNames, value);
        }


        /// <summary>
        /// The size of the icon
        /// </summary>
        public IconSize ChildrenIconSize
        {
            get => _ChildrenIconSize;
            set
            {
                Set(ref _ChildrenIconSize, value);
                Children.Where(x => x is LuncherViewModel).ForEach(x => ((LuncherViewModel)x).SetIcon());
            }
        }


        #endregion Properties



        #region Constructor

        public BaseLuncherViewModel()
        {
            Children.CollectionChanged += OnChildrenCollectionChanged; ;
        }

        #endregion Constructor



        #region Event handlers

        private void OnChildrenCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems is null)
                return;

            foreach (LuncherViewModel child in e.NewItems)
            {
                child.Parent = this;
                child.SetIcon();
            }

            MessengerInstance.Send(new HasUpdatedMessage());
        }


        #endregion Event handlers



        #region IDropTarget

        public bool CanDrop(object source, IDataObject data)
        {
            return true;
        }

        public void Drop(IDataObject data, int? index = null)
        {
            foreach (string file in ((DataObject)data).GetFileDropList())
            {
                // Desktop Shortcut
                string[] infos = new string[3];
                bool isWin10Shortcut = false;
                if (file.ToLower().EndsWith(".lnk"))
                {
                    infos = FileInfoReader.GetShortcutInfos(file);

                    // In case user drop a win10 app
                    //TODO: find how i created a url link for the new control panel, as it contain the correct icon location (as indirect string)
                    if (string.IsNullOrEmpty(infos[0]))
                    {
                        isWin10Shortcut = true;
                        infos = FileInfoReader.GetWin10Shortcut(file);
                    }
                }

                if (file.ToLower().EndsWith(".url"))
                    infos = FileInfoReader.GetInternetShortcutInfos(file);

                AddLuncher(new LuncherViewModel(infos[0], infos[1], infos[2], 0, isWin10Shortcut), index);
            }
        }

        private void AddLuncher(LuncherViewModel luncher, int? index = null)
        {
            if (index.HasValue)
                Children.Insert(index.Value, luncher);
            else
                Children.Add(luncher);
        }

        #endregion IDropTarget



        #region Serialize

        internal void DeserializeChildren(XElement xElement)
        {
            Children.Clear();
            foreach (XElement childXElement in xElement.Elements())
            {
                string typeName = childXElement.Name.LocalName;
                Type type = Type.GetType(typeName);
                if (type.IsSubclassOf(typeof(ViewModelBase)) && 
                    type.GetInterfaces().Contains(typeof(ISerialize)))
                {
                    ISerialize child = (ISerialize)Activator.CreateInstance(type);
                    child.Deserialize(childXElement);
                    Children.Add((ViewModelBase)child);
                }
            }
        }

        #endregion ISerialize

    }
}
