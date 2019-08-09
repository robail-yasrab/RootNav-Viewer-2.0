using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Windows.Media.Imaging;

namespace RootNav.Data.IO.RSML
{
    public class RSMLRootReader : IRootReader
    {
        string initialDirectory;
        Dictionary<string, string> fileMappings = new Dictionary<string, string>();
        Random r = new Random();
        System.Globalization.CultureInfo nullInfo = new System.Globalization.CultureInfo("");

        public RSMLRootReader(ConnectionParams connectionInfo)
        {
            initialDirectory = connectionInfo.Directory;
        }

        public bool Initialise()
        {
            if (!Directory.Exists(initialDirectory))
            {
                return false;
            }

            HashSet<string> fileExtensions = new HashSet<string>() { ".rsml", ".RSML" };

            foreach (string file in Directory.EnumerateFiles(initialDirectory, "*", SearchOption.AllDirectories))
            {
                if (!fileExtensions.Contains(Path.GetExtension(file)))
                    continue;

                XmlReaderSettings xrs = new XmlReaderSettings();

                // TODO: Schema Validation

                using (XmlReader reader = XmlReader.Create(file))
                {
                    if (reader.ReadToDescendant("metadata"))
                    {
                        XElement el = (XElement)XNode.ReadFrom(reader);

                        //XElement keyElement = 
                        var keyDescendants = el.Descendants("file-key").ToList();

                        if (keyDescendants.Count > 0)
                        {
                            string keyValue = keyDescendants[0].Value;

                            fileMappings.Add(keyValue, file);
                        }
                    }
                    else
                    {
                        // RSML Specification, no file-key means the format isn't valid.
                    }
                }
            }

            return fileMappings.Count > 0;
        }

        public Tuple<SceneMetadata, SceneInfo, ImageCollection> Read(string tag, bool image)
        {
            // Read plant information from an xml file.
            if (!fileMappings.ContainsKey(tag))
            {
                return null;
            }

            List<PlantInfo> plants = new List<PlantInfo>();

            // Load document
            XmlDocument doc = new XmlDocument();
            doc.Load(fileMappings[tag]);

            XmlNode metadataNode = doc.SelectSingleNode("/rsml/metadata");

            SceneMetadata metadata = ReadMetadata(metadataNode);

            XmlNode sceneNode = doc.SelectSingleNode("/rsml/scene");
            SceneInfo scene = ReadScene(sceneNode);

            var imageGroup = new ImageCollection();
            if (image)
            {
                imageGroup = ImageFinder.ImageSearch(Path.GetDirectoryName(fileMappings[tag]), tag);
            }

            return new Tuple<SceneMetadata,SceneInfo, ImageCollection>(metadata, scene, imageGroup);
        }

