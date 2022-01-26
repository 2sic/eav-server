﻿using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using ToSic.Eav.Configuration;
using ToSic.Eav.WebApi.Validation;

namespace ToSic.Eav.WebApi.Features
{
    public class FeaturesBackend: WebApiBackendBase<FeaturesBackend>
    {
        #region Constructor / DI

        public FeaturesBackend(
            IServiceProvider serviceProvider,
            Lazy<IGlobalConfiguration> globalConfiguration,
            Lazy<IFeaturesInternal> features, 
            Lazy<SystemLoader> systemLoaderLazy
            ) : base(serviceProvider, "Bck.Feats")
        {
            _globalConfiguration = globalConfiguration;
            _features = features;
            _systemLoaderLazy = systemLoaderLazy;
        }

        private readonly Lazy<IGlobalConfiguration> _globalConfiguration;
        private readonly Lazy<IFeaturesInternal> _features;

        /// <summary>
        /// Must be lazy, to avoid log being filled with sys-loading infos when this service is being used
        /// </summary>
        private readonly Lazy<SystemLoader> _systemLoaderLazy;

        #endregion

        public IEnumerable<FeatureState> GetAll(bool reload)
        {
            if (reload) _systemLoaderLazy.Value.Init(Log).ReloadFeatures();
            return _features.Value.All;
        }


        public bool SaveFeatures(FeaturesDto featuresManagementResponse)
        {
            // first do a validity check 
            if (featuresManagementResponse?.Msg?.Features == null) return false;

            // 1. valid json? 
            // - ensure signature is valid
            if (!Json.IsValidJson(featuresManagementResponse.Msg.Features)) return false;

            // then take the newFeatures (it should be a json)
            // and save to /desktopmodules/.data-custom/configurations/features.json
            if (!SaveFeaturesAndReload(featuresManagementResponse.Msg.Features)) return false;

            return true;
        }

        public bool SaveNewFeatures(List<FeatureNewDto> featuresManagementResponse)
        {
            var json = JsonConvert.SerializeObject(featuresManagementResponse);

            if (!SaveFeaturesAndReload(json)) return false;

            return true;
        }


        #region Helper Functions

        private bool SaveFeaturesAndReload(string features)
        {
            var wrapLog = Log.Call<bool>();
            try
            {
                var configurationsPath = Path.Combine(_globalConfiguration.Value.GlobalFolder, Constants.FolderDataCustom, FsDataConstants.ConfigFolder);

                if (!Directory.Exists(configurationsPath)) 
                    Directory.CreateDirectory(configurationsPath);

                var featureFilePath = Path.Combine(configurationsPath, FeatureConstants.FeaturesJson);

                File.WriteAllText(featureFilePath, features);

                _systemLoaderLazy.Value.Init(Log).ReloadFeatures();
                return wrapLog("ok", true);
            }
            catch (Exception ex)
            {
                Log.Exception(ex);
                return wrapLog("error", false);
            }
        }


        #endregion
    }
}
