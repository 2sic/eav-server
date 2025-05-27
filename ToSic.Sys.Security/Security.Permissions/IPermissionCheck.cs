namespace ToSic.Sys.Security.Permissions;

public interface IPermissionCheck: IHasLog
{
    bool HasPermissions { get; }

    PermissionCheckInfo UserMay(List<Grants> grants);
    
}