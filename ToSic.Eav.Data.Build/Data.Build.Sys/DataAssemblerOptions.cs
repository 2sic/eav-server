namespace ToSic.Eav.Data.Build.Sys;

/// <summary>
/// Options to configure the DataAssembler.
/// </summary>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
public record DataAssemblerOptions
{
    /// <summary>
    /// Special value to control if IEntity values may contain unexpected data types.
    /// </summary>
    public bool AllowUnknownValueTypes { get; init; }

    /// <summary>
    /// Settings how logging should happen, mainly to hide excessive logs unless actively debugging.
    /// </summary>
    public LogSettings LogSettings { get; init; } = new();
}