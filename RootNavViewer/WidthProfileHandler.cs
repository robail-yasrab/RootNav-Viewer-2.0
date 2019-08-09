using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Data;
using RootNav.Data;

namespace RootNav.Measurement
{
    public class WidthProfileHandler : MeasurementHandler
    {
        private Random random = new Random();

        public override string Name
        {
            get { return "Width Profile"; }
        }

        public override MeasurementType Measures
        {
            get { return MeasurementType.Plant; }
        }

        public override bool ReturnsSingleItem
        {
            get { return false; }
        }

        public override object MeasurePlant(PlantInfo plant)
        {
            double min = Int32.MaxValue;
            double max = Int32.MinValue;

            HashSet<Point> rootPoints = new HashSet<Point>();

            foreach (RootInfo root in plant)
            {
                SampledSpline s = root.Spline;
                Point[] splinePoints = s.Rasterise();
                foreach (Point p in splinePoints)
                {
                    rootPoints.Add(p);

                    if (p.Y < min)
                    {
                        min = p.Y;
                    }

                    if (p.Y > max)
                    {
                        max = p.Y;
                    }
                }
            }


            List<int>[] xs = new List<int>[(int)max - (int)min + 1];
            for (int i = 0; i < xs.Length; i++)
            {
                xs[i] = new List<int>();
            }


            foreach (Point p in rootPoints)
            {
                xs[(int)p.Y - (int)min].Add((int)p.X); 
            }

            int[] widths = new int[xs.Length];

            for (int pos = 0; pos < xs.Length; pos++)
            {
                if (xs[pos].Count > 0)
                {
                    int submin = xs[pos].Min();
                    int submax = xs[pos].Max();
                    widths[pos] = submax - submin + 1;
                }
                else
                {
                    widths[pos] = 0;
                }
            }

            int rowCount = widths.Length;

            List<List<object>> data = new List<List<object>>() { new List<object>(), new List<object>() };
            data[0].Add("Distance");
            data[1].Add("");

            // Obtain the curvature profile at the specified resolution
            for (int row = 0; row < rowCount; row++)
            {
                data[0].Add(row);
                data[1].Add(widths[row]);
            }

            return data;
        }

        private string RandomString(int size)
        {
            StringBuilder builder = new StringBuilder();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }

            return builder.ToString();
        }
    }
}
