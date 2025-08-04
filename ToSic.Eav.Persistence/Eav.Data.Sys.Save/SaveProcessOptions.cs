namespace ToSic.Eav.Data.Sys.Save;

/// <summary>
/// Configuration for an entire save process.
/// </summary>
/// <remarks>
/// Meant to disable some fixes / optimizations which make sense in edit-scenarios, but not in import scenarios.
/// </remarks>
public record SaveProcessOptions
{
    public bool TypeAttributeAutoSetTitle { get; init; } = true;
    public bool TypeAttributeAutoCorrectTitle { get; init; } = true;

    /// <summary>
    /// Special settings for import scenarios, which should not autocorrect data as it arrives.
    /// </summary>
    public static SaveProcessOptions Import => new()
    {
        TypeAttributeAutoSetTitle = false,
        TypeAttributeAutoCorrectTitle = false,
    };

    /// <summary>
    /// This one doesn't do much, but it's here to be optimized if ever needed.
    /// </summary>
    public static SaveProcessOptions CreateApp => new()
    {
        TypeAttributeAutoSetTitle = true,
        TypeAttributeAutoCorrectTitle = true,
    };
}
