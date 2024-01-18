namespace ToSic.Eav.Context;

internal class ContextResolverUserPermissions(IContextOfUserPermissions userPermissions)
    : IContextResolverUserPermissions
{
    public IContextOfUserPermissions UserPermissions() => userPermissions;
}