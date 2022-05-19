﻿using System.Collections.Generic;
using ToSic.Eav.Logging;

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
            var wrapLog = permCheck.Log.Call2<bool>(() => $"[{string.Join(",", grants)}]", () => "or throw");
            var ok = permCheck.UserMay(grants);
            error = ok ? null : "required permissions for this request are not given";
            return wrapLog.Return(ok, ok ? "ok" : "permissions not ok");
        }
    }
}
