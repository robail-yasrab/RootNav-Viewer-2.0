using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using RootNav.Data;

namespace RootNav.Measurement
{
    #region Total Length
    public class TotalLengthPlantHandler : MeasurementHandler
    {
        public override string Name
        {
            get { return "Total Length - All Roots"; }
        }

        public override MeasurementType Measures
        {
            get { return MeasurementType.Plant; }
        }

        public override bool ReturnsSingleItem
        {
            get { return true; }
        }

        public override object MeasurePlant(PlantInfo plant)
        {
            double totalLength = 0.0;
            
            foreach (RootInfo info in plant)
            {
                if (info.Spline != null)
                {
                    totalLength += info.Spline.Length;
                }

            }

            return Math.Round(totalLength, 2);
        }
    }

    public class TotalLengthRootHandler : MeasurementHandler
    {
        public override string Name
        {
            get { return "Total Length"; }
        }

        public override MeasurementType Measures
        {
            get { return MeasurementType.Root; }
        }

        public override bool ReturnsSingleItem
        {
            get { return true; }
        }

        public override object MeasureRoot(RootInfo root, RootInfo parent = null)
        {
            return Math.Round(root.Spline == null ? 0.0 : root.Spline.Length, 2);
        }
    }

    public class TotalPrimaryLengthPlantHandler : MeasurementHandler
    {
        public override string Name
        {
            get { return "Total Length - Primary Roots"; }
        }

        public override MeasurementType Measures
        {
            get { return MeasurementType.Plant; }
        }

        public override bool ReturnsSingleItem
        {
            get { return true; }
        }

        public override object MeasurePlant(PlantInfo plant)
        {
            double totalLength = 0.0;

            foreach (RootInfo primary in plant.Roots)
            {
                totalLength += primary.Spline.Length;
            }

            return Math.Round(totalLength, 2);
        }
    }

    public class TotalLateralLengthPlantHandler : MeasurementHandler
    {
        public override string Name
        {
            get { return "Total Length - Lateral Roots"; }
        }

        public override MeasurementType Measures
        {
            get { return MeasurementType.Plant; }
        }

        public override bool ReturnsSingleItem
        {
            get { return true; }
        }

        public override object MeasurePlant(PlantInfo plant)
        {
            double totalLength = 0.0;

            foreach (RootInfo primary in plant.Roots)
            {
                if (primary.Children != null)
                {
                    foreach (RootInfo child in primary.Children)
                    {
                        totalLength += child.Spline.Length;
                    }
                }
            }

            return Math.Round(totalLength, 2);
        }
    }
    #endregion

    #region Average Length
    public class AverageLengthPlantHandler : MeasurementHandler
    {
        public override string Name
        {
            get { return "Average Length - All roots"; }
        }

        public override MeasurementType Measures
        {
            get { return MeasurementType.Plant; }
        }

        public override bool ReturnsSingleItem
        {
            get { return true; }
        }

        public override object MeasurePlant(PlantInfo plant)
        {
            double totalLength = 0.0;
            double totalCount = 0.0;
            foreach (RootInfo info in plant)
            {
                if (info.Spline != null)
                {
                    totalLength += info.Spline.Length;
                    totalCount++;
                }

            }

            return totalCount > 0 ? Math.Round(totalLength / totalCount, 2) : 0;
        }
    }


    public class AverageLengthPrimaryPlantHandler : MeasurementHandler
    {
        public override string Name
        {
            get { return "Average Length - Primary roots"; }
        }

        public override MeasurementType Measures
        {
            get { return MeasurementType.Plant; }
        }

        public override bool ReturnsSingleItem
        {
            get { return true; }
        }

        public override object MeasurePlant(PlantInfo plant)
        {
            double totalLength = 0.0;
            double totalCount = 0.0;
          
            foreach (RootInfo primary in plant.Roots)
            {
                totalLength += primary.Spline.Length;
                totalCount++;
            }

            return totalCount > 0 ? Math.Round(totalLength / totalCount, 2) : 0;
        }
    }

    public class AverageLengthLateralPlantHandler : MeasurementHandler
    {
        public override string Name
        {
            get { return "Average Length - Lateral roots"; }
        }

        public override MeasurementType Measures
        {
            get { return MeasurementType.Plant; }
        }

        public override bool ReturnsSingleItem
        {
            get { return true; }
        }

        public override object MeasurePlant(PlantInfo plant)
        {
            double totalLength = 0.0;
            double totalCount = 0.0;

            foreach (RootInfo primary in plant.Roots)
            {
                if (primary.Children != null)
                {
                    foreach (RootInfo child in primary.Children)
                    {
                        totalLength += child.Spline.Length;
                        totalCount++;
                    }
                }
                
            }

            return totalCount > 0 ? Math.Round(totalLength / totalCount, 2) : 0;
        }
    }

    #endregion

    #region Root Counts
    public class LateralCountPlantHandler : MeasurementHandler
    {
        public override string Name
        {
            get { return "Lateral Root Count"; }
        }

