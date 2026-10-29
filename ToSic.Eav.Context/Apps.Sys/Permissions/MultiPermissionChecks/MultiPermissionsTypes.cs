using ToSic.Sys.Security.Permissions;

namespace ToSic.Eav.Apps.Sys.Permissions;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class MultiPermissionsTypes(IAppReaderFactory appReaderFactory, PermissionContextAdjuster permCtxAdjuster, PermissionCheckFactory permCheckFactory)
    : ServiceWithSetup<MultiPermissionsTypes.Options>("Sec.MPTyps", connect: [appReaderFactory, permCtxAdjuster, permCheckFactory]), IMultiPermissionCheck
{
    public record Options : MultiPermissionsApp.Options
    {
        public required IEnumerable<string> ContentTypes { get; init; }
    }

    public Dictionary<string, IPermissionCheck> PermissionCheckers
        => field ??= InitializePermissionChecks();

    private Dictionary<string, IPermissionCheck> InitializePermissionChecks()
    {
        var appState = appReaderFactory.GetOrKeep(MyOptions.App);
        return MyOptions.ContentTypes.Distinct()
            .ToDictionary(
                t => t,
                s => BuildTypePermissionChecker(appState, s)
            );

        // <summary>
        // Creates a permission checker for a type in this app
        // </summary>
        IPermissionCheck BuildTypePermissionChecker(IAppReader appState, string typeName)
        {
            var l = Log.Fn<IPermissionCheck>($"BuildTypePermissionChecker({typeName})");
            var app = MyOptions.App;

            // user has edit permissions on this app, and it's the same app as the user is coming from
            var ctxSiteOrBlock = permCtxAdjuster.AdjustContextForApp(MyOptions.SiteContext, app);
            var checker = permCheckFactory.ForType(ctxSiteOrBlock, app, appState.GetContentType(typeName));

            return l.ReturnAsOk(checker);
        }
    }
}