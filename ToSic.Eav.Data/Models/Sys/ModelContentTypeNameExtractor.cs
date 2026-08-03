using ToSic.Eav.Data.ContentTypes;
using ToSic.Sys.Utils.Types;
using static ToSic.Eav.Models.Sys.ModelContentTypeNameCacheKeys;

namespace ToSic.Eav.Models.Sys;

internal static class ModelContentTypeNameExtractor
{
    /// <summary>
    /// Figure out the names which the current combination of types would result in.
    /// </summary>
    /// <returns>A tuple containing the cache key prefix and a list of names.</returns>
    internal static (string CacheKeyPrefix, IList<string> Names) GetNames(ToModelSpecs specs)
    {
        // Extract types from the ToModelSpecs record
        var (entryType, concreteType, toModelOptions, _, _) = specs;
        var optionsTypeName = toModelOptions.TypeName;
        
        // 1. If we have options, then this will pre-determine what is checked, so this would be what we use
        if (optionsTypeName != null)
            return (CacheKeyOptions(optionsTypeName), optionsTypeName.CsvToArrayWithoutEmpty());

        // Check if we have one or two types to check (interface vs concrete type)
        Type[] typesToCheck = entryType == concreteType ? [entryType] : [entryType, concreteType];

        // Check the one or two types for ModelSpecsAttribute or ContentTypeAttribute, and return the first one found
        foreach (var type in typesToCheck)
        {
            var onAttribute = GetTypeNameOnModelSpecs(type)
                              ?? GetTypeNameOnContentType(type);
            if (onAttribute != null)
                return (CacheKeyForType(CachePrefixType, type), onAttribute.CsvToArrayWithoutEmpty());
        }

        // Check for automatically derived names
        // The values don't need to be in the cache, as the derived names are deterministic
        var namesDerived = typesToCheck
            .SelectMany(ModelNameVariants.GetCached)
            .ToArray();

        return (CacheKeyForType(CachePrefixDerived, entryType), namesDerived);
    }

    private static string? GetTypeNameOnModelSpecs(Type tCustom) =>
        TypeNameOnModelSpecsCache.Get<ModelSpecsAttribute>(tCustom, attribute => attribute?.ContentType);

    private static readonly TypeAttributeLookup<string?> TypeNameOnModelSpecsCache = new();

    private static string? GetTypeNameOnContentType(Type tCustom) =>
        TypeNameOnContentTypeCache.Get<ContentTypeAttribute>(tCustom, attribute => attribute?.Name);

    private static readonly TypeAttributeLookup<string?> TypeNameOnContentTypeCache = new();

}