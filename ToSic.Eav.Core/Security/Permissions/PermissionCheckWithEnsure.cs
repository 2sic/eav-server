using System.Collections.Generic;
using ToSic.Lib.Logging;

namespace ToSic.Eav.Security
{
    public static class PermissionCheckWithEnsure 
    {
        /// <summary>
        /// Run a permission check and return error if it failed
        /// </summary>
        /// <returns></returns>
        public static bool Ensure(this IPermissionCheck permCheck, List<Grants> grants, out string error)
        {
            var log = permCheck.Log;
            var l = log.Fn<bool>(log.Try(() => $"[{string.Join(",", grants)}]"), log.Try(() => "or throw"));
            var ok = permCheck.UserMay(grants);
            error = ok ? null : "required permissions for this request are not given";
            return l.Return(ok, ok ? "ok" : "permissions not ok");
        }
    }
}
