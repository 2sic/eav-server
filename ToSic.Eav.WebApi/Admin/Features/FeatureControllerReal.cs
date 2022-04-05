using System;
using System.Collections.Generic;
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
            Lazy<IFeaturesInternal> features,
            LazyInitLog<SystemLoader> systemLoaderLazy
            ) : base(serviceProvider, "Bck.Feats")
        {
            _features = features;
            _systemLoaderLazy = systemLoaderLazy.SetLog(Log);
        }
        private readonly Lazy<IFeaturesInternal> _features;

        /// <summary>
        /// Must be lazy, to avoid log being filled with sys-loading infos when this service is being used
        /// </summary>
        private readonly LazyInitLog<SystemLoader> _systemLoaderLazy;

        #endregion

        public bool SaveNew(List<FeatureNewDto> featuresManagementResponse)
        {
            // validity check 
            if (featuresManagementResponse == null) return false;

            var featureListStored = FeatureListStoredBuilder(featuresManagementResponse);

            return SaveFeaturesAndReload(featureListStored);
        }

        #region Helper Functions

        private FeatureListStored FeatureListStoredBuilder(List<FeatureNewDto> featuresManagementResponse)
        {
            var updatedIds = featuresManagementResponse.Select(f => f.FeatureGuid);

            var storedFeaturesButNotUpdated = _features.Value.All
                .Where(f => f.EnabledStored.HasValue && !updatedIds.Contains(f.Guid))
                .Select(FeatureConfigBuilder).ToList();

            var updatedFeatures = featuresManagementResponse
                .Where(f => f.Enabled.HasValue)
                .Select(FeatureConfigBuilder).ToList();

            return new FeatureListStored
            {
                Features = storedFeaturesButNotUpdated.Union(updatedFeatures).ToList(),
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

        private bool SaveFeaturesAndReload(FeatureListStored features)
        {
            var wrapLog = Log.Call<bool>();
            try
            {
                var sl = _systemLoaderLazy.Ready;
                
                sl.SaveFeatures(features);
                sl.ReloadFeatures();

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
