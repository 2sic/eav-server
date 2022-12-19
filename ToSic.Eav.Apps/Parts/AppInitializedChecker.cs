using System.Linq;
using ToSic.Eav.Data;
using ToSic.Lib.Logging;
using ToSic.Eav.Metadata;
using ToSic.Lib.DI;
using ToSic.Lib.Services;

namespace ToSic.Eav.Apps.Parts
{
    /// <summary>
    /// Lightweight tool to check if an app has everything. If not, it will generate all objects needed to then create what's missing.
    /// </summary>
    public class AppInitializedChecker : ServiceBase, IAppInitializedChecker
    {
        private readonly Generator<AppInitializer> _appInitGenerator;

        #region Constructor / DI

        public AppInitializedChecker(Generator<AppInitializer> appInitGenerator) : base("Eav.AppBld") 
            => ConnectServices(_appInitGenerator = appInitGenerator);

        #endregion

        /// <inheritdoc />
        public bool EnsureAppConfiguredAndInformIfRefreshNeeded(AppState appIdentity, string appName, ILog parentLog)
        {
            var log = new Log("Eav.AppChk", parentLog);

            var callLog = log.Fn<bool>($"..., {appName}");
            if (CheckIfAllPartsExist(appIdentity, out _, out _, out _, log))
                return callLog.ReturnFalse("ok");

            // something is missing, so we must build them
            _appInitGenerator.New()
                .Init(appIdentity)
                .InitializeApp(appName);
            return callLog.ReturnTrue();
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
            var callLogFindParts = log.Fn<bool>();
            appConfig = appIdentity.GetMetadata(TargetTypes.App, appIdentity.AppId, AppLoadConstants.TypeAppConfig).FirstOrDefault();
            appResources = appIdentity.GetMetadata(TargetTypes.App, appIdentity.AppId, AppLoadConstants.TypeAppResources).FirstOrDefault();
            appSettings = appIdentity.GetMetadata(TargetTypes.App, appIdentity.AppId, AppLoadConstants.TypeAppSettings).FirstOrDefault();


            // if nothing must be done, return now
            if (appConfig != null && appResources != null && appSettings != null)
                return callLogFindParts.ReturnTrue("all ok");

            log.A($"App Config: {appConfig != null}, Resources: {appResources != null}, Settings: {appSettings != null}");

            return callLogFindParts.ReturnFalse("some missing");
        }

    }
}
