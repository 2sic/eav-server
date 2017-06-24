using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using ToSic.Eav.Implementations.ValueConverter;
using ToSic.Eav.ImportExport;
using ToSic.Eav.ImportExport.Options;
using ToSic.Eav.ImportExport.Xml;
using ToSic.Eav.Persistence.Efc.Models;

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

        /// <param name="contentTypeId">ID of type</param>
        public void Init(int contentTypeId)
        {
            ContentType = DbContext.AttribSet.GetDbAttribSet(contentTypeId);
        }

        private ToSicEavAttributeSets ContentType { get; set; }

        public string NiceContentTypeName => ContentType.Name; 


        /// <summary>
        /// Create a blank xml scheme for data of one content-type
        /// </summary>
        /// <returns>A string containing the blank xml scheme</returns>
        public string EmptySchemaXml()
        {
            if (ContentType == null) 
                return null;

            // build two emtpy nodes for easier filling in by the user
            var firstRow = _xBuilder.BuildEntity("", "", ContentType.Name);
            var secondRow = _xBuilder.BuildEntity("", "", ContentType.Name);
            var rootNode = _xBuilder.BuildDocumentWithRoot(firstRow, secondRow);

            var attributes = ContentType.GetAttributes();
            foreach (var attribute in attributes)
            {
                firstRow.Append(attribute.StaticName, "");
                secondRow.Append(attribute.StaticName, "");  
            }

            return rootNode.Document?.ToString();
        }

        /// <summary>
        /// Serialize data to an xml string.
        /// </summary>
        /// <param name="languageSelected">Language of the data to be serialized (null for all languages)</param>
        /// <param name="languageFallback">Language fallback of the system</param>
        /// <param name="languageScope">Languages supported of the system</param>
        /// <param name="exportLanguageReference">How value references to other languages are handled</param>
        /// <param name="exportResourceReference">How value references to files and pages are handled</param>
        /// <param name="selectedIds">array of IDs to export only these</param>
        /// <returns>A string containing the xml data</returns>
        public string TableXmlFromDb(string languageSelected, string languageFallback, string[] languageScope, ExportLanguageResolution exportLanguageReference, ExportResourceReferenceMode exportResourceReference, int[] selectedIds)
        {
            if (ContentType == null) return null;

            var languages = new List<string>();
            if (!string.IsNullOrEmpty(languageSelected))// only selected language
                languages.Add(languageSelected);    
            else if (languageScope.Any())
                languages.AddRange(languageScope);// Export all languages
            else
                languages.Add(string.Empty); // default

            var documentRoot = _xBuilder.BuildDocumentWithRoot();

            // Query all entities, or just the ones with specified IDs
            var entities = DbContext.Entities.GetEntitiesByType(ContentType);
            if (selectedIds != null && selectedIds.Length > 0)
                entities = entities.Where(e => selectedIds.Contains(e.EntityId));
            var entList = entities.ToList();

            // Get the attribute definitions
            var attribsOfType = DbContext.AttributesDefinition.GetAttributeDefinitions(ContentType.AttributeSetId).ToList();

            var dbXml = new DbXmlBuilder(DbContext);
            foreach (var entity in entList)
            {
                var relationships = dbXml.GetSerializedRelationshipGuids(entity.EntityId);

                foreach (var language in languages)
                {
                    var documentElement = _xBuilder.BuildEntity(entity.EntityGuid, language, ContentType.Name);
                    documentRoot.Add(documentElement);

                    var attributes = attribsOfType;
                    foreach (var attribute in attributes)
                    {
                        if (attribute.Type == XmlConstants.Entity /* "Entity" */) // Special, handle separately
                            AppendEntityReferences(documentElement, attribute.StaticName, relationships.ContainsKey(attribute.StaticName) ? relationships[attribute.StaticName]:"");// entity, attribute);
                        else if (exportLanguageReference == ExportLanguageResolution.Resolve)
                            AppendValueResolved(documentElement, entity, attribute, language, languageFallback,
                                exportResourceReference);
                        else
                            AppendValueReferenced(documentElement, entity, attribute, language, languageFallback,
                                languageScope, languages.Count > 1, exportResourceReference);
                    }
                }
            }

            return documentRoot.Document?.ToString();
        }

        #region Helpers to assemble the xml

        private void AppendEntityReferences(XElement element, string attrName, string entityGuidsString)
        {
            element.Append(attrName, entityGuidsString);
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
                element.Append(valueName, XmlConstants.Null /* "[]" */);
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
                valueLanguageReferenced = valueLanguagesReferenced
                    .FirstOrDefault(lang => languageScope.IndexOf(lang) < languageScope.IndexOf(language));
            else if (valueLanguageReadOnly)
                valueLanguageReferenced = valueLanguagesReferenced.First();// If one language is serialized, do not serialize read-write values as references

            if (valueLanguageReferenced != null)
                element.Append(valueName, $"[ref({valueLanguageReferenced},{(valueLanguageReadOnly ? XmlConstants.ReadOnly /* "ro" */ : XmlConstants.ReadWrite /* "rw" */)})]");
            else
                AppendValue(element, valueName, value, exportResourceReferenceOption);
        }



        /// <summary>
        /// Append an element to this. The element will have the value of the EavValue. File and page references 
        /// can optionally be resolved.
        /// </summary>
        private void AppendValue(XElement element, XName name, ToSicEavValues value, ExportResourceReferenceMode exportResourceReferenceOption)
        {
            if (value == null)
                element.Append(name, XmlConstants.Null /* "[]" */);
            else if (value.Value == null)
                element.Append(name, XmlConstants.Null /* "[]" */);
            else if (exportResourceReferenceOption == ExportResourceReferenceMode.Resolve)
                element.Append(name, ResolveHyperlinksFromTennant(value));
            else if (value.Value == string.Empty)
                element.Append(name, XmlConstants.Empty /* "[\"\"]" */);
            else
                element.Append(name, value.Value);
        }

        /// <summary>
        /// If the value is a file or page reference, resolve it for example from 
        /// File:4711 to Content/file4711.jpg. If the reference cannot be reoslved, 
        /// the original value will be returned. 
        /// </summary>
        private string ResolveHyperlinksFromTennant(ToSicEavValues value)
        {
            if (value.Attribute.Type != Constants.Hyperlink) return value.Value;
            var vc = Factory.Resolve<IEavValueConverter>();
            return vc.Convert(ConversionScenario.GetFriendlyValue, Constants.Hyperlink, value.Value);
        }


        /// <summary>
        /// Get languages this value is referenced from, but not the language specified. The 
        /// method helps to find languages the value belongs to expect the current language.
        /// </summary>
        private IEnumerable<string> GetLanguagesReferenced(ToSicEavValues value, string valueLanguage, bool referenceReadWrite)
        {
            return value.ToSicEavValuesDimensions
                .Where(reference => !referenceReadWrite || !reference.ReadOnly)
                .Where(reference => reference.Dimension.ExternalKey != valueLanguage)
                .Select(reference => reference.Dimension.ExternalKey)
                .ToList();
        }

        #endregion
        

    }

    internal static class QuickExtensions
    {
        internal static int IndexOf<T>(this IEnumerable<T> list, T item) 
            => list.TakeWhile(i => !i.Equals(item)).Count();

        /// <summary>
        /// Apend an element to this.
        /// </summary>
        public static void Append(this XElement element, XName name, object value)
            => element.Add(new XElement(name, value));
        
    }
}