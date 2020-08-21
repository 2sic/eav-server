using System.Collections.Generic;

namespace ToSic.Eav.Security
{
    public interface IMultiPermissionCheck
    {
        bool UserMayOnAll(List<Grants> grants);

        bool EnsureAll(List<Grants> grants, out string error);
    }
}
