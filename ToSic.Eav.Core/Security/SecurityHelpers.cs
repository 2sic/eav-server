using System.Security.Authentication;
using ToSic.Eav.Context;
using ToSic.Lib.Logging;

namespace ToSic.Eav.Security;

public static class SecurityHelpers
{
    //public static void ThrowIfNotAdmin(bool isAdmin, ILog log)
    //{
    //    log.Fn($"{nameof(isAdmin)}: {isAdmin}");
    //    if (!isAdmin)
    //        throw new AuthenticationException("Needs admin permissions to do this");
    //}
    public static void ThrowIfNotSiteAdmin(IUser user, ILog log)
        => ThrowIfNot(user.IsSiteAdmin, "site-admin", log);

    public static void ThrowIfNotContentAdmin(IUser user, ILog log)
        => ThrowIfNot(user.IsContentAdmin, "content-admin", log);

    private static void ThrowIfNot(bool isTrue, string name, ILog log)
    {
        log.Fn($"{name}: {isTrue}");
        if (!isTrue)
            throw new AuthenticationException($"Needs {name} permissions to do this");
    }
}