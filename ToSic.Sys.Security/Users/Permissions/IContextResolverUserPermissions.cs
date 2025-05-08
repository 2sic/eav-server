namespace ToSic.Eav.Context;

/// <summary>
/// WIP: Get the best possible user permissions based on the current context.
/// </summary>
/// <remarks>
/// Added v15.04
/// </remarks>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public interface IContextResolverUserPermissions
{
    EffectivePermissions UserPermissions();
}