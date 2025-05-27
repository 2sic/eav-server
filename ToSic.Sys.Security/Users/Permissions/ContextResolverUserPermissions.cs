namespace ToSic.Sys.Users.Permissions;

internal class ContextResolverUserPermissions(IContextOfUserPermissions userPermissions)
    : IContextResolverUserPermissions
{
    public EffectivePermissions UserPermissions() => userPermissions.Permissions;
}