using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Practices.Unity;
using ToSic.Eav.Implementations.ValueConverter;
using ToSic.Eav.ImportExport.Options;
using ToSic.Eav.ImportExport.Xml;
using ToSic.Eav.Persistence.EFC11.Models;

namespace ToSic.Eav.Repository.Efc.Parts
{
    /// <summary>
    /// For exporting a content-type into xml, either just the schema or with data
    /// </summary>
    public class DbXmlExportTable: BllCommandBase
    {
        private readonly XmlBuilder _xBuilder = new XmlBuilder();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataController"></param>
        public DbXmlExportTable(DbDataController dataController) : base(dataController)
        {
        }

        /// <summary>
        /// Create a blank xml scheme for data of one content-type
        /// </summary>
        /// <param name="contentTypeId">ID of type</param>
        /// <returns>A string containing the blank xml scheme</returns>
        public string SchemaXmlFromDb(int contentTypeId)
        {
            var contentType = DbContext.AttribSet.GetAttributeSet(contentTypeId);
            if (contentType == null) 
                return null;

            // build two emtpy nodes for easier filling in by the user
            var firstRow = _xBuilder.BuildEntity("", "", contentType.Name);
            var secondRow = _xBuilder.BuildEntity("", "", contentType.Name);
            var rootNode = _xBuilder.BuildDocumentWithRoot(firstRow, secondRow);

            var attributes = contentType.GetAttributes();
            foreach (var attribute in attributes)
            {
                firstRow.Append(attribute.StaticName, "");
                secondRow.Append(attribute.StaticName, "");  
            }

            return rootNode.Document.ToString();
        }

        /// <summary>
        /// Serialize 2SexyContent data to an xml string.
        /// </summary>
        /// <param name="contentTypeId">ID of 2SexyContent type</param>
        /// <param name="languageSelected">Language of the data to be serialized (null for all languages)</param>
        /// <param name="languageFallback">Language fallback of the system</param>
        /// <param name="languageScope">Languages supported of the system</param>
        /// <param name="exportLanguageReference">How value references to other languages are handled</param>
        /// <param name="exportResourceReference">How value references to files and pages are handled</param>
        /// <param name="selectedIds">array of IDs to export only these</param>
        /// <returns>A string containing the xml data</returns>
        public string TableXmlFromDb(int contentTypeId, string languageSelected, string languageFallback, IEnumerable<string> languageScope, ExportLanguageResolution exportLanguageReference, ExportResourceReferenceMode exportResourceReference, int[] selectedIds)
        {
            var contentType = DbContext.AttribSet.GetAttributeSet(contentTypeId);
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

            //var documentRoot = BuildDocumentRoot(null);
            //var document = _xBuilder.BuildDocument(documentRoot);
            var documentRoot = _xBuilder.BuildDocumentWithRoot();

            var entities = contentType.ToSicEavEntities.Where(entity => entity.ChangeLogDeleted == null);
            if (selectedIds != null && selectedIds.Length > 0)
                entities = entities.Where(e => selectedIds.Contains(e.EntityId));

            foreach (var entity in entities)
            {

                foreach (var language in languages)
                {
                    var documentElement = _xBuilder.BuildEntity(entity.EntityGuid, language, contentType.Name);
                    documentRoot.Add(documentElement);
                    
                    var attributes = contentType.GetAttributes();
                    foreach (var attribute in attributes)
                    {
                        if (attribute.Type == "Entity")
                        {   // Handle separately
                            AppendEntityReferences(documentElement, entity, attribute);
                            // documentElement.AppendEntityReferences(entity, attribute);
                        }
                        else if (exportLanguageReference == ExportLanguageResolution.Resolve)
                        {
                            AppendValueResolved(documentElement, entity, attribute, language, languageFallback, exportResourceReference);
                            //documentElement.AppendValueResolved(entity, attribute, language, languageFallback, resourceReference);
                        }
                        else
                        {
                            AppendValueReferenced(documentElement, entity, attribute, language, languageFallback, languageScope, languages.Count > 1, exportResourceReference);
                            //documentElement.AppendValueReferenced(entity, attribute, language, languageFallback, languageScope, languages.Count > 1, resourceReference);
                        }
                    }
                }
            }

            return documentRoot.Document.ToString();// document.ToString();
        }

        #region Helpers to assemble the xml

        private void AppendEntityReferences(XElement element, ToSicEavEntities entity, ToSicEavAttributes attribute)
        {
            var entityGuids = attribute.ToSicEavEntityRelationships.Where(rel => rel.ParentEntityId == entity.EntityId)
                                                                     .Select(rel => rel.ChildEntity.EntityGuid);
            var entityGuidsString = string.Join(",", entityGuids);
            element.Append(attribute.StaticName, entityGuidsString);
        }

