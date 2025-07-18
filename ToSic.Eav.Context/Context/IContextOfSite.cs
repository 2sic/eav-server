﻿using ToSic.Sys.Users;
using ToSic.Sys.Users.Permissions;

namespace ToSic.Eav.Context;

[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IContextOfSite: IHasLog, ICurrentContextUserPermissions
{
    /// <summary>
    /// The website the current request is running in
    /// </summary>
    ISite Site { get; set; }

    /// <summary>
    /// The user in the current request / context
    /// </summary>
    IUser User { get; }

    /// <summary>
    /// Create a clone of the context, usually for then making a slightly different context
    /// </summary>
    /// <returns></returns>
    IContextOfSite Clone(ILog newParentLog);
}