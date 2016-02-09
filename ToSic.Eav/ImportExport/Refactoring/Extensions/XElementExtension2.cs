﻿using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using ToSic.Eav.ImportExport.Refactoring.Options;

namespace ToSic.Eav.ImportExport.Refactoring.Extensions
{
    internal static class XElementExtension2
    {
        public static void AppendEntityReferences(this XElement element, Eav.Entity entity, Attribute attribute)
        {
            var entityGuids = attribute.ToSIC_EAV_EntityRelationships.Where(rel => rel.ParentEntityID == entity.EntityID)
                                                                     .Select(rel => rel.ChildEntity.EntityGUID);
            var entityGuidsString = string.Join(",", entityGuids);
            element.Append(attribute.StaticName, entityGuidsString);
        }

        /// <summary>
        /// Append an element to this. The element will have the value of the EavValue. File and page references 
        /// can optionally be resolved.
        /// </summary>
        public static void AppendValue(this XElement element, XName name, EavValue value, ResourceReferenceExport resourceReferenceOption)
        {
            if (value == null)
            {
                element.Append(name, "[]");
            }
            else if (value.Value == null)
            {
                element.Append(name, "[]");
            }            
            else if (resourceReferenceOption.IsResolve())
            {
                element.Append(name, value.ResolveValueReference());
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
        /// Append an element to this. If the attribute is named xxx and the value is 4711 in the language specified, 
        /// the element appended will be <xxx>4711</xxx>. File and page references can be resolved optionally.
        /// </summary>
        public static void AppendValueResolved(this XElement element, Eav.Entity entity, Attribute attribute, string language, string languageFallback, ResourceReferenceExport resourceReferenceOption)
        {
            var valueName = attribute.StaticName;
            var value = entity.GetAttributeValue(attribute, language, languageFallback);
            element.AppendValue(valueName, value, resourceReferenceOption);
        }

        /// <summary>
        /// Append an element to this. The element will get the name of the attribute, and if possible the value will 
        /// be referenced to another language (for example [ref(en-US,ro)].
        /// </summary>
        public static void AppendValueReferenced(this XElement element, Eav.Entity entity, Attribute attribute, string language, string languageFallback, IEnumerable<string> languageScope, bool referenceParentLanguagesOnly, ResourceReferenceExport resourceReferenceOption)
        {
            var valueName = attribute.StaticName;
            var value = entity.GetAttributeValue(attribute, language);
            if (value == null)
            {
                element.Append(valueName, "[]");
                return;
            }

            var valueLanguage = value.ValuesDimensions.Select(reference => reference.Dimension.ExternalKey)
                                         .FirstOrDefault(l => l == language); // value.GetLanguage(language);
            if (valueLanguage == null)
            {   // If no language is found, serialize the plain value
                element.AppendValue(valueName, value, resourceReferenceOption);
                return;
            }

            var valueLanguagesReferenced = value.GetLanguagesReferenced(language, true)
                                                .OrderBy(lang => lang != languageFallback)
                                                .ThenBy(lan => lan);;
            if (valueLanguagesReferenced.Count() == 0)
            {   // If the value is a head value, serialize the plain value
                element.AppendValue(valueName, value, resourceReferenceOption);
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
                element.AppendValue(valueName, value, resourceReferenceOption);
                return;
            }

            element.Append(valueName, string.Format("[ref({0},{1})]", valueLanguageReferenced, valueLanguageReadOnly ? "ro" : "rw"));
        }
    
    }
}