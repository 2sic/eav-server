namespace ToSic.Eav.Data;

/// <summary>
/// Represents a Dimension to assign values to. Dimensions are usually languages (<see cref="ILanguage"/>),
/// but in future they could also be multidimensional, like values which are mapped to a language <em>and</em> to a specific edition, use case, etc.
/// </summary>
[InternalApi_DoNotUse_MayChangeWithoutNotice("this is just fyi")]
public interface IDimension
{
    /// <summary>
    /// Gets the internal DimensionId as it is stored in the DB.
    /// This is only used for scenarios where the dimensions are defined in relational data. 
    /// </summary>
    /// <remarks>
    /// This was called `DimensionId` till v22, but was renamed to `Id` to be more consistent with other entities.
    /// </remarks>
    int Id { get; }

    /// <summary>
    /// Gets the main dimension Key.
    /// </summary>
    /// <remarks>
    /// For languages, it's usually values like en-us or de-de (always lower case)
    /// </remarks>
    string Key { get; }

    /// <summary>
    /// Gets whether Dimension is assigned read only.
    /// </summary>
    /// <remarks>
    /// This affects the UI, so that the value cannot be edited in these dimensions.
    /// </remarks>
    bool ReadOnly { get; }
}