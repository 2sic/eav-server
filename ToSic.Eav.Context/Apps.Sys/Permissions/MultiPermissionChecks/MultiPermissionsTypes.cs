using ToSic.Eav.Context;
using ToSic.Sys.Security.Permissions;

namespace ToSic.Eav.Apps.Sys.Permissions;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class MultiPermissionsTypes(MultiPermissionsApp.Dependencies services, LazySvc<IAppReaderFactory> appReaderFactory)
    : MultiPermissionsApp(services, LogName, connect: [appReaderFactory])
{
    private const string LogName = "Sec.MPTyps";

    // Will be initialized in Init / InitTypesAfterInit;
    private IEnumerable<string> ContentTypes { get; set; } = null!;

    // Note: AppState must be public, as we have some extension methods that need it
    [field: AllowNull, MaybeNull]
    public IAppReader AppState => field ??= appReaderFactory.Value.GetOrKeep(App);

    public MultiPermissionsTypes Init(IContextOfSite context, IAppIdentity app, string contentType)
    {
        var l = Log.Fn<MultiPermissionsTypes>($"..., appId: {app.AppId}, contentType: '{contentType}'");
        Init(context, app);
        ContentTypes = [contentType];
        return l.Return(this);
    }

    /// <summary>
    /// This step is separate, because extension methods need it _after_ Init
    /// </summary>
    /// <param name="contentTypes"></param>
    /// <returns></returns>
    public MultiPermissionsTypes InitTypesAfterInit(IEnumerable<string> contentTypes)
    {
        ContentTypes = contentTypes;
        return this;
    }


    protected override Dictionary<string, IPermissionCheck> InitializePermissionChecks()
        => InitPermissionChecksForType(ContentTypes);

    protected Dictionary<string, IPermissionCheck> InitPermissionChecksForType(IEnumerable<string> contentTypes)
        => contentTypes.Distinct().ToDictionary(t => t, BuildTypePermissionChecker);


    /// <summary>
    /// Creates a permission checker for a type in this app
    /// </summary>
    /// <param name="typeName"></param>
    /// <returns></returns>
    private IPermissionCheck BuildTypePermissionChecker(string typeName)
    {
        Log.A($"BuildTypePermissionChecker({typeName})");
        // now do relevant security checks
        return BuildPermissionChecker(AppState.TryGetContentType(typeName));
    }
}