using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using ToSic.Eav.Implementations.ValueConverter;
using ToSic.Eav.ImportExport.Refactoring.Extensions;
using ToSic.Eav.ImportExport.Refactoring.Options;
using Microsoft.Practices.Unity;

namespace ToSic.Eav.ImportExport.Refactoring
{
    internal class XmlPartsBuilder
    {

        public void AppendEntityReferences(XElement element, Eav.Entity entity, Attribute attribute)
        {
            var entityGuids = attribute.ToSIC_EAV_EntityRelationships.Where(rel => rel.ParentEntityID == entity.EntityID)
                                                                     .Select(rel => rel.ChildEntity.EntityGUID);
            var entityGuidsString = string.Join(",", entityGuids);
            element.Append(attribute.StaticName, entityGuidsString);
        }

        /// <summary>
        /// Append an element to this. If the attribute is named xxx and the value is 4711 in the language specified, 
        /// the element appended will be <xxx>4711</xxx>. File and page references can be resolved optionally.
        /// </summary>
        public void AppendValueResolved(XElement element, Eav.Entity entity, Attribute attribute, string language, string languageFallback, ResourceReferenceExport resourceReferenceOption)
        {
            var valueName = attribute.StaticName;
            var value = entity.GetValueOfLanguageOrFallback(attribute, language, languageFallback);
            AppendValue(element, valueName, value, resourceReferenceOption);
        }

        /// <summary>
        /// Append an element to this. The element will get the name of the attribute, and if possible the value will 
        /// be referenced to another language (for example [ref(en-US,ro)].
        /// </summary>
        public void AppendValueReferenced(XElement element, Eav.Entity entity, Attribute attribute, string language, string languageFallback, IEnumerable<string> languageScope, bool referenceParentLanguagesOnly, ResourceReferenceExport resourceReferenceOption)
        {
            var valueName = attribute.StaticName;
            var value = entity.GetValueOfExactLanguage(attribute, language);
            if (value == null)
            {
                element.Append(valueName, "[]");
                return;
            }

            var valueLanguage = value.ValuesDimensions.Select(reference => reference.Dimension.ExternalKey)
                                         .FirstOrDefault(l => l == language); // value.GetLanguage(language);
            if (valueLanguage == null)
            {   // If no language is found, serialize the plain value
                AppendValue(element, valueName, value, resourceReferenceOption);
                return;
            }

            var valueLanguagesReferenced = GetLanguagesReferenced(value, language, true)
                                                .OrderBy(lang => lang != languageFallback)
                                                .ThenBy(lan => lan);
            if (!valueLanguagesReferenced.Any())
            {   // If the value is a head value, serialize the plain value
                AppendValue(element, valueName, value, resourceReferenceOption);
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
                AppendValue(element, valueName, value, resourceReferenceOption);
                return;
            }

            element.Append(valueName, $"[ref({valueLanguageReferenced},{(valueLanguageReadOnly ? "ro" : "rw")})]");
        }



        /// <summary>
        /// Append an element to this. The element will have the value of the EavValue. File and page references 
        /// can optionally be resolved.
        /// </summary>
        private void AppendValue(XElement element, XName name, EavValue value, ResourceReferenceExport resourceReferenceOption)
        {
            if (value == null)
            {
                element.Append(name, "[]");
            }
            else if (value.Value == null)
            {
                element.Append(name, "[]");
            }
            else if (resourceReferenceOption == ResourceReferenceExport.Resolve)
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
        private string ResolveHyperlinksFromTennant(EavValue value)
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
        private IEnumerable<string> GetLanguagesReferenced(EavValue value, string valueLanguage, bool referenceReadWrite)
        {
            return value.ValuesDimensions.Where(reference => !referenceReadWrite || !reference.ReadOnly)
                                         .Where(reference => reference.Dimension.ExternalKey != valueLanguage)
                                         .Select(reference => reference.Dimension.ExternalKey)
                                         .ToList();
        }

    }
}
