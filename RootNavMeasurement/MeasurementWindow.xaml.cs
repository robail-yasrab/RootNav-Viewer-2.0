using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Data;

namespace RootNav.Measurement
{
    /// <summary>
    /// Interaction logic for MeasurementWindow.xaml
    /// </summary>
    public partial class MeasurementWindow : Window, IDataOutputHandler
    {
        #region Variables
        private ObservableCollection<Dictionary<String, String>> data = new ObservableCollection<Dictionary<string, string>>();
        public ObservableCollection<Dictionary<String, String>> Data
        {
            get { return data; }
            set { data = value; }
        }

        private List<String> columns = new List<string>();

        private ContextMenu headerContextMenu;

        public ContextMenu HeaderContextMenu
        {
            get { return headerContextMenu; }
            set { headerContextMenu = value; }
        }
        #endregion

        #region IDataOutputHandler
        public void Clear()
        {
            this.data.Clear();
            this.columns.Clear();
            this.measurementsView.Columns.Clear();
            this.HeaderContextMenu = null;
        }

        public void Add(Dictionary<string, string> measurement)
        {
            bool columnsAltered = false;
            foreach (string key in measurement.Keys)
            {
                if (!this.columns.Contains(key))
                {
                    columnsAltered = true;
                    this.columns.Add(key);
                    this.measurementsView.Columns.Add(new DataGridTextColumn() { Header = key, Binding = new Binding("[" + key + "]") });
                }
            }

            // Add data
            if (this.data.Count == 0)
            {
                Dictionary<string, string> headers = new Dictionary<string, string>();
                foreach (string s in this.columns)
                {
                    headers.Add(s, s);
                }
                this.data.Add(headers);
            }
            else if (columnsAltered)
            {
                Dictionary<string, string> headers = this.data.First();
                headers.Clear();
                foreach (string s in this.columns)
                {
                    headers.Add(s, s);
                }
            }

            if (columnsAltered)
            {
                
                GenerateContextMenu();
            }

            this.data.Add(measurement);
        }
        #endregion

        public MeasurementWindow()
        {
            InitializeComponent();
        }

        #region Context Menu and Column Generation
        private void GenerateContextMenu()
        {
            System.Windows.Controls.ContextMenu cu = new ContextMenu();
            foreach (DataGridColumn col in this.measurementsView.Columns)
            {
                MenuItem mu = new MenuItem();
                mu.Header = col.Header;
                mu.IsCheckable = true;
                mu.IsChecked = col.Visibility == System.Windows.Visibility.Visible;
                mu.StaysOpenOnClick = true;
                mu.Checked += new RoutedEventHandler(menu_Checked);
                mu.Unchecked += new RoutedEventHandler(menu_Unchecked);
                cu.Items.Add(mu);
            }
            HeaderContextMenu = cu;
        }

        void menu_Checked(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            if (menuItem != null)
            {
                this.measurementsView.Columns[this.HeaderContextMenu.Items.IndexOf(menuItem)].Visibility = System.Windows.Visibility.Visible;
            }
        }

        void menu_Unchecked(object sender, RoutedEventArgs e)
        {
            int visibleCount = 0;
            foreach (DataGridColumn dgc in this.measurementsView.Columns)
            {
                if (dgc.Visibility == System.Windows.Visibility.Visible)
                    visibleCount++;
            }

            if (visibleCount <= 1)
            {
                (sender as MenuItem).IsChecked = true;
                e.Handled = true;
                return;
            }

            MenuItem menuItem = sender as MenuItem;
            if (menuItem != null)
            {
                this.measurementsView.Columns[this.HeaderContextMenu.Items.IndexOf(menuItem)].Visibility = System.Windows.Visibility.Hidden;
            }
        }

        private void Window_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            DependencyObject depObj = (DependencyObject)e.OriginalSource;

            while (depObj != null && !(depObj is System.Windows.Controls.DataGridRow))
            {
                depObj = VisualTreeHelper.GetParent(depObj);
            }

            if (depObj is System.Windows.Controls.DataGridRow)
            {
                DataGridRow currentRow = depObj as System.Windows.Controls.DataGridRow;
                int index = currentRow.GetIndex();

                if (index == 0)
                    currentRow.ContextMenu = HeaderContextMenu;
            }
        }

        #endregion

    }
}
