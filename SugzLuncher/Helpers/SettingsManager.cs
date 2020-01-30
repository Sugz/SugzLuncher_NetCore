using SugzLuncher.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Xml.Linq;

namespace SugzLuncher.Helpers
{
    internal class SettingsManager
    {

        #region Fields

        private bool _IsLoading;

        #endregion Fields



        #region Properties

        public Point WindowLocation { get; set; }
        public ObservableCollection<BaseViewModel> Lunchers { get; set; } = new ObservableCollection<BaseViewModel>();
        public bool ShowNames { get; set; }
        public bool ShowIcons { get; set; }
        public IconSize IconsSize { get; set; }
        public string FileFolder { get; set; }

        #endregion Properties


        #region Constructor


        public SettingsManager()
        {
            Debug.WriteLine("Create SettingsManager");
            Load();
        }


        #endregion Constructor



        #region Load

        private void Load()
        {
            _IsLoading = true;

            if (!Directory.Exists(Constants.SettingsFolder))
                Directory.CreateDirectory(Constants.SettingsFolder);

            LoadInitialConfiguration();

            _IsLoading = false;
        }


        internal void LoadInitialConfiguration()
        {
            WindowLocation = new Point(0, 0);
            ShowNames = true;
            ShowIcons = true;
            IconsSize = IconSize.Middle;

            LuncherViewModel work = new LuncherViewModel(Constants.LunchersName.Work);
            LuncherViewModel game = new LuncherViewModel(Constants.LunchersName.Game);
            LuncherViewModel web = new LuncherViewModel(Constants.LunchersName.Web);
            LuncherViewModel tools = new LuncherViewModel(Constants.LunchersName.Tools);
            LuncherViewModel files = new LuncherViewModel(Constants.LunchersName.Files);
            LuncherViewModel sound = new LuncherViewModel(Constants.LunchersName.Sound);

            Lunchers.Add(work);
            Lunchers.Add(game);
            Lunchers.Add(web);
            Lunchers.Add(tools);
            Lunchers.Add(files);
            Lunchers.Add(sound);
        }


        #endregion Load



        #region Save


        private void SaveLuncher(LuncherViewModel luncher, XElement xParent)
        {
            XElement xLuncher = new XElement("Luncher",
                new XAttribute("Name", luncher.Name),
                new XAttribute("TargetPath", luncher.TargetPath != null ? luncher.TargetPath : string.Empty),
                new XAttribute("IconPath", luncher.IconPath != null ? luncher.IconPath : string.Empty),
                new XAttribute("IconIndex", luncher.IconIndex));

            xParent.Add(xLuncher);
            //luncher.Children.ForEach(x => SaveLuncher(x, xLuncher));
        }

        internal void Save()
        {
            if (_IsLoading)
                return;


            XDocument document = new XDocument();
            XElement xSettings = new XElement("Settings");
            document.Add(xSettings);

            // Save the Window location
            XElement xWindowLocation = new XElement("WindowLocation",
                new XAttribute("Left", WindowLocation.X),
                new XAttribute("Top", WindowLocation.Y));
            xSettings.Add(xWindowLocation);

            // Save all the lunchers
            XElement xLunchers = new XElement("Lunchers");
            xSettings.Add(xLunchers);
            
            foreach(BaseViewModel baseViewModel in Lunchers)
            {
                
            }

            //Lunchers.ForEach(x => SaveLuncher(x, xLunchers));


        }


        #endregion Save

    }
}
