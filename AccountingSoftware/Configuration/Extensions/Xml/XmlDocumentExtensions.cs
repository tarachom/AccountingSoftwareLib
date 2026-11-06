using System.Xml;

namespace AccountingSoftware;

public static class XmlDocumentExtensions
{
    public static XmlElement CreateNode(this XmlDocument xmlDoc, string name, string? innerText = null)
    {
        XmlElement node = xmlDoc.CreateElement(name);

        if (innerText != null)
            node.InnerText = innerText;

        return node;
    }
}