        private SceneMetadata ReadMetadata(XmlNode metadataNode)
        {
            SceneMetadata data = new SceneMetadata();

            foreach (XmlNode child in metadataNode.ChildNodes)
            {
                switch (child.Name)
                {
                    case "version":
                        data.Version = child.InnerText;
                        break;
                    case "unit":
                        data.Unit = child.InnerText;
                        break;
                    case "resolution":
                        double res;
                        if (double.TryParse(child.InnerText, System.Globalization.NumberStyles.Any, nullInfo, out res))
                        {
                            data.Resolution = res;
                        }
                        break;
                    case "last-modified":
                        DateTime val;
                        if (DateTime.TryParse(child.InnerText, out val))
                        {
                            data.LastModified = val;
                        }
                        break;
                    case "software":
                        data.Software = child.InnerText;
                        break;
                    case "user":
                        data.User = child.InnerText;
                        break;
                    case "file-key":
                        data.Key = child.InnerText;
                        break;
                    case "property-definitions":
                        // For each property-definition
                        data.PropertyDefinitions = new List<PropertyDefinition>();

                        foreach (XmlNode propertyDefinitionNode in child.SelectNodes("property-definition"))
                        {
                            XmlNode labelNode = propertyDefinitionNode.SelectSingleNode("label");
                            XmlNode typeNode = propertyDefinitionNode.SelectSingleNode("type");
                            XmlNode defaultNode = propertyDefinitionNode.SelectSingleNode("default");
                            XmlNode unitNode = propertyDefinitionNode.SelectSingleNode("unit");

                            if (labelNode == null || typeNode == null || defaultNode == null)
                            {
                                // Invalid property definition
                                continue;
                            }

                            string unit = unitNode != null && unitNode.InnerText != "" ? unitNode.InnerText : null;

                            string typeString = typeNode.InnerText.Trim(new char[] { '\'', '"', ' ' });

                            // Only create a property if it is of a valid type
                            switch (typeString)
                            {
                                case "boolean":
                                    bool defaultBool = false;
                                    if (Boolean.TryParse(defaultNode.InnerText, out defaultBool))
                                    {
                                        data.PropertyDefinitions.Add(new PropertyDefinition(labelNode.InnerText, PropertyDefinition.PropertyType.Boolean, defaultBool, unit));
                                    }
                                    break;

                                case "string":
                                    string defaultString = defaultNode.InnerText.Trim(new char[] { ' ', '"', '\'' });
                                    if (defaultString != "")
                                    {
                                        data.PropertyDefinitions.Add(new PropertyDefinition(labelNode.InnerText, PropertyDefinition.PropertyType.String, defaultString, unit));
                                    }
                                    break;
                                case "integer":
                                    int defaultInt = 0;
                                    if (Int32.TryParse(defaultNode.InnerText, out defaultInt))
                                    {
                                        data.PropertyDefinitions.Add(new PropertyDefinition(labelNode.InnerText, PropertyDefinition.PropertyType.Integer, defaultInt, unit));
                                    }
                                    break;
                                case "float":
                                    float defaultFloat = 0;
                                    if (float.TryParse(defaultNode.InnerText, System.Globalization.NumberStyles.Any, nullInfo, out defaultFloat))
                                    {
                                        data.PropertyDefinitions.Add(new PropertyDefinition(labelNode.InnerText, PropertyDefinition.PropertyType.Float, defaultFloat, unit));
                                    }
                                    break;
                                case "double":
                                    double defaultDouble = 0;
                                    if (double.TryParse(defaultNode.InnerText, System.Globalization.NumberStyles.Any, nullInfo, out defaultDouble))
                                    {
                                        data.PropertyDefinitions.Add(new PropertyDefinition(labelNode.InnerText, PropertyDefinition.PropertyType.Double, defaultDouble, unit));
                                    }
                                    break;
                            }
                        }
                        break;
                    case "time-sequence":
                        data.Sequence = new SceneMetadata.TimeSequence();
                        foreach (XmlNode sequenceChild in child.ChildNodes)
                        {
                            if (sequenceChild.Name == "label")
                            {
                                data.Sequence.Label = sequenceChild.InnerText;
                            }
                            else if (sequenceChild.Name == "index")
                            {
                                int index;
                                if (Int32.TryParse(sequenceChild.InnerText, out index))
                                {
                                    data.Sequence.Index = index;
                                }
                            }
                            else if (sequenceChild.Name == "unified")
                            {
                                bool unified;
                                if (bool.TryParse(sequenceChild.InnerText, out unified))
                                {
                                    data.Sequence.Unified = unified;
                                }
                            }
                        }
                        break;
                    case "image":
                        data.Image = new SceneMetadata.ImageInfo();
                        foreach (XmlNode imageChild in child.ChildNodes)
                        {
                            if (imageChild.Name == "label")
                            {
                                data.Image.Label = imageChild.InnerText;
                            }
                            else if (imageChild.Name == "sha256")
                            {
                                data.Image.Hash = imageChild.InnerText;
                            }
                            else if (imageChild.Name == "captured")
                            {
                                DateTime captured;
                                if (DateTime.TryParse(imageChild.InnerText, out captured))
                                {
                                    data.Image.Captured = captured;
                                }
                            }
                        }
                        break;
                }
            }

            return data;
        }

