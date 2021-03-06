﻿using System;
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
            var wrapLog = Log.Call<bool>(useTimer: true);
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

            if (documentElementLanguagesCount.All(count => count == 1)) return wrapLog("ok", true);

            if (!documentElementLanguagesAll.Any(lang => _languages.Except(lang).Any())) return wrapLog("ok", true);

            ErrorLog.Add(ImportErrorCode.MissingElementLanguage,
                "Langs=" + string.Join(", ", _languages));
            return wrapLog("error", false);
        }

        private bool LoadStreamIntoDocumentElement(Stream dataStream)
        {
            var wrapLog = Log.Call<bool>(useTimer: true);
            Document = XDocument.Load(dataStream);
            dataStream.Position = 0;
            if (Document == null)
            {
                ErrorLog.Add(ImportErrorCode.InvalidDocument);
                return wrapLog($"error {ImportErrorCode.InvalidDocument}", false);
            }

            // #1 Check that document-root is the expected value
            var documentRoot = Document.Element(XmlConstants.Root)
                ?? Document.Element(XmlConstants.Root97);

            if (documentRoot == null)
                throw new Exception(Log.Add("can't import - document doesn't have a root element"));

            // #2 make sure it has elements to import
            DocumentElements = documentRoot.Elements(XmlConstants.Entity).ToList();
            if (!DocumentElements.Any())
            {
                ErrorLog.Add(ImportErrorCode.InvalidDocument);
                return wrapLog($"error {ImportErrorCode.InvalidDocument}", false);
            }

            // #3 Check the content type of the document (it can be found on each element in the Type attribute)
            var documentTypeAttribute = DocumentElements.First().Attribute(XmlConstants.EntityTypeAttribute);
            if (documentTypeAttribute?.Value == null ||
                documentTypeAttribute.Value != ContentType.Name.RemoveSpecialCharacters())
            {
                ErrorLog.Add(ImportErrorCode.InvalidRoot);
                return wrapLog($"error: {ImportErrorCode.InvalidRoot}", false);
            }

            return wrapLog("ok", true);
        }


    }
}
