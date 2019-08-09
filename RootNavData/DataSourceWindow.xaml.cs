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
using RootNav.Data;
using System.ComponentModel;
using RootNav.Data.IO;

namespace RootNav.Data
{
    /// <summary>
    /// Interaction logic for DataSourceWindow.xaml
    /// </summary>
    public partial class DataSourceWindow : Window
    {
        public ConnectionParams ConnectionInfo { get; set; }

        public DataSourceWindow()
        {
            this.InheritanceBehavior = InheritanceBehavior.SkipAllNow;

            InitializeComponent();

            // Attempt to read encrypted storage
            string xmlData = EncryptedStorage.ReadEncryptedString("C_DATA");

            if (xmlData != null && xmlData != "")
            {
                ConnectionInfo = ConnectionParams.FromXML(xmlData);
                
                if (ConnectionInfo == null)
                {
                    return;
                }

                // Populate text boxes
                directoryBox.Text = ConnectionInfo.Directory;
            }
            else
            {
                ConnectionInfo = null;
            }
        }

        System.Windows.Forms.FolderBrowserDialog fbdl = new System.Windows.Forms.FolderBrowserDialog() { RootFolder = Environment.SpecialFolder.Desktop };
        private void findDirectoryButton_Click(object sender, RoutedEventArgs e)
        {
            if (fbdl.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.directoryBox.Text = fbdl.SelectedPath;
            }

        }

        private void acceptButton_Click(object sender, RoutedEventArgs e)
        {
            // Update connection params
            this.ConnectionInfo = new ConnectionParams() { Directory = directoryBox.Text, Username = "" };
            this.ConnectionInfo.Source = ConnectionSource.RSMLDirectory;

            // Save connection params
            EncryptedStorage.SaveEncryptedString("C_DATA", this.ConnectionInfo.ToXML());

            this.DialogResult = true;
            this.Close();
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Do not save connection data
            this.DialogResult = false;
            this.Close();
        }

        private void testButton_Click(object sender, RoutedEventArgs e)
        {
            // Update connection params
            this.ConnectionInfo = new ConnectionParams() { Directory = directoryBox.Text, Username = "" };
            this.ConnectionInfo.Source = ConnectionSource.RSMLDirectory;
        
            // File connection, directory must exist
            bool exists = System.IO.Directory.Exists(this.ConnectionInfo.Directory);
            if (!exists)
            {
                this.failedConnectionLabel.Content = "Directory Not Found"; 
                this.failedConnectionBorder.Visibility = System.Windows.Visibility.Visible;
                this.notConnectedBorder.Visibility = System.Windows.Visibility.Hidden;
                this.connectingBorder.Visibility = System.Windows.Visibility.Hidden;
                this.connectedBorder.Visibility = System.Windows.Visibility.Hidden;
            }
            else
            {
                HashSet<string> validExtensions = new HashSet<string>() { ".rsml", ".RSML" };
                int count = 0;
                foreach (var file in System.IO.Directory.EnumerateFiles(this.ConnectionInfo.Directory))
                {
                    if (validExtensions.Contains(System.IO.Path.GetExtension(file)))
                    {
                        count++;
                    }
                }

                this.connectedLabel.Content = count == 1 ? "1 file found" : count.ToString() + " files found";
                this.failedConnectionBorder.Visibility = System.Windows.Visibility.Hidden;
                this.notConnectedBorder.Visibility = System.Windows.Visibility.Hidden;
                this.connectingBorder.Visibility = System.Windows.Visibility.Hidden;
                this.connectedBorder.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private bool ConnectionSuccessful { get; set; }

    }
}
