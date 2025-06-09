using System.Collections.Immutable;
using ToSic.Eav.Data.Dimensions.Sys;

namespace ToSic.Eav.Data.Build;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class DimensionBuilder
{
    public Language CreateFrom(ILanguage orig, bool? readOnly) => new(orig.Key, readOnly ?? orig.ReadOnly, orig.DimensionId);

    public IImmutableList<ILanguage> Merge(IEnumerable<ILanguage> languages, List<ILanguage> updates)
    {
        var lngList = languages
            .Select(l => updates.FirstOrDefault(ul => ul.Key.EqualsInsensitive(l.Key)) ?? l)
            .ToListOpt();
        var rest = updates
            .Where(ul => !lngList.Any(l => l.Key.EqualsInsensitive(ul.Key)));
        var final = lngList
            .Concat(rest)
            .ToImmutableSafe();
        return final;
    }


    public List<ILanguage> GetBestValueLanguages(string? language, bool languageReadOnly)
    {
        // sometimes language is passed in as an empty string - this would have side effects, so it must be neutralized
        if (string.IsNullOrWhiteSpace(language))
            language = null;

        var valueLanguages = language == null
            ? null // must be null if no languages are specified, to cause proper fallback
            : new List<ILanguage> { new Language(language, languageReadOnly) };

        return valueLanguages ?? [];
    }

}