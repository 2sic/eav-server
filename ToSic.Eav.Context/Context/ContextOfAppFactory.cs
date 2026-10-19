using ToSic.Eav.Apps;
using ToSic.Eav.Apps.Sys.AppStack;
using ToSic.Eav.Apps.Sys.Permissions;
using ToSic.Eav.Context.Sys;
using ToSic.Eav.Environment.Sys.Permissions;
using ToSic.Sys.Capabilities.Features;
using ToSic.Sys.Security.Permissions;
using ToSic.Sys.Users;


namespace ToSic.Eav.Context;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class ContextOfAppFactory: ServiceBase<ContextOfAppFactory.Dependencies>, IContextOfAppFactory
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
    public ContextOfAppFactory(Dependencies services) : this(services, "Sxc.CtxApp", [])
    { }

    // 2025-06-13 2dm - sometimes AppIdentity is null, but I'm not sure if this is something that should be kept this way
    // For now, the code works, but we should continuously optimize and review again.
    // Also with the AppReaderOrNull which is null if AppIdentity is null
#pragma warning disable CS9264 // Non-nullable property must contain a non-null value when exiting constructor. Consider adding the 'required' modifier, or declaring the property as nullable, or adding '[field: MaybeNull, AllowNull]' attributes.
    protected ContextOfAppFactory(Dependencies services, string logName, object[] connect) : base(services, logName, connect: connect)
    {
    }
#pragma warning restore CS9264 // Non-nullable property must contain a non-null value when exiting constructor. Consider adding the 'required' modifier, or declaring the property as nullable, or adding '[field: MaybeNull, AllowNull]' attributes.


    #endregion

    #region Factory Work

    /// <summary>
    /// New Implementation WIP
    /// </summary>
    /// <param name="appIdentity"></param>
    /// <returns></returns>
    public IContextOfApp Create(IAppIdentity? appIdentity)
    {
        var result = new ContextOfApp(
            Services.Site,
            Services.User,
            appIdentity == null ? null : Services.AppReaders.TryGet(appIdentity)
        )
        {
            InternalMachinery = this,
            SettingsStackSvc = Services.SettingsStack,
        };
        return result;
    }

    #endregion

    
    #region User Permissions / May Edit

    internal bool GetUserMayEdit(ContextOfApp contextOfApp)
    {
        var l = Log.Fn<bool>();

        // Case 1: Superuser always may
        if (Services.User.IsSystemAdmin)
            return l.ReturnTrue("super");

        // Case 2: No App-State
        if (contextOfApp.AppReaderOrNull == null)
        {
            if (contextOfApp.UserMayAdmin)
                return l.ReturnTrue("no app, use UserMayAdmin checks");

            // If user isn't allowed yet, it may be that the environment allows it
            var enfPermissions = Services.EnvironmentPermissions.New();
            var fromEnv = ((IEnvironmentPermissionSetup)enfPermissions)
                .Init<IContextOfSite>(contextOfApp, null)
                .EnvironmentAllows(GrantSets.WriteSomething)
                .Allowed;

            return l.Return(fromEnv, "no app, result from Env");
        }

        // Case 3: From App
        var fromApp = Services.AppPermissionCheck.New()
            .ForAppInInstance(contextOfApp, contextOfApp.AppReaderOrNull)
            .UserMay(GrantSets.WriteSomething)
            .Allowed;

        // Check if language permissions may alter / remove edit permissions
        if (fromApp && Services.Features.Value.IsEnabled(BuiltInFeatures.PermissionsByLanguage))
            fromApp = Services.LangChecks.Value.UserRestrictedByLanguagePermissions(contextOfApp.AppReaderOrNull) ?? true;

        return l.Return(fromApp, $"{fromApp}");
    }

    #endregion

}