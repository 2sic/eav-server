using ToSic.Eav.Data.Sys.Dimensions;

namespace ToSic.Eav.ImportExport.Sys.Xml;

/// <summary>
/// Helper to create the language map to assign imported data to target languages.
/// </summary>
internal class ImportLanguageMap
{

    internal static ICollection<TargetLanguageToSourceLanguage> PrepareTargetToSourceLanguageMapping(
        ICollection<DimensionDefinition> envLanguages,
        string envDefLang,
        ICollection<DimensionDefinition> srcLanguages,
        int? srcDefLang,
        ILog? logDetails)
    {
        var l = logDetails.Fn<ICollection<TargetLanguageToSourceLanguage>>($"Env has {envLanguages.Count} languages");

        // if the environment has languages, use these
        if (envLanguages.Any())
        {
            // Order before using, default language first, then A-Z
            envLanguages = envLanguages
                .OrderByDescending(p => p.Matches(envDefLang))
                .ThenBy(p => p.EnvironmentKey)
                .ToListOpt();

            var usingEnv = envLanguages
                .Select(el => new TargetLanguageToSourceLanguage
                {
                    Active = el.Active,
                    Key = el.Key,
                    Name = el.Name,
                    EnvironmentKey = el.EnvironmentKey,
                    Id = el.Id,
                    ParentId = null,
                    PrioritizedDimensions = FindPrioritizedMatchingDimensions(el, envDefLang, srcLanguages, srcDefLang),
                })
                .ToListOpt();
            return l.Return(usingEnv, $"Using Env: LanguageMap has {usingEnv.Count} items");
        }

        // if the environment doesn't have languages defined, we'll create a temp-entry for the main language to allow mapping
        var tempDimension = new DimensionDefinition
        {
            Active = true,
            Id = 0,
            ParentId = null,
            EnvironmentKey = envDefLang,
            Key = "dummy-not-used",
            Name = "Default",
        };
        var result = new TargetLanguageToSourceLanguage
        {
            Id = 0,
            ParentId = null,
            Active = true,
            EnvironmentKey = envDefLang,
            PrioritizedDimensions = FindPrioritizedMatchingDimensions(tempDimension, envDefLang, srcLanguages, srcDefLang),
            Name = tempDimension.Name,
            Key = tempDimension.Key,
        };

        return l.Return([result], "No Env: LanguageMap has 1 items");
    }

    private static List<DimensionDefinition> FindPrioritizedMatchingDimensions(DimensionDefinition targetLang, string envDefLang, ICollection<DimensionDefinition> srcLangs, int? srcDefLang)
    {
        var languageMap = new List<DimensionDefinition>();

        // Add exact match source language, if exists
        var exactMatchSourceDimension = srcLangs
            .FirstOrDefault(p => targetLang.Matches(p.EnvironmentKey));
        if (exactMatchSourceDimension != null)
            languageMap.Add(exactMatchSourceDimension);

        // Add non-exact match language
        var unExactMatchSourceDimensions = srcLangs
            .Where(sd =>
                !targetLang.Matches(sd.EnvironmentKey) &&
                sd.EnvironmentKey.StartsWith(targetLang.EnvironmentKey.ToLowerInvariant().Substring(0, 2))
            )
            .OrderByDescending(p => p.EnvironmentKey == envDefLang)
            .ThenByDescending(p => p.EnvironmentKey.Length == 2 || p.EnvironmentKey.Length == 5 &&
                p.EnvironmentKey.Substring(0, 2) == p.EnvironmentKey.Substring(3, 2)
            )
            .ThenBy(p => p.EnvironmentKey);
        languageMap.AddRange(unExactMatchSourceDimensions);

        // Add primary language, if current target is primary
        if (targetLang.Matches(envDefLang) && srcDefLang.HasValue)
        {
            var sourcePrimaryLanguage = srcLangs
                .FirstOrDefault(p => p.Id == srcDefLang);
            if (sourcePrimaryLanguage != null && !languageMap.Contains(sourcePrimaryLanguage))
                languageMap.Add(sourcePrimaryLanguage);
        }

        return languageMap;
    }
}