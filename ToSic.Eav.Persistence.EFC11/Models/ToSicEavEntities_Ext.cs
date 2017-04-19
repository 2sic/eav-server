using System.Linq;

namespace ToSic.Eav.Persistence.EFC11.Models
{
    public partial class ToSicEavEntities
    {
        /// <summary>
        /// Get the value of an attribute in the language specified.
        /// </summary>
        public ToSicEavValues GetValueOfLanguageOrFallback(ToSicEavAttributes attribute, string language,
            string languageFallback)
            => GetValueOfExactLanguage(attribute, language) ??
               GetValueOfExactLanguage(attribute, languageFallback);

        /// <summary>
        /// Get the value of an attribute in the language specified.
        /// </summary>
        public ToSicEavValues GetValueOfExactLanguage(ToSicEavAttributes attribute, string language)
        {
            var values = ToSicEavValues.Where(value => value.Attribute.StaticName == attribute.StaticName).ToList();
            if (string.IsNullOrEmpty(language))
                return values.FirstOrDefault(value => !value.ToSicEavValuesDimensions.Any());

            // When we enable languages in 2sxc, but have not saved the content yet, then it doesn't have an assigned language
            var rootValue = values.FirstOrDefault();
            if (rootValue != null && rootValue.ToSicEavValuesDimensions.Count == 0)
                return rootValue;

            return
                values.FirstOrDefault(
                    value => value.ToSicEavValuesDimensions.Any(reference => reference.Dimension.ExternalKey == language));
        }
    }
}