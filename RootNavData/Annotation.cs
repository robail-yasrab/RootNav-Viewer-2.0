using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RootNav.Data
{
    /// <summary>
    /// Represents Scene, Plant and Root Annotations
    /// </summary>
    public class Annotation
    {
        public Annotation (string name, IEnumerable<Point3D> points, string value, string software)
        {
            this.Name = name;
            this.Points = new List<Point3D>();
            foreach (Point3D p in points)
            {
                this.Points.Add(p);
            }

            this.Value = value;
            this.Software = software;
        }

        public string Name { get; set; }
        public List<Point3D> Points { get; set; }
        public string Value { get; set; }
        public string Software { get; set; }
    }
}
