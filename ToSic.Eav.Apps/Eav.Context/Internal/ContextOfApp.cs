using ToSic.Eav.Apps;
using ToSic.Eav.Apps.Services;
using ToSic.Eav.Apps.State;
using ToSic.Eav.Cms.Internal.Languages;
using ToSic.Eav.Data;
using ToSic.Eav.Internal.Features;
using ToSic.Eav.Security;
using ToSic.Eav.Security.Permissions;
using ToSic.Lib.DI;
using ToSic.Lib.Helpers;
using ToSic.Lib.Logging;
using ToSic.Lib.Services;
using static ToSic.Eav.Apps.AppStackConstants;


namespace ToSic.Eav.Context.Internal;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class ContextOfApp: ContextOfSite, IContextOfApp
{
    #region Constructor / DI

    /// <summary>
    /// These dependencies are a bit special, because they can be re-used for child context-of...
    /// This is why we gave them a much clearer name, not just the normal "Dependencies"
    /// </summary>
    public new class MyServices: MyServicesBase<ContextOfSite.MyServices>
    {
        public MyServices(
            ContextOfSite.MyServices siteServices,
            IAppStates appStates,
            LazySvc<IEavFeaturesService> features,
            LazySvc<AppUserLanguageCheck> langChecks,
            Generator<IEnvironmentPermission> environmentPermissions,
            LazySvc<AppDataStackService> settingsStack
        ): base(siteServices)
        {
            ConnectServices(
                EnvironmentPermissions = environmentPermissions,
                AppStates = appStates,
                Features = features,
                LangChecks = langChecks,
                SettingsStack = settingsStack
            );
        }

        public IAppStates AppStates { get; }
        public LazySvc<IEavFeaturesService> Features { get; }
        public LazySvc<AppUserLanguageCheck> LangChecks { get; }
        public LazySvc<AppDataStackService> SettingsStack { get; }
        internal readonly Generator<IEnvironmentPermission> EnvironmentPermissions;
    }

    /// <summary>
    /// Constructor for DI
    /// </summary>
    /// <param name="services"></param>
    public ContextOfApp(MyServices services) : this(services, "Sxc.CtxApp")
    {
    }
    protected ContextOfApp(MyServices services, string logName) : base(services, logName)
    {
        AppServices = services;
    }
    protected readonly MyServices AppServices;

    #endregion

    public void ResetApp(IAppIdentity appIdentity) => Log.Do(() =>
    {
        if (AppIdentity == null || AppIdentity.AppId != appIdentity.AppId)
            AppIdentity = appIdentity;
    });

    protected virtual IAppIdentity AppIdentity
    {
        get => _appIdentity;
        set
        {
            _appIdentity = value;
            _appStateInternal.Reset();
            _appSettingsStack.Reset();
            _settings.Reset();
            _resources.Reset();
            _userMayEditGet.Reset();
        }
    }
    private IAppIdentity _appIdentity;

    public override bool UserMayEdit => _userMayEditGet.Get(() => Log.GetterM(() =>
    {
        // Case 1: Superuser always may
        if (User.IsSystemAdmin) return (true, "super");

        // Case 2: No App-State
        if (AppState == null)
        {
            if (base.UserMayEdit) return (true, "no app, use default checks");

            // If user isn't allowed yet, it may be that the environment allows it
            var fromEnv = AppServices.EnvironmentPermissions.New()
                .Init(this as IContextOfSite, null)
                .EnvironmentAllows(GrantSets.WriteSomething);

            return (fromEnv, "no app, result from Env");
        }

        // Case 3: From App
        var fromApp = Services.AppPermissionCheck.New()
            .ForAppInInstance(this, AppState)
            .UserMay(GrantSets.WriteSomething);

        // Check if language permissions may alter / remove edit permissions
        if (fromApp && AppServices.Features.Value.IsEnabled(BuiltInFeatures.PermissionsByLanguage))
            fromApp = AppServices.LangChecks.Value.UserRestrictedByLanguagePermissions(AppState) ?? true;

        return (fromApp, $"{fromApp}");
    }));
    private readonly GetOnce<bool> _userMayEditGet = new();

    public IAppStateInternal AppState => _appStateInternal.Get(() => AppIdentity == null ? null : AppServices.AppStates.GetReader(AppIdentity));
    private readonly GetOnce<IAppStateInternal> _appStateInternal = new();

    #region Settings and Resources

    private AppDataStackService AppDataStackService => _appSettingsStack.Get(() => AppServices.SettingsStack.Value.Init(AppState));
    private readonly GetOnce<AppDataStackService> _appSettingsStack = new();

    public PropertyStack AppSettings => _settings.Get(() => AppDataStackService.GetStack(RootNameSettings));
    private readonly GetOnce<PropertyStack> _settings = new();
    public PropertyStack AppResources => _resources.Get(() => AppDataStackService.GetStack(RootNameResources));
    private readonly GetOnce<PropertyStack> _resources = new();

    #endregion
}