using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using SugzLuncher.Helpers;
using SugzLuncher.Interfaces;
using SugzLuncher.Messages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Xml.Linq;

namespace SugzLuncher.ViewModels
{
    public class MainViewModel : BaseLuncherViewModel, ISerialize
    {

        #region Fields

        private SettingsManager _SettingsManager;
        private Point _WindowLocation = new Point(0, 0);

        #endregion Fields


        public Point WindowLocation 
        {
            get => _WindowLocation;
            set => Set(ref _WindowLocation, value);
        }



        public MainViewModel()
        {
            LoadSettings();
            MessengerInstance.Register<HasUpdatedMessage>(this, x => SaveSettings());
        }


        #region Settings

        private void LoadSettings()
        {
            if (_SettingsManager is null)
                _SettingsManager = SimpleIoc.Default.GetInstance<SettingsManager>();

            if (_SettingsManager.Load() is XElement xElement)
                Deserialize(xElement);
            else
            {
                Children.Clear();
                _SettingsManager.Lunchers.ForEach(x => Children.Add(x));
                WindowLocation = _SettingsManager.WindowLocation;
                ShowChildrenNames = _SettingsManager.ShowNames;
                ChildrenIconSize = _SettingsManager.IconsSize;
            }
            
        }


        private void SaveSettings()
        {
            _SettingsManager.Save(Serialize());
        }


        #endregion Settings

        
        #region Serialize

        public XElement Serialize()
        {
            XElement xElement = new XElement(GetType().ToString(),
                new XAttribute("WindowLeft", WindowLocation.X),
                new XAttribute("WindowTop", WindowLocation.Y),
                new XAttribute("ShowChildrenNames", ShowChildrenNames),
                new XAttribute("ChildrenIconSize", ChildrenIconSize));

            foreach (ISerialize vm in Children)
                xElement.Add(vm.Serialize());

            return xElement;
        }

        public void Deserialize(XElement xElement)
        {
            WindowLocation = new Point {
                X = double.Parse(xElement.Attribute("WindowLeft").Value),
                Y = double.Parse(xElement.Attribute("WindowTop").Value)
            };

            ShowChildrenNames = Convert.ToBoolean(xElement.Attribute("ShowChildrenNames").Value);
            ChildrenIconSize = (IconSize)Enum.Parse(typeof(IconSize), xElement.Attribute("ChildrenIconSize").Value);

            DeserializeChildren(xElement);
        }

        #endregion Serialize

    }
}
