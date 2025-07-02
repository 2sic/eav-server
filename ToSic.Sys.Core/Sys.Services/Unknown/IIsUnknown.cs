namespace ToSic.Sys.Services;

/// <summary>
/// Marks un-implemented objects like SiteUnknown etc.
/// This will often reduce functionality if such objects are found
/// </summary>
[PrivateApi]
public interface IIsUnknown;