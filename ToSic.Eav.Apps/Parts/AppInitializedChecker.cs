using System;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Logging;
using ToSic.Eav.Logging.Simple;
using ToSic.Eav.Metadata;
using ToSic.Eav.Plumbing;

namespace ToSic.Eav.Apps.Parts
{
    /// <summary>
    /// Lightweight tool to check if an app has everything. If not, it will generate all objects needed to then create what's missing.
    /// </summary>
    public class AppInitializedChecker : HasLog, IAppInitializedChecker
    {
        #region Constructor / DI

        private IServiceProvider ServiceProvider { get; }

        public AppInitializedChecker(IServiceProvider serviceProvider) : base("Eav.AppBld")
        {
            ServiceProvider = serviceProvider;
        }

        #endregion

        /// <inheritdoc />
        public bool EnsureAppConfiguredAndInformIfRefreshNeeded(AppState appIdentity, string appName, ILog parentLog)
        {
            var log = new Log("Eav.AppChk", parentLog);

            var callLog = log.Call<bool>($"..., {appName}");
            if (CheckIfAllPartsExist(appIdentity, out _, out _, out _, log))
                return callLog("ok", false);

            // something is missing, so we must build them
            ServiceProvider.Build<AppInitializer>() // (ServiceProvider)
                .Init(appIdentity, log)
                .InitializeApp(appName);
            return callLog(null, true);
        }

        /// <summary>
        /// Quickly check if the desired content-types already exist or not
        /// </summary>
        /// <remarks>
        /// This should remain static, because it's used 2x
        /// </remarks>
        /// <param name="appIdentity"></param>
        /// <param name="appConfig"></param>
        /// <param name="appResources"></param>
        /// <param name="appSettings"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        internal static bool CheckIfAllPartsExist(
            AppState appIdentity,
            out IEntity appConfig,
            out IEntity appResources,
            out IEntity appSettings,
            ILog log)
        {
            var callLogFindParts = log.Call<bool>();
            appConfig = appIdentity.GetMetadata((int)TargetTypes.App, appIdentity.AppId, AppLoadConstants.TypeAppConfig).FirstOrDefault();
            appResources = appIdentity.GetMetadata((int)TargetTypes.App, appIdentity.AppId, AppLoadConstants.TypeAppResources).FirstOrDefault();
            appSettings = appIdentity.GetMetadata((int)TargetTypes.App, appIdentity.AppId, AppLoadConstants.TypeAppSettings).FirstOrDefault();


            // if nothing must be done, return now
            if (appConfig != null && appResources != null && appSettings != null)
                return callLogFindParts("all ok", true);

            log.Add($"App Config: {appConfig != null}, Resources: {appResources != null}, Settings: {appSettings != null}");

            return callLogFindParts("some missing", false);
        }

    }
}
