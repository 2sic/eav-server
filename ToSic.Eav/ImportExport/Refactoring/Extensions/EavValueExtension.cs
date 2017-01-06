using System.Collections.Generic;
using System.Linq;
using Microsoft.Practices.Unity;
using ToSic.Eav.Implementations.ValueConverter;

namespace ToSic.Eav.ImportExport.Refactoring.Extensions
{
    internal static class EavValueExtension
    {
        /// <summary>
        /// If the value is a file or page reference, resolve it for example from 
        /// File:4711 to Content/file4711.jpg. If the reference cannot be reoslved, 
        /// the original value will be returned. 
        /// </summary>
        public static string ResolveValueReference(this EavValue value)
        {
            if (value.Attribute.Type == Constants.Hyperlink)
            {
                var vc = Factory.Container.Resolve<IEavValueConverter>();
                return vc.Convert(ConversionScenario.GetFriendlyValue, Constants.Hyperlink, value.Value);
                //return ResolveHyperlink(value);
            }
            return value.Value;
        }

        
        //public static string GetLanguage(this EavValue value, string valueLanguage)
        //{
        //    return value.ValuesDimensions.Select(reference => reference.Dimension.ExternalKey)
        //                                 .FirstOrDefault(language => language == valueLanguage);
        //}

        ///// <summary>
        ///// Get all languages this value is referenced from.
        ///// </summary>
        //public static IEnumerable<string> GetLanguages(this EavValue value)
        //{
        //    return value.ValuesDimensions.Select(reference => reference.Dimension.ExternalKey);
        //}

        /// <summary>
        /// Get languages this value is referenced from, but not the language specified. The 
        /// method helps to find languages the value belongs to expect the current language.
        /// </summary>
        public static IEnumerable<string> GetLanguagesReferenced(this EavValue value, string valueLanguage, bool referenceReadWrite)
        {
            return value.ValuesDimensions.Where(reference => referenceReadWrite ? !reference.ReadOnly : true)
                                         .Where(reference => reference.Dimension.ExternalKey != valueLanguage)
                                         .Select(reference => reference.Dimension.ExternalKey)
                                         .ToList();
        }

        /// <summary>
        /// Check if a language reference is read-only.
        /// </summary>
        public static bool IsLanguageReadOnly(this EavValue value, string language)
        {
            var languageReference = value.ValuesDimensions.FirstOrDefault(reference => reference.Dimension.ExternalKey == language);
            if (languageReference == null)
            {
                return false;
            }
            return languageReference.ReadOnly;
        }
    }
}