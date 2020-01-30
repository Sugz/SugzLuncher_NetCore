using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Windows.UI.ViewManagement;

namespace SugzLuncher.Helpers
{
    internal static class BitmapHelper
    {
        internal static Color Win10AccentColorRGB
        {
            get
            {
                UISettings uiSettings = new UISettings();
                Windows.UI.Color accentColor = uiSettings.GetColorValue(UIColorType.Accent);
                return Color.FromRgb(accentColor.R, accentColor.G, accentColor.B);
            }
        }

        internal static Color MicrosoftEdgeShortcutBackgroundColor => Color.FromRgb(0, 123, 217);

        internal static BitmapSource CreateBackground(int width, int height, Color color)
        {
            int stride = width / 8;
            byte[] pixels = new byte[height * stride];

            BitmapSource image = BitmapSource.Create(
                width,
                height,
                96,
                96,
                PixelFormats.Indexed1,
                new BitmapPalette(new List<Color> { color }),
                pixels,
                stride);

            return image;
        }

        internal static BitmapSource ReplaceTransparency(BitmapSource bitmap, Color color)
        {
            Rect rect = new Rect(0, 0, bitmap.PixelWidth, bitmap.PixelHeight);
            DrawingVisual visual = new DrawingVisual();
            DrawingContext context = visual.RenderOpen();

            context.DrawRectangle(new SolidColorBrush(color), null, rect);
            context.DrawImage(bitmap, rect);
            context.Close();

            RenderTargetBitmap render = new RenderTargetBitmap(bitmap.PixelWidth, bitmap.PixelHeight,
                96, 96, PixelFormats.Pbgra32);
            render.Render(visual);

            return render;
        }
    }


    /// <summary>
    /// Provides methods for checking whether a file can likely be opened as a BitmapImage, based upon its file extension
    /// </summary>
    public class BitmapSourceCheck : IDisposable
    {

        #region Fields

        private readonly string _BaseKeyPath;
        private readonly RegistryKey _BaseKey;
        private const string _WICDecoderCategory = "{7ED96837-96F0-4812-B211-F13C24117ED3}";

        #endregion Fields


        #region Properties

        /// <summary>
        /// File extensions that are supported by decoders found elsewhere on the system
        /// </summary>
        public string[] CustomSupportedExtensions { get; private set; }

        /// <summary>
        /// File extensions that are supported natively by .NET
        /// </summary>
        public string[] NativeSupportedExtensions { get; private set; }

        /// <summary>
        /// File extensions that are supported both natively by NET, and by decoders found elsewhere on the system
        /// </summary>
        public string[] AllSupportedExtensions { get; private set; }

        #endregion Properties


        #region Constructor

        public BitmapSourceCheck()
        {
            _BaseKeyPath = Environment.Is64BitOperatingSystem && !Environment.Is64BitProcess ?
                "Wow6432Node\\CLSID" : "CLSID";
            _BaseKey = Registry.ClassesRoot.OpenSubKey(_BaseKeyPath, false);
            RecalculateExtensions();
        }

        #endregion Constructor


        #region Public Methods

        /// <summary>
        /// Check whether a file is likely to be supported by BitmapImage based upon its extension
        /// </summary>
        /// <param name="extension">File extension (with or without leading full stop), file name or file path</param>
        /// <returns>True if extension appears to contain a supported file extension, false if no suitable extension was found</returns>
        public bool IsExtensionSupported(string extension)
        {
            //prepare extension, should a full path be given
            if (extension.Contains("."))
                extension = extension.Substring(extension.LastIndexOf('.') + 1);

            extension = extension.ToUpper();
            extension = extension.Insert(0, ".");

            return AllSupportedExtensions.Contains(extension);
        }

        #endregion Public Methods


        #region Private Methods

