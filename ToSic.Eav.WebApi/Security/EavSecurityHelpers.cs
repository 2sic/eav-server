using System;
using System.Security.Authentication;
using ToSic.Eav.Run;

namespace ToSic.Eav.WebApi.Security
{
    public class EavSecurityHelpers
    {
        public static T RunIfAdmin<T>(IUser user, Func<T> task)
        {
            ThrowIfNotAdmin(user);
            return task.Invoke();
        }

        public static void ThrowIfNotAdmin(IUser user)
        {
            if (!user.IsAdmin)
                throw new AuthenticationException("Needs admin permissions to do this");
        }

    }
}
