using ToSic.Eav.Apps.Internal;
using ToSic.Eav.Apps.Internal.Insights;
using ToSic.Eav.Context;
using ToSic.Eav.Internal.Licenses;
using ToSic.Eav.Security.Fingerprint;
using ToSic.Eav.WebApi.Errors;
using JsonSerializer = ToSic.Eav.ImportExport.Json.JsonSerializer;

namespace ToSic.Eav.WebApi.Sys.Insights;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public partial class InsightsControllerReal(
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
    IEnumerable<IInsightsProvider> insightsProviders
    )
    : ServiceBase("Api.SysIns",
        connect:
        [
            workEntities, appStates, logStore, licenseServiceLazy, fingerprint, licenseCatalog, user, jsonSerializer,
            dsCache, appCachePurger, insightsProviders
        ])
{
    public const string LogSuffix = "Insight";
    #region Constructor / DI

    protected readonly AppCachePurger AppCachePurger = appCachePurger;

    private InsightsHtmlTable HtmlTableBuilder { get; } = new();

    #endregion

    private static Exception CreateBadRequest(string msg) => HttpException.BadRequest(msg);

    private void ThrowIfNotSystemAdmin()
    {
        if (!user.IsSystemAdmin) throw HttpException.PermissionDenied("requires Superuser permissions");
    }

    private IAppState AppState(int appId) => appStates.GetReader(appId);

}