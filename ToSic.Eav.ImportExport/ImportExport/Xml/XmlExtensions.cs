namespace ToSic.Eav.ImportExport.Xml
{
    /// <summary>
    /// Extension methods to help build XML
    /// </summary>
    internal static class XmlExtensions
    {
        // 2021-09 2dm: Experimental, but doesn't look good
        // Reason is that we would usually need it in extension methods like this:
        // node.AddTag("title", Resources.Title);
        // But this fails in razor, because Resources.Title is dynamic
        // so in the end you would need
        // node.AddTag("Title", (string)Resources.Title); 
        // which kind of sucks and starts to get messy in all kinds of variations
        // Requiring things like
        // node.AddTag("MaxItems", (string)Resources.MaxItems.ToString());

        //private const string ErrorNeedsOwnerDocument = "the parent xml node must not be null and have a OwnerDocument";

        //public static XmlElement AddTag(this XmlElement parent, string name, string value)
        //{
        //    var node = parent?.OwnerDocument?.CreateElement(name) ?? throw new ArgumentException(ErrorNeedsOwnerDocument);
        //    node.InnerText = value;
        //    parent.AppendChild(node);
        //    return node;
        //}
        //public static XmlElement AddTag(this XmlElement parent, string name, object value)
        //{
        //    return AddTag(parent, name, value.ToString());
        //}

        //public static XmlAttribute AddAttribute(this XmlElement parent, string name, string value)
        //{
        //    var rssDoc = parent.OwnerDocument ?? throw new ArgumentException(ErrorNeedsOwnerDocument);
        //    var newAttribute = parent.Attributes.Append(rssDoc.CreateAttribute(name));
        //    newAttribute.Value = value;
        //    return newAttribute;
        //}
    }
}
