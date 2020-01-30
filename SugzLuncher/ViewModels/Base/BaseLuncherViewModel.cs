using SugzLuncher.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;

namespace SugzLuncher.ViewModels
{
    public abstract class BaseLuncherViewModel : BaseViewModel
    {

        #region Fields

        private bool _ShowChildrenNames = true;
        private IconSize _ChildrenIconSize = IconSize.Middle;

        #endregion Fields



        #region Properties


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



        #region Overrides

        internal override void SetChild(BaseViewModel child)
        {
            ((LuncherViewModel)child).SetIcon();
        }

        #endregion Overrides



        #region IDropTarget

        public override void Drop(IDataObject data, int? index = null)
        {
            foreach (string file in ((DataObject)data).GetFileDropList())
            {
                // Desktop Shortcut
                string[] infos = new string[3];
                if (file.ToLower().EndsWith(".lnk"))
                {
                    infos = FileInfoReader.GetShortcutInfos(file);

                    // In case user drop a win10 app
                    //TODO: find how i created a url link for the new control panel, as it contain the correct indirect icon location
                    if (string.IsNullOrEmpty(infos[0]))
                    {
                        AddLuncher(new LuncherViewModel(FileInfoReader.GetWin10Shortcut(file)), index);
                        return;
                    }
                }

                if (file.ToLower().EndsWith(".url"))
                    infos = FileInfoReader.GetInternetShortcutInfos(file);

                AddLuncher(new LuncherViewModel(infos[0], infos[1], infos[2]), index);
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

    }
}
