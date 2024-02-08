using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace PascalVOC
{
    [XmlRoot("", IsNullable = false)]   
    public class PascalXMLStructure
    {
        Annotation annotations = new Annotation();

        [XmlElement(ElementName = "annotations")]
        public Annotation Annotations { get => annotations; set => annotations = value; }        

        /// <summary>
        /// Parse the XML file string to Pascal object
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static PascalXMLStructure ParsePascal(string filename)
        {
            PascalXMLStructure structure = new PascalXMLStructure();
            XDocument xmlDoc = XDocument.Load(filename);
            string rootName = xmlDoc.Root.Name.ToString();

            structure.Annotations.Folder = xmlDoc.Root.Elements("folder").First().Value;
            structure.Annotations.Filename = xmlDoc.Root.Elements("filename").First().Value;
            structure.Annotations.Path = xmlDoc.Root.Elements("path").First().Value;
            structure.Annotations.Source.Database = xmlDoc.Root.Elements("source").Elements("database").First().Value;
            structure.Annotations.Size.Width = long.Parse(xmlDoc.Root.Elements("size").Elements("width").First().Value);
            structure.Annotations.Size.Height = long.Parse(xmlDoc.Root.Elements("size").Elements("height").First().Value);
            structure.Annotations.Size.Depth = long.Parse(xmlDoc.Root.Elements("size").Elements("depth").First().Value);
            structure.Annotations.Segmented = int.Parse(xmlDoc.Root.Elements("segmented").First().Value);
            int _Xmax = 0;
            int _Ymax = 0;
            int _Xmin = 0;
            int _Ymin = 0;
            int _centerX = 0;
            int _centerY = 0;
            int boxID = 0;
            foreach (var box in xmlDoc.Root.Elements("object"))
            {
                boxID = boxID + 1;
                if (box.Elements("bndbox").Elements("xmax").Count() > 0)
                {
                    _Xmax = int.Parse(box.Elements("bndbox").Elements("xmax").First().Value);
                    _Ymax = int.Parse(box.Elements("bndbox").Elements("ymax").First().Value);
                    _Ymin = int.Parse(box.Elements("bndbox").Elements("ymin").First().Value);
                    _Xmin = int.Parse(box.Elements("bndbox").Elements("xmin").First().Value);

                    _centerX = _Xmin + ((_Xmax - _Xmin) / 2);
                    _centerY = _Ymin + ((_Ymax - _Ymin) / 2);

                    string _confidence = "";
                    try
                    {
                        _confidence = box.Elements("conf").First().Value;
                    }
                    catch (Exception)
                    {
                        _confidence = "0.0";
                    }

                    structure.Annotations.ImgObject.Add(new ImgBoxObject
                    {
                        Id = boxID,
                        Name = box.Elements("name").First().Value,
                        Pose = box.Elements("pose").First().Value,
                        Truncated = int.Parse(box.Elements("truncated").First().Value),
                        Difficult = int.Parse(box.Elements("difficult").First().Value),
                        Conf = double.Parse(_confidence, NumberStyles.Any, CultureInfo.InvariantCulture),
                        Bndbox = new Boundbox
                        {
                            Xmax = _Xmax,
                            Ymax = _Ymax,
                            Ymin = _Ymin,
                            Xmin = _Xmin,
                            CenterX = _centerX,
                            CenterY = _centerY
                        }
                    });
                }

            }
            return structure;
        }

        /// <summary>
        /// Save to Pascal xml format
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="xmlStructure"></param>
        public static void SavePascal(string filename, Annotation xmlStructure)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Annotation));
            XmlWriterSettings xmlWriterSettings = new XmlWriterSettings
            {
                OmitXmlDeclaration = true
            };
            XmlSerializerNamespaces xmlSerializerNamespaces = new XmlSerializerNamespaces();
            xmlSerializerNamespaces.Add(string.Empty, string.Empty);


            using (FileStream stream = new FileStream(filename, FileMode.Create))
            {
                using (XmlWriter xmlWriter = XmlWriter.Create(stream, xmlWriterSettings))
                {
                    serializer.Serialize(xmlWriter, xmlStructure, xmlSerializerNamespaces);
                }
            }
        }

        /// <summary>
        /// Parse the TXT Yolo file format into a Pascal object.
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="labels"></param>
        /// <param name="imageWidth"></param>
        /// <param name="imageHeight"></param>
        /// <param name="imageDepth"></param>
        /// <returns></returns>
        public static PascalXMLStructure ParseYolo(string filename, string[] labels, int imageWidth, int imageHeight, int imageDepth)
        {
            PascalXMLStructure structure = new PascalXMLStructure();
            try
            {
                structure.Annotations.Folder = Path.GetDirectoryName(filename);
                structure.Annotations.Path = "";
                structure.Annotations.Filename = Path.GetFileName(filename);
                structure.Annotations.Size = new ImgSize { Width = imageWidth, Height = imageHeight, Depth = imageDepth };
                Source src = new Source();
                src.Database = "";
                structure.Annotations.Source = src;
                structure.Annotations.Segmented = 0;
                foreach (string line in File.ReadLines(filename))
                {
                    string[] items = line.Split(' ');
                    int classID = int.Parse(items[0], NumberStyles.Any, CultureInfo.InvariantCulture);
                    string clsname = labels[classID];
                    float cX = items.Count() > 0 ? float.Parse(items[1], NumberStyles.Any, CultureInfo.InvariantCulture) : 0.0f;
                    float cY = items.Count() > 1 ? float.Parse(items[2], NumberStyles.Any, CultureInfo.InvariantCulture) : 0.0f;
                    float boxW = items.Count() > 2 ? float.Parse(items[3], NumberStyles.Any, CultureInfo.InvariantCulture) : 0.0f;
                    float boxH = items.Count() > 3 ? float.Parse(items[4], NumberStyles.Any, CultureInfo.InvariantCulture) : 0.0f;
                    int _centerX = (int)(cX * imageWidth);
                    int _centerY = (int)(cY * imageHeight);
                    int _Xmin = _centerX - (int)(boxW * imageWidth) / 2;
                    int _Ymin = _centerY - (int)(boxH * imageHeight) / 2;
                    int _Xmax = _centerX + (int)(boxW * imageWidth) / 2;
                    int _Ymax = _centerY + (int)(boxH * imageHeight) / 2;

                    structure.Annotations.ImgObject.Add(new ImgBoxObject
                    {
                        Id = classID,
                        Name = clsname,
                        Pose = "",
                        Truncated = 0,
                        Difficult = 0,
                        Conf = items.Count() > 4 ? float.Parse(items[5], NumberStyles.Any, CultureInfo.InvariantCulture) : 0.0f,
                        Bndbox = new Boundbox
                        {
                            CenterX = _centerX,
                            CenterY = _centerY,
                            Xmax = _Xmax,
                            Ymax = _Ymax,
                            Ymin = _Ymin,
                            Xmin = _Xmin
                        }
                    });
                }

                return structure;
            }
            catch (Exception)
            {
                Debug.WriteLine("The exception occurs when reading the yolo txt. Probably wrong format.");
                return structure;
            }            
        }

        /// <summary>
        /// Save to Yolo TXT format
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="structure"></param>
        /// <param name="labels"></param>
        public static void SaveYolo(string filename, Annotation structure, Dictionary<string,int> labels)
        {
            using (StreamWriter writer = new StreamWriter(filename))
            {
                foreach (var item in structure.ImgObject) 
                {
                    string conf = item.Conf.ToString();
                    string className = labels[item.Name].ToString();
                    string centerX = ((double)(item.Bndbox.CenterX + item.Bndbox.Xmin) / (2 * structure.Size.Width)).ToString("0.0000");
                    string centerY = ((double)(item.Bndbox.CenterY + item.Bndbox.Ymin) / (2 * structure.Size.Height)).ToString("0.0000");
                    string boxWidth = ((double)((item.Bndbox.Xmax - item.Bndbox.Xmin)) / structure.Size.Width).ToString("0.0000");
                    string boxHeight = ((double)((item.Bndbox.Ymax - item.Bndbox.Ymin)) / structure.Size.Height).ToString("0.0000");
                    string yoloFormatLine = $"{labels[item.Name]} {centerX:F6} {centerY:F6} {boxWidth:F6} {boxHeight:F6} {conf}";
                    writer.WriteLine(yoloFormatLine);

                }
                
            }
        }

    }

    /*
    public class YoloTXTStructure
    {
        List<YoloAnnotation> yoloAnnotations = new List<YoloAnnotation>();

        /// <summary>
        /// YoloAnnotation list object
        /// </summary>
        public List<YoloAnnotation> YoloAnnotations { get => yoloAnnotations; set => yoloAnnotations = value; }
        /// <summary>
        /// Parse a yolo TXT file and returns a YoloAnnotations object - Class CenterX CenterY Width Height Class
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="labels"></param>
        /// <returns></returns>
        public static List<YoloAnnotation> ParseYolo(string filename, string[] labels)
        {
            List<YoloAnnotation> structure = new List<YoloAnnotation>();                        
            try
            {
                foreach (string line in File.ReadLines(filename))
                {
                    string[] items = line.Split(' ');
                    string clsname = labels[int.Parse(items[0], NumberStyles.Any, CultureInfo.InvariantCulture)];
                    structure.Add(new YoloAnnotation
                    {

                        className = clsname,
                        centerX = items.Count() > 0 ? float.Parse(items[1], NumberStyles.Any, CultureInfo.InvariantCulture) : 0.0f,
                        centerY = items.Count() > 1 ? float.Parse(items[2], NumberStyles.Any, CultureInfo.InvariantCulture) : 0.0f,
                        width = items.Count() > 2 ? float.Parse(items[3], NumberStyles.Any, CultureInfo.InvariantCulture) : 0.0f,
                        height = items.Count() > 3 ? float.Parse(items[4], NumberStyles.Any, CultureInfo.InvariantCulture) : 0.0f,
                        confidence = items.Count() > 4 ? float.Parse(items[5], NumberStyles.Any, CultureInfo.InvariantCulture) : 0.0f
                    });
                }
            }catch(Exception)
            {
                Debug.WriteLine("The exception occurs when reading the yolo txt. Probably wrong format.");
                return structure;
            }
            
            return structure;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filename"></param>
        /// <exception cref="NotImplementedException"></exception>
        public static void SaveYolo(string filename)
        {
            throw new NotImplementedException();
        }
    }

    public class YoloAnnotation
    {
        public string className = "";
        public float centerX = 0.0f;
        public float centerY = 0.0f;
        public float width = 0.0f;
        public float height = 0.0f;
        public float confidence = 0.0f;

    }*/

    /// <summary>
    /// Annotation Object Class
    /// </summary>    
    public class Annotation
    {
        string folder;
        string filename;
        string path;
        Source source = new Source();
        ImgSize size = new ImgSize();
        int segmented;
        List<ImgBoxObject> imgObject = new List<ImgBoxObject>();

        [XmlElement(ElementName = "folder")]
        public string Folder { get => folder; set => folder = value; }
        [XmlElement(ElementName = "filename")]
        public string Filename { get => filename; set => filename = value; }
        [XmlElement(ElementName = "path")]
        public string Path { get => path; set => path = value; }
        [XmlElement(ElementName = "segmented")]
        public int Segmented { get => segmented; set => segmented = value; }
        [XmlElement(ElementName = "source")]
        public Source Source { get => source; set => source = value; }
        [XmlElement(ElementName = "size")]
        public ImgSize Size { get => size; set => size = value; }
        [XmlElement(ElementName = "object")]
        public List<ImgBoxObject> ImgObject { get => imgObject; set => imgObject = value; }
    }

    /// <summary>
    /// Data source
    /// </summary>
    public class Source
    {
        string database;
        [XmlElement(ElementName = "database")]
        public string Database { get => database; set => database = value; }
    }

    /// <summary>
    /// Size of the image
    /// </summary>
    public class ImgSize
    {
        long width;
        long height;
        long depth;
        [XmlElement(ElementName = "width")]
        public long Width { get => width; set => width = value; }
        [XmlElement(ElementName = "height")]
        public long Height { get => height; set => height = value; }
        [XmlElement(ElementName = "depth")]
        public long Depth { get => depth; set => depth = value; }
    }

    /// <summary>
    /// Defect annotation object. (Box), if the box has been evaluated
    /// </summary>
    public class ImgBoxObject
    {
        string name;
        int id;
        string pose;
        int truncated;
        int difficult;
        double conf;
        bool evaluated;
        Boundbox bndbox = new Boundbox();

        [XmlElement(ElementName = "difficult")]
        public int Difficult { get => difficult; set => difficult = value; }
        [XmlElement(ElementName = "truncated")]
        public int Truncated { get => truncated; set => truncated = value; }
        [XmlElement(ElementName = "pose")]
        public string Pose { get => pose; set => pose = value; }
        [XmlElement(ElementName = "name")]
        public string Name { get => name; set => name = value; }
        [XmlElement(ElementName = "evalated")]
        public bool Evaluated { get => evaluated; set => evaluated = value; }
        [XmlElement(ElementName = "conf")]
        public double Conf { get => conf; set => conf = value; }
        [XmlElement(ElementName = "bndbox")]
        public Boundbox Bndbox { get => bndbox; set => bndbox = value; }
        [XmlElement(ElementName = "id")]
        public int Id { get => id; set => id = value; }

    }

    /// <summary>
    /// The box size, center position
    /// </summary>
    public class Boundbox
    {
        int xmin;
        int ymin;
        int xmax;
        int ymax;
        int centerX;
        int centerY;
        [XmlElement(ElementName = "ymax")]
        public int Ymax { get => ymax; set => ymax = value; }
        [XmlElement(ElementName = "xmax")]
        public int Xmax { get => xmax; set => xmax = value; }
        [XmlElement(ElementName = "ymin")]
        public int Ymin { get => ymin; set => ymin = value; }
        [XmlElement(ElementName = "xmin")]
        public int Xmin { get => xmin; set => xmin = value; }
        [XmlElement(ElementName = "centerX")]
        public int CenterX { get => centerX; set => centerX = value; }
        public int CenterY { get => centerY; set => centerY = value; }

    }

}