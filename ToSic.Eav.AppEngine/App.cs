﻿using System;
using ToSic.Eav.Documentation;
using ToSic.Eav.Logging;

namespace ToSic.Eav.Apps
{
    /// <summary>
    /// A <em>single-use</em> app-object providing quick simple api to access
    /// name, folder, data, metadata etc.
    /// </summary>
    [PublicApi_Stable_ForUseInYourCode]
    public partial class App: AppBase, IApp
    {
        [PrivateApi]
        public const int AutoLookupZone = -1;

        /// <inheritdoc />
        public string Name { get; private set; }
        /// <inheritdoc />
        public string Folder { get; private set; }
        /// <inheritdoc />
        public bool Hidden { get; private set; }

        /// <inheritdoc />
        public string AppGuid { get; }

        /// <inheritdoc />
        public bool ShowDrafts { get; private set; }
        /// <inheritdoc />
        public bool EnablePublishing { get; private set; }

        [PrivateApi]
        protected const string IconFile = "/" + AppConstants.AppIconFile;

        internal App(int zoneId, 
            int appId, 
            bool allowSideEffects,
            Func<App, IAppDataConfiguration> buildConfiguration,
            ILog parentLog, 
            string logMsg)
            // first, initialize the AppIdentity and log it's use
            : base(zoneId, appId, new CodeRef(),  parentLog, "App.2sxcAp", $"prep App z#{zoneId}, a#{appId}, allowSE:{allowSideEffects}, hasDataConfig:{buildConfiguration != null}, {logMsg}")
        {
            // if zone is missing, try to find it; if still missing, throw error
            if (zoneId == AutoLookupZone) throw new Exception("Cannot find zone-id for portal specified");

            // Look up name in cache
            var cache = /*Factory.GetAppsCache*/Eav.Apps.State.Cache;//.Resolve<IAppsCache>();
            AppState = cache.Get(this); // for metadata

            AppGuid = cache.Zones[zoneId].Apps[appId];

            // v10.25 from now on the DefaultApp can also have settings and resources
            // v10.25.02 reactivated this protection, because it causes side-effects. On content-app, let's only do this if people start editing the resources...?
            // note that on imported apps, this would automatically work, as those would already have these things
            if (AppGuid != Constants.DefaultAppName)
            {
                // if it's a real App (not content/default), do more
                Log.Add($"create app resources? allowSE:{allowSideEffects}");

                if (allowSideEffects)
                    new AppManager(this, Log).EnsureAppIsConfigured(/*ZoneId, AppId, Log*/); // make sure additional settings etc. exist
            }

            InitializeResourcesSettingsAndMetadata();

            // do this after initializing resources to certainly set the content-name
            if(AppGuid == Constants.DefaultAppName)
                Name = Folder = Constants.ContentAppName;

            // for deferred initialization as needed
            _dataConfigurationBuilder = buildConfiguration;
        }
    }
}
