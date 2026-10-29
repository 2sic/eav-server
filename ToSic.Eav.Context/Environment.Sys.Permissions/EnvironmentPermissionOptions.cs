using ToSic.Eav.Apps;
using ToSic.Eav.Context;
using ToSic.Sys.Security.Permissions;
using ToSic.Sys.Users;

namespace ToSic.Eav.Environment.Sys.Permissions;

/// <summary>
/// Options to be used when setting up <see cref="IEnvironmentPermission"/>.
/// </summary>
/// <param name="ContextOfSiteOrBlock">Context information of site, or also of block if possible. Required.</param>
/// <param name="AppIdentityOrNull">Current app identity - if available</param>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
public record EnvironmentPermissionOptions(IContextOfSite ContextOfSiteOrBlock, IAppIdentity? AppIdentityOrNull)
    : EnvironmentPermissionOptionsBase
{
    public IContextOfSite ContextOfSiteOrBlock { get; init; }
        = ContextOfSiteOrBlock ?? throw new ArgumentException($@"Must be an {nameof(IContextOfSite)}", nameof(ContextOfSiteOrBlock));

    public IUser User => ContextOfSiteOrBlock.User;
    public ISite Site => ContextOfSiteOrBlock.Site;
}
