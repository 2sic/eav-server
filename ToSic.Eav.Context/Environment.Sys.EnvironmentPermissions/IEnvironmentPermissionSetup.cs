using ToSic.Eav.Apps;
using ToSic.Eav.Context;
using ToSic.Sys.Security.Permissions;

namespace ToSic.Eav.Security;

[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IEnvironmentPermissionSetup : IEnvironmentPermission
{
    /// <summary>
    /// Init the checker
    /// </summary>
    /// <typeparam name="TContext">Important: Special type info for the context because the Eav.Core doesn't know about these types yet</typeparam>
    /// <param name="context"></param>
    /// <param name="appIdentityOrNull"></param>
    /// <returns></returns>
    IEnvironmentPermission Init<TContext>(IContextOfSite context, IAppIdentity appIdentityOrNull);

}