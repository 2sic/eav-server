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
public class ContextOfAppFactory(
    ISite site,
    IUser user,
    IAppReaderFactory appReaders,
    LazySvc<ISysFeaturesService> features,
    LazySvc<AppUserLanguageCheck> langChecks,
    Generator<IEnvironmentPermission, EnvironmentPermissionOptions> environmentPermissions,
    LazySvc<PermissionCheckFactory> appPermissionCheck,
    LazySvc<AppDataStackService> settingsStack,
    LazySvc<IDraftOverrideService> draftService)
    : ServiceBase("Sxc.CtxApp", connect: [environmentPermissions, appReaders, features, langChecks, appPermissionCheck, settingsStack, draftService]),
        IContextOfAppFactory
{

    #region Factory Work

    public IContextOfApp Create(int appId) => Create(appReaders.AppIdentity(appId));

    /// <summary>
    /// New Implementation WIP v22.01 2026-10-18
    /// </summary>
    /// <param name="appIdentity"></param>
    /// <returns></returns>
    public IContextOfApp Create(IAppIdentity? appIdentity)
    {
        // First create context
        var result = new ContextOfApp(site, user, AppReaderOrNull: appIdentity == null ? null : appReaders.TryGet(appIdentity))
        {
            InternalMachinery = this,
            SettingsStackSvc = settingsStack,
        };
        // Now using the context, get the generated permissions
        // and check if we should update them, based on the cookie
        var updatedPermissions = draftService.Value.UserPermissions(result.Permissions);
        result = result with { Permissions = updatedPermissions };
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
        if (user.IsSystemAdmin)
            return l.ReturnTrue("super");

        // Case 2: No App-State
        if (contextOfApp.AppReaderOrNull == null)
        {
            if (user.MayAdmin())
                return l.ReturnTrue("no app, use UserMayAdmin checks");

            // If user isn't allowed yet, it may be that the environment allows it
            var fromEnv = environmentPermissions.New(new(contextOfApp, null))
                .EnvironmentAllows(GrantSets.WriteSomething)
                .Allowed;

            return l.Return(fromEnv, "no app, result from Env");
        }

        // Case 3: From App
        var fromApp = appPermissionCheck.Value
            .ForApp(contextOfApp, contextOfApp.AppReaderOrNull)
            .UserMay(GrantSets.WriteSomething)
            .Allowed;

        // Check if language permissions may alter / remove edit permissions
        if (fromApp && features.Value.IsEnabled(BuiltInFeatures.PermissionsByLanguage))
            fromApp = langChecks.Value.UserRestrictedByLanguagePermissions(contextOfApp.AppReaderOrNull) ?? true;

        return l.Return(fromApp, $"{fromApp}");
    }

    #endregion

}