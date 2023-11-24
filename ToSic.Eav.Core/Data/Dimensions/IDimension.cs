using ToSic.Lib.Documentation;

namespace ToSic.Eav.Data;

/// <summary>
/// Represents a Dimension to assign values to. Dimensions are usually languages (<see cref="ILanguage"/>),
/// but in future they could also be multi-dimensional, like values which are mapped to a language <em>and</em> to a specific edition, use case, etc.
/// </summary>
[InternalApi_DoNotUse_MayChangeWithoutNotice("this is just fyi")]
public interface IDimension
{
    /// <summary>
    /// Gets the internal DimensionId as it is stored in the DB. This is only used for scenarios where the dimensions are defined in relational data. 
    /// </summary>
    int DimensionId { get; }

    /// <summary>
    /// Gets the dimension Key. For languages it's usually values like en-US or de-DE
    /// </summary>
    string Key { get; }

    /// <summary>
    /// Gets whether Dimension is assigned read only. This affects the UI, so that the value cannot be edited in these dimensions.
    /// </summary>
    bool ReadOnly { get; }
}