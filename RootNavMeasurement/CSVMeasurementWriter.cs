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
using System.IO;

namespace RootNav.Measurement
{
    public class CSVMeasurementWriter : IDataOutputHandler
    {
        private ObservableCollection<Dictionary<String, String>> data = new ObservableCollection<Dictionary<string, string>>();
        public ObservableCollection<Dictionary<String, String>> Data
        { 
            get { return data; }
            set { data = value; }
        }

        private List<String> columns = new List<string>();


        #region IMeasurementHandler
        public void Clear()
        {
            this.data.Clear();
            this.columns.Clear();
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

            this.data.Add(measurement);
        }
        #endregion

        public void Write(String path)
        {
            StreamWriter strm = new StreamWriter(path);

            foreach (var row in this.data)
            {
                foreach (String colID in this.columns)
                {
                    if (row.ContainsKey(colID))
                    {
                        strm.Write(row[colID] + ",");
                    }
                }
                strm.WriteLine();
            }

            strm.Close();


        }
        
    }
}
