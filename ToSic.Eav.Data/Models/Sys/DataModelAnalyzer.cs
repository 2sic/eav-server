using ToSic.Sys.Utils.Types;

namespace ToSic.Eav.Models.Sys;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class DataModelAnalyzer
{
    public static IList<string> GetValidTypeNames(Type tCustom, string? preset)
        => preset?.CsvToArrayPreserveEmpty() as IList<string>
           ?? GetValidTypeNames(tCustom);

    /// <summary>
    /// Figure out the expected ContentTypeName of a DataWrapper type.
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    /// If it is decorated with <see cref="ModelSpecsAttribute"/> then use the information it provides, otherwise
    /// use the type name.
    /// </remarks>
    public static IList<string> GetValidTypeNames(Type tCustom) =>
        ContentTypeNamesCache
            .Get<ModelSpecsAttribute>(tCustom, attribute =>
                DataModelNameVariants.UseSpecifiedNameOrDeriveFromType(tCustom, attribute?.ContentType)
            );
    private static readonly TypeAttributeLookup<IList<string>> ContentTypeNamesCache = new();

    private static string? GetExplicitTypeNames(Type tCustom) =>
        ExplicitTypeNamesCache.Get<ModelSpecsAttribute>(tCustom, attribute => attribute?.ContentType);
    
    private static readonly TypeAttributeLookup<string?> ExplicitTypeNamesCache = new();

    internal static (bool Skip, string CacheKey, IList<string>? Names) FindPriorityTypeNames(string? optionsTypeName, Type entryType, Type concreteType, IContentType contentType)
    {
        if (optionsTypeName == ToModelOptions.TypeNameAny)
            return (true, "any", null);

        if (optionsTypeName != null)
            return (false, "option:" + optionsTypeName, optionsTypeName.CsvToArrayWithoutEmpty());

        var explicitOnEntryType = GetExplicitTypeNames(entryType);

        if (explicitOnEntryType != null)
            return (false, "entry:" + explicitOnEntryType, explicitOnEntryType.CsvToArrayWithoutEmpty());

        var typesDiffer = entryType != concreteType;
        var explicitOnConcrete = typesDiffer ? GetExplicitTypeNames(concreteType) : null;

        if (explicitOnConcrete != null)
            return (false, "concrete:" + explicitOnConcrete, explicitOnConcrete.CsvToArrayWithoutEmpty());

        var namesDerived = typesDiffer
            ? GetValidTypeNames(entryType)
                .Concat(GetValidTypeNames(concreteType))
                .ToArray()
            : GetValidTypeNames(entryType);

        return (false, "derived:" + entryType.FullName, namesDerived);
    }

    public static (bool Throw, IList<string>? Names) IsTypeNameAllowed(string? optionsTypeName, Type entryType, Type concreteType, IContentType contentType)
    {
        if (optionsTypeName == ToModelOptions.TypeNameAny)
            return (false, null);

        if (optionsTypeName != null)
            return CheckAndReturn( "option:" + optionsTypeName, optionsTypeName.CsvToArrayWithoutEmpty());

        var explicitOnEntryType = GetExplicitTypeNames(entryType);
        
        if (explicitOnEntryType != null)
            return CheckAndReturn("entry:" + explicitOnEntryType, explicitOnEntryType.CsvToArrayWithoutEmpty());

        var typesDiffer = entryType != concreteType;
        var explicitOnConcrete = typesDiffer ? GetExplicitTypeNames(concreteType) : null;
        
        if (explicitOnConcrete != null)
            return CheckAndReturn("concrete:" + explicitOnConcrete, explicitOnConcrete.CsvToArrayWithoutEmpty());

        var namesDerived = typesDiffer
            ? GetValidTypeNames(entryType)
                .Concat(GetValidTypeNames(concreteType))
                .ToArray()
            : GetValidTypeNames(entryType);
        
        return CheckAndReturn("derived:" + entryType.FullName, namesDerived);


        (bool Throw, IList<string>? Names) CheckAndReturn(string source, IList<string> typeNames)
        {
            // CacheKey - note that we'll only cache it if it's ok, never if it fails, to avoid RAM consumption for invalid types
            // We only need the initial type, because even if it's an interface, it will always result in the same concrete type
            var cacheKey = source + "|" + contentType.NameId;
            
            if (TypeNameAllowedCache.Contains(cacheKey))
                return (false, null);

            if (!typeNames.Any(t => t == ToModelOptions.TypeNameAny || contentType.Is(t)))
                return (true, typeNames);
            
            TypeNameAllowedCache.Add(cacheKey);
            return (false, typeNames);

        }
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
            DataModelNameVariants.UseSpecifiedNameOrDeriveFromType<TCustom>(attribute?.Stream));
    }

    private static readonly TypeAttributeLookup<IList<string>> StreamNames = new();

    #endregion

}