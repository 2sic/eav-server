using ToSic.Eav.Data.ContentTypes;
using ToSic.Sys.Utils.Types;
using static ToSic.Eav.Models.Sys.ModelContentTypeNameCacheKeys;

namespace ToSic.Eav.Models.Sys;

internal static class ModelContentTypeNameExtractor
{
    /// <summary>
    /// Figure out the names which the current combination of types would result in.
    /// </summary>
    /// <param name="optionsTypeName">Options provided to check first.</param>
    /// <param name="entryType">The entry type to check - could be an interface.</param>
    /// <param name="concreteType">The concrete type to check - could be the same as the entry type or if the entry is an interface, it would be the implementing class.</param>
    /// <returns>A tuple containing the cache key prefix and a list of names.</returns>
    internal static (string CacheKeyPrefix, IList<string> Names) GetNames(string? optionsTypeName, Type entryType, Type concreteType)
    {
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