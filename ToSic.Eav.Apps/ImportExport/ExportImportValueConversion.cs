using System;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.ImportExport;
using ToSic.Eav.Logging;
using ToSic.Eav.Run;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.Apps.ImportExport
{
    /// <summary>
    /// For exporting a content-type into xml, either just the schema or with data
    /// </summary>
    public class ExportImportValueConversion : HasLog
    {
        #region Dependency Injection

        public ExportImportValueConversion(IValueConverter valueConverter) : base("App.EXValC")
        {
            ValueConverter = valueConverter;
        }
        public IValueConverter ValueConverter { get; }

        #endregion



        #region Helpers to assemble the xml

        /// <summary>
        /// Append an element to this. If the attribute is named xxx and the value is 4711 in the language specified, 
        /// the element appended will be <xxx>4711</xxx>. File and page references can be resolved optionally.
        /// </summary>
        internal string ValueWithFullFallback(IEntity entity, IContentTypeAttribute attribute, string language, string languageFallback, bool resolveLinks)
        {
            var value = entity.GetBestValue(attribute.Name, new []{ language, languageFallback } ).ToString();
            return ResolveValue(entity, attribute.Type, value, resolveLinks);
        }

        /// <summary>
        /// Append an element to this. The element will get the name of the attribute, and if possible the value will 
        /// be referenced to another language (for example [ref(en-US,ro)].
        /// </summary>
        internal string ValueOrLookupCode(IEntity entity, IContentTypeAttribute attribute, string language, string languageFallback, string[] sysLanguages, bool useRefToParentLanguage, bool resolveLinks)
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
                return ResolveValue(entity, attribute.Type, valueItem.Serialized, resolveLinks);

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
                return ResolveValue(entity, attribute.Type, valueItem.Serialized, resolveLinks);

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
                : ResolveValue(entity, attribute.Type, valueItem.Serialized, resolveLinks);
        }

        public static IValue GetExactAssignedValue(IAttribute attrib, string language, string languageFallback)
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
        internal string ResolveValue(IEntity entity, string attrType, string value, bool resolveLinks) 
            => ResolveValue(entity.AppId, entity.EntityGuid, attrType, value, resolveLinks);


        /// <summary>
        /// Append an element to this. The element will have the value of the EavValue. File and page references 
        /// can optionally be resolved.
        /// </summary>
        internal string ResolveValue(int appId, Guid itemGuid, string attrType, string value, bool resolveLinks)
        {
            if (value == null)
                return XmlConstants.Null;
            if (value == string.Empty)
                return XmlConstants.Empty;
            if (resolveLinks)
                return ResolveHyperlinksFromSite(appId, itemGuid, value, attrType);
            return value;
        }

        /// <summary>
        /// If the value is a file or page reference, resolve it for example from 
        /// File:4711 to Content/file4711.jpg. If the reference cannot be resolved, 
        /// the original value will be returned. 
        /// </summary>
        internal string ResolveHyperlinksFromSite(int appId, Guid itemGuid, string value, string attrType)
            => attrType != Constants.DataTypeHyperlink
                ? value
                : ValueConverter.ToValue(value, itemGuid);

        #endregion


    }

}