using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using ToSic.Eav.Plumbing;

namespace ToSic.Eav.Data.Build;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class DimensionBuilder
{
    public Language CreateFrom(ILanguage orig, bool? readOnly) => new(orig.Key, readOnly ?? orig.ReadOnly, orig.DimensionId);

    // Note: can't be a new ImmutableArray<ILanguage>() because that causes problems!
    // Not sure why, but it complains that it can't tolist it
    public static IImmutableList<ILanguage> NoLanguages = ImmutableList<ILanguage>.Empty;


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
            : new List<ILanguage> { new Language(language, languageReadOnly) };

        return valueLanguages;
    }

}