using GalaSoft.MvvmLight.Ioc;
using SugzLuncher.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;

namespace SugzLuncher.ViewModels
{
    public class MainViewModel : BaseLuncherViewModel
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
        }


        #region Settings


        private void LoadSettings()
        {
            if (_SettingsManager is null)
                _SettingsManager = SimpleIoc.Default.GetInstance<SettingsManager>();

            Children.Clear();
            _SettingsManager.Lunchers.ForEach(x => Children.Add(x));
            WindowLocation = _SettingsManager.WindowLocation;
            ShowChildrenNames = _SettingsManager.ShowNames;
            ChildrenIconSize = _SettingsManager.IconsSize;
        }


        private void SaveSettings()
        {
            _SettingsManager.Lunchers = Children;
            _SettingsManager.ShowNames = ShowChildrenNames;
            _SettingsManager.IconsSize = ChildrenIconSize;
            _SettingsManager.Save();
        }


        #endregion Settings


        

    }
}
