namespace ToSic.Eav.Models.Sys;

/// <summary>
/// Helper to figure out the true Content-Type names of models, based on the class name and some common suffixes.
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public class DataModelNames
{
    internal static List<string> UseSpecifiedNameOrDeriveFromType<TCustom>(string? names)
        where TCustom : class
    {
        var list = names != null
            ? names.Split(',').Select(n => n.Trim()).ToList()
            : CreateListOfNameVariants(typeof(TCustom).Name, typeof(TCustom).IsInterface);
        return list;
    }

    internal static List<string> UseSpecifiedNameOrDeriveFromType(Type type, string? names)
    {
        var list = names != null
            ? names.Split(',').Select(n => n.Trim()).ToList()
            : CreateListOfNameVariants(type.Name, type.IsInterface);
        return list;
    }


    /// <summary>
    /// Take a class/interface name and create a list
    /// which also checks for the same name without leading "I" or without trailing "Model".
    /// </summary>
    internal static List<string> CreateListOfNameVariants(string name, bool isInterface)
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