        private SceneInfo ReadScene(XmlNode sceneNode)
        {
            SceneInfo scene = new SceneInfo() { Plants = new List<PlantInfo>() };

            // Read scene properties
            XmlNode propertiesNode = sceneNode.SelectSingleNode("properties");
            if (propertiesNode != null)
            {
                scene.Properties = ReadProperties(propertiesNode);
            }

            int plantID = 1;
            foreach (XmlNode plant in sceneNode.SelectNodes("plant"))
            {
                scene.Plants.Add(ReadPlant(plant, plantID++.ToString()));
            }

            // Scene annotations
            XmlNode annotationsNode = sceneNode.SelectSingleNode("annotations");
            if (annotationsNode != null)
            {
                scene.Annotations = ReadAnnotations(annotationsNode);
            }
            
            return scene;
        }

        private PlantInfo ReadPlant(XmlNode plantNode, string RelativeID)
        {
            PlantInfo plant = new PlantInfo() { Roots = new List<RootInfo>() };
            
            // Attributes
            XmlAttribute id = plantNode.Attributes["ID"] == null ? plantNode.Attributes["id"] : plantNode.Attributes["ID"];
            if (id != null)
            {
                plant.RsmlID = id.InnerText;
            }

            XmlAttribute label = plantNode.Attributes["label"];
            if (label != null)
            {
                plant.Label = label.InnerText;
            }

            // RelativeID
            plant.RelativeID = RelativeID;

            // If no id was supplied in the rsml file, use the relative ID.
            if (plant.RsmlID == null) plant.RsmlID = RelativeID;

            // Plant properties
            XmlNode propertiesNode = plantNode.SelectSingleNode("properties");
            if (propertiesNode != null)
            {
                plant.Properties = ReadProperties(propertiesNode);
            }

            // Roots
            int rootID = 1;
            foreach (XmlNode rootNode in plantNode.SelectNodes("root"))
            {
                plant.Roots.Add(ReadRoot(rootNode, plant.RelativeID + "." + rootID++.ToString(), null));
            }

            // Annotations
            XmlNode annotationsNode = plantNode.SelectSingleNode("annotations");
            if (annotationsNode != null)
            {
                plant.Annotations = ReadAnnotations(annotationsNode);
            }

            return plant;
        }

        private RootInfo ReadRoot(XmlNode rootNode, string RelativeID, RootInfo parent)
        {
            RootInfo root = new RootInfo() { Children = new List<RootInfo>() };
            
            // Attributes
            XmlAttribute id = rootNode.Attributes["ID"] == null ? rootNode.Attributes["id"] : rootNode.Attributes["ID"];
            if (id != null)
            {
                root.RsmlID = id.InnerText;
            }

            XmlAttribute label = rootNode.Attributes["label"];
            if (label != null)
            {
                root.Label = label.InnerText;
            }

            // RelativeID
            root.RelativeID = RelativeID;

            // If no id was supplied in the rsml file, use the relative ID.
            if (root.RsmlID == null) root.RsmlID = RelativeID;

            // Root Properties
            XmlNode propertiesNode = rootNode.SelectSingleNode("properties");
            if (propertiesNode != null)
            {
                root.Properties = ReadProperties(propertiesNode);
            }

            // Geometry - not currently preserved
            XmlNode geometry = rootNode.SelectSingleNode("geometry");
            if (geometry != null)
            {
                Polyline polyline;
                SampledSpline spline;
                
                ReadGeometry(geometry, out polyline, out spline);
                root.Polyline = polyline;
                root.Spline = spline;
            }
            
            // Start position
            if (parent != null)
            {
                var start = root.Spline.Start;
                root.StartReference = parent.Spline.GetPositionReference(start);
            }

            // Functions
            XmlNode functionsNode = rootNode.SelectSingleNode("functions");
            if (functionsNode != null)
            {
                root.Functions = ReadFunctions(functionsNode);
            }

            // Child Roots
            int rootID = 1;
            foreach (XmlNode childRootNode in rootNode.SelectNodes("root"))
            {
                root.Children.Add(ReadRoot(childRootNode, root.RelativeID + "." + rootID++.ToString(), root));
            }

            // Annotations
            XmlNode annotationsNode = rootNode.SelectSingleNode("annotations");
            if (annotationsNode != null)
            {
                root.Annotations = ReadAnnotations(annotationsNode);
            }

            return root;
        }

