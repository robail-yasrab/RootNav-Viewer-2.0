using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RootNav.Data
{
    /// <summary>
    /// Function sample structure
    /// </summary>
    public struct Sample
    {
        public double Position;
        public double Value;

        public Sample(double value, double position)
        {
            this.Position = position;
            this.Value = value;
        }

        public Sample(double value)
        {
            this.Position = double.NaN;
            this.Value = value;
        }
    }

    /// <summary>
    /// Base function class
    /// </summary>
    public abstract class Function
    {
        public string Name { get; set; }
        public List<Sample> Samples { get; set; }

        protected Function(string name, List<Sample> samples)
        {
            if (name != "")
            {
                this.Name = name;
            }
            else
            {
                this.Name = "Unspecified";
            }

            this.Samples = samples;
        }

        public abstract double ReadMeasurement(double position);
    }

    /// <summary>
    /// Sub class representing functions in the polyline domain
    /// </summary>
    public class PolylineFunction : Function
    {
        public PolylineFunction(string name, List<Sample> samples)
            : base(name, samples)
        {
        }

        public override double ReadMeasurement(double position)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Sub class representing functions in the uniform domain
    /// </summary>
    public class UniformFunction : Function
    {
        public UniformFunction(string name, List<Sample> samples)
            : base(name, samples)
        {
        }

        public override double ReadMeasurement(double position)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Sub class representing functions in the length domain
    /// </summary>
    public class LengthFunction : Function
    {
        public FunctionOrigin Origin;

        public enum FunctionOrigin
        {
            RootTip,
            RootBase
        }

        public LengthFunction(string name, List<Sample> samples, FunctionOrigin origin)
            : base(name, samples)
        {
            this.Origin = origin;
        }

        public override double ReadMeasurement(double position)
        {
            throw new NotImplementedException();
        }
    }

}
