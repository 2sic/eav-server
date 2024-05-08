using ToSic.Eav.Apps.Internal;
using ToSic.Eav.Apps.Internal.Insights;
using ToSic.Eav.Caching;
using ToSic.Eav.Context;
using ToSic.Eav.Internal.Licenses;
using ToSic.Eav.Security.Fingerprint;
using ToSic.Eav.WebApi.Errors;
using JsonSerializer = ToSic.Eav.ImportExport.Json.JsonSerializer;

namespace ToSic.Eav.WebApi.Sys.Insights;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public partial class InsightsControllerReal: ServiceBase
{
    private readonly IEnumerable<IInsightsProvider> _insightsProviders;
    private readonly GenWorkPlus<WorkEntities> _workEntities;
    private readonly LazySvc<InsightsDataSourceCache> _dsCache;
    private readonly LazySvc<LicenseCatalog> _licenseCatalog;
    private readonly LazySvc<SystemFingerprint> _fingerprint;
    private readonly Generator<JsonSerializer> _jsonSerializer;
    public const string LogSuffix = "Insight";
    #region Constructor / DI

    public InsightsControllerReal(
        IAppStates appStates, 
        AppCachePurger appCachePurger,
        ILogStoreLive logStore, 
        LazySvc<ILicenseService> licenseServiceLazy, 
        LazySvc<SystemFingerprint> fingerprint,
        LazySvc<LicenseCatalog> licenseCatalog,
        GenWorkPlus<WorkEntities> workEntities,
        IUser user, 
        Generator<JsonSerializer> jsonSerializer,
        LazySvc<InsightsDataSourceCache> dsCache,
        IEnumerable<IInsightsProvider> insightsProviders)
        : base("Api.SysIns")
    {
        ConnectLogs([
            _workEntities = workEntities,
            _appStates = appStates,
            _logStore = logStore,
            _licenseServiceLazy = licenseServiceLazy,
            _fingerprint = fingerprint,
            _licenseCatalog = licenseCatalog,
            _user = user,
            _jsonSerializer = jsonSerializer,
            _dsCache = dsCache,
            AppCachePurger = appCachePurger,
            _insightsProviders = insightsProviders
        ]);
        _logHtml = new(_logStore);
    }
    private readonly IAppStates _appStates;
    private readonly ILogStoreLive _logStore;
    private readonly LazySvc<ILicenseService> _licenseServiceLazy;
    private readonly IUser _user;
    protected readonly AppCachePurger AppCachePurger;

    private InsightsHtmlTable HtmlTableBuilder { get; } = new();
    private readonly InsightsHtmlLog _logHtml;

    #endregion

    private Exception CreateBadRequest(string msg) => HttpException.BadRequest(msg);

    private void ThrowIfNotSystemAdmin()
    {
        if(!_user.IsSystemAdmin) throw HttpException.PermissionDenied("requires Superuser permissions");
    }

    private IAppState AppState(int appId) => _appStates.GetReader(appId);

}