using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace PROProtocol
{
    public class Language
    {
        private const string FileName = "Resources/Lang.xml";

        private Dictionary<string, string> _texts = new Dictionary<string, string>();

        public Language()
        {
            if (File.Exists(FileName))
            {
                XmlDocument xml = new XmlDocument();
                xml.Load(FileName);
                LoadXmlDocument(xml);
            }
        }

        private void LoadXmlDocument(XmlDocument xml)
        {
            XmlNode languageNode = xml.DocumentElement.GetElementsByTagName("English")[0];
            if (languageNode != null)
            {
                foreach (XmlElement textNode in languageNode)
                {
                    _texts.Add(textNode.GetAttribute("name"), textNode.InnerText);
                }
            }
        }

        public string GetText(string name)
        {
            return _texts[name];
        }

        public string Replace(string originalText)
        {
            if (originalText.IndexOf('$') != -1)
            {
                foreach (var text in _texts)
                {
                    originalText = originalText.Replace("$" + text.Key, text.Value);
                }
            }
            return originalText;
        }
    }
}
