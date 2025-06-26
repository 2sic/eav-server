namespace ToSic.Sys.Users.Permissions;

internal class CurrentContextUserPermissionsService(ICurrentContextUserPermissions userPermissions)
    : ICurrentContextUserPermissionsService
{
    public EffectivePermissions UserPermissions() => userPermissions.Permissions;
}