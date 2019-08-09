using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Windows;

namespace RootNav.Data.IO.RSML
{
    public class RSMLRootWriter : IRootWriter
    {
        System.Globalization.CultureInfo nullInfo = new System.Globalization.CultureInfo("");
        string initialDirectory;
        Random r = new Random();

        public RSMLRootWriter(ConnectionParams connectionInfo)
        {
            initialDirectory = connectionInfo.Directory;
        }

        public bool Initialise()
        {
            if (!Directory.Exists(initialDirectory))
            {
                // Attempt to create directory
                try
                {
                    return Directory.CreateDirectory(initialDirectory).Exists;
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                // Directory already exist
                return true;
            }
        }

        public bool Write(SceneMetadata metadata, SceneInfo scene)
        {
            // Create XML document
            XmlDocument document = new XmlDocument();
            
            // RSML node
            XmlNode rsmlNode = document.CreateElement("rsml");
            document.AppendChild(rsmlNode);

            // Metadata
            rsmlNode.AppendChild(CreateMetadata(document, metadata));

            // Scene
            rsmlNode.AppendChild(CreateScene(document, scene));
            
            // Write document to file
            if (initialDirectory.Last() != '\\')
            {
                initialDirectory += '\\';
            }
            
            string fileName = metadata.Key;

            foreach (char c in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c, '_');
            }

            string file = System.IO.Path.Combine(initialDirectory, fileName + ".rsml");
            document.Save(file);

            return true;
        }

        private XmlNode CreateMetadata(XmlDocument document, SceneMetadata metadata)
        {
            // Create metadata node
            XmlNode metadataNode = document.CreateElement("metadata");

            // Version
            metadataNode.AppendChild(document.CreateElement("version")).InnerText = metadata.Version;

            // Unit
            metadataNode.AppendChild(document.CreateElement("unit")).InnerText = metadata.Unit != "" ? metadata.Unit : "pixel";

            // Resolution
            metadataNode.AppendChild(document.CreateElement("resolution")).InnerText = metadata.Unit != "" ? string.Format(nullInfo, "{0}", metadata.Resolution) : "1";

            // Last-modified
            metadataNode.AppendChild(document.CreateElement("last-modified")).InnerText = DateTime.Now.ToString("s");

            // Software
            metadataNode.AppendChild(document.CreateElement("software")).InnerText = metadata.Software != "" ? metadata.Software : "RootNav";

            // User
            metadataNode.AppendChild(document.CreateElement("user")).InnerText = metadata.User != "" ? metadata.User : System.Environment.UserName;

            // File Key - Should be created before RSML is written to ensure uniqueness
            metadataNode.AppendChild(document.CreateElement("file-key")).InnerText = metadata.Key;

            // Property definitions
            if (metadata.PropertyDefinitions != null && metadata.PropertyDefinitions.Count > 0)
            {
                XmlNode propertyDefinitionsNode = metadataNode.AppendChild(document.CreateElement("property-definitions"));
                foreach (PropertyDefinition definition in metadata.PropertyDefinitions)
                {
                    XmlNode definitionNode = propertyDefinitionsNode.AppendChild(document.CreateElement("property-definition"));
                    definitionNode.AppendChild(document.CreateElement("label")).InnerText = definition.Label;
                    definitionNode.AppendChild(document.CreateElement("type")).InnerText = definition.Type.ToString().ToLower();
                    
                    // Add unit if available
                    if (definition.Unit != null && definition.Unit != "")
                    {
                        definitionNode.AppendChild(document.CreateElement("unit")).InnerText = definition.Unit.ToLower();
                    }
                    //definitionNode.AppendChild(document.CreateElement("unit")).InnerText = definition.Label;
                    
                    if (definition.DefaultValue != null)
                    {
                        definitionNode.AppendChild(document.CreateElement("default")).InnerText = string.Format(nullInfo, "{0}", definition.DefaultValue).ToLower();
                    }
                }
            }

            // Time sequence
            if (metadata.Sequence != null)
            {
                XmlNode sequenceNode = metadataNode.AppendChild(document.CreateElement("time-sequence"));
                sequenceNode.AppendChild(document.CreateElement("label")).InnerText = metadata.Sequence.Label;
                sequenceNode.AppendChild(document.CreateElement("index")).InnerText = metadata.Sequence.Index.ToString();
                sequenceNode.AppendChild(document.CreateElement("unified")).InnerText = metadata.Sequence.Unified.ToString();
            }

            // Image
            if (metadata.Image != null)
            {
                XmlNode imageNode = metadataNode.AppendChild(document.CreateElement("image"));
                
                if (metadata.Image.Label != "")
                    imageNode.AppendChild(document.CreateElement("label")).InnerText = metadata.Image.Label;

                if (metadata.Image.Hash != "")
                    imageNode.AppendChild(document.CreateElement("sha256")).InnerText = metadata.Image.Hash;

                if (metadata.Image.Captured.HasValue)
                {
                    imageNode.AppendChild(document.CreateElement("captured")).InnerText = metadata.Image.Captured.Value.ToString("s");
                }
            }

            return metadataNode;
        }

