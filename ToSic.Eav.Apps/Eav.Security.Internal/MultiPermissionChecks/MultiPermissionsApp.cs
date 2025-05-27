using ToSic.Eav.Apps;
using ToSic.Eav.Context;
using ToSic.Eav.Integration;
using ToSic.Eav.Internal.Features;
using ToSic.Sys.Capabilities.Features;
using ToSic.Sys.Security.Permissions;
using IEntity = ToSic.Eav.Data.IEntity;

namespace ToSic.Eav.Security.Internal;

/// <summary>
/// Do consolidate permission checks on a set of permissions
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public class MultiPermissionsApp: MultiPermissionsBase<MultiPermissionsApp.MyServices>
{
    #region Constructors and DI

    public class MyServices(
        LazySvc<IZoneMapper> zoneMapper,
        Generator<AppPermissionCheck> appPermCheckGenerator,
        Generator<ISysFeaturesService> featIntGen)
        : MyServicesBase(connect: [zoneMapper, appPermCheckGenerator, featIntGen])
    {
        internal LazySvc<IZoneMapper> ZoneMapper { get; } = zoneMapper;
        internal Generator<AppPermissionCheck> AppPermCheckGenerator { get; } = appPermCheckGenerator;
        internal Generator<ISysFeaturesService> FeatIntGen { get; } = featIntGen;
    }

    /// <summary>
    /// Constructor for DI
    /// </summary>
    public MultiPermissionsApp(MyServices services) : this(services, "Api.Perms") { }

    protected MultiPermissionsApp(MyServices services, string logName) : base(services, logName) {}

    public MultiPermissionsApp Init(IContextOfSite context, IAppIdentity app)
    {
        var l = Log.Fn<MultiPermissionsApp>($"..., appId: {app.AppId}, ...");
        Context = context;
        App = app;

        SamePortal = context.Site.ZoneId == App.ZoneId;
        SiteForSecurityCheck = SamePortal 
            ? context.Site 
            // if the app is of another zone check that, but in multi-zone portals this won't find anything, so use current zone
            // todo: probably enhance with a Site.IsMultiZone check
            : Services.ZoneMapper.Value.SiteOfZone(App.ZoneId)
              ?? context.Site;
        return l.Return(this, $"ready for app:{app.Show()} tenant/zone:{SiteForSecurityCheck.Id}/{context.Site.ZoneId} same:{SamePortal}");
    }
    /// <summary>
    /// The current app which will be used and can be re-used externally
    /// </summary>
    protected IAppIdentity App { get; private set; }
    public IContextOfSite Context { get; private set; }
    protected ISite SiteForSecurityCheck { get; private set; }
    protected bool SamePortal { get; private set; }
    public ISysFeaturesService FeaturesInternal => Services.FeatIntGen.New();

    #endregion

    protected override Dictionary<string, IPermissionCheck> InitializePermissionChecks()
        => new() { { "App", BuildPermissionChecker() } };
        
    public bool ZoneIsOfCurrentContextOrUserIsSuper(out string error)
    {
        var l = Log.Fn<bool>();
        var zoneSameOrSuperUser = SamePortal || Context.User.IsSystemAdmin;
        error = zoneSameOrSuperUser ? null: $"accessing app {App.AppId} in zone {App.ZoneId} is not allowed for this user";
        return l.Return(zoneSameOrSuperUser, zoneSameOrSuperUser ? $"SamePortal:{SamePortal} - ok": "not ok, generate error");
    }



    /// <summary>
    /// Creates a permission checker for an app
    /// Optionally you can provide a type-name, which will be 
    /// included in the permission check
    /// </summary>
    /// <returns></returns>
    protected IPermissionCheck BuildPermissionChecker(IContentType type = null, IEntity item = null)
    {
        var l = Log.Fn<IPermissionCheck>($"BuildPermissionChecker(type:{type?.Name}, item:{item?.EntityId})");

        // user has edit permissions on this app, and it's the same app as the user is coming from
        var modifiedContext = Context.Clone(Log);
        modifiedContext.Site = SiteForSecurityCheck;
        var result = Services.AppPermCheckGenerator.New().ForParts(modifiedContext, App, type, item);

        return l.Return(result, $"for {App.Show()} in {SiteForSecurityCheck?.Id}");
    }

}