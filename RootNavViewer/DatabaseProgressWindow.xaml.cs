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
    /// Interaction logic for DatabaseIntegrityWindow.xaml
    /// </summary>
    public partial class DatabaseProgressWindow : Window
    {
        public string Task { get; set; }

        private bool canCancel = false;

        public bool CanCancel
        {
            get { return canCancel; }
            set
            {
                canCancel = value;
                this.cancelButton.Visibility = CanCancel ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            }
        }
        

        public DatabaseProgressWindow()
        {
            InitializeComponent();
        }

        public void SetProgress(int plants, int totalPlants)
        {
            progressLabel.Content = Task + ": " + plants.ToString() + " of " + totalPlants.ToString();
            progressBar.Value = (plants / (double)totalPlants) * 100;
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
