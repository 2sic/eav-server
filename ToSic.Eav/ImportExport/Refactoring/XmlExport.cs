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
        private readonly XmlPartsBuilder _builder = new XmlPartsBuilder();

        /// <summary>
        /// Create a blank xml scheme for data.
        /// </summary>
        /// <param name="zoneId">ID of zone</param>
        /// <param name="applicationId">ID of application</param>
        /// <param name="contentTypeId">ID of type</param>
        /// <returns>A string containing the blank xml scheme</returns>
        public string CreateBlankXml(int zoneId, int applicationId, int contentTypeId, string helpText = "")
        {
            var contentType = GetContentType(zoneId, applicationId, contentTypeId);
            if (contentType == null) 
                return null;

            var documentElement1 = GetDocumentEntityElement("", "", contentType.Name);
            var documentElement2 = GetDocumentEntityElement("", "", contentType.Name);
            var documentRoot = BuildDocumentRoot(documentElement1, documentElement2);
            var document = BuildDocument(documentRoot);

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
        /// <param name="selectedIds">array of IDs to export only these</param>
        /// <returns>A string containing the xml data</returns>
        public string CreateXml(int zoneId, int applicationId, int contentTypeId, string languageSelected, string languageFallback, IEnumerable<string> languageScope, LanguageReferenceExport languageReference, ResourceReferenceExport resourceReference, int[] selectedIds)
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

            var documentRoot = BuildDocumentRoot(null);
            var document = BuildDocument(documentRoot);

            var entities = contentType.Entities.Where(entity => entity.ChangeLogIDDeleted == null);
            if (selectedIds != null && selectedIds.Length > 0)
                entities = entities.Where(e => selectedIds.Contains(e.EntityID));

            foreach (var entity in entities)
            {

                foreach (var language in languages)
                {
                    var documentElement = GetDocumentEntityElement(entity.EntityGUID, language, contentType.Name);
                    documentRoot.Add(documentElement);
                    
                    var attributes = contentType.GetAttributes();
                    foreach (var attribute in attributes)
                    {
                        if (attribute.Type == "Entity")
                        {   // Handle separately
                            _builder.AppendEntityReferences(documentElement, entity, attribute);
                            // documentElement.AppendEntityReferences(entity, attribute);
                        }
                        else if (languageReference == LanguageReferenceExport.Resolve)
                        {
                            _builder.AppendValueResolved(documentElement, entity, attribute, language, languageFallback, resourceReference);
                            //documentElement.AppendValueResolved(entity, attribute, language, languageFallback, resourceReference);
                        }
                        else
                        {
                            _builder.AppendValueReferenced(documentElement, entity, attribute, language, languageFallback, languageScope, languages.Count > 1, resourceReference);
                            //documentElement.AppendValueReferenced(entity, attribute, language, languageFallback, languageScope, languages.Count > 1, resourceReference);
                        }
                    }
                }
            }

            return document.ToString();
        }


        private AttributeSet GetContentType(int zoneId, int applicationId, int contentTypeId)
        {
            var contentContext = DbDataController.Instance(zoneId, applicationId) ;
            return contentContext.AttribSet.GetAttributeSet(contentTypeId);
        }

        private XDocument BuildDocument(params object[] content) 
            => new XDocument(new XDeclaration("1.0", "UTF-8", "yes"), content);

        private XElement BuildDocumentRoot(params object[] content) 
            => new XElement(XmlConstants.Root, content);

        private XElement GetDocumentEntityElement(object elementGuid, object elementLanguage, string contentTypeName)
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