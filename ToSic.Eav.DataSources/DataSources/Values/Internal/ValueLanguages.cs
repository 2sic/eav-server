﻿using ToSic.Eav.Context;

namespace ToSic.Eav.DataSources.Internal;

[PrivateApi]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class ValueLanguages(IZoneCultureResolver cultureResolver) : ServiceBase("Ds.ValLng", connect: [cultureResolver])
{
    /// <summary>
    /// Constants for certain resolution modes
    /// </summary>
    internal const string LanguageDefaultPlaceholder = "default";
    internal const string LanguageCurrentPlaceholder = "current";

    /// <summary>
    /// Prepare language list to use in lookup
    /// </summary>
    /// <returns></returns>
    internal string[] PrepareLanguageList(string languages) => Log.Func(languages, l =>
    {
        var lang = languages.ToLowerInvariant().Trim();

        var resolved = ResolveOneLanguageCode(lang);

        // if null, not ok - continue to error
        // if String.Empty, then we just want the default, so use empty array (faster)
        if (resolved != null)
            return (resolved == string.Empty ? Array.Empty<string>() : [resolved], resolved);


        l.E($"Error - can't figure out '{lang}'");
        var ex = new Exception($"Can't figure out what language to use: '{lang}'. Expected '{LanguageDefaultPlaceholder}', '{LanguageCurrentPlaceholder}' or a 2-character code");
        throw l.Done(ex);
    });

    private string ResolveOneLanguageCode(string lang) => Log.Func(lang, () =>
    {
        if (string.IsNullOrWhiteSpace(lang) || lang == LanguageDefaultPlaceholder)
            return (string.Empty, LanguageDefaultPlaceholder); // empty string / no language means the default language

        if (lang == LanguageCurrentPlaceholder)
            return (cultureResolver.SafeCurrentCultureCode(), LanguageCurrentPlaceholder);

        if (lang.Length == 2 || lang.Length == 5 && lang.Contains("-"))
            return (lang, "Exact" + lang);

        return (null, "");
    });
}