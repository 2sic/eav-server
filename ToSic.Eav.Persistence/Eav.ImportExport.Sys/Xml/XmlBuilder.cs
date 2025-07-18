﻿using System.Xml.Linq;

namespace ToSic.Eav.ImportExport.Sys.Xml;

public class XmlBuilder
{

    public static XDocument BuildDocument(params object[] content) => new(new("1.0", "UTF-8", "yes"), content);

    public XElement BuildRootNode(params object[] content) => new(XmlConstants.Root, content);


    public XElement BuildDocumentWithRoot(params object[] content)
    {
        var documentRoot = BuildRootNode(content);
        BuildDocument(documentRoot);
        return documentRoot;
    }

    public XElement BuildEntity(object elementGuid, object elementLanguage, string contentTypeName)
    {
        return new(
            XmlConstants.Entity,
            new XAttribute(XmlConstants.EntityTypeAttribute, contentTypeName.RemoveSpecialCharacters()),
            new XElement(XmlConstants.EntityGuid, elementGuid),
            new XElement(XmlConstants.EntityLanguage, elementLanguage)
        );
    }



}