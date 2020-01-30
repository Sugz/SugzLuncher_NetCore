using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using SugzLuncher.Helpers;

namespace SugzLuncher.ViewModels
{
    public class ViewModelLocator
    {
        public ViewModelLocator()
        {
            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<SettingsManager>();
        }

        public MainViewModel Main => SimpleIoc.Default.GetInstance<MainViewModel>();

    }
}
