using ToSic.Eav.Apps;
using ToSic.Eav.Apps.Services;
using ToSic.Eav.Cms.Internal.Languages;
using ToSic.Eav.Internal.Features;
using ToSic.Eav.Security;
using ToSic.Eav.Security.Permissions;
using ToSic.Lib.Helpers;
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
    public new class MyServices(
        ContextOfSite.MyServices siteServices,
        IAppReaderFactory appReaders,
        LazySvc<IEavFeaturesService> features,
        LazySvc<AppUserLanguageCheck> langChecks,
        Generator<IEnvironmentPermission> environmentPermissions,
        LazySvc<AppDataStackService> settingsStack)
        : MyServicesBase<ContextOfSite.MyServices>(siteServices,
            connect: [environmentPermissions, appReaders, features, langChecks, settingsStack])
    {
        public IAppReaderFactory AppReaders { get; } = appReaders;
        public LazySvc<IEavFeaturesService> Features { get; } = features;
        public LazySvc<AppUserLanguageCheck> LangChecks { get; } = langChecks;
        public LazySvc<AppDataStackService> SettingsStack { get; } = settingsStack;
        internal readonly Generator<IEnvironmentPermission> EnvironmentPermissions = environmentPermissions;
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

    public void ResetApp(IAppIdentity appIdentity)
    {
        var l = Log.Fn(appIdentity.Show());
        if (AppIdentity == null || AppIdentity.AppId != appIdentity.AppId)
            AppIdentity = appIdentity;
        l.Done();
    }

    protected virtual IAppIdentity AppIdentity
    {
        get => field;
        set
        {
            field = value;
            _appStateInternal.Reset();
            _appSettingsStack.Reset();
            _settings.Reset();
            _resources.Reset();
            _userMayEditGet.Reset();
        }
    }

    #region User Permissions / May Edit

    EffectivePermissions IContextOfUserPermissions.Permissions => field ??= new(isSiteAdmin: UserMayAdmin, isContentAdmin: UserMayEdit || User.IsContentAdmin);

    private bool UserMayEdit => _userMayEditGet.Get(() => Log.GetterM(() =>
    {
        // Case 1: Superuser always may
        if (User.IsSystemAdmin)
            return (true, "super");

        // Case 2: No App-State
        if (AppReader == null)
        {
            if (UserMayAdmin)
                return (true, "no app, use UserMayAdmin checks");

            // If user isn't allowed yet, it may be that the environment allows it
            var fromEnv = AppServices.EnvironmentPermissions.New()
                .Init(this as IContextOfSite, null)
                .EnvironmentAllows(GrantSets.WriteSomething);

            return (fromEnv, "no app, result from Env");
        }

        // Case 3: From App
        var fromApp = Services.AppPermissionCheck.New()
            .ForAppInInstance(this, AppReader)
            .UserMay(GrantSets.WriteSomething);

        // Check if language permissions may alter / remove edit permissions
        if (fromApp && AppServices.Features.Value.IsEnabled(BuiltInFeatures.PermissionsByLanguage))
            fromApp = AppServices.LangChecks.Value.UserRestrictedByLanguagePermissions(AppReader) ?? true;

        return (fromApp, $"{fromApp}");
    }));
    private readonly GetOnce<bool> _userMayEditGet = new();

    #endregion


    public IAppReader AppReader => _appStateInternal.Get(() => AppIdentity == null ? null : AppServices.AppReaders.Get(AppIdentity));
    private readonly GetOnce<IAppReader> _appStateInternal = new();

    #region Settings and Resources

    private AppDataStackService AppDataStackService => _appSettingsStack.Get(() => AppServices.SettingsStack.Value.Init(AppReader));
    private readonly GetOnce<AppDataStackService> _appSettingsStack = new();

    public PropertyStack AppSettings => _settings.Get(() => AppDataStackService.GetStack(RootNameSettings));
    private readonly GetOnce<PropertyStack> _settings = new();
    public PropertyStack AppResources => _resources.Get(() => AppDataStackService.GetStack(RootNameResources));
    private readonly GetOnce<PropertyStack> _resources = new();

    #endregion
}