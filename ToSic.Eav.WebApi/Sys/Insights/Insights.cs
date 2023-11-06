using System;
using ToSic.Eav.Apps;
using ToSic.Eav.Apps.Work;
using ToSic.Eav.Caching;
using ToSic.Eav.Configuration.Licenses;
using ToSic.Eav.Context;
using ToSic.Eav.Security.Fingerprint;
using ToSic.Eav.WebApi.Errors;
using ToSic.Lib.DI;
using ToSic.Lib.Logging;
using ToSic.Lib.Services;
using JsonSerializer = ToSic.Eav.ImportExport.Json.JsonSerializer;

namespace ToSic.Eav.WebApi.Sys.Insights
{
    public partial class InsightsControllerReal: ServiceBase
    {
        private readonly GenWorkPlus<WorkEntities> _workEntities;
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
            LightSpeedStats lightSpeedStats,
            Generator<JsonSerializer> jsonSerializer)
            : base("Api.SysIns")
        {
            ConnectServices(
                _workEntities = workEntities,
                _appStates = appStates,
                _logStore = logStore,
                _licenseServiceLazy = licenseServiceLazy,
                _fingerprint = fingerprint,
                _licenseCatalog = licenseCatalog,
                _user = user,
                _lightSpeedStats = lightSpeedStats,
                _jsonSerializer = jsonSerializer,
                AppCachePurger = appCachePurger
            );
            _logHtml = new InsightsHtmlLog(_logStore);
        }
        private readonly IAppStates _appStates;
        private readonly ILogStoreLive _logStore;
        private readonly LazySvc<ILicenseService> _licenseServiceLazy;
        private readonly IUser _user;
        private readonly LightSpeedStats _lightSpeedStats;
        protected readonly AppCachePurger AppCachePurger;

        private InsightsHtmlTable HtmlTableBuilder { get; } = new InsightsHtmlTable();
        private readonly InsightsHtmlLog _logHtml;

        #endregion

        private Exception CreateBadRequest(string msg) => HttpException.BadRequest(msg);

        private void ThrowIfNotSystemAdmin()
        {
            if(!_user.IsSystemAdmin) throw HttpException.PermissionDenied("requires Superuser permissions");
        }

        private AppState AppState(int? appId) => _appStates.Get(appId.Value);
    }
}
