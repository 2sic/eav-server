using System.Security.Authentication;

namespace ToSic.Eav.WebApi.Security
{
    public static class SecurityHelpers
    {
        public static void ThrowIfNotAdmin(bool isAdmin)
        {
            if (!isAdmin)
                throw new AuthenticationException("Needs admin permissions to do this");
        }

    }
}
