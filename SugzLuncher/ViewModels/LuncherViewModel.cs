using GalaSoft.MvvmLight.Command;
using SugzLuncher.Helpers;
using SugzLuncher.Contracts;
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
using System.Xml.Linq;
using System.Xml.Serialization;

namespace SugzLuncher.ViewModels
{
    public class LuncherViewModel : BaseLuncherViewModel, ISerialize
    {

        #region Fields

        private ImageSource _IconSource;
        private string _Name;
        private string _TargetPath;
        private bool _IsSelected;

        #endregion Fields



        #region Properties

        public bool IsWin10Shortcut { get; set; }

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
            //TODO: check if no problem when always having explorer.exe
            if (!string.IsNullOrEmpty(TargetPath))
                Process.Start("explorer.exe", TargetPath);

        }

        #endregion Commands



        #region Constructors

        public LuncherViewModel() { }

        internal LuncherViewModel(Constants.LunchersName name)
        {
            if (Constants.Lunchers.FirstOrDefault(x => x.Item1 == name) is var luncher)
            {
                Name = luncher.Item2;
                //IconPath = "pack://application:,,,/SugzLuncher;component/Resources/Icons/" + name + ".png";
                IconPath = "pack://application:,,,/SugzLuncher;component/Resources/Icons/" + luncher.Item3 + ".png";
                //IconSource = ResourceProvider.IconPng(luncher.Item3);
            }
        }

        internal LuncherViewModel(string targetPath, string name, string iconPath, int iconIndex = 0, bool isWin10Shortcut = false)
        {
            TargetPath = isWin10Shortcut ? @"shell:appsFolder\" + targetPath : targetPath;
            Name = name;
            IconIndex = iconIndex;
            IconPath = !string.IsNullOrEmpty(iconPath) ? iconPath : TargetPath;
            IsWin10Shortcut = isWin10Shortcut;
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
                //TODO: simplify the all thing by creating a String path variable and assign to IconPath or targetPath ?
                //if (!_IconPath.ToLower().EndsWith(".png"))
                if (IconPath.ToLower().EndsWith(".ico"))
                    IconSource = IconUtils.ExtractIcons(IconPath, (int)((BaseLuncherViewModel)Parent).ChildrenIconSize, IconIndex);

                else if (IsWin10Shortcut)
                {
                    Color color = Name == "Microsoft Edge" ?
                        BitmapHelper.MicrosoftEdgeShortcutBackgroundColor : BitmapHelper.Win10AccentColorRGB;

                    
                    IconSource = BitmapHelper.ReplaceTransparency(BitmapHelper.FromFile(IconPath), color);
                }

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



        #region Serialize

        public XElement Serialize()
        {
            XElement xElement = new XElement(GetType().ToString(),
                new XAttribute("Name", Name),
                new XAttribute("TargetPath", TargetPath ?? string.Empty),
                new XAttribute("IconPath", IconPath ?? string.Empty),
                new XAttribute("IconIndex", IconIndex),
                new XAttribute("IsWin10Shortcut", IsWin10Shortcut),
                new XAttribute("ShowChildrenNames", ShowChildrenNames),
                new XAttribute("ChildrenIconSize", ChildrenIconSize));

            foreach (ISerialize vm in Children)
                xElement.Add(vm.Serialize());

            return xElement;
        }

        public void Deserialize(XElement xElement)
        {
            TargetPath = xElement.Attribute("TargetPath").Value;
            Name = xElement.Attribute("Name").Value;
            IconPath = xElement.Attribute("IconPath").Value;
            IconIndex = int.Parse(xElement.Attribute("IconIndex").Value);
            IsWin10Shortcut = Convert.ToBoolean(xElement.Attribute("IsWin10Shortcut").Value);
            ShowChildrenNames = Convert.ToBoolean(xElement.Attribute("ShowChildrenNames").Value);
            ChildrenIconSize = (IconSize)Enum.Parse(typeof(IconSize), xElement.Attribute("ChildrenIconSize").Value);

            DeserializeChildren(xElement);
        }

        #endregion Serialize

    }
}