        private List<Property> ReadProperties(XmlNode propertiesNode)
        {
            List<Property> properties = new List<Property>();
            
            foreach (XmlNode propertyNode in propertiesNode.ChildNodes)
            {
                properties.Add(ReadProperty(propertyNode));
            }

            return properties;
        }

        private Property ReadProperty(XmlNode propertyNode)
        {
            return new Property(propertyNode.Name, propertyNode.InnerText);
        }

        private Point3D ReadPoint(XmlNode pointNode)
        { 
            // Parse x, y and optional z
            XmlAttribute xAttr = pointNode.Attributes["x"];
            XmlAttribute yAttr = pointNode.Attributes["y"];
            XmlAttribute zAttr = pointNode.Attributes["z"];

            if (xAttr == null || yAttr == null)
            {
                throw new InvalidOperationException("Invalid point in polyline geometry");
            }

            double x = double.Parse(xAttr.InnerText, nullInfo);
            double y = double.Parse(yAttr.InnerText, nullInfo);

            if (zAttr == null)
            {
                return new Point3D(x, y);
            }
            else
            {
                return new Point3D(x, y, double.Parse(zAttr.InnerText, nullInfo));
            }

        }

        private Sample ReadSample(XmlNode sampleNode)
        {
            // Parse value and optional position
            XmlAttribute positionAttr = sampleNode.Attributes["position"];
            XmlAttribute valueAttr = sampleNode.Attributes["value"];

            if (valueAttr == null)
            {
                throw new InvalidOperationException("Invalid sample in function");
            }

            double value = double.Parse(valueAttr.InnerText, nullInfo);

            if (positionAttr == null)
            {
                return new Sample(value);
            }
            else
            {
                return new Sample(value, double.Parse(positionAttr.InnerText, nullInfo));
            }
        }

        private List<Function> ReadFunctions(XmlNode functionsNode)
        {
            List<Function> functions = new List<Function>();
            foreach (XmlNode functionNode in functionsNode.SelectNodes("function"))
            {
                // Name attribute
                string name;
                XmlAttribute nameAttribute = functionNode.Attributes["name"];
                if (nameAttribute != null)
                {
                    name = nameAttribute.InnerText;
                }
                else
                {
                    name = "Unspecified";
                }

                // Domain attribute
                string domain;
                XmlAttribute domainAttribute = functionNode.Attributes["domain"];
                if (domainAttribute != null)
                {
                    domain = domainAttribute.InnerText;
                }
                else
                {
                    domain = "Unspecified";
                }

                string[] permittedDomains = new string[] { "polyline", "uniform", "length" };
                if (domain == "Unspecified" || !permittedDomains.Contains(domain))
                {
                    // Domain is mandetory, and must be one of polyline, uniform or length
                    continue;
                }

                List<Sample> samples = new List<Sample>();
                foreach (XmlNode sampleNode in functionNode.SelectNodes("sample"))
                {
                    samples.Add(ReadSample(sampleNode));
                }

                // A function must have at least one sample
                if (samples.Count < 1)
                {
                    continue;
                }

                switch (domain)
                {
                    case "polyline":
                        functions.Add(new PolylineFunction(name, samples));
                        break;
                    case "uniform":
                        functions.Add(new UniformFunction(name, samples));
                        break;
                    case "length":
                        // Optional origin attribute
                        XmlAttribute originAttribute = functionNode.Attributes["origin"];
                        if (originAttribute != null && originAttribute.InnerText == "base")
                        {
                            functions.Add(new LengthFunction(name, samples, LengthFunction.FunctionOrigin.RootBase));
                        }
                        else
                        {
                            // Either tip specified, or any other incorrect value
                            functions.Add(new LengthFunction(name, samples, LengthFunction.FunctionOrigin.RootTip));
                        }

                        break;
                }
            }

            return functions;
        }

