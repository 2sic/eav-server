using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using ToSic.Eav.Apps;
using ToSic.Eav.Caching;
using ToSic.Eav.Data;
using ToSic.Eav.Documentation;
using ToSic.Eav.Logging;
using ToSic.Eav.Logging.Simple;
using ToSic.Eav.Run;
using ToSic.Eav.Security.Encryption;
using ToSic.Eav.Types;

namespace ToSic.Eav.Configuration
{
    [PrivateApi]
    public class SystemLoader: HasLog
    {

        #region Constructor / DI

        public SystemLoader(GlobalTypeLoader typeLoader, IFingerprint fingerprint, IRuntime runtime, IAppsCache appsCache, IFeaturesInternal features, LogHistory logHistory) : base($"{LogNames.Eav}SysLdr")
        {
            _appsCache = appsCache;
            _features = features;
            _logHistory = logHistory;
            logHistory.Add(GlobalTypes.LogHistoryGlobalTypes, Log);
            _typeLoader = typeLoader.Init(Log);
            _fingerprint = fingerprint;
            _runtime = runtime;

            if (Features.FeaturesFromDI == null)
                Features.FeaturesFromDI = features;
        }

        private readonly GlobalTypeLoader _typeLoader;
        private readonly IFingerprint _fingerprint;
        private readonly IRuntime _runtime;
        private readonly IAppsCache _appsCache;
        private readonly IFeaturesInternal _features;
        private readonly LogHistory _logHistory;

        #endregion

        /// <summary>
        /// Do things needed at application start
        /// </summary>
        public void StartUp()
        {
            // Prevent multiple Inits
            if (_startupAlreadyRan) throw new Exception("Startup should never be called twice.");
            _startupAlreadyRan = true;

            // Initialize AppState Cache, which could be DI but has a static accessor
#pragma warning disable 618
            State.StartUp(_appsCache);
#pragma warning restore 618

            // Build the cache of all system-types. Must happen before everything else
            _typeLoader.BuildCache();
            //Types.Global.TypeLoader = _typeLoader;
            Types.GlobalTypes.TypeLoader = _typeLoader;

            // Now do a normal reload of configuration and features
            Reload();
        }

        private bool _startupAlreadyRan;

        /// <summary>
        /// Reset the features to force reloading of the features
        /// </summary>
        [PrivateApi]
        public void Reload()
        {
            // Reset global data which stores the features
            LoadRuntimeConfiguration();
            LoadFeatures();
        }

        /// <summary>
        /// All content-types available in Reflection; will cache on the Global.List after first scan
        /// </summary>
        /// <returns></returns>
        public void LoadRuntimeConfiguration()
        {
            var log = new Log($"{LogNames.Eav}.Global");
            log.Add("Load Global Configurations");
            _logHistory.Add(Types.GlobalTypes.LogHistoryGlobalTypes, log);
            var wrapLog = log.Call();

            try
            {
                var runtime = _runtime.Init(log);
                var list = runtime?.LoadGlobalItems(Global.GroupConfiguration)?.ToList() ?? new List<IEntity>();
                Global.List = list;
                wrapLog($"{list.Count}");
            }
            catch (Exception e)
            {
                log.Exception(e);
                Global.List = new List<IEntity>();
                wrapLog("error");
            }
        }


        private void LoadFeatures()
        {
            FeatureListWithFingerprint feats = null;
            try
            {
                var entity = Global.For(Features.TypeName);
                var featStr = entity?.Value<string>(Features.FeaturesField);
                var signature = entity?.Value<string>(Features.SignatureField);

                // Verify signature from security-system
                if (!string.IsNullOrWhiteSpace(featStr))
                {
                    if (!string.IsNullOrWhiteSpace(signature))
                    {
                        try
                        {
                            var data = new UnicodeEncoding().GetBytes(featStr);
                            FeaturesService.ValidInternal = Sha256.VerifyBase64(Features.FeaturesValidationSignature2Sxc930, signature, data);
                        }
                        catch { /* ignore */ }
                    }

                    // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                    if (FeaturesService.ValidInternal || Features.AllowUnsignedFeatures)
                    {
                        FeatureListWithFingerprint feats2 = null;
                        if (featStr.StartsWith("{"))
                            feats2 = JsonConvert.DeserializeObject<FeatureListWithFingerprint>(featStr);

                        if (feats2 != null)
                        {
                            var fingerprint = feats2.Fingerprint;
                            if (fingerprint != _fingerprint.GetSystemFingerprint()) 
                                FeaturesService.ValidInternal = false;

                            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                            if (FeaturesService.ValidInternal || Features.AllowUnsignedFeatures)
                                feats = feats2;
                        }
                    }
                }
            }
            catch { /* ignore */ }
            //Features.CacheTimestamp = DateTime.Now.Ticks;
            //Features.Stored = feats ?? new FeatureList();
            _features.Stored = feats ?? new FeatureList();
            _features.CacheTimestamp = DateTime.Now.Ticks;
        }


    }
}
