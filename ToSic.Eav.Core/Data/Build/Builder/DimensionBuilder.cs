using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Plumbing;

namespace ToSic.Eav.Data.Build
{
    public class DimensionBuilder
    {
        public Language Clone(ILanguage orig) => Clone(orig, null);

        public Language Clone(ILanguage orig, bool? readOnly) => new Language(orig.Key, readOnly ?? orig.ReadOnly, orig.DimensionId);

        //public IImmutableList<ILanguage> Clone(IList<ILanguage> orig) => orig
        //    // 2023-02-24 2dm optimized this, keep comment till ca. 2023-04 in case something breaks
        //    //.Select(l => new Language { DimensionId = l.DimensionId, Key = l.Key } as ILanguage)
        //    .Select(l => Clone(l, null) as ILanguage)
        //    .ToImmutableList();

        // Note: can't be a new ImmutableArray<ILanguage>() because that causes problems!
        // Not sure why, but it complains that it can't tolist it
        public static IImmutableList<ILanguage> NoLanguages = ImmutableList<ILanguage>.Empty; // new List<ILanguage>().ToImmutableList();// new ImmutableArray<ILanguage>();


        public ImmutableList<ILanguage> Merge(IEnumerable<ILanguage> languages, List<ILanguage> updates)
        {
            languages = languages.ToList();
            languages = languages
                .Select(l => updates.FirstOrDefault(ul => ul.Key.EqualsInsensitive(l.Key)) ?? l)
                .ToList();
            var rest = updates.Where(ul => !languages.Any(l => l.Key.EqualsInsensitive(ul.Key)));
            var final = languages.Concat(rest).ToImmutableList();
            return final;
        }


        public List<ILanguage> GetBestValueLanguages(string language, bool languageReadOnly)
        {
            // sometimes language is passed in as an empty string - this would have side effects, so it must be neutralized
            if (string.IsNullOrWhiteSpace(language)) language = null;

            var valueLanguages = language == null
                ? null // must be null if no languages are specified, to cause proper fallback
                // 2023-02-24 2dm #immutable
                //: new List<ILanguage> { new Language { Key = language, ReadOnly = languageReadOnly } }, allEntitiesForRelationships);
                : new List<ILanguage> { new Language(language, languageReadOnly) };

            return valueLanguages;
        }

    }
}
