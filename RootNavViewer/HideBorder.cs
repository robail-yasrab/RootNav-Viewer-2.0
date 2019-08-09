using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Reflection;
using System.Windows.Controls;

namespace RootNav.Viewer
{
    class HideBorder : Border
    {
        public static readonly DependencyProperty HideProperty =
          DependencyProperty.Register("Hide", typeof(bool), typeof(HideBorder), new PropertyMetadata(false));

        public bool Hide
        {
            get
            {
                return (bool)GetValue(HideProperty);
            }
            set
            {
                SetValue(HideProperty, value);
            }
        }
    }
}