        private List<Annotation> ReadAnnotations(XmlNode annotationsNode)
        {
            List<Annotation> annotations = new List<Annotation>();
            foreach (XmlNode annotationNode in annotationsNode.SelectNodes("annotation"))
            {
                // Name attribute
                string name;
                XmlAttribute nameAttribute = annotationNode.Attributes["name"];
                if (nameAttribute != null)
                {
                    name = nameAttribute.InnerText;
                }
                else
                {
                    name = "Unspecified";
                }

                List<Point3D> points = new List<Point3D>();
                foreach (XmlNode pointNode in annotationNode.SelectNodes("point"))
                {
                    points.Add(ReadPoint(pointNode));
                }

                // An annotation must have at least one point
                if (points.Count < 1)
                {
                    continue;
                }

                XmlNode valueNode = annotationNode.SelectSingleNode("value");
                XmlNode softwareNode = annotationNode.SelectSingleNode("software");

                // Annotation must have a value
                if (valueNode == null)
                {
                    continue;
                }

                string value = valueNode.InnerText;

                if (softwareNode == null)
                {
                    annotations.Add(new Annotation(name, points, value, null));
                }
                else
                {
                    annotations.Add(new Annotation(name, points, value, softwareNode.InnerText));
                }
            }

            return annotations;
        }

        private void ReadGeometry(XmlNode geometryNode, out Polyline polyLine, out SampledSpline spline)
        {
            polyLine = null; spline = null;

            // Polyline
            XmlNode polylineNode = geometryNode.SelectSingleNode("polyline");

            if (polylineNode != null)
            {
                polyLine = new Polyline() { Points = new List<Point3D>() };
                foreach (XmlNode pointNode in polylineNode.SelectNodes("point"))
                {
                    polyLine.Points.Add(ReadPoint(pointNode));
                }
            }


            // Load <spline></spline> tags if available
            XmlNode splineNode = geometryNode.SelectSingleNode("rootnavspline");
            if (splineNode != null)
            {
                List<System.Windows.Point> controlPoints = new List<System.Windows.Point>();

                foreach (XmlNode pointNode in splineNode.SelectNodes("point"))
                {
                    Point3D p = ReadPoint(pointNode);
                    controlPoints.Add(new System.Windows.Point(p.X, p.Y));
                }

                XmlNode tensionAttr = splineNode.Attributes["tension"];
                XmlNode separationAttr = splineNode.Attributes["controlpointseparation"];

                double tension;
                int controlPointSeparation;

                if (tensionAttr == null || !double.TryParse(tensionAttr.InnerText, System.Globalization.NumberStyles.Any, nullInfo, out tension))
                {
                    tension = 0.5;
                }

                if (separationAttr == null || !Int32.TryParse(separationAttr.InnerText, out controlPointSeparation))
                {
                    controlPointSeparation = 40;
                }

                spline = new SampledSpline();
                spline.InitialiseFromControlPoints(controlPoints, tension, controlPointSeparation);
            }
            else
            {
                // Create spline based on polyline
                List<System.Windows.Point> splinePoints = new List<System.Windows.Point>();
                if (polyLine != null)
                {

                    foreach (Point3D p in polyLine.Points)
                    {
                        splinePoints.Add(new System.Windows.Point(p.X, p.Y));
                    }
                }

                if (splinePoints.Count >= 2)
                {
                    spline = new SampledSpline();
                    spline.InitialiseWithVectorList(splinePoints);
                }
            }

            return;
        }

        public List<String> FilterTags(String[] searchTerms, bool any, bool ImageOnly, DateTime? date = null)
        {
            if (ImageOnly)
            {
                throw new NotImplementedException("RSMLRootReader does not implement image only searching yet.");
            }

            List<string> filteredTags = new List<string>();
            foreach (string s in fileMappings.Keys)
            {
                int count = 0;
                foreach (string term in searchTerms)
                {
                    // String found?
                    if (s.IndexOf(term) >= 0)
                    {
                        // Found any string = pass
                        if (any)
                        {
                            filteredTags.Add(s);
                            break;
                        }

                        // Count all strings
                        count++;
                    }
                    // Missed one of all = fail
                    else if (!any)
                    {
                        break;
                    }
                }

                // Found all terms - pass
                if (count == searchTerms.Length)
                {
                    filteredTags.Add(s);
                }
            }

            return filteredTags;
        }

        public List<String> ReadAllTags()
        {
            List<string> tags = new List<string>();
            foreach (string s in fileMappings.Keys)
            {
                tags.Add(s);
            }

            return tags;
        }

        public bool Connected
        {
            get
            {
                return fileMappings.Count > 0;
            }
        }
    }
}
