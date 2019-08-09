using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RootNav.Data
{
    public class RootInfo
    {
        public String RsmlID { get; set; }
        public String Label { get; set; }
        public String RelativeID { get; set; }
        public SplinePositionReference StartReference { get; set; }
        public Polyline Polyline { get; set; }
        public SampledSpline Spline { get; set; }
        public List<RootInfo> Children { get; set; }
        public DateTime Stamp { get; set; }
        public List<Annotation> Annotations { get; set; }
        public List<Function> Functions { get; set; }
        public List<Property> Properties { get; set; }
    }
}
