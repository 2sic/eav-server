namespace ToSic.Eav.Context;

internal class ContextResolverUserPermissions(IContextOfUserPermissions userPermissions)
    : IContextResolverUserPermissions
{
    public EffectivePermissions UserPermissions() => userPermissions.Permissions;
}