        public override MeasurementType Measures
        {
            get { return MeasurementType.Plant; }
        }

        public override bool ReturnsSingleItem
        {
            get { return true; }
        }

        public override object MeasurePlant(PlantInfo plant)
        {
            double lateralCount = 0;

            foreach (RootInfo primary in plant.Roots)
            {
                if (primary.Children != null)
                {
                    lateralCount += primary.Children.Count;
                }
            }

            return lateralCount;
        }
    }

    public class PrimaryCountPlantHandler : MeasurementHandler
    {
        public override string Name
        {
            get { return "Primary Root Count"; }
        }

        public override MeasurementType Measures
        {
            get { return MeasurementType.Plant; }
        }

        public override bool ReturnsSingleItem
        {
            get { return true; }
        }

        public override object MeasurePlant(PlantInfo plant)
        {
            double primaryCount = 0;

            foreach (RootInfo primary in plant.Roots)
            {
                primaryCount++;
            }

            return primaryCount;
        }
    }

    public class LateralCountRootHandler : MeasurementHandler
    {
        public override string Name
        {
            get { return "Lateral Root Count"; }
        }

        public override MeasurementType Measures
        {
            get { return MeasurementType.Root; }
        }

        public override bool ReturnsSingleItem
        {
            get { return true; }
        }

        public override object MeasureRoot(RootInfo root, RootInfo parent = null)
        {
            if (root.Children != null)
            {
                return root.Children.Count;
            }
            else
            {
                return 0;
            }
        }
    }
    #endregion

    #region Convex hull
    public class ConvexHullHandler : MeasurementHandler
    {
        public override string Name
        {
            get { return "Convex Hull Area"; }
        }

        public override MeasurementType Measures
        {
            get { return MeasurementType.Plant; }
        }

        public override bool ReturnsSingleItem
        {
            get { return true; }
        }

        public override object MeasurePlant(PlantInfo plant)
        {
            List<Point> samplePoints = new List<Point>();
            foreach (RootInfo child in plant)
            {
                samplePoints.AddRange(child.Spline.SampledPoints);
            }

            List<Point> hullPoints = ConvexHull.FindHull(samplePoints);

            double area = 0.0;
            for (int i = 0; i < hullPoints.Count; i++)
            {
                Point one = hullPoints[i];
                Point two = hullPoints[(i + 1) % hullPoints.Count];

                area += one.X * two.Y - two.X * one.Y;
            }

            return Math.Round(area / 2.0, 2);
        }
    }
    #endregion

    #region Tortuosity
    public class TortuosityPlantHandler : MeasurementHandler
    {
        public override string Name
        {
            get { return "Average Tortuosity"; }
        }

        public override MeasurementType Measures
        {
            get { return MeasurementType.Plant; }
        }

        public override bool ReturnsSingleItem
        {
            get { return true; }
        }

        public override object MeasurePlant(PlantInfo plant)
        {
            double totalTortuosity = 0.0;
            double totalCount = 0.0;

            foreach (RootInfo info in plant)
            {
                if (info.Spline != null)
                {
                    totalTortuosity += info.Spline.Length / (info.Spline.End - info.Spline.Start).Length;
                    totalCount++;
                }
            }

            return totalCount > 0 ? Math.Round(totalTortuosity / totalCount, 2) : 0.0;
        }
    }


    public class TortuosityRootHandler : MeasurementHandler
    {
        public override string Name
        {
            get { return "Tortuosity"; }
        }

        public override MeasurementType Measures
        {
            get { return MeasurementType.Root; }
        }

        public override bool ReturnsSingleItem
        {
            get { return true; }
        }

        public override object MeasureRoot(RootInfo root, RootInfo parent = null)
        {
            double chordLength = (root.Spline.End - root.Spline.Start).Length;
            return Math.Round(root.Spline == null ? 0.0 : root.Spline.Length / chordLength, 2);
        }
    }

    #endregion

    #region Maximum Width
    public class MaximumWidthPlantHandler : MeasurementHandler
    {
        public override string Name
        {
            get { return "Maximum Width"; }
        }

        public override MeasurementType Measures
        {
            get { return MeasurementType.Plant; }
        }

        public override bool ReturnsSingleItem
        {
            get { return true; }
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

            int widthmax = int.MinValue;

            for (int pos = 0; pos < xs.Length; pos++)
            {
                if (xs[pos].Count > 0)
                {
                    int submin = xs[pos].Min();
                    int submax = xs[pos].Max();
                    int width = submax - submin + 1;
                    if (widthmax < width)
                    {
                        widthmax = width;
                    }
                }
            }

            return widthmax;
        }
    }

    #endregion

    #region Maximum Depth
    public class MaximumDepthPlantHandler : MeasurementHandler
    {
        public override string Name
        {
            get { return "Maximum Depth"; }
        }

        public override MeasurementType Measures
        {
            get { return MeasurementType.Plant; }
        }

