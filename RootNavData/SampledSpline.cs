using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace RootNav.Data
{
    [Serializable]
    public class SampledSpline
    {
        private double tension = 0.7;

        public double Tension
        {
            get { return tension; }
            set { tension = value; }
        }

        private double sampleResolution = 1.0;

        public double SampleResolution
        {
            get { return sampleResolution; }
            set { sampleResolution = value; }
        }

        private int controlPointSeparation = 40;

        public int ControlPointSeparation
        {
            get { return controlPointSeparation; }
            set { controlPointSeparation = value; }
        }

        private Point[] sampledPoints;

        public Point[] SampledPoints
        {
            get { return sampledPoints; }
            set { sampledPoints = value; }
        }

        public Rect BoundingBox
        {
            get
            {
                double left = double.MaxValue, right = double.MinValue, top = double.MaxValue, bottom = double.MinValue;

                foreach (Point p in this.sampledPoints)
                {
                    if (p.X < left)
                    {
                        left = p.X;
                    }

                    if (p.X > right)
                    {
                        right = p.X;
                    }

                    if (p.Y < top)
                    {
                        top = p.Y;
                    }

                    if (p.Y > bottom)
                    {
                        bottom = p.Y;
                    }
                }

                return new Rect(new Point(left, top), new Point(right, bottom));
            }
        }

        private double[] sampledPointsLengths;

        public double[] SampledPointsLengths
        {
            get { return sampledPointsLengths; }
            set { sampledPointsLengths = value; }
        }

        
        public Point Start
        {
            get
            {
                return this.sampledPoints[0];
            }
        }

        public Point End
        {
            get
            {
                return this.sampledPoints[this.sampledPoints.Length - 1];
            }
        }
        

        public double Length
        {
            get
            {
                return this.sampledPointsLengths == null ? 0 : this.sampledPointsLengths[this.sampledPointsLengths.Length - 1];
            }
        }

        public List<Point> ControlPoints
        {
            get
            {
                return CreateControlPoints(this.controlPointSeparation, this.sampledPoints.ToList());
            }
        }

        public void Initialise(List<Point> basePath)
        {
            // Calculate the necessary number of points to represent the curves at a given resolution, to a minimum of 3
            int intermediatePointCount = Math.Max(3,(int)Math.Round(controlPointSeparation * this.SampleResolution));

            this.sampledPoints = SampledSpline.SampleCardinalSpline(intermediatePointCount, tension, CreateControlPoints(this.controlPointSeparation, basePath).ToArray()).ToArray();
            this.sampledPointsLengths = SampledSpline.MeasurePoints(this.sampledPoints);
        }

        public void InitialiseFromControlPoints(List<Point> controlPoints, double tension, int controlPointSeparation)
        {
            this.tension = tension;
            this.controlPointSeparation = controlPointSeparation;

            int intermediatePointCount = Math.Max(3, (int)Math.Round(controlPointSeparation * this.SampleResolution));

            this.sampledPoints = SampledSpline.SampleCardinalSpline(intermediatePointCount, tension, controlPoints.ToArray()).ToArray();
            this.sampledPointsLengths = SampledSpline.MeasurePoints(this.sampledPoints);
        }

        public void InitialiseWithVectorList(List<Point> basePath)
        {
            this.sampledPoints = basePath.ToArray();
            this.sampledPointsLengths = SampledSpline.MeasurePoints(this.sampledPoints);
        }

        public void Reverse()
        {
            this.sampledPoints = this.sampledPoints.Reverse().ToArray();
            this.sampledPointsLengths = SampledSpline.MeasurePoints(this.sampledPoints);
        }

        public static List<Point> CreateControlPoints(int separation, List<Point> points)
        {
            List<Point> outputPoints = new List<Point>();
            outputPoints.Add(points[0]);

            int controlPointCount = (int)Math.Round(points.Count / (double)separation);

            for (int i = 0; i < controlPointCount; i++)
            {
                double t = (i + 1.0) / (controlPointCount + 1.0);
                int index = (int)(t * points.Count);

                outputPoints.Add(points[index]);
            }

            outputPoints.Add(points.Last());
            
            return outputPoints;
        }

        public SplinePositionReference GetPositionReference(double length)
        {
            if (length <= 0)
            {
                return new SplinePositionReference(0, 0);
            }
            else if (length >= this.sampledPointsLengths[this.sampledPointsLengths.Length - 1])
            {
                return new SplinePositionReference(this.sampledPointsLengths.Length - 2, 1.0);
            }

            int index = 0;
            for (int i = 0; i < this.sampledPointsLengths.Length; i++)
            {
                if (this.sampledPointsLengths[i] > length)
                {
                    index = i - 1;
                    break;
                }
            }

            // Looking between i and i + 1
            double d = (length - this.sampledPointsLengths[index]) / (this.sampledPointsLengths[index + 1] - this.sampledPointsLengths[index]);
            return new SplinePositionReference(index, d);
        }
        
        public double GetLength(SplinePositionReference positionReference)
        {
            if (positionReference.ControlPoint < 0)
            {
                return 0.0;
            }
            else if (positionReference.ControlPoint >= this.sampledPointsLengths.Length - 1)
            {
                return this.sampledPointsLengths[this.sampledPointsLengths.Length - 1];
            }

            // Interpolate
            double t = positionReference.T;
            int index = positionReference.ControlPoint;
            return this.sampledPointsLengths[index] * (1 - t) + this.sampledPointsLengths[index + 1] * t;
        }

        public SplinePositionReference GetPositionReference(Point p)
        {
            int cpIndex = 0;
            double distanceSquared = double.MaxValue;

            // Begin by finding closest sampled point that isn't an end point
            for (int i = 1; i < this.sampledPoints.Length - 1; i++)
            {
                double d = Math.Pow(sampledPoints[i].X - p.X,2.0) + Math.Pow(sampledPoints[i].Y - p.Y,2.0);

                if (d < distanceSquared)
                {
                    cpIndex = i;
                    distanceSquared = d;
                }
            }

            // Next find the closest point on the two lines i-1 <> i and i <> i + 1
            double leftT, rightT;
            Point leftClosest = GetClosestPoint(sampledPoints[cpIndex - 1], sampledPoints[cpIndex], p, true, out leftT);
            Point rightClosest = GetClosestPoint(sampledPoints[cpIndex], sampledPoints[cpIndex + 1], p, true, out rightT);

            // If we are closer to the left side
            if ((p - leftClosest).LengthSquared < (p - rightClosest).LengthSquared)
            {
                return new SplinePositionReference(cpIndex - 1, 1 - leftT);
            }
            else
            {
                return new SplinePositionReference(cpIndex, rightT);
            }
        }

        /// <summary>
        /// Calculates the closest point to P on some vector AB.
        /// </summary>
        /// <param name="A">The point at the beginning of vector AB.</param>
        /// <param name="B">The point at the end of vector AB.</param>
        /// <param name="P">The point to compare with vector AB.</param>
        /// <param name="segmentClamp">A boolean signalling whether to clamp any result between AB, or to allow the point to lie elsewhere along that line.</param>
        /// <returns></returns>
        Point GetClosestPoint(Point A, Point B, Point P, bool segmentClamp, out double t)
        {
            Vector AP = P - A;
            Vector AB = B - A;
            double ab2 = AB.X * AB.X + AB.Y * AB.Y;
            double ap_ab = AP.X * AB.X + AP.Y * AB.Y;
            t = ap_ab / ab2;

            if (segmentClamp)
            {
                if (t < 0.0f)
                    t = 0.0f;
                else if (t > 1.0f)
                    t = 1.0f;
            }

            return A + AB * t;
        }

        public Point GetPoint(SplinePositionReference positionReference)
        {
            if (positionReference.ControlPoint < 0)
            {
                return this.sampledPoints[0];
            }
            else if (positionReference.ControlPoint >= this.sampledPointsLengths.Length - 1)
            {
                return this.sampledPoints[this.sampledPoints.Length - 1];
            }

            // Interpolate
            double t = positionReference.T;
            int index = positionReference.ControlPoint;

            return new Point(this.sampledPoints[index].X * (1 - t) + this.sampledPoints[index + 1].X * t,
                             this.sampledPoints[index].Y * (1 - t) + this.sampledPoints[index + 1].Y * t);
        }
                
        private static List<Point> SampleCardinalSpline(int intermediatePointCount, double tension, params Point[] points)
        {
            double s = (1 - tension) / 2;

            Point reflectedStart = new Point(points[0].X - (points[1].X - points[0].X), points[0].Y - (points[1].Y - points[0].Y));
            Point reflectedEnd = new Point(points[points.Length - 1].X - (points[points.Length - 2].X - points[points.Length - 1].X),
                                           points[points.Length - 1].Y - (points[points.Length - 2].Y - points[points.Length - 1].Y));
            
            List<Point> outputPoints = new List<Point>();

            Point p1, p2, p3, p4;

            for (int pointIndex = 0; pointIndex < points.Length - 1; pointIndex++)
            {
                outputPoints.Add(points[pointIndex]);

                if (pointIndex == 0)
                {
                    if (points.Length > 2)
                    {
                        p1 = reflectedStart;
                        p2 = points[0];
                        p3 = points[1];
                        p4 = points[2];
                    }
                    else
                    {
                        // Very short spline
                        p1 = reflectedStart;
                        p2 = points[0];
                        p3 = points[1];
                        p4 = reflectedEnd;
                    }
                }
                else if (pointIndex < points.Length - 2)
                {
                    p1 = points[pointIndex - 1];
                    p2 = points[pointIndex];
                    p3 = points[pointIndex + 1];
                    p4 = points[pointIndex + 2];
                }
                else
                {
                    p1 = points[points.Length - 3];
                    p2 = points[points.Length - 2];
                    p3 = points[points.Length - 1];
                    p4 = reflectedEnd;
                }

                for (int i = 0; i < intermediatePointCount; i++)
                {
                    double t = (i + 1.0) / (intermediatePointCount + 1.0);
                    outputPoints.Add(CalculateCardinalSplinePoint(s, t, p1, p2, p3, p4));
                }
            }
               
            outputPoints.Add(points.Last());
            return outputPoints;
        }

        private static double[] MeasurePoints(Point[] points)
        {
            double[] outputDistances = new double[points.Length];
            outputDistances[0] = 0.0;

            for (int i = 1; i < points.Length; i++)
            {
                Point a = points[i - 1];
                Point b = points[i];

                // Distance between a and b
                double d = Math.Sqrt(Math.Pow(a.X - b.X, 2.0) + Math.Pow(a.Y - b.Y, 2.0));

                outputDistances[i] = outputDistances[i - 1] + d;
            }

            return outputDistances;
        }

        private static Point CalculateCardinalSplinePoint(double s, double t, Point p1, Point p2, Point p3, Point p4)
        {
            double t2 = t * t, t3 = t * t * t;
            return new Point(
                        // x
                        s * (-t3 + 2 * t2 - t) * p1.X +
                        s * (-t3 + t2) * p2.X +
                        (2 * t3 - 3 * t2 + 1) * p2.X +
                        s * (t3 - 2 * t2 + t) * p3.X +
                        (-2 * t3 + 3 * t2) * p3.X +
                        s * (t3 - t2) * p4.X,
                        // y
                        s * (-t3 + 2 * t2 - t) * p1.Y +
                        s * (-t3 + t2) * p2.Y +
                        (2 * t3 - 3 * t2 + 1) * p2.Y +
                        s * (t3 - 2 * t2 + t) * p3.Y +
                        (-2 * t3 + 3 * t2) * p3.Y +
                        s * (t3 - t2) * p4.Y
                        );
        }

        public Point[] Rasterise()
        {
            List<Point> points = new List<Point>();
            for (int i = 0; i < sampledPoints.Length - 1; i++)
            {
                Point currentPoint = new Point((int)sampledPoints[i].X, (int)sampledPoints[i].Y);
                Point destinationPoint = new Point((int)sampledPoints[i + 1].X, (int)sampledPoints[i + 1].Y);

                double dx = Math.Abs(destinationPoint.X - currentPoint.X);
                double dy = Math.Abs(destinationPoint.Y - currentPoint.Y);

                int sx = currentPoint.X < destinationPoint.X ? 1 : -1;
                int sy = currentPoint.Y < destinationPoint.Y ? 1 : -1;
                double err = dx - dy;

                int x = (int)currentPoint.X,
                    y = (int)currentPoint.Y,
                    fx = (int)destinationPoint.X,
                    fy = (int)destinationPoint.Y;

                while (true)
                {
                    if (x == fx && y == fy)
                        break;
                 
                    // Use (x, y) Position
                    points.Add(new Point(x, y));

                    double err2 = 2 * err;
                    if (err2 > -dy)
                    {
                        err -= dy;
                        x += sx;
                    }
                    if (err2 < dx)
                    {
                        err += dx;
                        y += sy;
                    }
                }
            }

            return points.ToArray();
        }

    }
}
