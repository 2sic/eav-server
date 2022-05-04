using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ToSic.Eav.Logging;

namespace ToSic.Eav.Configuration
{
    public class FeatureConfigManager : HasLog
    {
        private readonly Lazy<IGlobalConfiguration> _globalConfiguration;

        public FeatureConfigManager(Lazy<IGlobalConfiguration> globalConfiguration) : base("FeatCfgMng")
        {
            _globalConfiguration = globalConfiguration;
        }

        /// <summary>
        /// Return content from 'features.json' file.
        /// Also return full path to 'features.json'.
        /// </summary>
        /// <returns>file path and content as string</returns>
        internal (string filePath, string fileContent) LoadFeaturesFile()
        {
            // folder with "features.json"
            var configurationsPath = Path.Combine(_globalConfiguration.Value.GlobalFolder, Constants.FolderDataCustom, FsDataConstants.ConfigFolder);

            // ensure that path to store files already exits
            Directory.CreateDirectory(configurationsPath);

            var featureFilePath = Path.Combine(configurationsPath, FeatureConstants.FeaturesJson);

            return File.Exists(featureFilePath) ? (featureFilePath, File.ReadAllText(featureFilePath)) : (null, null);
        }


        /// <summary>
        /// Handle old 'features.json' format.
        /// - detect old format
        /// - backup old 'features.json'
        /// - save 'features.json' in new format
        /// - return features stored
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="fileContent"></param>
        /// <returns>features stored</returns>
        internal FeatureListStored ConvertOldFeaturesFile(string filePath, string fileContent)
        {
            var fileJson = JObject.Parse(fileContent);

            // check json format in "features.json" to find is it old version (v12)
            if (fileJson["_"]?["V"] != null && (int)fileJson["_"]["V"] == 1) // detect old "features.json" format (v12)
            {
                // rename old file format "features.json" to "features.json.v12.bak"
                var oldFeatureFilePathForBackup = $"{filePath}.v12.bak";
                if (File.Exists(oldFeatureFilePathForBackup)) File.Delete(oldFeatureFilePathForBackup);
                File.Move(filePath, oldFeatureFilePathForBackup);

                // get stored features from old format
                var stored = GetFeaturesFromOldFormat(fileJson);

                // save features in new format
                SaveFeaturesNew(stored);

                return stored;
            }
            return null;
        }


        /// <summary>
        /// Get features from json old format (v12)
        /// </summary>
        /// <param name="json"></param>
        private FeatureListStored GetFeaturesFromOldFormat(JObject json)
        {
            var features = new FeatureListStored();

            var fs = (string)json["Entity"]["Attributes"]["Custom"]["Features"]["*"];
            var oldFeatures = JObject.Parse(fs);

            features.Fingerprint = (string)oldFeatures["fingerprint"];

            foreach (var f in (JArray)oldFeatures["features"])
            {
                features.Features.Add(new FeatureConfig()
                {
                    Id = (Guid)f["id"],
                    Enabled = (bool)f["enabled"],
                    Expires = (DateTime)f["expires"],
                });
            }

            return features;
        }


        /// <summary>
        /// Save new features config in "features.json".
        /// </summary>
        private bool SaveFeaturesNew(FeatureListStored features)
        {
            var wrapLog = Log.Call<bool>($"f:{features?.Features?.Count ?? -1}");

            try
            {
                // save new format (v13)
                var fileContent = JsonConvert.SerializeObject(features,
                    //JsonSettings.Defaults()
                    // reduce datetime serialization precision from 'yyyy-MM-ddTHH:mm:ss.FFFFFFFK'
                    new IsoDateTimeConverter() { DateTimeFormat = "yyyy-MM-ddTHH:mm:ss" });

                var configurationsPath = Path.Combine(_globalConfiguration.Value.GlobalFolder, Constants.FolderDataCustom, FsDataConstants.ConfigFolder);

                // ensure that path to store files already exits
                Directory.CreateDirectory(configurationsPath);

                var filePath = Path.Combine(configurationsPath, FeatureConstants.FeaturesJson);

                return wrapLog("features new saved:", SaveFile(filePath, fileContent));
            }
            catch (Exception e)
            {
                Log.Exception(e);
                return wrapLog("save features failed:" + e.Message, false);
            }
        }


        /// <summary>
        /// Update existing features config in "features.json". 
        /// </summary>
        private bool SaveFile(string filePath, string fileContent)
        {
            var wrapLog = Log.Call<bool>($"fp={filePath}");

            try
            {
                File.WriteAllText(filePath, fileContent);
                return wrapLog("ok, file saved", true);
            }
            catch (Exception e)
            {
                Log.Exception(e);
                return wrapLog("save file failed:" + e.Message, false);
            }
        }


        /// <summary>
        /// Update existing features config and save. 
        /// </summary>
        internal bool SaveFeaturesUpdate(List<FeatureManagementChange> changes)
        {
            var wrapLog = Log.Call<bool>($"c:{changes?.Count ?? -1}");

            try
            {
                var (filePath, fileContent) = LoadFeaturesFile();
                var fileJson = JObject.Parse(fileContent);
                foreach (var change in changes)
                {
                    var feature = ((JArray)fileJson["features"]).FirstOrDefault(f => (Guid)f["id"] == change.FeatureGuid);
                    if (feature == null) // insert
                    {
                        var featureConfig = JsonConvert.SerializeObject(FeatureConfigBuilder(change),
                            //JsonSettings.Defaults()
                            // reduce datetime serialization precision from 'yyyy-MM-ddTHH:mm:ss.FFFFFFFK'
                            new IsoDateTimeConverter() { DateTimeFormat = "yyyy-MM-ddTHH:mm:ss" });
                        ((JArray)fileJson["features"]).Add(JObject.Parse(featureConfig));
                    }
                    else
                    {
                        if (change.Enabled.HasValue) // update
                            feature["enabled"] = change.Enabled.Value;
                        else // delete
                            feature.Remove();
                    }
                }

                return wrapLog("features saved:", SaveFile(filePath, fileJson.ToString()));
            }
            catch (Exception e)
            {
                Log.Exception(e);
                return wrapLog("save features failed:" + e.Message, false);
            }
        }


        internal static FeatureConfig FeatureConfigBuilder(FeatureState featureState) =>
            new FeatureConfig
            {
                Id = featureState.Guid,
                Enabled = featureState.Enabled
            };

        
        internal static FeatureConfig FeatureConfigBuilder(FeatureManagementChange change) =>
            new FeatureConfig
            {
                Id = change.FeatureGuid,
                Enabled = change.Enabled ?? false
            };
    }
}
