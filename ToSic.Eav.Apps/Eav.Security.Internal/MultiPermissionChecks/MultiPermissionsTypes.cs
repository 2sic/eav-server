using ToSic.Eav.Apps;
using ToSic.Eav.Apps.Services;
using ToSic.Eav.Context;

namespace ToSic.Eav.Security.Internal;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class MultiPermissionsTypes: MultiPermissionsApp
{
    private const string LogName = "Sec.MPTyps";
    protected IEnumerable<string> ContentTypes;

    public MultiPermissionsTypes(MyServices services, LazySvc<IAppReaders> appStates): base(services, LogName)
    {
        ConnectLogs([
            _appStates = appStates
        ]);
    }
    private readonly LazySvc<IAppReaders> _appStates;

    // Note: AppState must be public, as we have some extension methods that need it
    public IAppDataService AppState => _appState ??= _appStates.Value.KeepOrGetReader(App);
    private IAppDataService _appState;

    public MultiPermissionsTypes Init(IContextOfSite context, IAppIdentity app, string contentType)
    {
        Init(context, app);
        return InitTypesAfterInit([contentType]);
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

    //public MultiPermissionsTypes Init(IContextOfSite context, IAppIdentity app, List<ItemIdentifier> items, ILog parentLog)
    //{
    //    Init(context, app, parentLog, LogName);
    //    ContentTypes = ExtractTypeNamesFromItems(items);
    //    return this;
    //}


    protected override Dictionary<string, IPermissionCheck> InitializePermissionChecks()
        => InitPermissionChecksForType(ContentTypes);

    protected Dictionary<string, IPermissionCheck> InitPermissionChecksForType(IEnumerable<string> contentTypes)
        => contentTypes.Distinct().ToDictionary(t => t, BuildTypePermissionChecker);

    //private IEnumerable<string> ExtractTypeNamesFromItems(IEnumerable<ItemIdentifier> items)
    //{
    //    var appData = AppState.List;
    //    // build list of type names
    //    var typeNames = items.Select(item =>
    //        !string.IsNullOrEmpty(item.ContentTypeName) || item.EntityId == 0
    //            ? item.ContentTypeName
    //            : appData.FindRepoId(item.EntityId).Type.NameId);

    //    return typeNames;
    //}


    /// <summary>
    /// Creates a permission checker for an type in this app
    /// </summary>
    /// <param name="typeName"></param>
    /// <returns></returns>
    private IPermissionCheck BuildTypePermissionChecker(string typeName)
    {
        Log.A($"BuildTypePermissionChecker({typeName})");
        // now do relevant security checks
        return BuildPermissionChecker(AppState.GetContentType(typeName));
    }
}