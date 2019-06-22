using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using ToSic.Eav.App;
using ToSic.Eav.Implementations.ValueConverter;
using ToSic.Eav.ImportExport;
using ToSic.Eav.ImportExport.Options;
using ToSic.Eav.ImportExport.Xml;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Logging;
using ToSic.Eav.Logging.Simple;

namespace ToSic.Eav.Apps.ImportExport
{
    /// <summary>
    /// For exporting a content-type into xml, either just the schema or with data
    /// </summary>
    public class ExportListXml: HasLog
    {
        private readonly XmlBuilder _xBuilder = new XmlBuilder();

        private AppDataPackage App { get; }
        public IContentType ContentType { get; }

        public ExportListXml(AppDataPackage app, IContentType contentType, Log parentLog): base("App.LstExp", parentLog)
        {
            App = app;
            ContentType = contentType;
        }


        /// <summary>
        /// Create a blank xml scheme for data of one content-type
        /// </summary>
        /// <returns>A string containing the blank xml scheme</returns>
        public string EmptyListTemplate()
        {
            Log.Add("export schema xml");
            if (ContentType == null) 
                return null;

            // build two empty nodes for easier filling in by the user
            var firstRow = _xBuilder.BuildEntity("", "", ContentType.Name);
            var secondRow = _xBuilder.BuildEntity("", "", ContentType.Name);
            var rootNode = _xBuilder.BuildDocumentWithRoot(firstRow, secondRow);

            var attributes = ContentType.Attributes;
            foreach (var attribute in attributes)
            {
                firstRow.Append(attribute.Name, "");
                secondRow.Append(attribute.Name, "");  
            }

            return rootNode.Document?.ToString();
        }

        /// <summary>
        /// Serialize data to an xml string.
        /// </summary>
        /// <param name="languageSelected">Language of the data to be serialized (null for all languages)</param>
        /// <param name="languageFallback">Language fallback of the system</param>
        /// <param name="sysLanguages">Languages supported of the system</param>
        /// <param name="exportLanguageReference">How value references to other languages are handled</param>
        /// <param name="resolveLinks">How value references to files and pages are handled</param>
        /// <param name="selectedIds">array of IDs to export only these</param>
        /// <returns>A string containing the xml data</returns>
        public string GenerateXml(string languageSelected, string languageFallback, string[] sysLanguages, ExportLanguageResolution exportLanguageReference, bool resolveLinks, int[] selectedIds)
        {
            if (ContentType == null) return null;

            Log.Add($"start export lang selected:{languageSelected} with fallback:{languageFallback} and type:{ContentType.Name}");

            #region build languages-list, but this must still be case-sensitive
            // note: reason for case-sensitive is that older imports need the en-US or will fail
            // newer imports would work correctly, but we want to be sure we can still re-import in older eav-systems
            var languages = new List<string>();
            if (!string.IsNullOrEmpty(languageSelected))// only selected language
                languages.Add(languageSelected);    
            else if (sysLanguages.Any())
                languages.AddRange(sysLanguages);// Export all languages
            else
                languages.Add(string.Empty); // default
            #endregion

            // neutralize languages AFTER creating the languages list, as that may not be lower-invariant!
            languageFallback = languageFallback.ToLowerInvariant();
            sysLanguages = sysLanguages.Select(l => l.ToLowerInvariant()).ToArray();

            var documentRoot = _xBuilder.BuildDocumentWithRoot();

            // Query all entities, or just the ones with specified IDs
            var entities = App.ListPublished.Where(e => e.Type == ContentType);
            if (selectedIds != null && selectedIds.Length > 0)
                entities = entities.Where(e => selectedIds.Contains(e.EntityId));
            var entList = entities.ToList();

            // Get the attribute definitions
            var attribsOfType = ContentType.Attributes;
            Log.Add($"will export {entList.Count} entities X {attribsOfType.Count} attribs");

            var resolver = Factory.Resolve<IEavValueConverter>();

            foreach (var entity in entList)
                foreach (var language in languages)
                {
                    var xmlEntity = _xBuilder.BuildEntity(entity.EntityGuid, language, ContentType.Name);
                    var langLow = language.ToLowerInvariant();
                    documentRoot.Add(xmlEntity);

                    foreach (var attribute in attribsOfType)
                    {
                        string value;
                        if (attribute.Type == XmlConstants.Entity) // Special, handle separately
                            value = entity.Attributes[attribute.Name].Values.FirstOrDefault()?.Serialized;
                        else
                            value = exportLanguageReference == ExportLanguageResolution.Resolve
                                ? ValueWithFullFallback(entity, attribute, langLow, languageFallback, resolveLinks, resolver)
                                : ValueOrLookupCode(entity, attribute, langLow, languageFallback,
                                    sysLanguages, languages.Count > 1, resolveLinks, resolver);

                        xmlEntity.Append(attribute.Name, value);
                    }
                }

            return documentRoot.Document?.ToString();
        }

        #region Helpers to assemble the xml

