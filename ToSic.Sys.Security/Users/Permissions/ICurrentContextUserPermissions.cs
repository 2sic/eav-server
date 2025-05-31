namespace ToSic.Sys.Users.Permissions;

/// <summary>
/// WIP 15.04.
/// Should solve problem that sometimes we need to know about the user permissions, but
/// depending on the full context it can be more or less.
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface ICurrentContextUserPermissions
{
    [ShowApiWhenReleased(ShowApiMode.Never)]
    EffectivePermissions Permissions { get; }
}