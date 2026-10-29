namespace ToSic.Eav.Data.Sys.Dimensions;

// Docs History
// - Before 2021-09-30: Was marked as internal
// - 2021-09-30: Marked as PrivateApi
// - 2026-10-29: Making internal again, to show in docs.

/// <summary>
/// The definition of a dimension / language.
/// </summary>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
[ShowApiWhenReleased(ShowApiMode.Never)]
public record DimensionDefinition
{
    /// <summary>
    /// The ID as is used in the DB.
    /// </summary>
    [PrivateApi("might be changed to just 'id' or something")]
    public required int Id { get; init; }

    /// <summary>
    /// Parent definition node (or `null`).
    /// </summary>
    [PrivateApi]
    public required int? ParentId { get; init; }

    /// <summary>
    /// The name - in case of a language this would be something like "English"
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// The key/id, in case of a language this would be something like "en" or "en-us".
    /// </summary>
    /// <remarks>It's always lower case</remarks>
    public required string Key { get; init; }

    /// <summary>
    /// The id / marker in the environment, which can deviate from the key through casing or other aspects.
    /// </summary>
    [PrivateApi]
    public required string EnvironmentKey { get; init; }

    /// <summary>
    /// Compares two keys to see if they are the same.
    /// </summary>
    /// <param name="environmentKey"></param>
    /// <returns></returns>
    public bool Matches(string environmentKey)
        => string.Equals(EnvironmentKey, environmentKey, StringComparison.InvariantCultureIgnoreCase);

    /// <summary>
    /// If this dimension is active.
    /// </summary>
    public required bool Active { get; init; } = true;

}