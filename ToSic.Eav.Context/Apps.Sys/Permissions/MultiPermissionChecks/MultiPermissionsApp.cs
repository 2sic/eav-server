using ToSic.Eav.Context;
using ToSic.Sys.Security.Permissions;


namespace ToSic.Eav.Apps.Sys.Permissions;

/// <summary>
/// Do consolidate permission checks on a set of permissions
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public class MultiPermissionsApp(PermissionCheckFactory permCheckFactory, PermissionContextAdjuster permCtxAdjuster)
    : ServiceWithSetup<MultiPermissionsApp.Options>("Api.Perms", connect: [permCheckFactory, permCtxAdjuster]),
        IMultiPermissionCheck
{
    /// <summary>
    /// Options, every field required, and this is enforced using errors.
    /// </summary>
    public record Options
    {
        public required IContextOfSite SiteContext { get; init; }

        public required IAppIdentity App { get; init; }

    }


    public Dictionary<string, IPermissionCheck> PermissionCheckers
        => field ??= InitializePermissionChecks();


    private Dictionary<string, IPermissionCheck> InitializePermissionChecks()
        => new() { { "App", BuildPermissionChecker(MyOptions.App) } };
       
    /// <summary>
    /// Creates a permission checker for an app
    /// </summary>
    private IPermissionCheck BuildPermissionChecker(IAppIdentity app)
    {
        var l = Log.Fn<IPermissionCheck>($"app:{app.Show()}");

        // user has edit permissions on this app, and it's the same app as the user is coming from
        var ctxSiteOrBlock = permCtxAdjuster.AdjustContextForApp(MyOptions.SiteContext, app);
        var result = permCheckFactory.ForApp(ctxSiteOrBlock, app);

        return l.Return(result, $"for {app.Show()} in {ctxSiteOrBlock?.Site?.Id}");
    }

}
