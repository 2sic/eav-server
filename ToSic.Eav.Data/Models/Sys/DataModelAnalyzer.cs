namespace ToSic.Eav.Models.Sys;

public class DataModelAnalyzer
{
    /// <summary>
    /// Figure out the expected ContentTypeName of a DataWrapper type.
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    /// If it is decorated with <see cref="ModelSpecsAttribute"/> then use the information it provides, otherwise
    /// use the type name.
    /// </remarks>
    public static List<string> GetValidTypeNames(Type tCustom)
    {
        return ContentTypeNamesCache
            .Get<ModelSpecsAttribute>(tCustom, attribute =>
                DataModelNames.UseSpecifiedNameOrDeriveFromType(tCustom, attribute?.ContentType)
            );
    }

    private static readonly ClassAttributeLookup<List<string>> ContentTypeNamesCache = new();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="type"></param>
    /// <param name="entity"></param>
    /// <param name="id"></param>
    /// <param name="skipTypeCheck"></param>
    /// <returns></returns>
    /// <exception cref="InvalidCastException">Thrown if the names don't match and skipTypeCheck is `false` (default).</exception>
    public static bool IsTypeNameAllowedOrThrow(Type type, IEntity entity, object id, bool skipTypeCheck)
    {
        if (skipTypeCheck)
            return true;

        // Do Type-Name check
        var typeNames = GetValidTypeNames(type);

        // Check all type names if they are `*` or match the data ContentType
        if (typeNames.Any(t => t == ModelSpecsAttribute.ForAnyContentType || entity.Type.Is(t)))
            return true;

        throw new InvalidCastException(
            $"Item with ID {id} is a '{entity.Type.Name}'/'{entity.Type.NameId}' but not a '{string.Join(",", typeNames)}'. " +
            $"This is probably a mistake, otherwise use '{nameof(skipTypeCheck)}: true' " +
            $"or apply an attribute [{nameof(ModelSpecsAttribute)}({nameof(ModelSpecsAttribute.ContentType)} = \"{entity.Type.Name}\")] to your model class. "
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
        return StreamNames.Get<TCustom, ModelSpecsAttribute>(attribute =>
            DataModelNames.UseSpecifiedNameOrDeriveFromType<TCustom>(attribute?.Stream));
    }

    private static readonly ClassAttributeLookup<List<string>> StreamNames = new();

    #endregion

}