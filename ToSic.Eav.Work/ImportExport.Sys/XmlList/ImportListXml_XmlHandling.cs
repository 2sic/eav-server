using System.Xml.Linq;
using ToSic.Eav.ImportExport.Sys.Xml;
using ToSic.Eav.Persistence.Sys.Logging;

namespace ToSic.Eav.ImportExport.Sys.XmlList;

partial class ImportListXml
{

    private bool RunDocumentValidityChecks(List<XElement> xmlEntities)
    {
        var l = LogSummary.Fn<bool>();
        // #1 Assure that each element has a GUID and language child element
        foreach (var element in xmlEntities)
        {
            if (element.Element(XmlConstants.EntityGuid) == null)
                element.Add(new XElement(XmlConstants.EntityGuid, ""));
            if (element.Element(XmlConstants.EntityLanguage) == null)
                element.Add(new XElement(XmlConstants.EntityLanguage, ""));
        }

        // collect languages and items belonging to each language
        var documentElementLanguagesAll = xmlEntities
            .GroupBy(element => element.Element(XmlConstants.EntityGuid)?.Value)
            .Select(group => group
                .Select(element => element.Element(XmlConstants.EntityLanguage)?.Value.ToLowerInvariant())
                .ToList())
            .ToList();

        // count languages
        var documentElementLanguagesCount = documentElementLanguagesAll.Select(item => item.Count);

        if (documentElementLanguagesCount.All(count => count == 1))
            return l.ReturnTrue("ok");

        if (!documentElementLanguagesAll.Any(lang => ImportConfig.Languages.Except(lang).Any()))
            return l.ReturnTrue("ok");

        ErrorLog.Add(ImportErrorCode.MissingElementLanguage,
            "Langs=" + string.Join(", ", ImportConfig.Languages));
        return l.ReturnFalse("error");
    }

    private bool LoadStreamIntoDocumentElement(IContentType contentType, Stream dataStream,
        [NotNullWhen(true)] out List<XElement>? xmlEntityNodes)
    {
        var l = LogSummary.Fn<bool>(timer: true);
        var document = XDocument.Load(dataStream);
        dataStream.Position = 0;
        if (document == null)
        {
            ErrorLog.Add(ImportErrorCode.InvalidDocument);
            xmlEntityNodes = null;
            return l.ReturnFalse($"error {ImportErrorCode.InvalidDocument}");
        }

        // #1 Check that document-root is the expected value
        var documentRoot = document.Element(XmlConstants.Root)
                           ?? document.Element(XmlConstants.Root97);

        if (documentRoot == null)
        {
            const string msg = "can't import - document doesn't have a root element";
            LogDetails.A(msg);
            throw l.Ex(new Exception(msg));
        }

        // #2 make sure it has elements to import
        var docNodes = documentRoot
            .Elements(XmlConstants.Entity)
            .ToList();
        if (!docNodes.Any())
        {
            ErrorLog.Add(ImportErrorCode.InvalidDocument);
            xmlEntityNodes = null;
            return l.ReturnFalse($"error {ImportErrorCode.InvalidDocument}");
        }

        // #3 Check the content type of the document (it can be found on each element in the Type attribute)
        var documentTypeAttribute = docNodes
            .First()
            .Attribute(XmlConstants.EntityTypeAttribute);

        if (documentTypeAttribute?.Value == null ||
            documentTypeAttribute.Value != contentType.Name.RemoveSpecialCharacters())
        {
            ErrorLog.Add(ImportErrorCode.InvalidContentType);
            xmlEntityNodes = null;
            return l.ReturnFalse($"error: {ImportErrorCode.InvalidContentType} - Trying to import of type {contentType} but file contains {documentTypeAttribute}");
        }

        DocumentElements = docNodes;
        xmlEntityNodes = docNodes;
        return l.ReturnTrue("ok");
    }


}