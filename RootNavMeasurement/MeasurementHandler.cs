using System;
using System.Collections.Generic;
using System.Windows;
using System.Reflection;
using System.IO;
using RootNav.Data;

namespace RootNav.Measurement
{
    public enum MeasurementType
    {
        Plant, Root
    }

    public abstract class MeasurementHandler
    {
        public abstract string Name { get; }

        public abstract MeasurementType Measures { get; }

        public abstract bool ReturnsSingleItem { get; }

        public virtual object MeasurePlant(PlantInfo plant)
        {
            throw new NotImplementedException();
        }

        public virtual object MeasureRoot(RootInfo root, RootInfo parent = null)
        {
            throw new NotImplementedException();
        }
    }
}
