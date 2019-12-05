using System.Collections.Generic;
using ToSic.Eav.Logging;

namespace ToSic.Eav.Security
{
    public interface IPermissionCheck: IHasLog
    {
        bool HasPermissions { get; }


        bool UserMay(List<Grants> grants);

        Conditions GrantedBecause { get; }
    }
}