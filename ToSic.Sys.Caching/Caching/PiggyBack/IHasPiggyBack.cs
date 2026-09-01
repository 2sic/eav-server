namespace ToSic.Sys.Caching.PiggyBack;

/// <summary>
/// Marks objects which can piggy-back other data on them for caching purposes.
/// This is useful for scenarios where you want to attach additional information to an object without modifying its original structure.
/// </summary>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IHasPiggyBack
{
    /// <summary>
    /// The piggy-back helper.
    /// </summary>
    PiggyBack PiggyBack { get; }
}