namespace ToSic.Eav.Context;

/// <summary>
/// WIP 15.04.
/// Should solve problem that sometimes we need to know about the user permissions, but
/// depending on the full context it can be more or less.
/// </summary>
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public interface IContextOfUserPermissions
{
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    AdminPermissions Permissions { get; }
}