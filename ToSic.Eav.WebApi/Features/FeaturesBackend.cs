using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ToSic.Eav.Configuration;
using ToSic.Eav.WebApi.Validation;

namespace ToSic.Eav.WebApi.Features
{
    public class FeaturesBackend : WebApiBackendBase<FeaturesBackend>
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
            // validity check 
            if (featuresManagementResponse == null || featuresManagementResponse.Count == 0) return false;

            var featureListStored = FeatureListStoredBuilder(featuresManagementResponse);

            var json = JsonConvert.SerializeObject(featureListStored,
                //JsonSettings.Defaults()
                // reduce datetime serialization precision from 'yyyy-MM-ddTHH:mm:ss.FFFFFFFK'
                new IsoDateTimeConverter() { DateTimeFormat = "yyyy-MM-ddTHH:mm:ss" }
                );

            //var systemLoader = _systemLoaderLazy.Value.Init(Log);
            //systemLoader.Features.Stored = featureListStored;
            //systemLoader.Features.CacheTimestamp = DateTime.Now.Ticks;
            //var appState = systemLoader.AppStateBuilder();
            //var featureConfigurationEntity = systemLoader.FeatureConfigurationEntity(appState);
            //var entity = appState.List.FirstOrDefaultOfType(FeatureConstants.TypeName);
            //var featStr = entity?.Value<string>("Features"/*FeatureConstants.FeaturesField*/);
            //var signature = entity?.Value<string>("Signature"/*FeatureConstants.SignatureField*/);
            //var ser = _serviceProvider.Build<Eav.ImportExport.Json.JsonSerializer>().Init(appState, Log);
            //var json = ser.Serialize(featureConfigurationEntity);

            if (!SaveFeaturesAndReload(json)) return false;

            return true;
        }

        #region Helper Functions

        private FeatureListStored FeatureListStoredBuilder(List<FeatureNewDto> featuresManagementResponse) =>
            new FeatureListStored
            {
                Features = featuresManagementResponse
                    .Where(f => f.Enabled.HasValue)
                    .Select(FeatureConfigBuilder).ToList(),
                Fingerprint = _systemLoaderLazy.Value.Init(Log).Fingerprint.GetFingerprint()
            };

        private static FeatureConfig FeatureConfigBuilder(FeatureNewDto featureNewDto) =>
            new FeatureConfig
            {
                Id = featureNewDto.FeatureGuid,
                Enabled = featureNewDto.Enabled.Value
            };

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
