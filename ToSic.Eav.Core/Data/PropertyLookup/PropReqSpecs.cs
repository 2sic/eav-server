namespace ToSic.Eav.Data.PropertyLookup;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class PropReqSpecs(string field, string[] dimensions, ILog logOrNull = null, bool treatEmptyAsDefault = true)
    : ICanDump
{
    public PropReqSpecs(string field): this(field, null) { }

    public readonly string Field = field;

    public readonly string[] Dimensions = dimensions ?? Array.Empty<string>();

    public readonly bool TreatEmptyAsDefault = treatEmptyAsDefault;

    /// <summary>
    /// Log is null if no logging should happen, or a real logger if it's in use
    /// </summary>
    public readonly ILog LogOrNull = logOrNull;

    public PropReqSpecs ForOtherField(string field) => new(field, Dimensions, LogOrNull, TreatEmptyAsDefault);

    public PropReqSpecs SubLog(string title) =>
        new(Field, Dimensions, LogOrNull.SubLogOrNull(title), TreatEmptyAsDefault);
    public PropReqSpecs SubLog(string title, bool enabled) => new(Field, Dimensions,
        LogOrNull.SubLogOrNull(title, enabled), TreatEmptyAsDefault);

    public string Dump() => _dump ??= $"{nameof(PropReqSpecs)} {{ {nameof(Field)}:{Field}, {nameof(Dimensions)}:{string.Join(",", Dimensions)} }}";
    private string _dump;
}