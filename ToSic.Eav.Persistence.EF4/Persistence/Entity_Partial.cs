using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToSic.Eav
{
    public partial class Entity
    {
        /// <summary>
        /// Get the value of an attribute in the language specified.
        /// </summary>
        public EavValue GetValueOfLanguageOrFallback(Attribute attribute, string language,
            string languageFallback)
            => GetValueOfExactLanguage(attribute, language) ??
               GetValueOfExactLanguage(attribute, languageFallback);

        /// <summary>
        /// Get the value of an attribute in the language specified.
        /// </summary>
        public EavValue GetValueOfExactLanguage(Attribute attribute, string language)
        {
            var values = Values.Where(value => value.Attribute.StaticName == attribute.StaticName);
            if (string.IsNullOrEmpty(language))
                return values.FirstOrDefault(value => !value.ValuesDimensions.Any());

            // When we enable languages in 2sxc, but have not saved the content yet, then it doesn't have an assigned language
            var rootValue = values.FirstOrDefault();
            if (rootValue != null && rootValue.ValuesDimensions.Count == 0)
                return rootValue;

            return values.FirstOrDefault(value => value.ValuesDimensions.Any(reference => reference.Dimension.ExternalKey == language));
        }
    }
}