        private XmlNode CreateScene(XmlDocument document, SceneInfo scene)
        {
            XmlNode sceneNode = document.CreateElement("scene");

            // Properties
            if (scene.Properties != null && scene.Properties.Count > 0)
            {
                sceneNode.AppendChild(CreateProperties(document, scene.Properties));
            }

            // Plants
            foreach (PlantInfo plant in scene.Plants)
            {
                sceneNode.AppendChild(CreatePlant(document, plant));
            }
            
            // Annotations
            if (scene.Annotations != null && scene.Annotations.Count > 0)
            {
                sceneNode.AppendChild(CreateAnnotations(document, scene.Annotations));
            }

            return sceneNode;
        }

        private XmlNode CreatePlant(XmlDocument document, PlantInfo plant)
        {
            XmlNode plantNode = document.CreateElement("plant");

            // Attributes
            string id = plant.RsmlID != null && plant.RsmlID != "" ? plant.RsmlID : plant.RelativeID;
            plantNode.Attributes.Append(document.CreateAttribute("ID")).InnerText = id;
            
            if (plant.Label != "")
            {
                plantNode.Attributes.Append(document.CreateAttribute("label")).InnerText = plant.Label;
            }

            // Properties
            if (plant.Properties != null && plant.Properties.Count > 0)
            {
                plantNode.AppendChild(CreateProperties(document, plant.Properties));
            }

            // Roots
            foreach (RootInfo root in plant.Roots)
            {
                plantNode.AppendChild(CreateRoot(document, root));
            }

            // Annotations
            if (plant.Annotations != null && plant.Annotations.Count > 0)
            {
                plantNode.AppendChild(CreateAnnotations(document, plant.Annotations));
            }

            return plantNode;
        }

        private XmlNode CreateRoot(XmlDocument document, RootInfo root)
        {
            XmlNode rootNode = document.CreateElement("root");

            // Attributes
            string id = root.RsmlID != "" ? root.RsmlID : root.RelativeID;
            rootNode.Attributes.Append(document.CreateAttribute("ID")).InnerText = id;
            if (root.Label != "")
            {
                rootNode.Attributes.Append(document.CreateAttribute("label")).InnerText = root.Label;
            }

            // Properties
            if (root.Properties != null && root.Properties.Count > 0)
            {
                rootNode.AppendChild(CreateProperties(document, root.Properties));
            }

            // Geometry
            rootNode.AppendChild(CreateGeometry(document, root));

            // Functions
            if (root.Functions != null && root.Functions.Count > 0)
            {
                rootNode.AppendChild(CreateFunctions(document, root.Functions));
            }

            // Child Roots
            if (root.Children != null)
            {
                foreach (RootInfo childRoot in root.Children)
                {
                    rootNode.AppendChild(CreateRoot(document, childRoot));
                }
            }

            // Annotations
            if (root.Annotations != null && root.Annotations.Count > 0)
            {
                rootNode.AppendChild(CreateAnnotations(document, root.Annotations));
            }

            return rootNode;
        }

        private XmlNode CreateSample(XmlDocument document, Sample sample, bool usePosition)
        {
            XmlNode sampleNode = document.CreateElement("sample");
            
            if (usePosition && !double.IsNaN(sample.Position))
            {
                sampleNode.Attributes.Append(document.CreateAttribute("position")).InnerText = string.Format(nullInfo, "{0}", sample.Position);
            }
            
            sampleNode.Attributes.Append(document.CreateAttribute("value")).InnerText = string.Format(nullInfo, "{0}", sample.Value);

            return sampleNode;
        }

        private XmlNode CreateFunctions(XmlDocument document, List<Function> functions)
        {
            XmlNode functionsNode = document.CreateElement("functions");

            foreach (Function function in functions)
            {
                XmlNode functionNode = functionsNode.AppendChild(document.CreateElement("function"));

                functionNode.Attributes.Append(document.CreateAttribute("name")).InnerText = function.Name;

                // Use reflection to determine the type of function we are handling
                string functionDomain = function is PolylineFunction ? "polyline" : function is UniformFunction ? "uniform" : "length";

                functionNode.Attributes.Append(document.CreateAttribute("domain")).InnerText = functionDomain;

                // Append all samples
                foreach (Sample s in function.Samples)
                {
                    functionNode.AppendChild(CreateSample(document, s, functionDomain == "length"));
                }
            }

            return functionsNode;
        }

        private XmlNode CreateAnnotations(XmlDocument document, List<Annotation> annotations)
        {
            XmlNode annotationsNode = document.CreateElement("annotations");

            if (annotations != null && annotations.Count > 0)
            {
                foreach (Annotation a in annotations)
                {
                    XmlNode annotationNode = annotationsNode.AppendChild(document.CreateElement("annotation"));
                    annotationNode.Attributes.Append(document.CreateAttribute("name")).InnerText = a.Name;

                    foreach (Point3D point in a.Points)
                    {
                        annotationNode.AppendChild(CreatePoint(document, point));
                    }

                    annotationNode.AppendChild(document.CreateElement("value")).InnerText = a.Value;
                    annotationNode.AppendChild(document.CreateElement("software")).InnerText = a.Software;
                }
            }

            return annotationsNode;
        }

