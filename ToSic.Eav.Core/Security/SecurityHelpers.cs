using System.Security.Authentication;
using ToSic.Eav.Context;

namespace ToSic.Eav.Security;

[ShowApiWhenReleased(ShowApiMode.Never)]
public static class SecurityHelpers
{
    public static void ThrowIfNotSiteAdmin(IUser user, ILog log)
        => ThrowIfNot(user.IsSiteAdmin, "site-admin", log);

    public static void ThrowIfNotContentAdmin(IUser user, ILog log)
        => ThrowIfNot(user.IsContentAdmin, "content-admin", log);

    private static void ThrowIfNot(bool isTrue, string name, ILog log)
    {
        var l = log.Fn($"{name}: {isTrue}");
        if (!isTrue)
            throw new AuthenticationException($"Needs {name} permissions to do this");
        l.Done();
    }
}