using System.Collections.Concurrent;

namespace ToSic.Eav.Models.Sys;

/// <summary>
/// Helper to figure out the true Content-Type names of models, based on the class name and some common suffixes.
/// </summary>
/// <remarks>
/// This is just the analyzer, no caching etc.
/// For that, use the DataModelAnalyzer
/// </remarks>
[ShowApiWhenReleased(ShowApiMode.Never)]
public class ModelNameVariants
{
    /// <summary>
    /// Figure out the expected ContentTypeName of a DataWrapper type.
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    /// If it is decorated with <see cref="ModelSpecsAttribute"/> then use the information it provides, otherwise
    /// use the type name.
    ///
    /// Note: as the code changed, we're not really doing the attribute check here anymore, but still keeping the structure for a quick cache.
    /// should probably be changed some day
    /// </remarks>
    internal static IList<string> GetCached(Type type) =>
        NameVariantsCache.GetOrAdd(type, _ =>
            CreateListOfNameVariants(type.Name, type.IsInterface)
        );

    private static readonly ConcurrentDictionary<Type, IList<string>> NameVariantsCache = new();

    internal static IList<string> GetFromNameOrFromType(string? names, Type type)
        => names?.CsvToArrayPreserveEmpty().ToListOpt()
           ?? GetCached(type);


    /// <summary>
    /// Take a class/interface name and create a list
    /// which also checks for the same name without leading "I" or without trailing "Model".
    /// </summary>
    internal static IList<string> CreateListOfNameVariants(string name, bool isInterface)
    {
        // Catch empty
        if (string.IsNullOrWhiteSpace(name))
            return [];

        // Start list containing initial name
        List<string> result = [name];

        // Check if it ends with Model
        foreach (var s in Suffixes)
            IfSuffixAddWithoutSuffix(name, s);
        
        // If it's not an interface beginning with "I", stop here
        if (!isInterface
            || !name.StartsWith("I", StringComparison.Ordinal)
            || name.Length <= 1 // Skip if only 1 char long, else below the Substring would be empty
           )
            return result;

        // ...otherwise add name without prefix, and retry combinations of suffixes
        // Add names without leading I - since it has a leading I
        var nameWithoutI = name.Substring(1);
        result.Add(nameWithoutI);

        foreach (var s in Suffixes)
            IfSuffixAddWithoutSuffix(nameWithoutI, s);

        return result;

        
        
        void IfSuffixAddWithoutSuffix(string baseName, string suf)
        {
            var s = baseName.EndsWith(suf)
                ? baseName.Substring(0, baseName.Length - suf.Length)
                : null;
            if (s != null)
                result.Add(s);
        }
    }

    private static readonly IList<string> Suffixes =
    [
        "FromEntity",
        "ModelFromEntity",
        "Model",
        // For now, don't support "Raw" as these should never be used to create models from entities.
        //"Raw",
        //"ModelRaw",
    ];
}