using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using ToSic.Eav.BLL;
using ToSic.Eav.ImportExport.Refactoring.Extensions;
using ToSic.Eav.ImportExport.Refactoring.Options;

namespace ToSic.Eav.ImportExport.Refactoring
{
    public class XmlExport
    {
        /// <summary>
        /// Create a blank xml scheme for 2SexyContent data.
        /// </summary>
        /// <param name="zoneId">ID of 2SexyContent zone</param>
        /// <param name="applicationId">ID of 2SexyContent application</param>
        /// <param name="contentTypeId">ID of 2SexyContent type</param>
        /// <returns>A string containing the blank xml scheme</returns>
        public string CreateBlankXml(int zoneId, int applicationId, int contentTypeId, string helpText = "")
        {
            var contentType = GetContentType(zoneId, applicationId, contentTypeId);
            if (contentType == null) 
                return null;

            var documentElement1 = GetDocumentEntityElement("", "", contentType.Name);
            var documentElement2 = GetDocumentEntityElement("", "", contentType.Name);
            var documentRoot = GetDocumentRoot(documentElement1, documentElement2);
            var document = GetDocument(documentRoot);

            var attributes = contentType.GetAttributes();
            var isFirstAttribute = true;
            foreach (var attribute in attributes)
            {
                  if (isFirstAttribute)
                  {
                      documentElement1.Append(attribute.StaticName, helpText);
                      isFirstAttribute = false;
                  }
                  else
                  {
                    documentElement1.Append(attribute.StaticName, "");
                  }
                  documentElement2.Append(attribute.StaticName, "");  
            }
       
            return document.ToString();
        }

        /// <summary>
        /// Serialize 2SexyContent data to an xml string.
        /// </summary>
        /// <param name="zoneId">ID of 2SexyContent zone</param>
        /// <param name="applicationId">ID of 2SexyContent application</param>
        /// <param name="contentTypeId">ID of 2SexyContent type</param>
        /// <param name="languageSelected">Language of the data to be serialized (null for all languages)</param>
        /// <param name="languageFallback">Language fallback of the system</param>
        /// <param name="languageScope">Languages supported of the system</param>
        /// <param name="languageReference">How value references to other languages are handled</param>
        /// <param name="resourceReference">How value references to files and pages are handled</param>
        /// <returns>A string containing the xml data</returns>
        public string CreateXml(int zoneId, int applicationId, int contentTypeId, string languageSelected, string languageFallback, IEnumerable<string> languageScope, LanguageReferenceExport languageReference, ResourceReferenceExport resourceReference)
        {
            var contentType = GetContentType(zoneId, applicationId, contentTypeId);
            if (contentType == null)
                return null;

            var languages = new List<string>();
            if (!string.IsNullOrEmpty(languageSelected))
            {
                languages.Add(languageSelected);
            }
            else if (languageScope.Any())
            {   // Export all languages
                languages.AddRange(languageScope);
            }
            else
            {
                languages.Add(string.Empty);
            }

            var documentRoot = GetDocumentRoot(null);
            var document = GetDocument(documentRoot);

            var entities = contentType.Entities.Where(entity => entity.ChangeLogIDDeleted == null);
            foreach (var entity in entities)
            {
                Entity x;
                // 2dm Test code to debug - temporary only
                if (entity.EntityGUID == new Guid("31d93b03-cfb3-483b-8134-e08bbee9cd2c"))
                    x = entity;


                foreach (var language in languages)
                {
                    var documentElement = GetDocumentEntityElement(entity.EntityGUID, language, contentType.Name);
                    documentRoot.Add(documentElement);
                    
                    var attributes = contentType.GetAttributes();
                    foreach (var attribute in attributes)
                    {
                        if (attribute.Type == "Entity")
                        {   // Handle separately
                            documentElement.AppendEntityReferences(entity, attribute);
                        }
                        else if (languageReference.IsResolve())
                        {
                            documentElement.AppendValueResolved(entity, attribute, language, languageFallback, resourceReference);
                        }
                        else
                        {
                            documentElement.AppendValueReferenced(entity, attribute, language, languageFallback, languageScope, languages.Count() > 1, resourceReference);
                        }
                    }
                }
            }

            return document.ToString();
        }


        private static AttributeSet GetContentType(int zoneId, int applicationId, int contentTypeId)
        {
            // todo: changed by 2dm 2015-06-02, must be checked by 2tk
            var contentContext = EavDataController.Instance(zoneId, applicationId) ;// new SexyContent.SexyContent(zoneId, applicationId).ContentContext;
            return contentContext.AttribSet.GetAttributeSet(contentTypeId);
        }

        private static XDocument GetDocument(params object[] content)
        {
            return new XDocument(new XDeclaration("1.0", "UTF-8", "yes"), content);
        }

        private static XElement GetDocumentRoot(params object[] content)
        {
            return new XElement(DocumentNodeNames.Root, content);
        }

        private static XElement GetDocumentEntityElement(object elementGuid, object elementLanguage, string contentTypeName)
        {
            return new XElement
                (
                    DocumentNodeNames.Entity, 
                    new XAttribute(DocumentNodeNames.EntityTypeAttribute, contentTypeName.RemoveSpecialCharacters()),
                    new XElement(DocumentNodeNames.EntityGuid, elementGuid), 
                    new XElement(DocumentNodeNames.EntityLanguage, elementLanguage)
                );
        }
    }
}