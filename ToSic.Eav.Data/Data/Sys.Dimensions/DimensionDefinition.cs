namespace ToSic.Eav.Data.Dimensions.Sys;

/// <summary>
/// The definition of a dimension / language
/// </summary>
[PrivateApi("hidden 2021-09-30, previously visible as internal don't use")]
[ShowApiWhenReleased(ShowApiMode.Never)]
public record DimensionDefinition
{
    [PrivateApi("might be changed to just 'id' or something")]
    public int DimensionId { get; set; }

    [PrivateApi]
    public int? Parent { get; set; }

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
    public bool Active { get; set; } = true;

}