        /// <summary>
        /// Append an element to this. If the attribute is named xxx and the value is 4711 in the language specified, 
        /// the element appended will be <xxx>4711</xxx>. File and page references can be resolved optionally.
        /// </summary>
        private void AppendValueResolved(XElement element, ToSicEavEntities entity, ToSicEavAttributes attribute, string language, string languageFallback, ExportResourceReferenceMode exportResourceReferenceOption)
        {
            var valueName = attribute.StaticName;
            var value = entity.GetValueOfLanguageOrFallback(attribute, language, languageFallback);
            AppendValue(element, valueName, value, exportResourceReferenceOption);
        }

        /// <summary>
        /// Append an element to this. The element will get the name of the attribute, and if possible the value will 
        /// be referenced to another language (for example [ref(en-US,ro)].
        /// </summary>
        private void AppendValueReferenced(XElement element, ToSicEavEntities entity, ToSicEavAttributes attribute, string language, string languageFallback, IEnumerable<string> languageScope, bool referenceParentLanguagesOnly, ExportResourceReferenceMode exportResourceReferenceOption)
        {
            var valueName = attribute.StaticName;
            var value = entity.GetValueOfExactLanguage(attribute, language);
            if (value == null)
            {
                element.Append(valueName, "[]");
                return;
            }

            var valueLanguage = value.ToSicEavValuesDimensions.Select(reference => reference.Dimension.ExternalKey)
                                         .FirstOrDefault(l => l == language); // value.GetLanguage(language);
            if (valueLanguage == null)
            {   // If no language is found, serialize the plain value
                AppendValue(element, valueName, value, exportResourceReferenceOption);
                return;
            }

            var valueLanguagesReferenced = GetLanguagesReferenced(value, language, true)
                                                .OrderBy(lang => lang != languageFallback)
                                                .ThenBy(lan => lan);
            if (!valueLanguagesReferenced.Any())
            {   // If the value is a head value, serialize the plain value
                AppendValue(element, valueName, value, exportResourceReferenceOption);
                return;
            }

            var valueLanguageReferenced = default(string);
            var valueLanguageReadOnly = value.IsLanguageReadOnly(language);
            if (referenceParentLanguagesOnly)
            {
                valueLanguageReferenced = valueLanguagesReferenced.FirstOrDefault
                    (
                        lang => languageScope.IndexOf(lang) < languageScope.IndexOf(language)
                    );
            }
            else if (valueLanguageReadOnly)
            {   // If one language is serialized, do not serialize read-write values 
                // as references
                valueLanguageReferenced = valueLanguagesReferenced.First();
            }

            if (valueLanguageReferenced == null)
            {
                AppendValue(element, valueName, value, exportResourceReferenceOption);
                return;
            }

            element.Append(valueName, $"[ref({valueLanguageReferenced},{(valueLanguageReadOnly ? "ro" : "rw")})]");
        }



        /// <summary>
        /// Append an element to this. The element will have the value of the EavValue. File and page references 
        /// can optionally be resolved.
        /// </summary>
        private void AppendValue(XElement element, XName name, ToSicEavValues value, ExportResourceReferenceMode exportResourceReferenceOption)
        {
            if (value == null)
            {
                element.Append(name, "[]");
            }
            else if (value.Value == null)
            {
                element.Append(name, "[]");
            }
            else if (exportResourceReferenceOption == ExportResourceReferenceMode.Resolve)
            {
                element.Append(name, ResolveHyperlinksFromTennant(value));
            }
            else if (value.Value == string.Empty)
            {
                element.Append(name, "[\"\"]");
            }
            else
            {
                element.Append(name, value.Value);
            }
        }

        /// <summary>
        /// If the value is a file or page reference, resolve it for example from 
        /// File:4711 to Content/file4711.jpg. If the reference cannot be reoslved, 
        /// the original value will be returned. 
        /// </summary>
        private string ResolveHyperlinksFromTennant(ToSicEavValues value)
        {
            if (value.Attribute.Type == Constants.Hyperlink)
            {
                var vc = Factory.Container.Resolve<IEavValueConverter>();
                return vc.Convert(ConversionScenario.GetFriendlyValue, Constants.Hyperlink, value.Value);
            }
            return value.Value;
        }


        /// <summary>
        /// Get languages this value is referenced from, but not the language specified. The 
        /// method helps to find languages the value belongs to expect the current language.
        /// </summary>
        private IEnumerable<string> GetLanguagesReferenced(ToSicEavValues value, string valueLanguage, bool referenceReadWrite)
        {
            return value.ToSicEavValuesDimensions.Where(reference => !referenceReadWrite || !reference.ReadOnly)
                                         .Where(reference => reference.Dimension.ExternalKey != valueLanguage)
                                         .Select(reference => reference.Dimension.ExternalKey)
                                         .ToList();
        }

        #endregion
        

    }

    internal static class QuickExtensions
    {
        internal static int IndexOf<T>(this IEnumerable<T> list, T item)
        {
            return list.TakeWhile(i => !i.Equals(item)).Count();
        }

        /// <summary>
        /// Apend an element to this.
        /// </summary>
        public static void Append(this XElement element, XName name, object value)
        {
            element.Add(new XElement(name, value));
        }
        
    }
}