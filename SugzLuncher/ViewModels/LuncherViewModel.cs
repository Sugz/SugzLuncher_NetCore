using GalaSoft.MvvmLight.Command;
using SugzLuncher.Helpers;
using SugzLuncher.Resources;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SugzLuncher.ViewModels
{
    public class LuncherViewModel : BaseLuncherViewModel
    {

        #region Fields

        private bool _IsWin10Shortcut = false;
        private System.Drawing.Icon _Icon;
        private ImageSource _IconSource;
        //private string _IconPath;
        //private int _IconIndex;
        private string _Name;
        private string _TargetPath;
        private bool _IsSelected;

        #endregion Fields



        #region Properties


        /// <summary>
        /// Binding for the displayed icon
        /// </summary>
        public ImageSource IconSource
        {
            get => _IconSource;
            set => Set(ref _IconSource, value);
        }

        /// <summary>
        /// The path for the custom icon
        /// </summary>
        public string IconPath { get; set; }

        /// <summary>
        /// The index of the icon when getting it from a .dll
        /// </summary>
        public int IconIndex { get; set; }

        /// <summary>
        /// The luncher's name
        /// </summary>
        public string Name
        {
            get => _Name;
            set => Set(ref _Name, value);
        }

        /// <summary>
        /// The executable path
        /// </summary>
        public string TargetPath
        {
            get => _TargetPath;
            set => Set(ref _TargetPath, value);
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsSelected
        {
            //get => IsEditing ? true : _IsSelected;
            get => _IsSelected;
            set
            {
                Set(ref _IsSelected, value);
                //Children.ForEach(x => x.IsSelected = false);
            }
        }


        #endregion Properties


        #region Commands


        private RelayCommand _ExecuteAppCommand;
        public ICommand ExecuteAppCommand => _ExecuteAppCommand ?? (_ExecuteAppCommand = new RelayCommand(ExecuteApp));
        private void ExecuteApp()
        {
            if (!string.IsNullOrEmpty(TargetPath))
            {
                //TODO: check if no problem when always having explorer.exe
                Process.Start("explorer.exe", TargetPath);
                
                //if (_IsWin10Shortcut)
                //    Process.Start("explorer.exe", TargetPath);
                //else
                //    Process.Start(TargetPath);
            }
                
        }

        #endregion Commands



        #region Constructors

        internal LuncherViewModel() { }

        internal LuncherViewModel(Constants.LunchersName name)
        {
            if (Constants.Lunchers.FirstOrDefault(x => x.Item1 == name) is var luncher)
            {
                Name = luncher.Item2;
                IconSource = ResourceProvider.IconPng(luncher.Item3);
            }
        }

        internal LuncherViewModel(string targetPath, string name, string iconPath, int iconIndex = 0)
        {
            TargetPath = targetPath;
            Name = name;
            IconIndex = iconIndex;
            IconPath = !string.IsNullOrEmpty(iconPath) ? iconPath : TargetPath;
        }


        internal LuncherViewModel(Tuple<string, string, BitmapSource> infos)
        {
            _IsWin10Shortcut = true;

            TargetPath = @"shell:appsFolder\" + infos.Item1;
            Name = infos.Item2;

            Color color = Name == "Microsoft Edge" ? 
                BitmapHelper.MicrosoftEdgeShortcutBackgroundColor : BitmapHelper.Win10AccentColorRGB;

            IconSource = BitmapHelper.ReplaceTransparency(infos.Item3, color);
        }


        #endregion Constructors



        #region Methods

        /// <summary>
        /// Call when parent collection changed or when parent iconSize changed
        /// </summary>
        public void SetIcon()
        {
            if (IconSource == null && IconPath != null)
            {
                //TODO: check everything still works (can't be for .dll files and won't check for best icon when size changes since IconSource will already be defined)
                //TODO: simplify the all thing path creating path var and assign to IconPath or targetPath ?
                //if (!_IconPath.ToLower().EndsWith(".png"))
                if (IconPath.ToLower().EndsWith(".ico"))
                    IconSource = IconUtils.ExtractIcons(IconPath, (int)((BaseLuncherViewModel)Parent).ChildrenIconSize, IconIndex);

                // check if file if a valid bitmapsource file
                else
                {
                    BitmapSourceCheck bic = new BitmapSourceCheck();
                    if (bic.IsExtensionSupported(IconPath))
                        IconSource = new BitmapImage(new Uri(IconPath));
                }
            }

            // retest for IconSource == null here as there might be an issue with the previous method
            if (IconSource == null && TargetPath != null)
            {
                IconSource = IconUtils.ExtractIcons(TargetPath, (int)((BaseLuncherViewModel)Parent).ChildrenIconSize);
            }
        }

        #endregion Methods



        

    }
}
