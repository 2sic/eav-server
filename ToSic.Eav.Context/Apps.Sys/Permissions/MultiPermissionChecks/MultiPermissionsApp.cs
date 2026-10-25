using ToSic.Eav.Context;
using ToSic.Sys.Security.Permissions;


namespace ToSic.Eav.Apps.Sys.Permissions;

/// <summary>
/// Do consolidate permission checks on a set of permissions
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public class MultiPermissionsApp: ServiceWithSetup<MultiPermissionsApp.Dependencies, MultiPermissionsApp.Options>, IMultiPermissionCheck
{
    #region Constructors and DI

    public record Dependencies(
        PermissionCheckFactory PermCheckFactory,
        PermissionContextAdjuster PermCtxAdjuster)
        : DependenciesBase(connect: [PermCheckFactory, PermCtxAdjuster]);

    public record Options
    {
        public IContextOfSite SiteContext
        {
            get => field ?? throw new ArgumentNullException(nameof(SiteContext));
            init;
        }
        public IAppIdentity App
        {
            get => field ?? throw new ArgumentNullException(nameof(SiteContext));
            init;
        }

    }

    public virtual Dictionary<string, IPermissionCheck> PermissionCheckers
        => field ??= InitializePermissionChecks();

    /// <inheritdoc/>
    public bool UserMayOnAll(List<Grants> grants) => EnsureAll(grants, out _);

    /// <inheritdoc/>>
    public bool EnsureAll(List<Grants> grants, out string? error) => this.EnsureAllInternal(grants, out error);

    public MultiPermissionsApp(Dependencies services) : base(services, "Api.Perms") { }

    protected MultiPermissionsApp(Dependencies services, string logName, object[]? connect = default)
        : base(services, logName, connect: connect)
    { }

    //protected bool SamePortal => _samePortal ??= MyOptions.SiteContext.Site.ZoneId == MyOptions.App.ZoneId;
    //private bool? _samePortal;

    ///// <summary>
    ///// The current app which will be used and can be re-used externally
    ///// </summary>
    //private ISite SiteForSecurityCheck => field ??= GetSiteForSecurityCheck();

    //private ISite GetSiteForSecurityCheck()
    //{
    //    var app = MyOptions.App;
    //    var context = MyOptions.SiteContext;
    //    var l = Log.Fn($"..., appId: {app.AppId}, ...");

    //    var result = SamePortal
    //        ? context.Site
    //        // if the app is of another zone check that, but in multi-zone portals this won't find anything, so use current zone
    //        // todo: probably enhance with a Site.IsMultiZone check
    //        : Services.ZoneMapper.Value.SiteOfZone(app.ZoneId)
    //          ?? context.Site;
    //    l.Done($"ready for app:{app.Show()} tenant/zone:{result.Id}/{context.Site.ZoneId} same:{SamePortal}");
    //    return result;
    //}

    #endregion

    private Dictionary<string, IPermissionCheck> InitializePermissionChecks()
        => new() { { "App", BuildPermissionChecker(MyOptions.App) } };
       
    /// <summary>
    /// Creates a permission checker for an app
    /// </summary>
    private IPermissionCheck BuildPermissionChecker(IAppIdentity app)
    {
        var l = Log.Fn<IPermissionCheck>($"app:{app.Show()}");

        // user has edit permissions on this app, and it's the same app as the user is coming from
        var ctxSiteOrBlock = Services.PermCtxAdjuster.AdjustContextForApp(MyOptions.SiteContext, app);
        var result = Services.PermCheckFactory.ForAppInInstance(ctxSiteOrBlock, app);

        return l.Return(result, $"for {app.Show()} in {ctxSiteOrBlock?.Site?.Id}");
    }

    // 2026-10-25 2dm moved to only place it's used
    //public bool ZoneIsOfCurrentContextOrUserIsSuper(out string? error)
    //{
    //    var l = Log.Fn<bool>();
    //    var isSameSite = MyOptions.SiteContext.Site.ZoneId == MyOptions.App.ZoneId;

    //    var zoneSameOrSuperUser = isSameSite || MyOptions.SiteContext.User.IsSystemAdmin;
    //    error = zoneSameOrSuperUser
    //        ? null
    //        : $"accessing app {MyOptions.App.Show()} is not allowed for this user as it changes zones";
    //    return l.Return(zoneSameOrSuperUser, zoneSameOrSuperUser ? $"SamePortal:{isSameSite} - ok": "not ok, generate error");
    //}



    ///// <summary>
    ///// Creates a permission checker for an app
    ///// Optionally you can provide a type-name, which will be 
    ///// included in the permission check
    ///// </summary>
    ///// <returns></returns>
    //protected IPermissionCheck BuildPermissionChecker(IContentType? type = null, IEntity? item = null)
    //{
    //    var l = Log.Fn<IPermissionCheck>($"BuildPermissionChecker(type:{type?.Name}, item:{item?.EntityId})");

    //    // user has edit permissions on this app, and it's the same app as the user is coming from
    //    var ctxSiteOrBlock = MyOptions.SiteContext.WithSwappedSite(SiteForSecurityCheck);
    //    var result = Services.PermCheckFactory.Value.ForParts(ctxSiteOrBlock, MyOptions.App, type, item);

    //    return l.Return(result, $"for {MyOptions.App.Show()} in {SiteForSecurityCheck?.Id}");
    //}


}
