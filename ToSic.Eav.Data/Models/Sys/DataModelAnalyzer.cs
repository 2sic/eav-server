namespace ToSic.Eav.Models.Sys;

public class DataModelAnalyzer
{
    /// <summary>
    /// Figure out the expected ContentTypeName of a DataWrapper type.
    /// </summary>
    /// <typeparam name="TCustom"></typeparam>
    /// <returns></returns>
    /// <remarks>
    /// If it is decorated with <see cref="ModelSourceAttribute"/> then use the information it provides, otherwise
    /// use the type name.
    /// </remarks>
    public static List<string> GetValidTypeNames<TCustom>()
        where TCustom : class
    {
        return ContentTypeNamesCache
            .Get<TCustom, ModelSourceAttribute>(attribute =>
                UseSpecifiedNameOrDeriveFromType<TCustom>(attribute?.ContentType)
            );
    }

    private static readonly ClassAttributeLookup<List<string>> ContentTypeNamesCache = new();

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TCustom"></typeparam>
    /// <param name="entity"></param>
    /// <param name="id"></param>
    /// <param name="skipTypeCheck"></param>
    /// <returns></returns>
    /// <exception cref="InvalidCastException">Thrown if the names don't match and skipTypeCheck is `false` (default).</exception>
    public static bool IsTypeNameAllowedOrThrow<TCustom>(IEntity entity, object id, bool skipTypeCheck)
        where TCustom : class
    {
        if (skipTypeCheck)
            return true;

        // Do Type-Name check
        var typeNames = GetValidTypeNames<TCustom>();

        // Check all type names if they are `*` or match the data ContentType
        if (typeNames.Any(t => t == ModelSourceAttribute.ForAnyContentType || entity.Type.Is(t)))
            return true;

        throw new InvalidCastException(
            $"Item with ID {id} is a '{entity.Type.Name}'/'{entity.Type.NameId}' but not a '{string.Join(",", typeNames)}'. " +
            $"This is probably a mistake, otherwise use '{nameof(skipTypeCheck)}: true' " +
            $"or apply an attribute [{nameof(ModelSourceAttribute)}({nameof(ModelSourceAttribute.ContentType)} = \"{entity.Type.Name}\")] to your model class. "
        );

    }

    #region Stream Names WIP

    /// <summary>
    /// Get the stream names of the current type.
    /// </summary>
    /// <typeparam name="TCustom"></typeparam>
    /// <returns></returns>
    public static List<string> GetStreamNameList<TCustom>() where TCustom : class
    {
        return StreamNames.Get<TCustom, ModelSourceAttribute>(attribute =>
            UseSpecifiedNameOrDeriveFromType<TCustom>(attribute?.Stream));
    }

    private static readonly ClassAttributeLookup<List<string>> StreamNames = new();

    #endregion

    internal static List<string> UseSpecifiedNameOrDeriveFromType<TCustom>(string? names)
        where TCustom : class
    {
        var list = names != null
            ? names.Split(',').Select(n => n.Trim()).ToList()
            : CreateListOfNameVariants(typeof(TCustom).Name, typeof(TCustom).IsInterface);
        return list;
    }


    /// <summary>
    /// Take a class/interface name and create a list
    /// which also checks for the same name without leading "I" or without trailing "Model".
    /// </summary>
    internal static List<string> CreateListOfNameVariants(string name, bool isInterface)
    {
        if (string.IsNullOrWhiteSpace(name))
            return [];

        // Start list with initial name
        List<string> result = [name];

        // Check if it ends with Model
        var nameWithoutModelSuffix = name.EndsWith("Model")
            ? name.Substring(0, name.Length - 5)
            : null;
        if (nameWithoutModelSuffix != null)
            result.Add(nameWithoutModelSuffix);

        // If it's not an interface beginning with "I", stop here
        if (!isInterface
            || !name.StartsWith("I", StringComparison.Ordinal)
            || name.Length <= 1 // Skip if only 1 char long, else below the Substring would be empty
           )
            return result;

        // Add names without leading I - since it has a leading I
        result.Add(name.Substring(1));
        if (nameWithoutModelSuffix != null)
            result.Add(nameWithoutModelSuffix.Substring(1));

        return result;
    }
}