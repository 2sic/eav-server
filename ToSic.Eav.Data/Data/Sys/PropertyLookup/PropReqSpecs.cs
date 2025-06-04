namespace ToSic.Eav.Data.PropertyLookup;

/// <summary>
/// Property Request Specifications - to ask an object such as an IEntity for a property with additional specs such as languages.
/// </summary>
/// <param name="Field"></param>
/// <param name="dimensions"></param>
/// <param name="LogOrNull">Log is null if no logging should happen, or a real logger if it's in use</param>
/// <param name="TreatEmptyAsDefault">If the result returns something empty but non-null, treat as final.</param>
/// <param name="DimsAreFinal">
/// Dimension list is final, doesn't need any further processing.
/// This is to prevent unnecessary lower-casing and extending dimensions with the default null-entry, when it has already been done before.
/// </param>
[ShowApiWhenReleased(ShowApiMode.Never)]
public record PropReqSpecs(string Field, string[] dimensions, bool DimsAreFinal, ILog LogOrNull = null, bool TreatEmptyAsDefault = true)
    : ICanDump
{
    public static string[] EmptyDimensions = [default];

    public PropReqSpecs(string field): this(field, EmptyDimensions, DimsAreFinal: true)
    { }

    public readonly string[] Dimensions = dimensions != null
        ? DimsAreFinal
            ? dimensions
            : ExtendDimsWithDefault(dimensions)
        : EmptyDimensions;
    
    /// <summary>
    /// Log is null if no logging should happen, or a real logger if it's in use
    /// </summary>
    //public readonly ILog LogOrNull = logOrNull;

    public PropReqSpecs ForOtherField(string field)
        => this with { Field = field, DimsAreFinal = true };

    public PropReqSpecs SubLog(string title)
        => this with { LogOrNull = LogOrNull.SubLogOrNull(title), DimsAreFinal = true };

    public PropReqSpecs SubLog(string title, bool enabled)
        => this with { LogOrNull = LogOrNull.SubLogOrNull(title, enabled), DimsAreFinal = true };

    public string Dump() => _dump ??= $"{nameof(PropReqSpecs)} {{ {nameof(Field)}:{Field}, {nameof(Dimensions)}:{string.Join(",", Dimensions)} }}";
    private string _dump;

    internal static string[] ExtendDimsWithDefault(string[] dimensions)
    {
        // empty list - add the default dimension
        if (dimensions == null || dimensions.Length == 0)
            return EmptyDimensions;

        // we have dimensions but no default, add it
        return dimensions.Last() == default
            ? dimensions
            : [.. dimensions, default];
    }

}