using ToSic.Sys.Security.Permissions;

namespace ToSic.Eav.Apps.Sys.Permissions;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class MultiPermissionsTypes(MultiPermissionsApp.Dependencies services, LazySvc<IAppReaderFactory> appReaderFactory)
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

    // Note: AppState must be public, as we have some extension methods that need it
    [field: AllowNull, MaybeNull]
    public IAppReader AppState => field ??= appReaderFactory.Value.GetOrKeep(MyOptions.App);

    public void Setup(Options options) => base.Setup(options);


    protected override Dictionary<string, IPermissionCheck> InitializePermissionChecks()
        => InitPermissionChecksForType(MyOptions.ContentTypes);

    protected Dictionary<string, IPermissionCheck> InitPermissionChecksForType(IEnumerable<string> contentTypes)
        => contentTypes.Distinct().ToDictionary(t => t, BuildTypePermissionChecker);


    /// <summary>
    /// Creates a permission checker for a type in this app
    /// </summary>
    /// <param name="typeName"></param>
    /// <returns></returns>
    private IPermissionCheck BuildTypePermissionChecker(string typeName)
    {
        var l = Log.Fn<IPermissionCheck>($"BuildTypePermissionChecker({typeName})");
        // now do relevant security checks
        var appState = appReaderFactory.Value.GetOrKeep(MyOptions.App);
        var result = BuildPermissionChecker(appState.TryGetContentType(typeName));
        return l.Return(result);
    }

}