using System.Security.Authentication;
using ToSic.Eav.Context;
using ToSic.Lib.Logging;

namespace ToSic.Eav.Security;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public static class SecurityHelpers
{
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