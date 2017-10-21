using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using ToSic.Eav.ImportExport;
using ToSic.Eav.ImportExport.Validation;
using ToSic.Eav.Persistence.Logging;

namespace ToSic.Eav.Apps.ImportExport
{
    public partial class ImportListXml
    {

        private bool RunDocumentValidityChecks()
        {
            // Assure that each element has a GUID and language child element
            foreach (var element in DocumentElements)
            {
                if (element.Element(XmlConstants.EntityGuid) == null)
                    element.Add(new XElement(XmlConstants.EntityGuid, ""));
                if (element.Element(XmlConstants.EntityLanguage) == null)
                    element.Add(new XElement(XmlConstants.EntityLanguage, ""));
            }

            var documentElementLanguagesAll = DocumentElements
                .GroupBy(element => element.Element(XmlConstants.EntityGuid)?.Value)
                .Select(group => group
                    .Select(element => element.Element(XmlConstants.EntityLanguage)?.Value.ToLowerInvariant())
                    .ToList())
                .ToList();

            var documentElementLanguagesCount = documentElementLanguagesAll.Select(item => item.Count);

            if (documentElementLanguagesCount.All(count => count == 1)) return true;

            if (!documentElementLanguagesAll.Any(lang => _languages.Except(lang).Any())) return true;

            ErrorLog.AppendError(ImportErrorCode.MissingElementLanguage,
                "Langs=" + string.Join(", ", _languages));
            return false;
        }

        private bool LoadStreamIntoDocumentElement(Stream dataStream)
        {
            Document = XDocument.Load(dataStream);
            dataStream.Position = 0;
            if (Document == null)
            {
                ErrorLog.AppendError(ImportErrorCode.InvalidDocument);
                return false;
            }

            var documentRoot = Document.Element(XmlConstants.Root)
                ?? Document.Element(XmlConstants.Root97);

            if (documentRoot == null)
                throw new Exception("can't import - document doesn't have a root element");

            DocumentElements = documentRoot.Elements(XmlConstants.Entity).ToList();
            if (!DocumentElements.Any())
            {
                ErrorLog.AppendError(ImportErrorCode.InvalidDocument);
                return false;
            }

            // Check the content type of the document (it can be found on each element in the Type attribute)
            var documentTypeAttribute = DocumentElements.First().Attribute(XmlConstants.EntityTypeAttribute);
            if (documentTypeAttribute?.Value == null ||
                documentTypeAttribute.Value != ContentType.Name.RemoveSpecialCharacters())
            {
                ErrorLog.AppendError(ImportErrorCode.InvalidRoot);
                return false;
            }

            return true;
        }


    }
}
