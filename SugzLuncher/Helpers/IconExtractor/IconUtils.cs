using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TsudaKageyu;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Drawing = System.Drawing;

namespace SugzLuncher.Helpers
{
    internal static class IconUtils
    {

        internal static ImageSource FromIcon(Drawing.Icon icon, int size)
        {
            Drawing.Icon[] splitIcons = IconUtil.Split(icon);
            return SplitIcon(splitIcons, size);
        }

        internal static ImageSource ExtractIcons(string path, int size, int index = 0)
        {
            Drawing.Icon[] splitIcons;
            try
            {
                Drawing.Icon icon;
                // Create an icon either from a .ico or from a .exe using IconExtractor
                if (Path.GetExtension(path).ToLower() == ".ico")
                    icon = new Drawing.Icon(path);
                else
                {
                    IconExtractor extractor = new IconExtractor(path);
                    icon = extractor.GetIcon(index);
                }

                splitIcons = IconUtil.Split(icon);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }

            return SplitIcon(splitIcons, size);

        }


        private static ImageSource SplitIcon(Drawing.Icon[] splitIcons, int size)
        {
            if (splitIcons != null)
            {
                // Collect the sizes
                int[] heights = new int[splitIcons.Length];
                for (int i = 0; i < splitIcons.Length; i++)
                    heights[i] = splitIcons[i].Height;

                // Find the closest size available (TODO: try the find the one bigger first)
                int closestHeight = heights.Aggregate((x, y) => Math.Abs(x - size) < Math.Abs(y - size) ? x : y);

                // Find the best layer that math the closest size and with the higher bit count
                Drawing.Icon item = splitIcons.Where(x => x.Height == closestHeight).OrderByDescending(i => IconUtil.GetBitCount(i)).FirstOrDefault();
                if (item != null)
                {
                    using Stream str = new MemoryStream();
                    item.Save(str);
                    str.Seek(0, SeekOrigin.Begin);
                    return new IconBitmapDecoder(str, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad).Frames[0];
                }
            }

            return null;
        }

    }
}
