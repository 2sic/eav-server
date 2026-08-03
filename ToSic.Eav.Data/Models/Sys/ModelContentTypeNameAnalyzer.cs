using static ToSic.Eav.Models.Sys.ModelContentTypeNameCacheKeys;

namespace ToSic.Eav.Models.Sys;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class ModelContentTypeNameAnalyzer
{
    /// <summary>
    /// Main cache, remembering which type names have already been verified as allowed for a specific type and nameId.
    /// The string is a cache key describing the parameters which were used to check if it's ok.
    /// </summary>
    internal static readonly HashSet<string> TypeNameAllowedCache = [];
    
    /// <summary>
    /// Do a pre-flight to check if the current combination of parameters was already checked and verified.
    /// </summary>
    /// <param name="optionsTypeName"></param>
    /// <param name="entryType"></param>
    /// <param name="nameIdOrNull"></param>
    /// <returns></returns>
    internal static bool PreFlightCheck(string? optionsTypeName, Type entryType, string? nameIdOrNull)
    {
        // If configured to match "Any" it's always ok.
        if (optionsTypeName == ToModelOptions.TypeNameAny)
            return true;

        // If we don't have a name-id, then we can't check the cache, so it's not ok; requires more information to check
        if (nameIdOrNull == null)
            return false;
        
        // If we have options, then this will pre-determine what is checked, so this would be the cached info
        if (optionsTypeName != null)
            return TypeNameAllowedCache.Contains(CacheKeyOptions(optionsTypeName, nameIdOrNull));

        // Check if we have a cache key for the type, which would have been cached if it was already verified as ok
        if (TypeNameAllowedCache.Contains(CacheKeyForType(CachePrefixType, entryType, nameIdOrNull)))
            return true;

        // Check if we have a cache key for derived names, which would have been cached if it was already verified as ok
        if (TypeNameAllowedCache.Contains(CacheKeyForType(CachePrefixDerived, entryType, nameIdOrNull)))
            return true;

        return false;
    }

    /// <summary>
    /// Check if the current set of parameters are allowed for a given ContentType.
    /// If not, return the list of type names which are allowed for this ContentType.
    /// </summary>
    /// <param name="optionsTypeName"></param>
    /// <param name="entryType"></param>
    /// <param name="concreteType"></param>
    /// <param name="contentType"></param>
    /// <returns></returns>
    public static (bool IsOk, IList<string>? Names) IsTypeNameAllowed(string? optionsTypeName, Type entryType, Type concreteType, IContentType contentType)
    {
        if (PreFlightCheck(optionsTypeName, entryType, contentType.NameId))
            return (true, null);

        var (cacheKeyPrefix, typeNames) = ModelContentTypeNameExtractor.GetNames(optionsTypeName, entryType, concreteType);

        var cacheKey = cacheKeyPrefix + contentType.NameId;
        
        // CacheKey - note that we'll only cache it if it's ok, never if it fails, to avoid RAM consumption for invalid types
        if (TypeNameAllowedCache.Contains(cacheKey))
            return (true, null);

        if (!typeNames.Any(t => t == ToModelOptions.TypeNameAny || contentType.Is(t)))
            return (false, typeNames);
        
        TypeNameAllowedCache.Add(cacheKey);
        return (true, null);

    }

    public static KeyNotFoundException KeyNotFoundMessage(IList<string>? typeNames, IContentType contentType, object ids)
        => new(
            $"Item with ID {ids} is a '{contentType.Name}'/'{contentType.NameId}' but not a '{string.Join(",", typeNames ?? [])}'. " +
            $"This is probably a mistake, otherwise set '{nameof(ToModelOptions.TypeName)}: '*' " +
            $"or apply an attribute [{nameof(ModelSpecsAttribute)}({nameof(ModelSpecsAttribute.ContentType)} = \"{contentType.Name}\")] to your model class. "
        );
}