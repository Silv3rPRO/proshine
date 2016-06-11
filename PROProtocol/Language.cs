using System.Collections.Generic;
using System.Xml;

namespace PROProtocol
{
    public class Language
    {
        private Dictionary<string, string> _texts;

        public Language()
        {
            XmlDocument xml = new XmlDocument();
            xml.Load("Resources/Lang.xml");

            _texts = new Dictionary<string, string>();
            
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
