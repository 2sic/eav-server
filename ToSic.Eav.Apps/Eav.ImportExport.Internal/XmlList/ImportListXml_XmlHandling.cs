using System.Xml.Linq;
using ToSic.Eav.ImportExport.Internal.Xml;
using ToSic.Eav.ImportExport.Validation;
using ToSic.Eav.Persistence.Logging;

namespace ToSic.Eav.ImportExport.Internal.XmlList;

partial class ImportListXml
{

    private bool RunDocumentValidityChecks() => Log.Func(timer: true, func: () =>
    {
        // #1 Assure that each element has a GUID and language child element
        foreach (var element in DocumentElements)
        {
            if (element.Element(XmlConstants.EntityGuid) == null)
                element.Add(new XElement(XmlConstants.EntityGuid, ""));
            if (element.Element(XmlConstants.EntityLanguage) == null)
                element.Add(new XElement(XmlConstants.EntityLanguage, ""));
        }

        // collect languages and items beloning to each language
        var documentElementLanguagesAll = DocumentElements
            .GroupBy(element => element.Element(XmlConstants.EntityGuid)?.Value)
            .Select(group => group
                .Select(element => element.Element(XmlConstants.EntityLanguage)?.Value.ToLowerInvariant())
                .ToList())
            .ToList();

        // count languages
        var documentElementLanguagesCount = documentElementLanguagesAll.Select(item => item.Count);

        if (documentElementLanguagesCount.All(count => count == 1)) return (true, "ok");

        if (!documentElementLanguagesAll.Any(lang => _languages.Except(lang).Any()))
            return (true, "ok");

        ErrorLog.Add(ImportErrorCode.MissingElementLanguage,
            "Langs=" + string.Join(", ", _languages));
        return (false, "error");
    });

    private bool LoadStreamIntoDocumentElement(Stream dataStream) => Log.Func(timer: true, func: () =>
    {
        Document = XDocument.Load(dataStream);
        dataStream.Position = 0;
        if (Document == null)
        {
            ErrorLog.Add(ImportErrorCode.InvalidDocument);
            return (false, $"error {ImportErrorCode.InvalidDocument}");
        }

        // #1 Check that document-root is the expected value
        var documentRoot = Document.Element(XmlConstants.Root)
                           ?? Document.Element(XmlConstants.Root97);

        if (documentRoot == null)
        {
            const string msg = "can't import - document doesn't have a root element";
            Log.A(msg);
            throw new(msg);
        }

        // #2 make sure it has elements to import
        DocumentElements = documentRoot.Elements(XmlConstants.Entity).ToList();
        if (!DocumentElements.Any())
        {
            ErrorLog.Add(ImportErrorCode.InvalidDocument);
            return (false, $"error {ImportErrorCode.InvalidDocument}");
        }

        // #3 Check the content type of the document (it can be found on each element in the Type attribute)
        var documentTypeAttribute = DocumentElements.First().Attribute(XmlConstants.EntityTypeAttribute);
        if (documentTypeAttribute?.Value == null ||
            documentTypeAttribute.Value != ContentType.Name.RemoveSpecialCharacters())
        {
            ErrorLog.Add(ImportErrorCode.InvalidContentType);
            return (false, $"error: {ImportErrorCode.InvalidContentType} - Trying to import of type {ContentType} but file contains {documentTypeAttribute}");
        }

        return (true, "ok");
    });


}