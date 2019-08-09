using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Windows.Media.Effects;

using RootNav.Data;
namespace RootNav.Viewer
{
    class AnnotationAdorner : Adorner
    {
        private SceneInfo scene = null;

        public SceneInfo Scene
        {
            get { return scene; }
            set
            {
                scene = value;
                this.InvalidateVisual();
            }
        }

        SolidColorBrush fill;
        Pen stroke;

        public double Left { get; set; }
        public double Top { get; set; }

        public AnnotationAdorner(UIElement adornedElement, SceneInfo scene) :
            base(adornedElement)
        {
            Color sceneFill = Color.FromArgb(100,80,255,80);
            Color sceneStroke = Color.FromArgb(255, 80, 255, 80);

            fill = new SolidColorBrush(sceneFill);
            stroke = new Pen(new SolidColorBrush(sceneStroke), 4.0);
            stroke.StartLineCap = PenLineCap.Round;
            stroke.EndLineCap = PenLineCap.Round;

            this.scene = scene;
        }   

        private bool ResetCalled { get; set; }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (ResetCalled)
            {
                ResetCalled = false;
                return;
            }

            base.OnRender(drawingContext);

            //drawingContext.DrawEllipse(Brushes.Red, null, new Point(50, 50), 10, 10);
            if (this.scene != null)
            {
                if (this.scene.Annotations != null && this.scene.Annotations.Count > 0)
                {
                    foreach (Annotation a in this.scene.Annotations)
                    {
                        Geometry g = GeometryFromAnnotation(a);
                        if (g != null)
                        {
                            drawingContext.DrawGeometry(fill, stroke, g);
                        }
                    }
                }
   
                if (this.scene.Plants != null)
                {
                    foreach (PlantInfo plant in this.scene.Plants)
                    {
                        if (plant.Annotations != null && plant.Annotations.Count > 0)
                        {
                            foreach (Annotation a in plant.Annotations)
                            {
                                Geometry g = GeometryFromAnnotation(a);
                                if (g != null)
                                {
                                    drawingContext.DrawGeometry(fill, stroke, g);
                                }
                            }
                        }

                        foreach (RootInfo root in plant)
                        {
                            if (root.Annotations != null && root.Annotations.Count > 0)
                            {
                                foreach (Annotation a in root.Annotations)
                                {
                                    Geometry g = GeometryFromAnnotation(a);
                                    if (g != null)
                                    {
                                        drawingContext.DrawGeometry(fill, stroke, g);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        protected Geometry GeometryFromAnnotation(Annotation annotation)
        {
            if (annotation == null || annotation.Points == null || annotation.Points.Count == 0)
                return null;
            
            if (annotation.Points.Count == 1)
            {
                Point p = new Point(annotation.Points[0].X + this.Left, annotation.Points[0].Y + this.Top);
                EllipseGeometry ellipse = new EllipseGeometry(p, 3.0, 3.0);

                if (ellipse.CanFreeze)
                {
                    ellipse.Freeze();
                }
               
                return ellipse;
            }
            else if (annotation.Points.Count == 2)
            {
                Point p1 = new Point(annotation.Points[0].X + this.Left, annotation.Points[0].Y + this.Top);
                Point p2 = new Point(annotation.Points[1].X + this.Left, annotation.Points[1].Y + this.Top);
                LineGeometry line = new LineGeometry(p1, p2);

                if (line.CanFreeze)
                {
                    line.Freeze();
                }

                return line;
            }
            else
            {
                StreamGeometry streamGeometry = new StreamGeometry();
                using (StreamGeometryContext sgc = streamGeometry.Open())
                {
                    Point start = new Point(annotation.Points[0].X + this.Left, annotation.Points[0].Y + this.Top);

                    sgc.BeginFigure(start, true, true);

                    for (int i = 1; i < annotation.Points.Count; i++)
                    {
                        Point next = new Point(annotation.Points[i].X + this.Left, annotation.Points[i].Y + this.Top);
                        sgc.LineTo(next, true, true);
                    }
                }

                if (streamGeometry.CanFreeze)
                {
                    streamGeometry.Freeze();
                }

                return streamGeometry;
            }
        }

    }
}
