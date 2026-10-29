namespace ToSic.Sys.Security.Permissions;

/// <summary>
/// Basic permission check API for simple consistency.
/// Non-injectable, as each implementation will have a different setup.
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IPermissionCheck: IHasLog
{
    bool HasPermissions { get; }

    PermissionCheckInfo UserMay(List<Grants> grants);

    PermissionCheckInfo PermissionsAllow(IReadOnlyCollection<Grants> grants);
}