using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace PROProtocol
{
    public class Language
    {
        private const string FileName = "Resources/Lang.xml";

        private readonly SortedDictionary<string, string> _texts;

        public Language()
        {
            _texts = new SortedDictionary<string, string>(new DescendingLengthComparer());

            if (File.Exists(FileName))
            {
                var xml = new XmlDocument();
                xml.Load(FileName);
                LoadXmlDocument(xml);
            }
        }

        private void LoadXmlDocument(XmlDocument xml)
        {
            var languageNode = xml.DocumentElement.GetElementsByTagName("English")[0];
            if (languageNode != null)
                foreach (XmlElement textNode in languageNode)
                    _texts.Add(textNode.GetAttribute("name"), textNode.InnerText);
        }

        public string GetText(string name)
        {
            return _texts[name];
        }

        public string Replace(string originalText)
        {
            if (originalText.IndexOf('$') != -1)
                foreach (var text in _texts)
                    originalText = originalText.Replace("$" + text.Key, text.Value);
            return originalText;
        }

        private class DescendingLengthComparer : IComparer<string>
        {
            public int Compare(string x, string y)
            {
                var result = y.Length.CompareTo(x.Length);
                return result != 0 ? result : x.CompareTo(y);
            }
        }
    }
}