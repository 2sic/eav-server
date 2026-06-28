namespace ToSic.Sys.Security.Permissions;

[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IPermissionCheck: IHasLog
{
    bool HasPermissions { get; }

    PermissionCheckInfo UserMay(List<Grants> grants);
    
}