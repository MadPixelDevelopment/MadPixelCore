using System;
using System.IO;
using System.Xml;

namespace Unity.Services.LevelPlay.Editor
{
    internal interface IXmlDocument
    {
        void Load(string filename);
        void Load(Stream stream);
        void LoadXml(string xml);
        #nullable enable
        System.Xml.XmlNode ? SelectSingleNode(string xpath);
        #nullable disable

        System.Xml.XmlNodeList GetElementsByTagName(string name);
        void Save(string filename);
    }

    internal interface IXmlDocumentFactory
    {
        IXmlDocument CreateXmlDocument();
    }
}
