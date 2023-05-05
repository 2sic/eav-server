using System.Security.Authentication;
using ToSic.Lib.Logging;

namespace ToSic.Eav.Security
{
    public static class SecurityHelpers
    {
        public static void ThrowIfNotAdmin(bool isAdmin, ILog log)
        {
            log.Fn($"{nameof(isAdmin)}: {isAdmin}");
            if (!isAdmin)
                throw new AuthenticationException("Needs admin permissions to do this");
        }

    }
}
