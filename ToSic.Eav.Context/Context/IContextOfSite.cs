using ToSic.Sys.Users;
using ToSic.Sys.Users.Permissions;

namespace ToSic.Eav.Context;

[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IContextOfSite: /*IHasLog,*/ ICurrentContextUserPermissions
{
    /// <summary>
    /// The website the current request is running in
    /// </summary>
    ISite Site { get; }

    /// <summary>
    /// The user in the current request / context
    /// </summary>
    IUser User { get; }

    /// <summary>
    /// Special method to swap the site on a context.
    /// It would be better in a factory, but since different implementations of IContextOfSite may have different levels of data,
    /// and these other implementations can be in a later DLL, this is the only way to ensure that the context can be swapped without losing data.
    /// </summary>
    /// <param name="site"></param>
    /// <returns></returns>
    IContextOfSite WithSwappedSite(ISite site);
}