        private XmlNode CreatePoint(XmlDocument document, Point3D point)
        {
            XmlNode pointNode = document.CreateElement("point");

            // Write x, y and optional z
            XmlAttribute xAttr = document.CreateAttribute("x");
            xAttr.InnerText = string.Format(nullInfo, "{0}", Math.Round(point.X, 5));
            pointNode.Attributes.Append(xAttr);
            
            XmlAttribute yAttr = document.CreateAttribute("y");
            yAttr.InnerText = string.Format(nullInfo, "{0}",Math.Round(point.Y, 5));
            pointNode.Attributes.Append(yAttr);

            if (!double.IsNaN(point.Z))
            {
                XmlAttribute zAttr = document.CreateAttribute("z");
                zAttr.InnerText = string.Format(nullInfo, "{0}",Math.Round(point.Z, 5));
                pointNode.Attributes.Append(zAttr);
            }

            return pointNode;
        }

        private XmlNode CreateProperties(XmlDocument document, List<Property> properties)
        {
            XmlNode propertiesNode = document.CreateElement("properties");

            if (properties != null && properties.Count > 0)
            {
                foreach (Property p in properties)
                {
                    propertiesNode.AppendChild(document.CreateElement(p.Label)).InnerText = p.Value;
                }
            }

            return propertiesNode;
        }

        private XmlNode CreateGeometry(XmlDocument document, RootInfo root)
        {
            XmlNode geometryNode = document.CreateElement("geometry");

            // Polyline
            if (root.Polyline == null || root.Polyline.Points.Count <= 0)
            {
                // No polyline exists, create point list from spline
                if (root.Polyline == null)
                {
                    root.Polyline = new Polyline() { Points = new List<Point3D>() };
                }

                foreach (var p in root.Spline.SampledPoints)
                {
                    root.Polyline.Points.Add(new Point3D(p.X, p.Y));
                }

                root.Polyline.Points = Optimise2DPoints(root.Polyline.Points, 3);
            }

            XmlNode polylineNode = geometryNode.AppendChild(document.CreateElement("polyline"));

            root.Polyline.Points = Optimise2DPoints(root.Polyline.Points, 3);

            foreach (Point3D point in root.Polyline.Points)
            {
                polylineNode.AppendChild(CreatePoint(document, point));
            }

            // Spline Geometry
            if (root.Spline != null)
            {
                XmlNode splineNode = geometryNode.AppendChild(document.CreateElement("rootnavspline"));
                splineNode.Attributes.Append(document.CreateAttribute("controlpointseparation")).InnerText = root.Spline.ControlPointSeparation.ToString();
                splineNode.Attributes.Append(document.CreateAttribute("tension")).InnerText = string.Format(nullInfo, "{0}", root.Spline.Tension);

                List<System.Windows.Point> controlPoints = root.Spline.ControlPoints;

                foreach (var p in controlPoints)
                {
                    splineNode.AppendChild(CreatePoint(document, new Point3D(p.X, p.Y)));
                }
            }

            return geometryNode;
        }

        private static List<Point3D> Optimise2DPoints(List<Point3D> points, double angleThreshold)
        {
            List<Point3D> optimisedPoints = new List<Point3D>();

            int wallPointCount = points.Count;

            Point3D currentPoint = new Point3D(0, 0), nextPoint = new Point3D(0, 0);
            Vector lineVector = new Vector(0, 0);

            Point3D startPoint = new Point3D(points[0].X, points[0].Y);
            optimisedPoints.Add(startPoint);

            for (int i = 1; i < wallPointCount; i++)
            {
                if (i == 1)
                {
                    currentPoint = points[i - 1];
                    nextPoint = points[i];
                    lineVector = new Vector(currentPoint.X - nextPoint.X, currentPoint.Y - nextPoint.Y);
                }
                else
                {
                    Point3D intermediatePoint = nextPoint;
                    nextPoint = points[i];
                    Vector nextVector = new Vector(intermediatePoint.X - nextPoint.X, intermediatePoint.Y - nextPoint.Y);

                    double angle = Math.Abs(Vector.AngleBetween(lineVector, nextVector));

                    // If we have curved too much
                    if (angle > angleThreshold)
                    {
                        currentPoint = intermediatePoint;
                        lineVector = new Vector(currentPoint.X - nextPoint.X, currentPoint.Y - nextPoint.Y);
                        optimisedPoints.Add(intermediatePoint);
                    }
                    else
                    {
                        lineVector = new Vector(currentPoint.X - nextPoint.X, currentPoint.Y - nextPoint.Y);
                    }
                }
            }

            // Draw final wall
            Point3D endPoint = new Point3D(nextPoint.X, nextPoint.Y);
            optimisedPoints.Add(endPoint);

            return optimisedPoints;
        }

        public bool Connected
        {
            get
            {
                return Directory.Exists(initialDirectory);
            }
        }
    }
}
