using ToSic.Eav.Context;
using ToSic.Eav.Context.Sys.ZoneMapper;
using ToSic.Sys.Capabilities.Features;
using ToSic.Sys.Security.Permissions;


namespace ToSic.Eav.Apps.Sys.Permissions;

/// <summary>
/// Do consolidate permission checks on a set of permissions
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public class MultiPermissionsApp: MultiPermissionsBase<MultiPermissionsApp.Dependencies, MultiPermissionsApp.Options>
{
    #region Constructors and DI

    public record Dependencies(
        LazySvc<IZoneMapper> ZoneMapper,
        Generator<AppPermissionCheck> AppPermCheckGenerator,
        Generator<ISysFeaturesService> FeatIntGen)
        : DependenciesRecord(connect: [ZoneMapper, AppPermCheckGenerator, FeatIntGen]);

    public record Options
    {
        public IContextOfSite Context
        {
            get => field ?? throw new ArgumentNullException(nameof(Context));
            init;
        }
        public IAppIdentity App
        {
            get => field ?? throw new ArgumentNullException(nameof(Context));
            init;
        }

    };

    /// <summary>
    /// Constructor for DI
    /// </summary>
    public MultiPermissionsApp(Dependencies services) : this(services, "Api.Perms") { }

    protected MultiPermissionsApp(Dependencies services, string logName, object[]? connect = default)
        : base(services, logName, connect)
    { }

    public override void Setup(Options options)
    {
        base.Setup(options);
        Init(options.Context, options.App);
    }

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
    protected IAppIdentity App { get; private set; } = null!;
    public IContextOfSite Context { get; private set; } = null!;
    protected ISite SiteForSecurityCheck { get; private set; } = null!;
    protected bool SamePortal { get; private set; }
    public ISysFeaturesService FeaturesInternal => Services.FeatIntGen.New();

    #endregion

    protected override Dictionary<string, IPermissionCheck> InitializePermissionChecks()
        => new() { { "App", BuildPermissionChecker() } };
        
    public bool ZoneIsOfCurrentContextOrUserIsSuper(out string? error)
    {
        var l = Log.Fn<bool>();
        var zoneSameOrSuperUser = SamePortal || Context.User.IsSystemAdmin;
        error = zoneSameOrSuperUser
            ? null
            : $"accessing app {App.AppId} in zone {App.ZoneId} is not allowed for this user";
        return l.Return(zoneSameOrSuperUser, zoneSameOrSuperUser ? $"SamePortal:{SamePortal} - ok": "not ok, generate error");
    }



    /// <summary>
    /// Creates a permission checker for an app
    /// Optionally you can provide a type-name, which will be 
    /// included in the permission check
    /// </summary>
    /// <returns></returns>
    protected IPermissionCheck BuildPermissionChecker(IContentType? type = null, IEntity? item = null)
    {
        var l = Log.Fn<IPermissionCheck>($"BuildPermissionChecker(type:{type?.Name}, item:{item?.EntityId})");

        // user has edit permissions on this app, and it's the same app as the user is coming from
        var modifiedContext = Context.Clone(Log);
        modifiedContext.Site = SiteForSecurityCheck;
        var result = Services.AppPermCheckGenerator.New().ForParts(modifiedContext, App, type, item);

        return l.Return(result, $"for {App.Show()} in {SiteForSecurityCheck?.Id}");
    }

}