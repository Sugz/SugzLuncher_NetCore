using Microsoft.WindowsAPICodePack.Shell;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;
using Shell32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Media.Imaging;
using Windows.Management.Deployment;

namespace SugzLuncher.Helpers
{
    internal static class FileInfoReader
    {
        
        internal static string[] GetInternetShortcutInfos(string file)
        {
            Shell shell = new Shell();
            Folder folder = shell.NameSpace(Path.GetDirectoryName(file));
            FolderItem folderItem = folder.ParseName(Path.GetFileName(file));
            if (folderItem != null)
            {
                ShellLinkObject link = (ShellLinkObject)folderItem.GetLink;
                link.GetIconLocation(out string iconPath);

                // the iconPath is an escaped string in prefix with "file:\\\", so using an Uri allow to get the real local path
                if (Uri.IsWellFormedUriString(iconPath, UriKind.RelativeOrAbsolute))
                {
                    Uri uri = new Uri(iconPath);
                    iconPath = uri.LocalPath;
                }
                
                // might be indirect string
                else if (iconPath.StartsWith("@{") && iconPath.EndsWith("}"))
                {
                    iconPath = ExtractNormalPath(iconPath);
                }

                return new []{ link.Path, Path.GetFileNameWithoutExtension(file), iconPath };
            }

            return null;
        }


        internal static Tuple<string, string, BitmapSource> GetWin10Shortcut(string file)
        {
            string pathOnly = System.IO.Path.GetDirectoryName(file);
            string filenameOnly = System.IO.Path.GetFileName(file);

            Shell shell = new Shell();
            Folder folder = shell.NameSpace(pathOnly);
            FolderItem folderItem = folder.ParseName(filenameOnly);
            if (folderItem != null)
            {
                ShellLinkObject link = (Shell32.ShellLinkObject)folderItem.GetLink;

                foreach (ShellObject app in InstalledApps.ToIEnumerable())
                {
                    if (app.Name == link.Target.Name)
                        return new Tuple<string, string, BitmapSource>(app.ParsingName, app.Name, app.Thumbnail.ExtraLargeBitmapSource);
                }
            }

            return null;
        }


        internal static string[] GetShortcutInfos(string file)
        {
            string targetPath, name;
            IWshRuntimeLibrary.WshShell shell = new IWshRuntimeLibrary.WshShell();
            try
            {
                IWshRuntimeLibrary.IWshShortcut shortcut = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(file);

                targetPath = ResolveMsiShortcut(file);
                if (targetPath == null)
                    targetPath = shortcut.TargetPath;
                name = Path.GetFileNameWithoutExtension(shortcut.FullName);

                return new string[] { targetPath, name, GetIconPath(shortcut.IconLocation) };
            }
            catch (COMException)
            {
                // A COMException is thrown if the file is not a valid shortcut (.lnk) file 
                return null;
            }
        }


        private static string ResolveMsiShortcut(string file)
        {
            StringBuilder product = new StringBuilder(NativeMethods.MaxGuidLength + 1);
            StringBuilder feature = new StringBuilder(NativeMethods.MaxFeatureLength + 1);
            StringBuilder component = new StringBuilder(NativeMethods.MaxGuidLength + 1);

            NativeMethods.MsiGetShortcutTarget(file, product, feature, component);

            int pathLength = NativeMethods.MaxPathLength;
            StringBuilder path = new StringBuilder(pathLength);

            NativeMethods.InstallState installState = NativeMethods.MsiGetComponentPath(product.ToString(), component.ToString(), path, ref pathLength);
            if (installState == NativeMethods.InstallState.Local)
            {
                return path.ToString();
            }
            else
            {
                return null;
            }
        }


        /// <summary>
        /// Check if the given path for the icon is valid then get it, otherwise try to get it from the .exe
        /// </summary>
        /// <param name="icon"></param>
        private static string GetIconPath(string iconPath)
        {
            // shortcuts add the icon index ("*,0") to the icon path
            iconPath = iconPath.EndsWith(",0") ? iconPath[..^2] : iconPath;
            if (System.IO.File.Exists(iconPath))
                return iconPath;
            return null;
        }


        class NativeMethods
        {
            [DllImport("msi.dll", CharSet = CharSet.Unicode)]
            public static extern uint MsiGetShortcutTarget(string targetFile, StringBuilder productCode, StringBuilder featureID, StringBuilder componentCode);

            [DllImport("msi.dll", CharSet = CharSet.Unicode)]
            public static extern InstallState MsiGetComponentPath(string productCode, string componentCode, StringBuilder componentPath, ref int componentPathBufferSize);

            [DllImport("shlwapi.dll", BestFitMapping = false, CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = false, ThrowOnUnmappableChar = true)]
            public static extern int SHLoadIndirectString(string pszSource, StringBuilder pszOutBuf, int cchOutBuf, IntPtr ppvReserved);

            public const int MaxFeatureLength = 38;
            public const int MaxGuidLength = 38;
            public const int MaxPathLength = 1024;

            public enum InstallState
            {
                NotUsed = -7,
                BadConfig = -6,
                Incomplete = -5,
                SourceAbsent = -4,
                MoreData = -3,
                InvalidArg = -2,
                Unknown = -1,
                Broken = 0,
                Advertised = 1,
                Removed = 1,
                Absent = 2,
                Local = 3,
                Source = 4,
                Default = 5
            }
        }





        

        private static string ExtractNormalPath(string indirectString)
        {
            StringBuilder outBuff = new StringBuilder(1024);
            int result = NativeMethods.SHLoadIndirectString(indirectString, outBuff, outBuff.Capacity, IntPtr.Zero);

            return outBuff.ToString();
        }
    }
}
