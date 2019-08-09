using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using RootNav.Data;

namespace RootNav.Measurement
{
    public class EmergenceAngleRootHandler : MeasurementHandler
    {
        public override string Name
        {
            get { return "Emergence Angle"; }
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
                double angleMax = 20.0;
                Point start, end;

                if (angleMax > root.Spline.Length)
                {
                    double newMinDistanceOffset = angleMax - root.Spline.Length;
                    start = root.Spline.Start;
                    end = root.Spline.GetPoint(root.Spline.GetPositionReference(root.Spline.Length));
                }
                else
                {
                    start = root.Spline.Start;
                    end = root.Spline.GetPoint(root.Spline.GetPositionReference(angleMax));
                }

                // Angle between horizontal and emergence vectors
                double angle = 90 - Vector.AngleBetween(new Vector(1, 0), end - start);
                return Math.Round(angle > 180 ? angle - 360 : angle, 2);
            }
            else
            {
                double angleMax = 30.0, angleMin = 10.0, parentAngleRadius = 20.0;

                Point start, end;
                if (angleMax > root.Spline.Length)
                {
                    double newMinDistanceOffset = angleMax - root.Spline.Length;
                    start = root.Spline.GetPoint(root.Spline.GetPositionReference(Math.Max(0.0, angleMin - newMinDistanceOffset)));
                    end = root.Spline.GetPoint(root.Spline.GetPositionReference(root.Spline.Length));
                }
                else
                {
                    start = root.Spline.GetPoint(root.Spline.GetPositionReference(Math.Min(root.Spline.Length, angleMin)));
                    end = root.Spline.GetPoint(root.Spline.GetPositionReference(angleMax));
                }

                Vector rootVector = end - start;

                SampledSpline parentSpline = parent.Spline;

                double parentIntersectionDistance = parentSpline.GetLength(root.StartReference);

                Point parentStart = parentSpline.GetPoint(parentSpline.GetPositionReference(Math.Max(0.0, parentIntersectionDistance - parentAngleRadius)));
                Point parentEnd = parentSpline.GetPoint(parentSpline.GetPositionReference(Math.Min(parent.Spline.Length, parentIntersectionDistance + parentAngleRadius)));

                Vector parentVector = parentEnd - parentStart;

                return Math.Round(Vector.AngleBetween(rootVector, parentVector), 2);
            }
        }
    }

    public class TipAngleRootHandler : MeasurementHandler
    {
        public override string Name
        {
            get { return "Tip Angle"; }
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
            double tipAngleDistance = 7.0;

            double tipDistance = root.Spline.SampledPointsLengths.Last();
            double innerDistance = Math.Max(0, tipDistance - tipAngleDistance);

            Point innerPoint = root.Spline.GetPoint(root.Spline.GetPositionReference(innerDistance));
            Point tipPoint = root.Spline.GetPoint(root.Spline.GetPositionReference(tipDistance));

            // Tip Angle is angle between horizontal vector and the tip vector
            double angle = 90 - Vector.AngleBetween(new Vector(1,0), tipPoint - innerPoint);
            return Math.Round(angle > 180 ? angle - 360 : angle, 2);
        }
    }

    public class LateralEmergenceAnglePlantHandler : MeasurementHandler
    {
        private static EmergenceAngleRootHandler rootEmergenceHandler = new EmergenceAngleRootHandler();

        public override string Name
        {
            get { return "Average Lateral Emergence Angle"; }
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
            double totalLaterals = 0.0;
            double totalAngle = 0.0;

            foreach (RootInfo info in plant)
            {
                if (info.Spline != null)
                {
                    if (plant.GetParent(info) != null)
                    {
                        totalAngle += Math.Abs((double)rootEmergenceHandler.MeasureRoot(info, plant.GetParent(info)));
                        totalLaterals++;
                    }
                }
            }

            if (totalLaterals > 0)
                return Math.Round(totalAngle / totalLaterals, 2);
            else return null;
        }
    }

    public class LateralTipAnglePlantHandler : MeasurementHandler
    {
        private static TipAngleRootHandler rootTipHandler = new TipAngleRootHandler();

        public override string Name
        {
            get { return "Average Lateral Tip Angle"; }
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
            double totalLaterals = 0.0;
            double totalAngle = 0.0;

            foreach (RootInfo info in plant)
            {
                if (info.Spline != null)
                {
                    if (plant.GetParent(info) != null)
                    {
                        totalAngle += Math.Abs((double)rootTipHandler.MeasureRoot(info, null));
                        totalLaterals++;
                    }
                }
            }

            if (totalLaterals > 0)
                return Math.Round(totalAngle / totalLaterals, 2);
            else return null;
        }
    }

    public class PrimaryTipAnglePlantHandler : MeasurementHandler
    {
        private static TipAngleRootHandler rootTipHandler = new TipAngleRootHandler();

        public override string Name
        {
            get { return "Average Primary Tip Angle"; }
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
            double totalLaterals = 0.0;
            double totalAngle = 0.0;

            foreach (RootInfo info in plant)
            {
                if (info.Spline != null)
                {
                    if (plant.GetParent(info) == null)
                    {
                        totalAngle += Math.Abs((double)rootTipHandler.MeasureRoot(info, null));
                        totalLaterals++;
                    }
                }
            }

            if (totalLaterals > 0)
                return Math.Round(totalAngle / totalLaterals, 2);
            else return null;
        }
    }

    public class PrimaryEmergenceAnglePlantHandler : MeasurementHandler
    {
        private static EmergenceAngleRootHandler rootEmergenceHandler = new EmergenceAngleRootHandler();

        public override string Name
        {
            get { return "Average Primary Emergence Angle"; }
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
            double totalLaterals = 0.0;
            double totalAngle = 0.0;

            foreach (RootInfo info in plant)
            {
                if (info.Spline != null)
                {
                    if (plant.GetParent(info) == null)
                    {
                        totalAngle += Math.Abs((double)rootEmergenceHandler.MeasureRoot(info, plant.GetParent(info)));
                        totalLaterals++;
                    }
                }
            }

            if (totalLaterals > 0)
                return Math.Round(totalAngle / totalLaterals, 2);
            else return null;
        }
    }

    /* Additional trait written for Dr. Richard Barker */
    public class TotalAngleRootHandler : MeasurementHandler
    {
        public override string Name
        {
            get { return "Total Primary Angle"; }
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
                Point start, end;
                start = root.Spline.Start;
                end = root.Spline.End;

                // Angle between horizontal and emergence vectors
                double angle = 90 - Vector.AngleBetween(new Vector(1, 0), end - start);
                return Math.Round(angle > 180 ? angle - 360 : angle, 2);
            }
            else
            {
                // Do not calculate for lateral roots
                return null;
            }
        }
    }

    #region
    public class EmergenceParentAngleRootHandler : MeasurementHandler
    {
        public override string Name
        {
            get { return "Emergence Parent Angle"; }
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
                double parentAngleRadius = 10.0;

                SampledSpline parentSpline = parent.Spline;

                double parentIntersectionDistance = parentSpline.GetLength(root.StartReference);

                Point parentStart = parentSpline.GetPoint(parentSpline.GetPositionReference(Math.Max(0.0, parentIntersectionDistance - parentAngleRadius)));
                Point parentEnd = parentSpline.GetPoint(parentSpline.GetPositionReference(Math.Min(parent.Spline.Length, parentIntersectionDistance + parentAngleRadius)));

                Vector parentVector = parentEnd - parentStart;

                double angle = 90 - Vector.AngleBetween(new Vector(1, 0), parentEnd - parentStart);
                return Math.Round(angle > 180 ? angle - 360 : angle, 2);
            }
        }
    }
    #endregion

}
