using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RootNav.Data
{
    public class SceneInfo : IEnumerable<PlantInfo>
    {
        public List<PlantInfo> Plants { get; set; }
        public List<Annotation> Annotations { get; set; }
        public List<Property> Properties { get; set; }

        #region IEnumerable<PlantInfo>
        public IEnumerator<PlantInfo> GetEnumerator()
        {
            return Plants.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
        #endregion
    }

    public class SceneMetadata
    {
        public string Version { get; set; }
        public string Unit { get; set; }
        public double Resolution { get; set; }
        public DateTime LastModified { get; set; }
        public string Software { get; set; }
        public string User { get; set; }
        public string Key { get; set; }
        public TimeSequence Sequence { get; set; }
        public ImageInfo Image { get; set; }
        public bool Complete { get; set; }
        
        public List<PropertyDefinition> PropertyDefinitions { get; set; }
        
        public class TimeSequence
        {
            public string Label { get; set; }
            public int Index { get; set; }
            public bool Unified { get; set; }
        }

        public class ImageInfo
        {
            private String label = "";

            public String Label
            {
                get { return label; }
                set { label = value; }
            }

            private string hash = "";

            public string Hash
            {
                get { return hash; }
                set { hash = value; }
            }

            private double dpi = 0.0;

            public double Dpi
            {
                get { return dpi; }
                set { dpi = value; }
            }

            private double timeInSequence = 0.0;

            public double TimeInSequence
            {
                get { return timeInSequence; }
                set { timeInSequence = value; }
            }

            private double resolution = 0.0;

            public double Resolution
            {
                get { return resolution; }
                set { resolution = value; }
            }

            private string unit = "pixels";

            public string Unit
            {
                get { return unit; }
                set { unit = value; }
            }

            private string background = "dark";

            public string Background
            {
                get { return background; }
                set { background = value; }
            }


            private DateTime? captured = null;

            public DateTime? Captured
            {
                get { return captured; }
                set { captured = value; }
            }



        }

        public SceneMetadata()
        {
        }
    }
}
