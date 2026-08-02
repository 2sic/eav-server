using System.Collections.Concurrent;
using ToSic.Eav.Data.ContentTypes;
using ToSic.Sys.Utils.Types;

namespace ToSic.Eav.Models.Sys;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class DataModelAnalyzer
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
    private static IList<string> GetValidTypeNamesCache(Type type) =>
        ContentTypeNamesCache.GetOrAdd(type, _ =>
            DataModelNameVariants.CreateListOfNameVariants(type.Name, type.IsInterface)
        );
    
    private static readonly ConcurrentDictionary<Type, IList<string>> ContentTypeNamesCache = new();

    private static string? GetTypeNameOnModelSpecs(Type tCustom) =>
        TypeNameOnModelSpecsCache.Get<ModelSpecsAttribute>(tCustom, attribute => attribute?.ContentType);
    
    private static readonly TypeAttributeLookup<string?> TypeNameOnModelSpecsCache = new();

    private static string? GetTypeNameOnContentType(Type tCustom) =>
        TypeNameOnContentTypeCache.Get<ContentTypeAttribute>(tCustom, attribute => attribute?.Name);

    private static readonly TypeAttributeLookup<string?> TypeNameOnContentTypeCache = new();
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
            return CheckAndReturn($"option:{optionsTypeName}|{nameIdOrNull}", optionsTypeName);

        // Check if we have one or two types to check (interface vs concrete type)
        var typesDiffer = entryType != concreteType;
        (Type type, string keyPrefix)[] typesToCheck = typesDiffer
            ? [(entryType, "entry"), (concreteType, "concrete")]
            : [(entryType, "entry")];

        // Check the one or two types for ModelSpecsAttribute or ContentTypeAttribute, and return the first one found
        foreach (var (type, keyPrefix) in typesToCheck)    
        {
            var explicitOnModelSpecs = GetTypeNameOnModelSpecs(type);
            if (explicitOnModelSpecs != null)
                return CheckAndReturn(CacheKeyForType(keyPrefix), explicitOnModelSpecs);

            var explicitOnCtSpecs = GetTypeNameOnContentType(type);
            if (explicitOnCtSpecs != null)
                return CheckAndReturn(CacheKeyForType(keyPrefix), explicitOnCtSpecs);
        }

        // Check for automatically derived names
        var cacheKeyFinal = CacheKeyForType("derived");
        if (TypeNameAllowedCache.Contains(cacheKeyFinal))
            return (true, cacheKeyFinal, null);
        
        var namesDerived = typesDiffer
            ? GetValidTypeNamesCache(entryType).Concat(GetValidTypeNamesCache(concreteType)).ToArray()
            : GetValidTypeNamesCache(entryType);

        return (false, cacheKeyFinal, namesDerived);

        
        
        (bool IsCachedAsVerified, string CacheKey, IList<string>? Names) CheckAndReturn(string cacheKey, string names)
            => nameIdOrNull != null && TypeNameAllowedCache.Contains(cacheKey)
                ? (true, cacheKey, null)
                : (false, cacheKey, names.CsvToArrayWithoutEmpty());
        
        string CacheKeyForType(string prefix)
            => $"{prefix}:{entryType.FullName}|{nameIdOrNull}";
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
    private static readonly HashSet<string> TypeNameAllowedCache = [];

    public static KeyNotFoundException KeyNotFoundMessage(IList<string>? typeNames, IContentType contentType, object idForErrors)
        => new(
            $"Item with ID {idForErrors} is a '{contentType.Name}'/'{contentType.NameId}' but not a '{string.Join(",", typeNames ?? [])}'. " +
            $"This is probably a mistake, otherwise set '{nameof(ToModelOptions.TypeName)}: '*' " +
            $"or apply an attribute [{nameof(ModelSpecsAttribute)}({nameof(ModelSpecsAttribute.ContentType)} = \"{contentType.Name}\")] to your model class. "
        );

    #region Stream Names WIP

    /// <summary>
    /// Get the stream names of the current type.
    /// </summary>
    /// <typeparam name="TCustom"></typeparam>
    /// <returns></returns>
    public static IList<string> GetStreamNameList<TCustom>() where TCustom : class
    {
        return StreamNames.Get<TCustom, ModelSpecsAttribute>(attribute =>
            DataModelNameVariants.UseSpecifiedNameOrDeriveFromType(typeof(TCustom), attribute?.Stream));
    }

    private static readonly TypeAttributeLookup<IList<string>> StreamNames = new();

    #endregion

}