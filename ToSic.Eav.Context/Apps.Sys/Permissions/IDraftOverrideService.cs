using ToSic.Sys.Users.Permissions;

namespace ToSic.Eav.Apps.Sys.Permissions;

/// <summary>
/// Special service to enable a user who would see draft data, to optionally see live data instead.
/// </summary>
public interface IDraftOverrideService
{
    /// <summary>
    /// Evaluate the supplied context's permissions, including the live/draft preference,
    /// without reading or changing the cached ambient permissions.
    /// </summary>
    EffectivePermissions UserPermissions(EffectivePermissions permissions);
}
