﻿using System;
using System.Linq;
using ToSic.Eav.Data;
using ToSic.Eav.Logging;
using ToSic.Eav.Logging.Simple;

namespace ToSic.Eav.Apps.Parts
{
    /// <summary>
    /// Lightweight tool to check if an app has everything. If not, it will generate all objects needed to then create what's missing.
    /// </summary>
    public class AppInitializedChecker : HasLog
    {
        #region Constructor / DI

        private IServiceProvider ServiceProvider { get; }

        public AppInitializedChecker(IServiceProvider serviceProvider) : base("Eav.AppBld")
        {
            ServiceProvider = serviceProvider;
        }

        #endregion

        /// <summary>
        /// Will quickly check if the app is initialized. It uses the App-State to do this.
        /// If it's not configured yet, it will trigger automatic
        /// </summary>
        /// <param name="appIdentity"></param>
        /// <param name="appName"></param>
        /// <param name="parentLog"></param>
        /// <returns></returns>
        public bool EnsureAppConfiguredAndInformIfRefreshNeeded(IAppIdentity appIdentity, string appName, ILog parentLog)
        {
            var log = new Log("Eav.AppChk", parentLog);
            var callLog = log.Call<bool>($"..., {appName}");
            if (CheckIfAllPartsExist(appIdentity, out _, out _, out _, log))
                return callLog("ok", false);

            // something is missing, so we must build them
            new AppInitializer(ServiceProvider)
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
            IAppIdentity appIdentity,
            out IEntity appConfig,
            out IEntity appResources,
            out IEntity appSettings,
            ILog log)
        {
            var mds = State.Get(appIdentity);
            var callLogFindParts = log.Call<bool>();
            appConfig = mds.Get(Constants.MetadataForApp, appIdentity.AppId, AppConstants.TypeAppConfig).FirstOrDefault();
            appResources = mds.Get(Constants.MetadataForApp, appIdentity.AppId, AppConstants.TypeAppResources).FirstOrDefault();
            appSettings = mds.Get(Constants.MetadataForApp, appIdentity.AppId, AppConstants.TypeAppSettings).FirstOrDefault();


            // if nothing must be done, return now
            if (appConfig != null && appResources != null && appSettings != null)
                return callLogFindParts("all ok", true);

            log.Add($"App Config: {appConfig != null}, Resources: {appResources != null}, Settings: {appSettings != null}");

            return callLogFindParts("some missing", false);
        }

    }
}
