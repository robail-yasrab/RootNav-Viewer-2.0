using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;


namespace RootNav.Data.IO
{
    public class ImageCollection : Dictionary<string, ImageDescriptor>
    {
        public static string COLOR_MASK = "Color_Mask";
        public static string SOURCE = "source";

        public BitmapImage Source
        {
            get
            {
                return ContainsKey(SOURCE) ? this[SOURCE].Image : null;
            }
        }

        public BitmapImage ColourMask
        {
            get
            {
                return ContainsKey(COLOR_MASK) ? this[COLOR_MASK].Image : null;
            }
        }

        public IEnumerable<BitmapImage> ChannelMasks
        {
            get
            {
                return from kvp in this orderby kvp.Key where kvp.Value.Usage == ImageType.Channel && kvp.Key != COLOR_MASK select kvp.Value.Image;
            }
        }

        public bool HasSegmentation
        {
            get
            {
                return this.Count(kvp => kvp.Value.Usage == ImageType.Channel && kvp.Key != COLOR_MASK) > 0;
            }
        }

        public bool HasColour
        {
            get
            {
                return ContainsKey(COLOR_MASK);
            }
        }

    }
}
