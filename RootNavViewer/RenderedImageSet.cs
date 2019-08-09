using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RootNav.Viewer
{
    public class RenderedImageSet
    {
        public ImageSource Background { get; }
        public ImageSource Segmentation { get; }
        public ImageSource Architecture { get; }
        public ImageSource FeatureImage { get; }

        public RenderedImageSet(ImageSource background, ImageSource segmentation, ImageSource architecture, ImageSource featureImage)
        {
            Background = background;
            Segmentation = segmentation;
            Architecture = architecture;
            FeatureImage = featureImage;
        }
    }
}
