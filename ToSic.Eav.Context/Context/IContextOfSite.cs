using ToSic.Sys.Users;
using ToSic.Sys.Users.Permissions;

namespace ToSic.Eav.Context;

[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IContextOfSite: IHasLog, ICurrentContextUserPermissions, IContextOfSiteOrMoreCloneable
{
    /// <summary>
    /// The website the current request is running in
    /// </summary>
    ISite Site { get; }

    /// <summary>
    /// The user in the current request / context
    /// </summary>
    IUser User { get; }
}

public interface IContextOfSiteOrMoreCloneable
{
    /// <summary>
    /// Create a clone of the context, usually for then making a slightly different context.
    /// </summary>
    /// <remarks>
    /// Implemented at the level of SiteContext and BlockContext.
    /// </remarks>
    /// <returns></returns>
    IContextOfSite CloneFullContext(ILog newParentLog, ISite site);
}