using ToSic.Sys.Security.Permissions;

namespace ToSic.Eav.Apps.Sys.Permissions;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class MultiPermissionsTypes(MultiPermissionsApp.Dependencies services, IAppReaderFactory appReaderFactory, PermissionContextAdjuster permCtxAdjuster, PermissionCheckFactory permCheckFactory)
    : MultiPermissionsApp(services, "Sec.MPTyps", connect: [appReaderFactory]),
        IServiceWithSetup<MultiPermissionsTypes.Options>,
        IHasOptions<MultiPermissionsTypes.Options>
{
    public new record Options : MultiPermissionsApp.Options
    {
        public IEnumerable<string> ContentTypes
        {
            get => field ?? throw new ArgumentNullException(nameof(ContentTypes));
            init;
        }
    }

    public new Options MyOptions => (Options)base.MyOptions;


    public void Setup(Options options) => base.Setup(options);

    public override Dictionary<string, IPermissionCheck> PermissionCheckers
        => field ??= InitializePermissionChecks();

    private Dictionary<string, IPermissionCheck> InitializePermissionChecks()
        => MyOptions.ContentTypes.Distinct()
            .ToDictionary(t => t, BuildTypePermissionChecker);


    /// <summary>
    /// Creates a permission checker for a type in this app
    /// </summary>
    /// <param name="typeName"></param>
    /// <returns></returns>
    private IPermissionCheck BuildTypePermissionChecker(string typeName)
    {
        var l = Log.Fn<IPermissionCheck>($"BuildTypePermissionChecker({typeName})");
        var app = MyOptions.App;

        // user has edit permissions on this app, and it's the same app as the user is coming from
        var ctxSiteOrBlock = permCtxAdjuster.AdjustContextForApp(MyOptions.SiteContext, app);
        var checker = permCheckFactory.ForType(ctxSiteOrBlock, app, AppState.GetContentType(typeName));

        //var result = MultiPermAppCheckBuilder.BuildPermissionChecker(MyOptions.App, AppState.TryGetContentType(typeName));
        return l.Return(checker);
    }

    [field: AllowNull, MaybeNull]
    private IAppReader AppState => field ??= appReaderFactory.GetOrKeep(MyOptions.App);

}