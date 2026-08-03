using ToSic.Eav.Data.ContentTypes;
using ToSic.Sys.Caching.Keys;
using ToSic.Sys.Utils.Types;

namespace ToSic.Eav.Models.Sys;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class ModelContentTypeNameAnalyzer
{
    /// <summary>
    /// Main cache, remembering which type names have already been verified as allowed for a specific type and nameId.
    /// The string is a cache key describing the parameters which were used to check if it's ok.
    /// </summary>
    private static readonly HashSet<string> TypeNameAllowedCache = [];

    private static string? GetTypeNameOnModelSpecs(Type tCustom) =>
        TypeNameOnModelSpecsCache.Get<ModelSpecsAttribute>(tCustom, attribute => attribute?.ContentType);
    
    private static readonly TypeAttributeLookup<string?> TypeNameOnModelSpecsCache = new();

    private static string? GetTypeNameOnContentType(Type tCustom) =>
        TypeNameOnContentTypeCache.Get<ContentTypeAttribute>(tCustom, attribute => attribute?.Name);

    private static readonly TypeAttributeLookup<string?> TypeNameOnContentTypeCache = new();

    internal bool PreFlightCheck(string? optionsTypeName, Type entryType, string? nameIdOrNull)
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

    private const string CachePrefixType = "type";
    private const string CachePrefixDerived = "derived";
    
    private static string CacheKeyOptions(string optionsTypeName, string nameId) => $"option:{optionsTypeName}|{nameId}";
    private static string CacheKeyForType(string prefix, Type entryType, string nameIdOrNull) => $"{prefix}:{entryType.FullName}|{nameIdOrNull}";

    /// <summary>
    /// The main system checking for name priorities.
    /// Will also check if it had already been verified by the cache, to reduce work in retrieving names etc.
    /// </summary>
    /// <param name="optionsTypeName"></param>
    /// <param name="entryType"></param>
    /// <param name="concreteType"></param>
    /// <param name="nameIdOrNull"></param>
    /// <returns></returns>
    internal static (bool IsCachedAsVerified, string CacheKey, IList<string>? Names) FindPriorityTypeNames(string? optionsTypeName, Type entryType, Type concreteType, string? nameIdOrNull)
    {
        if (optionsTypeName == ToModelOptions.TypeNameAny)
            return (true, "any", null);

        if (optionsTypeName != null)
            return CheckAndReturn(CacheKeyOptions(optionsTypeName, nameIdOrNull), optionsTypeName);

        // Check if we have one or two types to check (interface vs concrete type)
        var typesDiffer = entryType != concreteType;
        Type[] typesToCheck = typesDiffer
            ? [entryType, concreteType]
            : [entryType];

        // Check the one or two types for ModelSpecsAttribute or ContentTypeAttribute, and return the first one found
        foreach (var type in typesToCheck)    
        {
            var explicitOnModelSpecs = GetTypeNameOnModelSpecs(type);
            if (explicitOnModelSpecs != null)
                return CheckAndReturn(CacheKeyForType(CachePrefixType, type, nameIdOrNull), explicitOnModelSpecs);

            var explicitOnCtSpecs = GetTypeNameOnContentType(type);
            if (explicitOnCtSpecs != null)
                return CheckAndReturn(CacheKeyForType(CachePrefixType, type, nameIdOrNull), explicitOnCtSpecs);
        }

        // Check for automatically derived names
        // The values don't need to be in the cache, as the derived names are deterministic
        var cacheKeyFinal = CacheKeyForType(CachePrefixDerived, entryType, nameIdOrNull);
        if (TypeNameAllowedCache.Contains(cacheKeyFinal))
            return (true, cacheKeyFinal, null);
        
        var namesDerived = typesDiffer
            ? ModelNameVariants.GetCached(entryType).Concat(ModelNameVariants.GetCached(concreteType)).ToArray()
            : ModelNameVariants.GetCached(entryType);

        return (false, cacheKeyFinal, namesDerived);

        
        
        (bool IsCachedAsVerified, string CacheKey, IList<string>? Names) CheckAndReturn(string cacheKey, string names)
            => nameIdOrNull != null && TypeNameAllowedCache.Contains(cacheKey)
                ? (true, cacheKey, null)
                : (false, cacheKey, names.CsvToArrayWithoutEmpty());
        
        
    }

    public static (bool IsError, IList<string>? Names) IsTypeNameAllowed(string? optionsTypeName, Type entryType, Type concreteType, IContentType contentType)
    {
        var priorities = FindPriorityTypeNames(optionsTypeName, entryType, concreteType, contentType.NameId);

        if (priorities.IsCachedAsVerified)
            return (false, null);

        var typeNames = priorities.Names ?? [];
        
        // CacheKey - note that we'll only cache it if it's ok, never if it fails, to avoid RAM consumption for invalid types
        // We only need the initial type, because even if it's an interface, it will always result in the same concrete type
        if (TypeNameAllowedCache.Contains(priorities.CacheKey))
            return (false, null);

        if (!typeNames.Any(t => t == ToModelOptions.TypeNameAny || contentType.Is(t)))
            return (true, typeNames);
        
        TypeNameAllowedCache.Add(priorities.CacheKey);
        return (false, typeNames);

    }

    public static KeyNotFoundException KeyNotFoundMessage(IList<string>? typeNames, IContentType contentType, object idForErrors)
        => new(
            $"Item with ID {idForErrors} is a '{contentType.Name}'/'{contentType.NameId}' but not a '{string.Join(",", typeNames ?? [])}'. " +
            $"This is probably a mistake, otherwise set '{nameof(ToModelOptions.TypeName)}: '*' " +
            $"or apply an attribute [{nameof(ModelSpecsAttribute)}({nameof(ModelSpecsAttribute.ContentType)} = \"{contentType.Name}\")] to your model class. "
        );
}

public static class ModelNameVerifier
{
    
}