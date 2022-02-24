using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ToSic.Eav.Configuration;
using ToSic.Eav.Plumbing;

namespace ToSic.Eav.WebApi.Admin.Features
{
    public class FeatureControllerReal : WebApiBackendBase<FeatureControllerReal>, IFeatureController
    {
        public const string LogSuffix = "Feats";

        #region Constructor / DI

        public FeatureControllerReal(
            IServiceProvider serviceProvider,
            Lazy<IGlobalConfiguration> globalConfiguration,
            Lazy<IFeaturesInternal> features,
            LazyInitLog<SystemLoader> systemLoaderLazy
            ) : base(serviceProvider, "Bck.Feats")
        {
            _globalConfiguration = globalConfiguration;
            _features = features;
            _systemLoaderLazy = systemLoaderLazy.SetLog(Log);
        }
        private readonly Lazy<IGlobalConfiguration> _globalConfiguration;
        private readonly Lazy<IFeaturesInternal> _features;

        /// <summary>
        /// Must be lazy, to avoid log being filled with sys-loading infos when this service is being used
        /// </summary>
        private readonly LazyInitLog<SystemLoader> _systemLoaderLazy;

        #endregion

        public IEnumerable<FeatureState> List(bool reload)
        {
            if (reload) _systemLoaderLazy.Ready.ReloadFeatures();
            return _features.Value.All;
        }


        // TODO: PROBABLY REMOVE, PROBABLY NOT USED ANY MORE
        //public bool Save(FeaturesDto featuresManagementResponse)
        //{
        //    // first do a validity check 
        //    if (featuresManagementResponse?.Msg?.Features == null) return false;

        //    // 1. valid json? 
        //    // - ensure signature is valid
        //    if (!Json.IsValidJson(featuresManagementResponse.Msg.Features)) return false;

        //    // then take the newFeatures (it should be a json)
        //    // and save to /desktopmodules/.data-custom/configurations/features.json
        //    if (!SaveFeaturesAndReload(featuresManagementResponse.Msg.Features)) return false;

        //    return true;
        //}

        public bool SaveNew(List<FeatureNewDto> featuresManagementResponse)
        {
            // validity check 
            if (featuresManagementResponse == null) return false;

            var featureListStored = FeatureListStoredBuilder(featuresManagementResponse);

            var json = JsonConvert.SerializeObject(featureListStored,
                //JsonSettings.Defaults()
                // reduce datetime serialization precision from 'yyyy-MM-ddTHH:mm:ss.FFFFFFFK'
                new IsoDateTimeConverter() { DateTimeFormat = "yyyy-MM-ddTHH:mm:ss" }
                );

            if (!SaveFeaturesAndReload(json)) return false;

            return true;
        }

        #region Helper Functions

        private FeatureListStored FeatureListStoredBuilder(List<FeatureNewDto> featuresManagementResponse)
        {
            var updatedIds = featuresManagementResponse.Select(f => f.FeatureGuid);

            var currentFeatures = _features.Value.All
                .Where(f => !updatedIds.Contains(f.Guid))
                .Select(FeatureConfigBuilder).ToList();

            var updatedFeatures = featuresManagementResponse
                .Where(f => f.Enabled.HasValue)
                .Select(FeatureConfigBuilder).ToList();

            return new FeatureListStored
            {
                Features = currentFeatures.Union(updatedFeatures).ToList(),
                Fingerprint = _systemLoaderLazy.Ready.Fingerprint.GetFingerprint()
            };
        }

        private static FeatureConfig FeatureConfigBuilder(FeatureState featureState) => 
            new FeatureConfig
            {
                Id = featureState.Guid, 
                Enabled = featureState.Enabled
            };

        private static FeatureConfig FeatureConfigBuilder(FeatureNewDto featureNewDto) =>
            new FeatureConfig
            {
                Id = featureNewDto.FeatureGuid,
                Enabled = featureNewDto.Enabled ?? false
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

                _systemLoaderLazy.Ready.ReloadFeatures();

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
