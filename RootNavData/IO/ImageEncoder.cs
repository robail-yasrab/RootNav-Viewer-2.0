using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;

namespace RootNav.Data
{
    /// <summary>
    /// Static class responsible for encoding images into files using various encoding classes.
    /// </summary>
    public static class ImageEncoder
    {
        /// <summary>
        /// Enum specifying which encoder to use.
        /// </summary>
        public enum EncodingType { PNG, BMP, JPG };

        /// <summary>
        /// Save an image to a path using a specified encoder.
        /// </summary>
        /// <param name="Path">The path of the image to be saved.</param>
        /// <param name="Buffer">The image buffer to be saved.</param>
        /// <param name="Format">The encoding format to use.</param>
        /// <returns>A bool specifying whether the operation has been successful.</returns>
        public static bool SaveImage(String Path, WriteableBitmap Buffer, EncodingType Format)
        {
            try
            {
                BitmapEncoder Encoder = null;
                switch (Format)
                {
                    case EncodingType.PNG:
                        Encoder = new PngBitmapEncoder();
                        break;
                    case EncodingType.BMP:
                        Encoder = new BmpBitmapEncoder();
                        break;
                    case EncodingType.JPG:
                        Encoder = new JpegBitmapEncoder();
                        break;
                    default:
                        Encoder = null;
                        break;
                }

                if (Path == null || Encoder == null || Buffer == null)
                {
                    return false;
                }

                FileStream FS = File.Open(Path, FileMode.OpenOrCreate);
                Encoder.Frames.Add(BitmapFrame.Create(Buffer));
                Encoder.Save(FS);
                FS.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