        /// <summary>
        /// Re-calculate which extensions are available on this system. It's unlikely this ever needs to be called outside of the constructor.
        /// </summary>
        private void RecalculateExtensions()
        {
            CustomSupportedExtensions = GetSupportedExtensions().ToArray();
            NativeSupportedExtensions = new string[] { ".BMP", ".GIF", ".ICO", ".JPEG", ".PNG", ".TIFF", ".DDS", ".JPG", ".JXR", ".HDP", ".WDP" };

            string[] cse = CustomSupportedExtensions;
            string[] nse = NativeSupportedExtensions;
            string[] ase = new string[cse.Length + nse.Length];
            Array.Copy(nse, ase, nse.Length);
            Array.Copy(cse, 0, ase, nse.Length, cse.Length);
            AllSupportedExtensions = ase;
        }

        /// <summary>
        /// Represents information about a WIC decoder
        /// </summary>
        private struct DecoderInfo
        {
            public string FriendlyName;
            public string FileExtensions;
        }

        /// <summary>
        /// Gets a list of additionally registered WIC decoders
        /// </summary>
        /// <returns></returns>
        private IEnumerable<DecoderInfo> GetAdditionalDecoders()
        {
            var result = new List<DecoderInfo>();

            foreach (var codecKey in GetCodecKeys())
            {
                DecoderInfo decoderInfo = new DecoderInfo();
                decoderInfo.FriendlyName = Convert.ToString(codecKey.GetValue("FriendlyName", ""));
                decoderInfo.FileExtensions = Convert.ToString(codecKey.GetValue("FileExtensions", ""));
                result.Add(decoderInfo);
            }
            return result;
        }

        private List<string> GetSupportedExtensions()
        {
            var decoders = GetAdditionalDecoders();
            List<string> rtnlist = new List<string>();

            foreach (var decoder in decoders)
            {
                string[] extensions = decoder.FileExtensions.Split(',');
                foreach (var extension in extensions) rtnlist.Add(extension);
            }
            return rtnlist;
        }

        private IEnumerable<RegistryKey> GetCodecKeys()
        {
            var result = new List<RegistryKey>();

            if (_BaseKey != null)
            {
                var categoryKey = _BaseKey.OpenSubKey(_WICDecoderCategory + "\\instance", false);
                if (categoryKey != null)
                {
                    // Read the guids of the registered decoders
                    var codecGuids = categoryKey.GetSubKeyNames();

                    foreach (var codecGuid in GetCodecGuids())
                    {
                        // Read the properties of the single registered decoder
                        var codecKey = _BaseKey.OpenSubKey(codecGuid);
                        if (codecKey != null)
                        {
                            result.Add(codecKey);
                        }
                    }
                }
            }

            return result;
        }

        private string[] GetCodecGuids()
        {
            if (_BaseKey != null)
            {
                var categoryKey = _BaseKey.OpenSubKey(_WICDecoderCategory + "\\instance", false);
                if (categoryKey != null)
                {
                    // Read the guids of the registered decoders
                    return categoryKey.GetSubKeyNames();
                }
            }
            return null;
        }

        #endregion Private Methods


        #region Overrides

        public override string ToString()
        {
            string rtnstring = "";

            rtnstring += "\nNative support for the following extensions is available: ";
            foreach (var item in NativeSupportedExtensions)
            {
                rtnstring += item + ",";
            }
            if (NativeSupportedExtensions.Count() > 0) rtnstring = rtnstring.Remove(rtnstring.Length - 1);

            var decoders = GetAdditionalDecoders();
            if (decoders.Count() == 0) rtnstring += "\n\nNo custom decoders found.";
            else
            {
                rtnstring += "\n\nThese custom decoders were also found:";
                foreach (var decoder in decoders)
                {
                    rtnstring += "\n" + decoder.FriendlyName + ", supporting extensions " + decoder.FileExtensions;
                }
            }

            return rtnstring;
        }


        #endregion Overrides


        #region Dispose

        public void Dispose()
        {
            _BaseKey.Dispose();
        }

        #endregion Overrides
        
    }
}
