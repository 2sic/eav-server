using System;
using ToSic.Eav.Apps;
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
        private readonly LazySvc<LicenseCatalog> _licenseCatalog;
        private readonly LazySvc<SystemFingerprint> _fingerprint;
        private readonly Generator<JsonSerializer> _jsonSerializer;
        private readonly Generator<AppRuntime> _appRuntimeGenerator;
        public const string LogSuffix = "Insight";
        #region Constructor / DI

        public InsightsControllerReal(
            IAppStates appStates, 
            SystemManager systemManager,
            ILogStoreLive logStore, 
            LazySvc<ILicenseService> licenseServiceLazy, 
            LazySvc<SystemFingerprint> fingerprint,
            LazySvc<LicenseCatalog> licenseCatalog,
            IUser user, 
            LightSpeedStats lightSpeedStats,
            Generator<AppRuntime> appRuntimeGenerator,
            Generator<JsonSerializer> jsonSerializer)
            : base("Api.SysIns")
        {
            ConnectServices(
                _appStates = appStates,
                _logStore = logStore,
                _licenseServiceLazy = licenseServiceLazy,
                _fingerprint = fingerprint,
                _licenseCatalog = licenseCatalog,
                _user = user,
                _lightSpeedStats = lightSpeedStats,
                _appRuntimeGenerator = appRuntimeGenerator,
                _jsonSerializer = jsonSerializer,
                SystemManager = systemManager
            );
            _logHtml = new InsightsHtmlLog(_logStore);
        }
        private readonly IAppStates _appStates;
        private readonly ILogStoreLive _logStore;
        private readonly LazySvc<ILicenseService> _licenseServiceLazy;
        private readonly IUser _user;
        private readonly LightSpeedStats _lightSpeedStats;
        protected readonly SystemManager SystemManager;

        private InsightsHtmlTable HtmlTableBuilder { get; } = new InsightsHtmlTable();
        private readonly InsightsHtmlLog _logHtml;

        #endregion

        private Exception CreateBadRequest(string msg) => HttpException.BadRequest(msg);

        private void ThrowIfNotSystemAdmin()
        {
            if(!_user.IsSystemAdmin) throw HttpException.PermissionDenied("requires Superuser permissions");
        }


        private AppRuntime AppRt(int? appId) => _appRuntimeGenerator.New().Init(appId.Value);

        private AppState AppState(int? appId) => _appStates.Get(appId.Value);


    }
}
