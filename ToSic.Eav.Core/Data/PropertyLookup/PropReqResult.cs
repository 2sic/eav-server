namespace ToSic.Eav.Data.PropertyLookup;

/// <summary>
/// Internal intermediate object when retrieving an Entity property.
/// Will contain additional information for upstream processing
/// </summary>
[PrivateApi]
[ShowApiWhenReleased(ShowApiMode.Never)]
public class PropReqResult(object result, ValueTypesWithState valueType, PropertyLookupPath path)
{
    /// <summary>
    /// The result of the request - null if not found
    /// </summary>
    public object Result = result;

    /// <summary>
    /// Debug property to see if a result was wrapped to create something else
    /// </summary>
    internal object ResultOriginal;

    /// <summary>
    /// The IValue object, in case we need to use its cache
    /// </summary>
    public IValue Value { get; init; }
        
    /// <summary>
    /// A field type, like "Hyperlink" or "Entity" etc.
    /// </summary>
    public ValueTypesWithState ValueType = valueType;
        
    /// <summary>
    /// The entity which returned this property
    /// </summary>
    public object Source { get; init; }

    /// <summary>
    /// An optional name
    /// </summary>
    public string Name { get; set; }

    public readonly PropertyLookupPath Path = path;

    public int SourceIndex { get; set; } = -1;

    public bool IsFinal => SourceIndex != -1;

    /// <summary>
    /// Special Helper to return nothing, easier to spot in code when this is used
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static PropReqResult Null(PropertyLookupPath path)
        => new(result: null, valueType: ValueTypesWithState.Null, path: path);

    public static PropReqResult NullFinal(PropertyLookupPath path)
        => Null(path).AsFinal(0);

    public PropReqResult AsFinal(int sourceIndex)
    {
        SourceIndex = sourceIndex;
        return this;
    }
}