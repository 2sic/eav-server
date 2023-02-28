using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.Data;

namespace ToSic.Eav.Persistence.Efc
{
    internal class DataRepair
    {
        /// <summary>
        /// Background: there are rare cases, where data was stored incorrectly
        /// this happens when a attribute has multiple values, but some don't have languages assigned
        /// that would be invalid, as any property with a language code must have all the values (for that property) with language codes
        /// </summary>
        /// <param name="attrib"></param>
        /// <param name="primaryLanguage"></param>
        public static IList<IValue> FixIncorrectLanguageDefinitions(IList<IValue> values, string primaryLanguage)
        {
            //var values = attrib.Values;
            // Case 1 ok: Value has max 1 real value, so no risk
            if (values.Count <= 1) return values;
            // Case 2 ok: All values have languages assigned
            if (values.All(v => v.Languages.Any())) return values;


            var badValuesWithoutLanguage = values.Where(v => !v.Languages.Any()).ToList();
            if (!badValuesWithoutLanguage.Any()) return values;

            // new 2020-11-12 We sometimes ran into old data which had this problem
            // but since the primary language was the missing one, this caused a lot of follow up
            // so no we want to check if the primary language is missing - and if yes, assign that
            var hasPrimary = values.Any(v => v.Languages.Any(l => l.Key == primaryLanguage));

            // only attach the primary language to a value if we don't already have a primary value
            if (!hasPrimary)
            {
                var firstWithoutLanguage = badValuesWithoutLanguage.First();
                firstWithoutLanguage.Languages.Add(new Language(primaryLanguage, false));

                // Skip the modified item and check if we still have any to remove
                badValuesWithoutLanguage.Remove(firstWithoutLanguage);
            }

            if (badValuesWithoutLanguage.Any())
                badValuesWithoutLanguage.ForEach(badValue =>
                    values.Remove(badValue));
            
            return values;
        }
    }
}
