using GalaSoft.MvvmLight;
using SugzLuncher.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml;
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
        public ObservableCollection<ViewModelBase> Lunchers { get; set; } = new ObservableCollection<ViewModelBase>();
        public bool ShowNames { get; set; }
        public bool ShowIcons { get; set; }
        public IconSize IconsSize { get; set; }
        public string FileFolder { get; set; }

        #endregion Properties


        #region Constructor


        public SettingsManager()
        {
            //Debug.WriteLine("Create SettingsManager");
        }


        #endregion Constructor



        #region Load

        public XElement Load()
        {
            XElement xElement = null;

            if (!Directory.Exists(Constants.SettingsFolder))
                Directory.CreateDirectory(Constants.SettingsFolder);

            if (!File.Exists(Constants.UserSettingsFile))
            {
                LoadInitialConfiguration();
            }
            else
            {
                _IsLoading = true;
                XDocument document = XDocument.Load(Constants.UserSettingsFile);
                //XElement root = document.Root;
                //XElement main = root.Elements().First();

                xElement = document.Root.Elements().First();
            }

            _IsLoading = false;
            return xElement;
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



        internal void Save(XElement xElement)
        {
            if (_IsLoading)
                return;

            XDocument document = new XDocument();
            XElement xSettings = new XElement($"SugzLuncher_{Constants.Version}_UserSettings");
            document.Add(xSettings);

            xSettings.Add(xElement);

            // Save the formated document
            XmlWriterSettings settings = new XmlWriterSettings {
                Indent = true,
                IndentChars = "\t",
                NewLineOnAttributes = true
            };

            XmlWriter writer = XmlWriter.Create(Constants.UserSettingsFile, settings);
            document.Save(writer);
            writer.Flush();
            writer.Close();

        }


        #endregion Save

    }
}