        /// <summary>
        /// Append an element to this. If the attribute is named xxx and the value is 4711 in the language specified, 
        /// the element appended will be <xxx>4711</xxx>. File and page references can be resolved optionally.
        /// </summary>
        private static string ValueWithFullFallback(IEntity entity, IAttributeDefinition attribute, string language, string languageFallback, bool resolveLinks, IEavValueConverter resolver)
        {
            var value = entity.GetBestValue(attribute.Name, new []{ language, languageFallback } ).ToString();
            return ResolveValue(entity, attribute.Type, value, resolveLinks, resolver);
        }

        /// <summary>
        /// Append an element to this. The element will get the name of the attribute, and if possible the value will 
        /// be referenced to another language (for example [ref(en-US,ro)].
        /// </summary>
        private static string ValueOrLookupCode(IEntity entity, IAttributeDefinition attribute, string language, string languageFallback, string[] sysLanguages, bool useRefToParentLanguage, bool resolveLinks, IEavValueConverter resolver)
        {
            var attrib = entity.Attributes[attribute.Name];

            // Option 1: nothing (no value found at all)
            // create a special "null" entry, so the re-import will also null this
            if (attrib == null || attrib.Values.Count == 0)
                return XmlConstants.Null;

            // now try to find the exact value-item for this language
            var valueItem = GetExactAssignedValue(attrib, language, languageFallback);

            if (valueItem == null)
                return XmlConstants.Null;

            // Option 2: Exact match (non-shared) on no other languages
            if (valueItem.Languages.Count == 0 || valueItem.Languages.Count == 1)
                return ResolveValue(entity, attribute.Type, valueItem.Serialized, resolveLinks, resolver);

            // Option 4 - language is assigned - either shared or Read-only
            var sharedParentLanguages = valueItem.Languages
                .Where(reference => !reference.ReadOnly)
                .Where(reference => reference.Key != language)
                .Select(reference => reference.Key)
                .OrderBy(lang => lang != languageFallback)  // order so first is the fallback, if it's included in this set
                .ThenBy(lan => lan)
                .ToList(); // then a-z

            // Option 4a - no other parent languages assigned
            if (!sharedParentLanguages.Any()) 
                return ResolveValue(entity, attribute.Type, valueItem.Serialized, resolveLinks, resolver);

            var langsOfValue = valueItem.Languages;
            string primaryLanguageRef = null;

            var valueLanguageReadOnly = langsOfValue.First(l => l.Key == language).ReadOnly;
            if (useRefToParentLanguage)
                primaryLanguageRef = sharedParentLanguages
                    .FirstOrDefault(lang => sysLanguages.IndexOf(lang) < sysLanguages.IndexOf(language));
            else if (valueLanguageReadOnly)
                primaryLanguageRef = sharedParentLanguages.First();// If one language is serialized, do not serialize read-write values as references

            return primaryLanguageRef != null 
                ? $"[ref({primaryLanguageRef},{(valueLanguageReadOnly ? XmlConstants.ReadOnly : XmlConstants.ReadWrite)})]" 
                : ResolveValue(entity, attribute.Type, valueItem.Serialized, resolveLinks, resolver);
        }

        internal static IValue GetExactAssignedValue(IAttribute attrib, string language, string languageFallback)
        {
            var valueItem = string.IsNullOrEmpty(language)
                ? attrib.Values.FirstOrDefault(v => v.Languages.Any(l => l.Key == languageFallback)) // use default (fallback)
                  ?? attrib.Values.FirstOrDefault(v => !v.Languages.Any()) // or the node without any languages
                : attrib.Values.FirstOrDefault(v => v.Languages.Any(l => l.Key == language)); // otherwise really exact match
            return valueItem;
        }

        /// <summary>
        /// Append an element to this. The element will have the value of the EavValue. File and page references 
        /// can optionally be resolved.
        /// </summary>
        internal static string ResolveValue(IEntity entity, string attrType, string value, bool resolveLinks, IEavValueConverter resolver) 
            => ResolveValue(entity.AppId, entity.EntityGuid, attrType, value, resolveLinks, resolver);


        /// <summary>
        /// Append an element to this. The element will have the value of the EavValue. File and page references 
        /// can optionally be resolved.
        /// </summary>
        internal static string ResolveValue(int appId, Guid itemGuid, string attrType, string value, bool resolveLinks, IEavValueConverter resolver)
        {
            if (value == null)
                return XmlConstants.Null;
            if (value == string.Empty)
                return XmlConstants.Empty;
            if (resolveLinks)
                return ResolveHyperlinksFromTenant(appId, itemGuid, value, attrType, resolver);
            return value;
        }

        /// <summary>
        /// If the value is a file or page reference, resolve it for example from 
        /// File:4711 to Content/file4711.jpg. If the reference cannot be resolved, 
        /// the original value will be returned. 
        /// </summary>
        internal static string ResolveHyperlinksFromTenant(int appId, Guid itemGuid, string value, string attrType,
            IEavValueConverter resolver)
            => attrType != Constants.DataTypeHyperlink
                ? value
                : resolver.ToValue(itemGuid,value);

        #endregion


    }

    internal static class QuickExtensions
    {
        internal static int IndexOf<T>(this IEnumerable<T> list, T item) 
            => list.TakeWhile(i => !i.Equals(item)).Count();

        /// <summary>
        /// Apend an element to this.
        /// </summary>
        internal static void Append(this XElement element, XName name, object value)
            => element.Add(new XElement(name, value));
        
    }
}