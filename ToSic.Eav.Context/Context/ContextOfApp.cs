using ToSic.Eav.Apps;
using ToSic.Eav.Apps.Sys;
using ToSic.Eav.Apps.Sys.AppStack;
using ToSic.Eav.Apps.Sys.Permissions;
using ToSic.Eav.Context.Sys;
using ToSic.Eav.Context.Sys.Site;
using ToSic.Eav.Data.Sys.PropertyStack;
using ToSic.Eav.Environment.Sys.Permissions;
using ToSic.Sys.Capabilities.Features;
using ToSic.Sys.Security.Permissions;
using ToSic.Sys.Users;
using ToSic.Sys.Users.Permissions;
using static ToSic.Eav.Apps.Sys.AppStack.AppStackConstants;


namespace ToSic.Eav.Context;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class ContextOfApp: ServiceBase<ContextOfApp.Dependencies>, IContextOfApp, IContextOfAppFactory
{
    #region Constructor / DI

    /// <summary>
    /// These dependencies are a bit special, because they can be re-used for child context-of...
    /// This is why we gave them a much clearer name, not just the normal "Dependencies"
    /// </summary>
    public record Dependencies(
        ISite Site,
        IUser User,
        IAppReaderFactory AppReaders,
        LazySvc<ISysFeaturesService> Features,
        LazySvc<AppUserLanguageCheck> LangChecks,
        Generator<IEnvironmentPermission> EnvironmentPermissions,
        Generator<AppPermissionCheck> AppPermissionCheck,
        LazySvc<AppDataStackService> SettingsStack)
        : DependenciesBase(connect: [EnvironmentPermissions, AppReaders, Features, LangChecks, AppPermissionCheck, SettingsStack]);

    /// <summary>
    /// Constructor for DI
    /// </summary>
    /// <param name="services"></param>
    public ContextOfApp(Dependencies services) : this(services, "Sxc.CtxApp", [])
    { }

    // 2025-06-13 2dm - sometimes AppIdentity is null, but I'm not sure if this is something that should be kept this way
    // For now, the code works, but we should continuously optimize and review again.
    // Also with the AppReaderOrNull which is null if AppIdentity is null
#pragma warning disable CS9264 // Non-nullable property must contain a non-null value when exiting constructor. Consider adding the 'required' modifier, or declaring the property as nullable, or adding '[field: MaybeNull, AllowNull]' attributes.
    protected ContextOfApp(Dependencies services, string logName, object[] connect) : base(services, logName, connect: connect)
    {
    }
#pragma warning restore CS9264 // Non-nullable property must contain a non-null value when exiting constructor. Consider adding the 'required' modifier, or declaring the property as nullable, or adding '[field: MaybeNull, AllowNull]' attributes.


    #endregion

    /// <summary>
    /// New Implementation WIP
    /// </summary>
    /// <param name="appIdentity"></param>
    /// <returns></returns>
    public IContextOfApp Create(IAppIdentity appIdentity)
    {
        ResetApp(appIdentity);
        return this;
    }

    public void ResetApp(IAppIdentity appIdentity)
    {
        var l = Log.Fn(appIdentity.Show());
        if (AppIdentity?.AppId != appIdentity.AppId)
            AppIdentity = appIdentity;
        l.Done();
    }

    protected virtual IAppIdentity? AppIdentity
    {
        get;
        set
        {
            field = value;
            _appReaderOrNull.Reset();
            AppSettings = null!;
            AppResources = null!;
            AppDataStackService = null!;
            _userMayAdmin = null;
        }
    }

    #region User Permissions / May Edit

    private bool UserMayAdmin => _userMayAdmin ??= ContextOfSite.GetUserMayAdmin(User);
    private bool? _userMayAdmin;

    [field: AllowNull, MaybeNull]
    EffectivePermissions ICurrentContextUserPermissions.Permissions
        => field ??= new(isSiteAdmin: UserMayAdmin, isContentAdmin: GetUserMayEdit() || User.IsContentAdmin);

    private bool GetUserMayEdit()
    {
        var l = Log.Fn<bool>();

        // Case 1: Superuser always may
        if (User.IsSystemAdmin)
            return l.ReturnTrue("super");

        // Case 2: No App-State
        if (AppReaderOrNull == null)
        {
            if (UserMayAdmin)
                return l.ReturnTrue("no app, use UserMayAdmin checks");

            // If user isn't allowed yet, it may be that the environment allows it
            var enfPermissions = Services.EnvironmentPermissions.New();
            var fromEnv = ((IEnvironmentPermissionSetup)enfPermissions)
                .Init<IContextOfSite>(this, null)
                .EnvironmentAllows(GrantSets.WriteSomething)
                .Allowed;

            return l.Return(fromEnv, "no app, result from Env");
        }

        // Case 3: From App
        var fromApp = Services.AppPermissionCheck.New()
            .ForAppInInstance(this, AppReaderRequired)
            .UserMay(GrantSets.WriteSomething)
            .Allowed;

        // Check if language permissions may alter / remove edit permissions
        if (fromApp && Services.Features.Value.IsEnabled(BuiltInFeatures.PermissionsByLanguage))
            fromApp = Services.LangChecks.Value.UserRestrictedByLanguagePermissions(AppReaderRequired) ?? true;

        return l.Return(fromApp, $"{fromApp}");
    }

    #endregion


    #region Site Context

    public ISite Site => Services.Site;

    public IUser User => Services.User;

    #endregion


    #region AppReaders


    public IAppReader? AppReaderOrNull => _appReaderOrNull.Get(() => AppIdentity == null! ? null : Services.AppReaders.TryGet(AppIdentity));
    private readonly LazyGetAndReset<IAppReader?> _appReaderOrNull = new();

    public IAppReader AppReaderRequired => AppReaderOrNull
                                           ?? throw new NullReferenceException("Trying to get AppReader but AppIdentity is null");

    #endregion


    #region Settings and Resources

    [field: AllowNull, MaybeNull]
    public PropertyStack AppSettings
    {
        get => field ??= AppDataStackService.GetStack(RootNameSettings);
        private set;
    }

    [field: AllowNull, MaybeNull]
    public PropertyStack AppResources
    {
        get => field ??= AppDataStackService.GetStack(RootNameResources);
        private set;
    }

    [field: AllowNull, MaybeNull]
    private AppDataStackService AppDataStackService
    {
        get => field ??= Services.SettingsStack.Value.Init(AppReaderRequired);
        set;
    }

    #endregion

    IContextOfSite IContextOfSiteOrMoreCloneable.CloneFullContext(ILog newParentLog, ISite site) =>
        new ContextOfApp(Services with { Site = site })
        {
            AppIdentity = AppIdentity,
        };

}