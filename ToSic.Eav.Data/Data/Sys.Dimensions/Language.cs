namespace ToSic.Eav.Data.Sys.Dimensions;

// Docs History
// * completely #immutable since v15.04
// - Before 2021-09-30: Was marked as PublicApi_Stable_ForUseInYourCode
// - 2021-09-30: Marked as PrivateApi
// - 2026-10-29: Making internal again, to show in docs.


/// <inheritdoc />
/// <summary>
/// Represents a Dimension / Language Assignment to a value.
/// </summary>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
[ShowApiWhenReleased(ShowApiMode.Never)]
public record Language(string Key, bool ReadOnly, int Id = 0) : ILanguage
{
    /// <inheritdoc />
    public string Key { get; } = Key.ToLowerInvariant();
}