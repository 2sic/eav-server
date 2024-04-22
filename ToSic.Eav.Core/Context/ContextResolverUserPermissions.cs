namespace ToSic.Eav.Context;

internal class ContextResolverUserPermissions(IContextOfUserPermissions userPermissions)
    : IContextResolverUserPermissions
{
    public AdminPermissions UserPermissions() => userPermissions.Permissions;
}