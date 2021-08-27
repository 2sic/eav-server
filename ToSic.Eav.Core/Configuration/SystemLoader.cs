using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
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

        public SystemLoader(GlobalTypeLoader typeLoader, IFingerprint fingerprint, IRuntime runtime) : base($"{LogNames.Eav}SysLdr")
        {
            History.Add(Types.Global.LogHistoryGlobalTypes, Log);
            TypeLoader = typeLoader.Init(Log);
            Fingerprint = fingerprint;
            Runtime = runtime;
        }

        public GlobalTypeLoader TypeLoader { get; }
        public IFingerprint Fingerprint { get; }
        public IRuntime Runtime { get; }

        #endregion

        /// <summary>
        /// Do things needed at application start
        /// </summary>
        public void StartUp()
        {
            if (_startupAlreadyRan) throw new Exception("Startup should never be called twice.");
            _startupAlreadyRan = true;

            // Build the cache of all system-types. Must happen before everything else
            TypeLoader.BuildCache();
            Types.Global.TypeLoader = TypeLoader;

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
            History.Add(Types.Global.LogHistoryGlobalTypes, log);
            var wrapLog = log.Call();

            try
            {
                var runtime = Runtime.Init(log);
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
                            Features.Valid = Sha256.VerifyBase64(Features.FeaturesValidationSignature2Sxc930, signature, data);
                        }
                        catch { /* ignore */ }
                    }

                    // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                    if (Features.Valid || Features.AllowUnsignedFeatures)
                    {
                        FeatureListWithFingerprint feats2 = null;
                        if (featStr.StartsWith("{"))
                            feats2 = JsonConvert.DeserializeObject<FeatureListWithFingerprint>(featStr);

                        if (feats2 != null)
                        {
                            var fingerprint = feats2.Fingerprint;
                            if (fingerprint != Fingerprint.GetSystemFingerprint()) 
                                Features.Valid = false;

                            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                            if (Features.Valid || Features.AllowUnsignedFeatures)
                                feats = feats2;
                        }
                    }
                }
            }
            catch { /* ignore */ }
            Features.CacheTimestamp = DateTime.Now.Ticks;
            Features.Stored = feats ?? new FeatureList();
        }


    }
}
