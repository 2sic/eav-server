using System.Collections.Generic;

namespace ToSic.Eav.Security.Permissions
{
    public interface IPermissionCheck
    {
        bool HasPermissions { get; }


        bool UserMay(List<PermissionGrant> grants);
        bool UserMay(PermissionGrant grant);
    }
}