        public override bool ReturnsSingleItem
        {
            get { return true; }
        }

        public override object MeasurePlant(PlantInfo plant)
        {
            double maxdepth = double.MinValue;
            foreach (RootInfo root in plant)
            {
                SampledSpline s = root.Spline;
                if (s != null)
                {
                    double depth = s.BoundingBox.Bottom;
                    if (maxdepth < depth)
                    {
                        maxdepth = depth;
                    }
                }
                else
                {
                    // Bad Spline
                }
               
            }

            return maxdepth;
        }
    }
    #endregion

    #region Width / Depth Ratio
    public class WidthDepthRatioPlantHandler : MeasurementHandler
    {
        private static MaximumDepthPlantHandler depthHandler = new MaximumDepthPlantHandler();
        private static MaximumWidthPlantHandler widthHandler = new MaximumWidthPlantHandler();

        public override string Name
        {
            get { return "Width / Depth Ratio"; }
        }

        public override MeasurementType Measures
        {
            get { return MeasurementType.Plant; }
        }

        public override bool ReturnsSingleItem
        {
            get { return true; }
        }

        public override object MeasurePlant(PlantInfo plant)
        {
            int width = (int)widthHandler.MeasurePlant(plant);
            double depth = (double)depthHandler.MeasurePlant(plant);

            if (depth == 0.0)
            {
                return 0.0;
            }

            return Math.Round(width / depth, 2);
        }
    }
    #endregion

    #region Plant Centroid
    public class CentroidXPlantHandler : MeasurementHandler
    {
        public override string Name
        {
            get { return "Centroid X"; }
        }

        public override MeasurementType Measures
        {
            get { return MeasurementType.Plant; }
        }

        public override bool ReturnsSingleItem
        {
            get { return true; }
        }

        public override object MeasurePlant(PlantInfo plant)
        {
            Point source = default(Point);
            double totalX = 0.0, totalY = 0.0;
            double count = 0;
            foreach (RootInfo root in plant)
            {
                if (root.RelativeID == "1.1")
                {
                    source = root.Spline.Start;
                }

                SampledSpline s = root.Spline;

                foreach (Point p in s.SampledPoints)
                {
                    totalX += p.X;
                    totalY += p.Y;
                    ++count;
                }
            }

            return String.Format("{0}", Math.Round(totalX / count - source.X, 3));
        }
    }
    public class CentroidYPlantHandler : MeasurementHandler
    {
        public override string Name
        {
            get { return "Centroid Y"; }
        }

        public override MeasurementType Measures
        {
            get { return MeasurementType.Plant; }
        }

        public override bool ReturnsSingleItem
        {
            get { return true; }
        }

        public override object MeasurePlant(PlantInfo plant)
        {
            Point source = default(Point);
            double totalX = 0.0, totalY = 0.0;
            double count = 0;
            foreach (RootInfo root in plant)
            {
                if (root.RelativeID == "1.1")
                {
                    source = root.Spline.Start;
                }

                SampledSpline s = root.Spline;

                foreach (Point p in s.SampledPoints)
                {
                    totalX += p.X;
                    totalY += p.Y;
                    ++count;
                }
            }

            return String.Format("{0}", Math.Round(totalY / count - source.Y, 3));
        }
    }
    #endregion

    #region Label
    public class LabelPlantHandler : MeasurementHandler
    {
        public override string Name
        {
            get { return "Label"; }
        }

        public override MeasurementType Measures
        {
            get { return MeasurementType.Plant; }
        }

        public override bool ReturnsSingleItem
        {
            get { return true; }
        }

        public override object MeasurePlant(PlantInfo plant)
        {
            return plant.Label == null ? "" : plant.Label;
        }
    }

    public class LabelRootHandler : MeasurementHandler
    {
        public override string Name
        {
            get { return "Label"; }
        }

        public override MeasurementType Measures
        {
            get { return MeasurementType.Root; }
        }

        public override bool ReturnsSingleItem
        {
            get { return true; }
        }

        public override object MeasureRoot(RootInfo root, RootInfo parent = null)
        {
            return root.Label == null ? "" : root.Label;
        }
    }
    #endregion

    #region Emergence Distance
    public class EmergenceDistanceRootHandler : MeasurementHandler
    {
        public override string Name
        {
            get { return "Emergence Distance"; }
        }

        public override MeasurementType Measures
        {
            get { return MeasurementType.Root; }
        }

        public override bool ReturnsSingleItem
        {
            get { return true; }
        }

        public override object MeasureRoot(RootInfo root, RootInfo parent = null)
        {
            // If parent == null, then this is a primary root and there is no emergence angle
            if (parent == null)
            {
                return "";
            }
            else
            {
                SampledSpline parentSpline = parent.Spline;
                double parentIntersectionDistance = parentSpline.GetLength(root.StartReference);
                return Math.Round(parentIntersectionDistance, 2);
            }
        }
    }
    #endregion

}
