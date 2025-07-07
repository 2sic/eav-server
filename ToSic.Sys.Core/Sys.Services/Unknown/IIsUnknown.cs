namespace ToSic.Sys.Services;

/// <summary>
/// Marks un-implemented objects like SiteUnknown etc.
/// </summary>
/// <remarks>
/// Such objects are similar to Mock objects, but they are registered in DI to be used (and warn) if no implementation was provided.
/// This will often reduce functionality if such objects are found.
/// </remarks>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IIsUnknown;