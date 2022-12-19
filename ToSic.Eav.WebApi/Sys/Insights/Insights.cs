using System;
using ToSic.Eav.Apps;
using ToSic.Eav.Caching;
using ToSic.Eav.Configuration.Licenses;
using ToSic.Eav.Context;
using ToSic.Lib.Logging;
using ToSic.Eav.WebApi.Errors;
using ToSic.Lib.DI;

namespace ToSic.Eav.WebApi.Sys
{
    public partial class InsightsControllerReal: HasLog
    {
        public const string LogSuffix = "Insight";
        #region Constructor / DI

        public InsightsControllerReal(
            IServiceProvider serviceProvider, 
            IAppStates appStates, 
            SystemManager systemManager,
            History logHistory, 
            Lazy<ILicenseService> licenseServiceLazy, 
            IUser user, 
            LightSpeedStats lightSpeedStats)
            : base("Api.SysIns")
        {
            _serviceProvider = serviceProvider;
            _appStates = appStates;
            _logHistory = logHistory;
            _licenseServiceLazy = licenseServiceLazy;
            _user = user;
            _lightSpeedStats = lightSpeedStats;
            SystemManager = systemManager.Init(Log);
        }
        private readonly IServiceProvider _serviceProvider;
        private readonly IAppStates _appStates;
        private readonly History _logHistory;
        private readonly Lazy<ILicenseService> _licenseServiceLazy;
        private readonly IUser _user;
        private readonly LightSpeedStats _lightSpeedStats;
        protected readonly SystemManager SystemManager;

        #endregion

        private Exception CreateBadRequest(string msg) => HttpException.BadRequest(msg);

        private void ThrowIfNotSystemAdmin()
        {
            if(!_user.IsSystemAdmin) throw HttpException.PermissionDenied("requires Superuser permissions");
        }


        private AppRuntime AppRt(int? appId) => _serviceProvider.Build<AppRuntime>().Init(appId.Value, true, Log);

        private AppState AppState(int? appId) => _appStates.Get(appId.Value);


    }
}
