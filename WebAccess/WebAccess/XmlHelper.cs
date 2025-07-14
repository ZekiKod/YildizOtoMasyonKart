using System.Xml;

namespace WebAccess
{
    using System.Xml;

    public class XmlHelper
    {
        public static void CreateOrUpdateXml(string filePath, string reader, string kartno)
        {
            XmlDocument doc = new XmlDocument();

            XmlElement root = doc.CreateElement("Settings");
            doc.AppendChild(root);

            XmlElement veri1Element = doc.CreateElement("reader");
            veri1Element.InnerText = reader;
            root.AppendChild(veri1Element);

            XmlElement veri2Element = doc.CreateElement("kartno");
            veri2Element.InnerText = kartno;
            root.AppendChild(veri2Element);

            doc.Save(filePath);
        }

        public static (string reader, string kartno) ReadXml(string filePath)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(filePath);

            XmlNode veri1Node = doc.SelectSingleNode("/Settings/reader");
            XmlNode veri2Node = doc.SelectSingleNode("/Settings/kartno");

            string reader = veri1Node?.InnerText ?? string.Empty;
            string kartno = veri2Node?.InnerText ?? string.Empty;

            return (reader, kartno);
        }
    }

}
