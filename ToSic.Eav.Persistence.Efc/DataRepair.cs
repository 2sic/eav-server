using System.Linq;
using ToSic.Eav.Data;

namespace ToSic.Eav.Persistence.Efc
{
    internal class DataRepair
    {
        public static void FixIncorrectLanguageDefinitions(IAttribute attrib, string primaryLanguage)
        {
            // Background: there are rare cases, where data was stored incorrectly
            // this happens when a attribute has multiple values, but some don't have languages assigned
            // that would be invalid, as any property with a language code must have all the values (for that property) with language codes
            // Case 1 ok: Value has max 1 real value, so no risk
            if (attrib.Values.Count <= 1) return;
            // Case 2 ok: All values have languages assigned
            if (attrib.Values.All(v => v.Languages.Any())) return;


            var badValuesWithoutLanguage = attrib.Values.Where(v => !v.Languages.Any()).ToList();
            if (!badValuesWithoutLanguage.Any()) return;

            // new 2020-11-12 We sometimes ran into old data which had this problem
            // but since the primary language was the missing one, this caused a lot of follow up
            // so no we want to check if the primary language is missing - and if yes, assign that
            var hasPrimary = attrib.Values.Any(v => v.Languages.Any(l => l.Key == primaryLanguage));

            // only attach the primary language to a value if we don't already have a primary value
            if (!hasPrimary)
            {
                var firstWithoutLanguage = badValuesWithoutLanguage.First();
                firstWithoutLanguage.Languages.Add(new Language
                {
                    DimensionId = 0, // unknown - should be fine...
                    Key = primaryLanguage,
                    ReadOnly = false
                });

                // Skip the modified item and check if we still have any to remove
                badValuesWithoutLanguage.Remove(firstWithoutLanguage);
            }

            if (badValuesWithoutLanguage.Any())
                badValuesWithoutLanguage.ForEach(badValue =>
                    attrib.Values.Remove(badValue));
        }
    }
}
