namespace ToSic.Sys.Users.Permissions;

/// <summary>
/// WIP: Get the best possible user permissions based on the current context.
/// </summary>
/// <remarks>
/// Added v15.04
/// </remarks>
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface ICurrentContextUserPermissionsService
{
    EffectivePermissions UserPermissions();
}