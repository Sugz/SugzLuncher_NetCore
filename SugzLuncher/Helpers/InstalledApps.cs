using Microsoft.WindowsAPICodePack.Shell;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SugzLuncher.Helpers
{
    internal class InstalledApps
    {

        internal static IEnumerable<ShellObject> ToIEnumerable()
        {
            // GUID taken from https://docs.microsoft.com/en-us/windows/win32/shell/knownfolderid
            var FODLERID_AppsFolder = new Guid("{1e87508d-89c2-42f0-8a7e-645a0f50ca58}");
            ShellObject appsFolder = (ShellObject)KnownFolderHelper.FromKnownFolderId(FODLERID_AppsFolder);
            foreach (ShellObject app in (IKnownFolder)appsFolder)
            {
                
                yield return app;
            }
                
        }

        /*
        
        string name = app.Name;
        string appUserModelID = app.ParsingName; // or app.Properties.System.AppUserModel.ID

        To start the app:
        System.Diagnostics.Process.Start("explorer.exe", @" shell:appsFolder\" + appUserModelID);
        */
    }
}
