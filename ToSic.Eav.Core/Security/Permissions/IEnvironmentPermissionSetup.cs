using ToSic.Eav.Apps;

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
    IEnvironmentPermission Init<TContext>(TContext context, IAppIdentity appIdentityOrNull);

}