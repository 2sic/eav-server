///*
// * Copyright 2022 by 2sic internet solutions in Switzerland - www.2sic.com
// *
// * This file and the code IS COPYRIGHTED.
// * 1. You may not change it.
// * 2. You may not copy the code to reuse in another way.
// *
// * Copying this or creating a similar service, 
// * especially when used to circumvent licensing features in EAV and 2sxc
// * is a copyright infringement.
// *
// * Please remember that 2sic has sponsored more than 10 years of work,
// * and paid more than 1 Million USD in wages for its development.
// * So asking for support to finance advanced features is not asking for much. 
// *
// */
//using System;
//using System.Text;
//using Newtonsoft.Json;
//using ToSic.Eav.Apps;
//using ToSic.Eav.Data;
//using ToSic.Eav.Documentation;
//using ToSic.Eav.Logging;
//using ToSic.Eav.Security.Encryption;

//namespace ToSic.Eav.Configuration
//{
//    internal class FeaturesLoader: LoaderBase
//    {
//        /// <summary>
//        /// Constructor - not meant for DI
//        /// </summary>
//        internal FeaturesLoader(LogHistory logHistory, ILog parentLog) 
//            : base(logHistory, parentLog, LogNames.Eav + "LicLdr", "Load Features")
//        {
//        }


//        /// <summary>
//        /// Pre-Load enabled / disabled global features
//        /// </summary>
//        [PrivateApi]
//        internal FeatureListStored LoadFeatures(AppState presetApp, string fingerprint)
//        {
//            var wrapLog = Log.Call<FeatureListStored>();
//            FeatureListStored feats = null;
//            try
//            {
//                var entity = presetApp.List.FirstOrDefaultOfType(FeatureConstants.TypeName);
//                var featStr = entity?.Value<string>(FeatureConstants.FeaturesField);
//                var signature = entity?.Value<string>(FeatureConstants.SignatureField);

//                // Verify signature from security-system
//                if (!string.IsNullOrWhiteSpace(featStr))
//                {
//                    if (!string.IsNullOrWhiteSpace(signature))
//                        try
//                        {
//                            var data = new UnicodeEncoding().GetBytes(featStr);
//                            FeaturesService.ValidInternal =
//                                new Sha256().VerifyBase64(FeatureConstants.FeaturesValidationSignature2Sxc930,
//                                    signature, data);
//                        }
//                        catch (Exception ex)
//                        {
//                            // Just log, and ignore
//                            Log.Exception(ex);
//                        }

//                    // ReSharper disable once ConditionIsAlwaysTrueOrFalse
//                    if (FeaturesService.ValidInternal || FeatureConstants.AllowUnsignedFeatures)
//                    {
//                        var feats2 = featStr.StartsWith("{")
//                            ? JsonConvert.DeserializeObject<FeatureListStored>(featStr)
//                            : null;

//                        if (feats2 != null)
//                        {
//                            if (feats2.Fingerprint != fingerprint)
//                                FeaturesService.ValidInternal = false;

//                            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
//                            if (FeaturesService.ValidInternal || FeatureConstants.AllowUnsignedFeatures)
//                                feats = feats2;
//                        }
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                // Just log and ignore
//                Log.Exception(ex);
//            }

//            return wrapLog("ok", feats ?? new FeatureListStored());
//        }
//    }
//}
