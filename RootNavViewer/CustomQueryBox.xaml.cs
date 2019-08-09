using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace RootNav.Viewer
{
    /// <summary>
    /// Interaction logic for CustomQueryBox.xaml
    /// </summary>
    public partial class CustomQueryBox : Window
    {
        private CustomQueryBox()
        {
            InitializeComponent();
            
        }

        private new void ShowDialog()
        {
            base.ShowDialog();
        }

        private new void Show()
        {
            base.Show();
        }

        private int optionSelected = -1;

        public static int Show(String caption, String message, String option1, String option2)
        {
            CustomQueryBox cqb = new CustomQueryBox();
            cqb.Option1.Content = option1;
            cqb.Option2.Content = option2;
            cqb.Title = caption;
            cqb.contentBlock.Text = message;
            cqb.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            cqb.ShowDialog();
            cqb.Close();
            return cqb.optionSelected;
        }

        private void Option_Click(object sender, RoutedEventArgs e)
        {
            if (sender as Button == this.Option1)
            {
                optionSelected = 1;
            }
            else
            {
                optionSelected = 2;
            }
            this.Hide();
        }
    }
}
