namespace ToSic.Eav.Context
{
    /// <summary>
    /// WIP: Get the best possible user permissions based on the current context.
    /// </summary>
    /// <remarks>
    /// Added v15.04
    /// </remarks>
    public interface IContextResolverUserPermissions
    {
        IContextOfUserPermissions UserPermissions();
    }
}
