namespace ToSic.Eav.Data.Dimensions.Sys;

/// <inheritdoc />
/// <summary>
/// Represents a Dimension / Language Assignment
/// </summary>
/// <remarks>
/// * completely #immutable since v15.04
/// </remarks>
[PrivateApi("2021-09-30 hidden, previously marked as PublicApi_Stable_ForUseInYourCode")]
[ShowApiWhenReleased(ShowApiMode.Never)]
public class Language(string key, bool readOnly, int dimensionId = 0) : ILanguage
{
    /// <inheritdoc />
    public int DimensionId { get; } = dimensionId;

    /// <inheritdoc />
    public bool ReadOnly { get; } = readOnly;

    /// <inheritdoc />
    public string Key { get; } = key.ToLowerInvariant();
}