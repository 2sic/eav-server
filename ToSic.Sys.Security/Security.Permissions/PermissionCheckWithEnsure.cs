namespace ToSic.Eav.Security;

[ShowApiWhenReleased(ShowApiMode.Never)]
public static class PermissionCheckWithEnsure 
{
    /// <summary>
    /// Run a permission check and return error if it failed
    /// </summary>
    /// <returns></returns>
    [ShowApiWhenReleased(ShowApiMode.Never)]
    public static bool Ensure(this IPermissionCheck permCheck, List<Grants> grants, out string error)
    {
        var log = permCheck.Log;
        var l = log.Fn<bool>(log.Try(() => $"[{string.Join(",", grants)}]"), log.Try(() => "or throw"));
        var ok = permCheck.UserMay(grants).Allowed;
        error = ok ? null : "required permissions for this request are not given";
        return l.Return(ok, ok ? "ok" : "permissions not ok");
    }
}