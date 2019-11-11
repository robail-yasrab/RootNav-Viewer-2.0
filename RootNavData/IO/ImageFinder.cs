using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using System.Text.RegularExpressions;


namespace RootNav.Data.IO
{
    public struct ImageDescriptor
    {
        public string Path { get; }
        public ImageType Usage { get; }
        public BitmapImage Image { get; }

        public ImageDescriptor (string path, ImageType usage, BitmapImage img)
        {
            Path = path;
            Usage = usage;
            Image = img;
        }

    }

    public enum ImageType
    {
        Source,
        Channel,
        Other
    }

    public static class ImageFinder
    {
        static HashSet<string> ValidImageExtensions = new HashSet<string> { ".jpg", ".jpeg", ".tif", ".tiff", ".png", ".bmp" };
        public static ImageCollection ImageSearch(string path, string tag)
        {
            ImageCollection output = new ImageCollection();

            string searchString = tag + "*";
            string[] files = Directory.GetFiles(path, searchString, SearchOption.TopDirectoryOnly);

            IEnumerable<string> imageFiles = from file in files where ValidImageExtensions.Contains(Path.GetExtension(file).ToLower()) select file;
            foreach (string file in imageFiles)
            {
                // Extension
                string ext = Path.GetExtension(file);

                string search = ".*" + tag + @"_(?<key>[\w]+)\.";
                var result = Regex.Match(file, search);
                if (!result.Success)
                {
                    output.Add("source", new ImageDescriptor(file, ImageType.Source, LoadAsBitmap(file)));
                }
                else
                {
                    string channelSearch = "C[0-9]+";
                    var channelResult = Regex.Match(result.Groups["key"].Value, channelSearch);
                    if (!channelResult.Success)
                    {
                        output.Add(result.Groups["key"].Value, new ImageDescriptor(file, ImageType.Other, LoadAsBitmap(file)));
                    }
                    else
                    {
                        output.Add(result.Groups["key"].Value, new ImageDescriptor(file, ImageType.Channel, LoadAsBitmap(file)));
                    }
                }

            }

            return output;
        }

        private static BitmapImage LoadAsBitmap(string path)
        {
            BitmapImage img = null;
            using (var fs = File.OpenRead(path))
            {
                img = new BitmapImage();
                img.BeginInit();
                img.StreamSource = fs;
                img.CacheOption = BitmapCacheOption.OnLoad;
                img.EndInit();
                img.Freeze();
            }
            return img;
        }

}
}
