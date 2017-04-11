using System;
using System.Linq;
using System.Xml.Linq;
using ToSic.Eav.ImportExport.Validation;

namespace ToSic.Eav.ImportExport.Xml
{
    public class XmlBuilder
    {

        public XDocument BuildDocument(params object[] content)
            => new XDocument(new XDeclaration("1.0", "UTF-8", "yes"), content);

        public XElement BuildRootNode(params object[] content)
            => new XElement(XmlConstants.Root, content);


        public XElement BuildDocumentWithRoot(params object[] content)
        {
            var documentRoot = BuildRootNode(content);
            var document = BuildDocument(documentRoot);
            return documentRoot;
        }

        public XElement BuildEntity(object elementGuid, object elementLanguage, string contentTypeName)
        {
            return new XElement
                (
                    XmlConstants.Entity,
                    new XAttribute(XmlConstants.EntityTypeAttribute, contentTypeName.RemoveSpecialCharacters()),
                    new XElement(XmlConstants.EntityGuid, elementGuid),
                    new XElement(XmlConstants.EntityLanguage, elementLanguage)
                );
        }

    }
}
