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
public class ContextOfAppFactory(ContextOfAppFactory.Dependencies services)
    : ServiceBase<ContextOfAppFactory.Dependencies>(services, "Sxc.CtxApp"),
        IContextOfAppFactory
{
    /// <summary>
    /// Dependencies
    /// </summary>
    public record Dependencies(
        ISite Site,
        IUser User,
        IAppReaderFactory AppReaders,
        LazySvc<ISysFeaturesService> Features,
        LazySvc<AppUserLanguageCheck> LangChecks,
        Generator<IEnvironmentPermission> EnvironmentPermissions,
        LazySvc<PermissionCheckFactory> AppPermissionCheck,
        LazySvc<AppDataStackService> SettingsStack
    ) : DependenciesBase(connect: [EnvironmentPermissions, AppReaders, Features, LangChecks, AppPermissionCheck, SettingsStack]);


    #region Factory Work

    public IContextOfApp Create(int appId)
    {
        var appIdentity = Services.AppReaders.AppIdentity(appId);
        return Create(appIdentity);
    }

    /// <summary>
    /// New Implementation WIP v22.01 2026-10-18
    /// </summary>
    /// <param name="appIdentity"></param>
    /// <returns></returns>
    public IContextOfApp Create(IAppIdentity? appIdentity)
    {
        var result = new ContextOfApp(
            Services.Site,
            Services.User,
            AppReaderOrNull: appIdentity == null ? null : Services.AppReaders.TryGet(appIdentity)
        )
        {
            InternalMachinery = this,
            SettingsStackSvc = Services.SettingsStack,
        };
        return result;
    }

    #endregion

    
    #region User Permissions / May Edit

    /// <summary>
    /// Helper to look up specific permissions - will be called by the implementation
    /// </summary>
    /// <param name="contextOfApp"></param>
    /// <returns></returns>
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
        var fromApp = Services.AppPermissionCheck.Value
            .ForApp(contextOfApp, contextOfApp.AppReaderOrNull)
            .UserMay(GrantSets.WriteSomething)
            .Allowed;

        // Check if language permissions may alter / remove edit permissions
        if (fromApp && Services.Features.Value.IsEnabled(BuiltInFeatures.PermissionsByLanguage))
            fromApp = Services.LangChecks.Value.UserRestrictedByLanguagePermissions(contextOfApp.AppReaderOrNull) ?? true;

        return l.Return(fromApp, $"{fromApp}");
    }

    #endregion

}