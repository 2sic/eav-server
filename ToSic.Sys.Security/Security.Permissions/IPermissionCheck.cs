using ToSic.Lib.Security.Permissions;

namespace ToSic.Eav.Security;

public interface IPermissionCheck: IHasLog
{
    bool HasPermissions { get; }

    PermissionCheckInfo UserMay(List<Grants> grants);
    
}