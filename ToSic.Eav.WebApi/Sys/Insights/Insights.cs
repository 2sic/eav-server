using System;
using ToSic.Eav.Apps;
using ToSic.Eav.Caching;
using ToSic.Eav.Configuration.Licenses;
using ToSic.Eav.Context;
using ToSic.Lib.Logging;
using ToSic.Eav.WebApi.Errors;
using ToSic.Lib.DI;
using ToSic.Lib.Services;

namespace ToSic.Eav.WebApi.Sys
{
    public partial class InsightsControllerReal: ServiceBase
    {
        public const string LogSuffix = "Insight";
        #region Constructor / DI

        public InsightsControllerReal(
            IServiceProvider serviceProvider, 
            IAppStates appStates, 
            SystemManager systemManager,
            ILogStoreLive logStore, 
            LazySvc<ILicenseService> licenseServiceLazy, 
            IUser user, 
            LightSpeedStats lightSpeedStats)
            : base("Api.SysIns")
        {
            ConnectServices(
                _serviceProvider = serviceProvider,
                _appStates = appStates,
                _logStore = logStore,
                _licenseServiceLazy = licenseServiceLazy,
                _user = user,
                _lightSpeedStats = lightSpeedStats,
                SystemManager = systemManager
            );
        }
        private readonly IServiceProvider _serviceProvider;
        private readonly IAppStates _appStates;
        private readonly ILogStoreLive _logStore;
        private readonly LazySvc<ILicenseService> _licenseServiceLazy;
        private readonly IUser _user;
        private readonly LightSpeedStats _lightSpeedStats;
        protected readonly SystemManager SystemManager;

        #endregion

        private Exception CreateBadRequest(string msg) => HttpException.BadRequest(msg);

        private void ThrowIfNotSystemAdmin()
        {
            if(!_user.IsSystemAdmin) throw HttpException.PermissionDenied("requires Superuser permissions");
        }


        private AppRuntime AppRt(int? appId) => _serviceProvider.Build<AppRuntime>().Init(Log).Init(appId.Value, true);

        private AppState AppState(int? appId) => _appStates.Get(appId.Value);


    }
}
