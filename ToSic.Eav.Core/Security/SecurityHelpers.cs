using System.Security.Authentication;
using ToSic.Eav.Context;

namespace ToSic.Eav.WebApi.Security
{
    public static class SecurityHelpers
    {
        public static void ThrowIfNotAdmin(IUser user)
        {
            if (!user.IsSiteAdmin)
                throw new AuthenticationException("Needs admin permissions to do this");
        }

    }
}
