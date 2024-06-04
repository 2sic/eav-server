namespace ToSic.Eav.Data.PropertyLookup;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class PropReqSpecs(string field, string[] dimensions, ILog logOrNull = null, bool treatEmptyAsDefault = true, bool skipAddingDefaultDimension = false)
    : ICanDump
{
    public static string[] EmptyDimensions = [null];

    public PropReqSpecs(string field): this(field, null)
    { }

    public PropReqSpecs(PropReqSpecs original, string field = default, string[] dimensions = null, ILog logOrNull = default, bool? treatEmptyAsDefault = default, bool? skipAddingDefaultDimension = default)
        : this(field ?? original.Field, 
            dimensions ?? original.Dimensions,
            logOrNull ?? original.LogOrNull,
            treatEmptyAsDefault ?? original.TreatEmptyAsDefault,
            skipAddingDefaultDimension ?? original.SkipAddingDefaultDimension)
    { }

    public readonly string Field = field;

    public readonly string[] Dimensions = dimensions ?? [];
    

    public readonly bool SkipAddingDefaultDimension = skipAddingDefaultDimension;

    public readonly bool TreatEmptyAsDefault = treatEmptyAsDefault;

    /// <summary>
    /// Log is null if no logging should happen, or a real logger if it's in use
    /// </summary>
    public readonly ILog LogOrNull = logOrNull;

    public PropReqSpecs ForOtherField(string field)
        => new(field, Dimensions, LogOrNull, TreatEmptyAsDefault, SkipAddingDefaultDimension);

    public PropReqSpecs SubLog(string title)
        => new(Field, Dimensions, LogOrNull.SubLogOrNull(title), TreatEmptyAsDefault, SkipAddingDefaultDimension);

    public PropReqSpecs SubLog(string title, bool enabled)
        => new(Field, Dimensions, LogOrNull.SubLogOrNull(title, enabled), TreatEmptyAsDefault, SkipAddingDefaultDimension);

    public string Dump() => _dump ??= $"{nameof(PropReqSpecs)} {{ {nameof(Field)}:{Field}, {nameof(Dimensions)}:{string.Join(",", Dimensions)} }}";
    private string _dump;


    /// <summary>
    /// Make sure the dimensions list also has a null-entry,
    /// for fallback to the first/only language (if any are provided and no match was made first)
    /// </summary>
    /// <returns></returns>
    internal static string[] ExtendDimsWithDefault(PropReqSpecs specs)
    {
        // empty list - add the default dimension
        var dimensions = specs.Dimensions;
        if (dimensions == null || dimensions.Length == 0)
            return specs.SkipAddingDefaultDimension ? [] : [null];

        // we have dimensions but no default, add it
        return specs.SkipAddingDefaultDimension || dimensions.Last() == default
            ? dimensions
            : [.. dimensions, default];
    }

}