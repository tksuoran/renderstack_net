using System;
using System.Xml;
using System.IO;

using RenderStack.Geometry;

namespace example.Brushes
{
    /*  Comment: Highly experimental  */ 
    [Serializable]
    public class XmlPolyhedron : RenderStack.Geometry.Geometry
    {
        public XmlPolyhedron(string file)
        {
            if(
                string.IsNullOrEmpty(file) || (File.Exists(file) == false)
            )
            {
                return;
            }
            XmlDocument xml = new XmlDocument();
            xml.Load(file);

            XmlNode m = xml.SelectSingleNode("m");
            if(m == null)
            {
                return;
            }

            try
            {
                foreach(XmlNode node in m.ChildNodes)
                {
                    if(node.Name == "p")
                    {
                        float x = 0.0f;
                        float y = 0.0f;
                        float z = 0.0f;

                        if(float.TryParse(node.Attributes["x"].Value, out x) == false)
                        {
                            return;
                        }
                        if(float.TryParse(node.Attributes["y"].Value, out y) == false)
                        {
                            return;
                        }
                        if(float.TryParse(node.Attributes["z"].Value, out z) == false)
                        {
                            return;
                        }
                        MakePoint(-x, -y, -z);
                    }
                    if(node.Name == "f")
                    {
                        Polygon polygon = MakePolygon();
                        if(node.ChildNodes == null)
                        {
                            continue;
                        }
                        foreach(XmlNode v in node.ChildNodes)
                        {
                            int index = 0;
                            if(int.TryParse(v.InnerText, out index) == false)
                            {
                                return;
                            }
                            Point point = Points[index];
                            polygon.MakeCorner(point);
                        }
                    }
                }
            }
            catch(System.Exception)
            {
                return;
            }
        }
    }
}
