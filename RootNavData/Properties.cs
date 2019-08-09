using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RootNav.Data
{
    public class Property
    {
        public string Label { get; set; }
        public string Value { get; set; }

        public Property(string label, string value)
        {
            this.Label = label;
            this.Value = value;
        }
    }

    public class PropertyDefinition
    {
        public string Label { get; set; }
        public PropertyType Type { get; set; }
        public object DefaultValue { get; set; }
        public string Unit { get; set; }

        public PropertyDefinition(string label, PropertyType type, object defaultValue, string unit = null)
        {
            this.Label = label;
            this.Type = type;
            this.DefaultValue = defaultValue;
            this.Unit = unit;
        }

        public enum PropertyType
        {
            Boolean,
            String,
            Float,
            Double,
            Integer
        }
    }
}
