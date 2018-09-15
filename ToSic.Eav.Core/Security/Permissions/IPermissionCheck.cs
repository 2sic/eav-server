using System.Collections.Generic;

namespace ToSic.Eav.Security.Permissions
{
    public interface IPermissionCheck
    {
        bool HasPermissions { get; }


        bool UserMay(List<Grants> grants);

        //bool UserMay(Grants grant);

        ConditionType GrantedBecause { get; }
    }
}