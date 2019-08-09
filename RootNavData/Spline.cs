using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace RootNav.Data
{
    [Serializable]
    public class Spline
    {
        private double tension = 0.7;

        public double Tension
        {
            get { return tension; }
            set { tension = value; }
        }

        private List<Point> controlPoints = new List<Point>();

        public List<Point> ControlPoints
        {
            get { return controlPoints; }
            set { controlPoints = value; }
        }

        private List<Point> sampledPoints = new List<Point>();

        public List<Point> SampledPoints
        {
            get { return sampledPoints; }
            set { sampledPoints = value; }
        }

        private List<double> controlPointLengths = new List<double>();

        public List<double> ControlPointLengths
        {
            get { return controlPointLengths; }
            set { controlPointLengths = value; }
        }

        public Point Start
        {
            get
            {
                return this.GetPoint(this.GetPositionReference(0));
            }
        }

        public Point End
        {
            get
            {
                return this.GetPoint(this.GetPositionReference(this.Length));
            }
        }

        public double Length
        {
            get
            {
                return this.controlPointLengths == null ? 0 : this.controlPointLengths.Last();
            }
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

        public Spline(int controlPointSeparation, List<Point> basePath, double tension)
        {
            this.tension = tension;
            this.controlPoints = CreateControlPoints(controlPointSeparation, basePath);
            this.sampledPoints = CreateCardinalSpline(10, tension, this.controlPoints.ToArray());
            this.controlPointLengths = MeasureCardinalSpline(tension, this.controlPoints.ToArray());
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
            else if (length >= this.controlPointLengths.Last())
            {
                return new SplinePositionReference(this.controlPointLengths.Count - 2, 1.0);
            }

            int index = 0;
            for (int i = 0; i < this.controlPointLengths.Count; i++)
            {
                if (this.controlPointLengths[i] > length)
                {
                    index = i - 1;
                    break;
                }
            }

            // Looking between i and i + 1
            double d = (length - this.controlPointLengths[index]) / (this.controlPointLengths[index + 1] - this.controlPointLengths[index]);
            return new SplinePositionReference(index, d);
        }

        public double GetLength(SplinePositionReference positionReference)
        {
            if (positionReference.ControlPoint < 0)
            {
                return 0.0;
            }
            else if (positionReference.ControlPoint >= this.controlPointLengths.Count - 1)
            {
                return this.controlPointLengths.Last();
            }

            // Interpolate
            double t = positionReference.T;
            int index = positionReference.ControlPoint;
            return this.controlPointLengths[index] * (1 - t) + this.controlPointLengths[index + 1] * t;
        }

        public SplinePositionReference GetPositionReference(Point p)
        {
            double s = (1 - this.tension) / 2;
            Point[] points = this.controlPoints.ToArray();

            int cpIndex = 0;
            double tIndex = 0;
            double distanceSquared = double.MaxValue;

            Point reflectedStart = new Point(points[0].X - (points[1].X - points[0].X), points[0].Y - (points[1].Y - points[0].Y));
            Point reflectedEnd = new Point(points[points.Length - 1].X - (points[points.Length - 2].X - points[points.Length - 1].X),
                                           points[points.Length - 1].Y - (points[points.Length - 2].Y - points[points.Length - 1].Y));

            List<Point> outputPoints = new List<Point>();

            Point p1, p2, p3, p4;

            for (int pointIndex = 0; pointIndex < points.Length - 1; pointIndex++)
            {
                if ((points[pointIndex] - p).LengthSquared < distanceSquared)
                {
                    cpIndex = pointIndex;
                    tIndex = 0;
                    distanceSquared = (points[pointIndex] - p).LengthSquared;
                }

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

                for (int i = 0; i < 100; i++)
                {
                    double t = (i + 1.0) / (100 + 1.0);
                    Point cardinalPoint = CalculateCardinalSplinePoint(s, t, p1, p2, p3, p4);
                    if ((cardinalPoint - p).LengthSquared < distanceSquared)
                    {
                        cpIndex = pointIndex;
                        tIndex = t;
                        distanceSquared = (cardinalPoint - p).LengthSquared;
                    }
                }
            }

            outputPoints.Add(points.Last());

            if ((points.Last() - p).LengthSquared < distanceSquared)
            {
                cpIndex = points.Length - 1 ;
                tIndex = 1;
            }

            return new SplinePositionReference(cpIndex, tIndex);
        }

        public Point GetPoint(SplinePositionReference positionReference)
        {
            double s = (1 - this.tension) / 2;
            Point[] points = this.controlPoints.ToArray();

            int cpIndex = positionReference.ControlPoint;
            double t = positionReference.T;
        
            if (cpIndex < 0 || cpIndex >= points.Length - 1 || t < 0 || t > 1)
            {
                return default(Point);
            }

            Point p1, p2, p3, p4;

            if (cpIndex == 0)
            {
                if (points.Length > 2)
                {
                    p1 = new Point(points[0].X - (points[1].X - points[0].X), points[0].Y - (points[1].Y - points[0].Y));
                    p2 = points[0];
                    p3 = points[1];
                    p4 = points[2];
                }
                else
                {
                    p1 = new Point(points[0].X - (points[1].X - points[0].X), points[0].Y - (points[1].Y - points[0].Y));
                    p2 = points[0];
                    p3 = points[1];
                    p4 = new Point(points[points.Length - 1].X - (points[points.Length - 2].X - points[points.Length - 1].X),
                                   points[points.Length - 1].Y - (points[points.Length - 2].Y - points[points.Length - 1].Y));
                }
            }
            else if (cpIndex < points.Length - 2)
            {
                p1 = points[cpIndex - 1];
                p2 = points[cpIndex];
                p3 = points[cpIndex + 1];
                p4 = points[cpIndex + 2];
            }
            else
            {
                p1 = points[points.Length - 3];
                p2 = points[points.Length - 2];
                p3 = points[points.Length - 1];
                p4 = new Point(points[points.Length - 1].X - (points[points.Length - 2].X - points[points.Length - 1].X),
                               points[points.Length - 1].Y - (points[points.Length - 2].Y - points[points.Length - 1].Y));
            }
            
            return CalculateCardinalSplinePoint(s, t, p1, p2, p3, p4);
        }
        
        private static List<Point> CreateCardinalSpline(int intermediatePointCount, double tension, params Point[] points)
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

        private static List<double> MeasureCardinalSpline(double tension, Point[] controlPoints)
        {
            double s = (1 - tension) / 2;

            Point reflectedStart = new Point(controlPoints[0].X - (controlPoints[1].X - controlPoints[0].X), controlPoints[0].Y - (controlPoints[1].Y - controlPoints[0].Y));
            Point reflectedEnd = new Point(controlPoints[controlPoints.Length - 1].X - (controlPoints[controlPoints.Length - 2].X - controlPoints[controlPoints.Length - 1].X),
                                           controlPoints[controlPoints.Length - 1].Y - (controlPoints[controlPoints.Length - 2].Y - controlPoints[controlPoints.Length - 1].Y));

            List<double> outputDistances = new List<double>();

            Point p1, p2, p3, p4;
            Point previousPoint = controlPoints.First();
            double cumulativeDistance = 0.0;

            for (int pointIndex = 0; pointIndex < controlPoints.Length - 1; pointIndex++)
            {
                cumulativeDistance += (previousPoint - controlPoints[pointIndex]).Length;
                outputDistances.Add(cumulativeDistance);
                previousPoint = controlPoints[pointIndex];

                if (pointIndex == 0)
                {
                    if (controlPoints.Length > 2)
                    {
                        p1 = reflectedStart;
                        p2 = controlPoints[0];
                        p3 = controlPoints[1];
                        p4 = controlPoints[2];
                    }
                    else
                    {
                        // Very short spline
                        p1 = reflectedStart;
                        p2 = controlPoints[0];
                        p3 = controlPoints[1];
                        p4 = reflectedEnd;
                    }
                }
                else if (pointIndex < controlPoints.Length - 2)
                {
                    p1 = controlPoints[pointIndex - 1];
                    p2 = controlPoints[pointIndex];
                    p3 = controlPoints[pointIndex + 1];
                    p4 = controlPoints[pointIndex + 2];
                }
                else
                {
                    p1 = controlPoints[controlPoints.Length - 3];
                    p2 = controlPoints[controlPoints.Length - 2];
                    p3 = controlPoints[controlPoints.Length - 1];
                    p4 = reflectedEnd;
                }

                for (int i = 0; i < 50; i++)
                {
                    double t = (i + 1.0) / (50 + 1.0);
                    Point newPoint = CalculateCardinalSplinePoint(s, t, p1, p2, p3, p4);
                    cumulativeDistance += (previousPoint - newPoint).Length;
                    previousPoint = newPoint;
                }
            }

            cumulativeDistance += (previousPoint - controlPoints.Last()).Length;
            outputDistances.Add(cumulativeDistance);

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
    }

    [Serializable]
    public class SplinePositionReference
    {
        public SplinePositionReference(int c, double t)
        {
            controlPoint = c;
            this.t = t;
        }

        int controlPoint;

        public int ControlPoint
        {
            get { return controlPoint; }
            set { controlPoint = value; }
        }
        double t;

        public double T
        {
            get { return t; }
            set { t = value; }
        }
    }
}
