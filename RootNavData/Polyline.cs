using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace RootNav.Data
{
    public struct Point3D
    {
        public Point3D (double X, double Y, double Z = double.NaN)
        {
            this.X = X;
            this.Y = Y;
            this.Z = Z;
        }

        public double X, Y, Z;
    }

    public class Polyline
    {
        public List<Point3D> Points { get; set; }
    }
}
