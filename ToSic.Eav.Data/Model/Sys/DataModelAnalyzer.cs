namespace ToSic.Eav.Model.Sys;

public class DataModelAnalyzer
{

    /// <summary>
    /// Figure out the expected ContentTypeName of a DataWrapper type.
    /// If it is decorated with <see cref="ModelSourceAttribute"/> then use the information it provides, otherwise
    /// use the type name.
    /// </summary>
    /// <typeparam name="TCustom"></typeparam>
    /// <returns></returns>
    internal static string GetContentTypeNameCsv<TCustom>() where TCustom : class /*ICanWrapData*/ =>
        ContentTypeNames.Get<TCustom, ModelSourceAttribute>(a =>
        {
            // If we have an attribute, use the value provided (unless not specified)
            if (a?.ContentType != null)
                return a.ContentType;

            // If no attribute, use name of type
            var type = typeof(TCustom);
            var typeName = type.Name;
            // If type is Interface: drop the "I" as this can't be a content-type
            // TODO: not sure if this is a good idea
            return typeName.StartsWith("I") && type.IsInterface
                ? typeName.Substring(1)
                : typeName;
        });
    private static readonly ClassAttributeLookup<string> ContentTypeNames = new();

    /// <summary>
    /// Figure out the expected ContentTypeName of a DataWrapper type.
    /// </summary>
    /// <typeparam name="TCustom"></typeparam>
    /// <returns></returns>
    /// <remarks>
    /// If it is decorated with <see cref="ModelSourceAttribute"/> then use the information it provides, otherwise
    /// use the type name.
    /// </remarks>
    public static List<string> GetContentTypeNamesList<TCustom>() where TCustom : class // ICanWrapData
    {
        return ContentTypeNamesList.Get<TCustom, ModelSourceAttribute>(attribute =>
            UseSpecifiedNameOrDeriveFromType<TCustom>(attribute?.ContentType));
    }

    private static readonly ClassAttributeLookup<List<string>> ContentTypeNamesList = new();

    #region Stream Names WIP

    /// <summary>
    /// Get the stream names of the current type.
    /// </summary>
    /// <typeparam name="TCustom"></typeparam>
    /// <returns></returns>
    public static List<string> GetStreamNameList<TCustom>() where TCustom : class // ICanWrapData
    {
        return StreamNames.Get<TCustom, ModelSourceAttribute>(attribute =>
            UseSpecifiedNameOrDeriveFromType<TCustom>(attribute?.Stream));
    }

    private static readonly ClassAttributeLookup<List<string>> StreamNames = new();

    #endregion

    private static List<string> UseSpecifiedNameOrDeriveFromType<TCustom>(string? names)
        where TCustom : class // ICanWrapData
    {
        var list = !string.IsNullOrWhiteSpace(names)
            ? names!.Split(',').Select(n => n.Trim()).ToList()
            : CreateListOfNameVariants(typeof(TCustom).Name, typeof(TCustom).IsInterface);
        return list;
    }


    /// <summary>
    /// Take a class/interface name and create a list
    /// which also checks for the same name without leading "I" or without trailing "Model".
    /// </summary>
    internal static List<string> CreateListOfNameVariants(string name, bool isInterface)
    {
        // Start list with initial name
        List<string> result = [name];
        // Check if it ends with Model
        var nameWithoutModelSuffix = name.EndsWith("Model")
            ? name.Substring(0, name.Length - 5)
            : null;
        if (nameWithoutModelSuffix != null)
            result.Add(nameWithoutModelSuffix);

        if (isInterface && name.Length > 1 && name.StartsWith("I") && char.IsUpper(name, 1))
        {
            result.Add(name.Substring(1));
            if (nameWithoutModelSuffix != null)
                result.Add(nameWithoutModelSuffix.Substring(1));
        }

        return result;
    }
}