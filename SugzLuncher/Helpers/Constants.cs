using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SugzLuncher.Helpers
{
    public enum IconSize
    {
        [Description("32")]
        Small = 32,

        [Description("48")]
        Middle = 48,

        [Description("64")]
        Large = 64,
    }


    internal class Constants
    {

        #region Predefined Lunchers

        internal enum LunchersName
        {
            Bin,
            Files,
            Game,
            Sound,
            Tools,
            Web,
            Work,
        }

        internal static readonly Tuple<LunchersName, string, string>[] Lunchers = new Tuple<LunchersName, string, string>[]
        {
            new Tuple<LunchersName, string, string>(LunchersName.Bin, "Recycle Bin", "bin_empty"),
            new Tuple<LunchersName, string, string>(LunchersName.Files, "Files", "files"),
            new Tuple<LunchersName, string, string>(LunchersName.Game, "Game", "game"),
            new Tuple<LunchersName, string, string>(LunchersName.Sound, "Sound", "sound"),
            new Tuple<LunchersName, string, string>(LunchersName.Tools, "Tool", "tools"),
            new Tuple<LunchersName, string, string>(LunchersName.Web, "Web", "web"),
            new Tuple<LunchersName, string, string>(LunchersName.Work, "Work", "work"),
        };

        #endregion Predefined Lunchers


        #region Product Infos

        internal static string Company => Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyCompanyAttribute>().Company;

        internal static string Product => Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyProductAttribute>().Product;

        internal static string Version => Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;

        internal static string SettingsFolder => $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\{Product}\\{Version}";

        internal static string SettingsFile => $"{SettingsFolder}\\SugzLuncherSettings.xml";

        #endregion Product Infos

    }
}
