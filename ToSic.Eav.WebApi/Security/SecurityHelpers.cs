using System;
using System.Security.Authentication;
using ToSic.Eav.Apps;
using ToSic.Eav.Apps.Run;
using ToSic.Eav.Logging;
using ToSic.Eav.Plumbing;
using ToSic.Eav.Run;
using ToSic.Eav.Security.Permissions;
using ToSic.Eav.WebApi.Errors;
using ToSic.Sxc.WebApi.Security;

namespace ToSic.Eav.WebApi.Security
{
    public static class SecurityHelpers
    {
        internal static void ThrowIfNotEditorOrIsPublicForm(IContextOfBlock context, IApp app, string contentTypeStaticName, ILog log)
        {
            var permCheck = context.ServiceProvider.Build<MultiPermissionsTypes>().Init(context, app, contentTypeStaticName, log);
            if (!permCheck.EnsureAll(GrantSets.WriteSomething, out var error))
                throw HttpException.PermissionDenied(error);

            if (!permCheck.UserCanWriteAndPublicFormsEnabled(out _, out error))
                throw HttpException.PermissionDenied(error);
        }

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
