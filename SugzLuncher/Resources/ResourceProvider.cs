using SugzLuncher.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media.Imaging;
using static SugzLuncher.Helpers.Constants;

namespace SugzLuncher.Resources
{
    internal static class ResourceProvider
    {

        internal static BitmapImage IconPng(string name) => 
            new BitmapImage(new Uri("pack://application:,,,/SugzLuncher;component/Resources/Icons/" + name + ".png"));


    }
}
