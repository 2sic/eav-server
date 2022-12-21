using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;
using ToSic.Lib.Logging;
using ToSic.Eav.Security.Fingerprint;
using ToSic.Eav.Serialization;
using ToSic.Lib.DI;
using ToSic.Lib.Services;

namespace ToSic.Eav.Configuration
{
    public class FeatureConfigManager : ServiceBase
    {
        private readonly LazySvc<IGlobalConfiguration> _globalConfiguration;
        private readonly LazySvc<SystemFingerprint> _fingerprint;

        public FeatureConfigManager(LazySvc<IGlobalConfiguration> globalConfiguration, LazySvc<SystemFingerprint> fingerprint) : base("FeatCfgMng")
        {
            ConnectServices(
                _globalConfiguration = globalConfiguration,
                _fingerprint = fingerprint
            );
        }

        /// <summary>
        /// Return content from 'features.json' file.
        /// Also return full path to 'features.json'.
        /// </summary>
        /// <returns>file path and content as string</returns>
        internal (string filePath, string fileContent) LoadFeaturesFile()
        {
            // folder with "features.json"
            var configurationsPath = _globalConfiguration.Value.ConfigFolder;

            // ensure that path to store files already exits
            Directory.CreateDirectory(configurationsPath);

            var featureFilePath = Path.Combine(configurationsPath, FeatureConstants.FeaturesJson);

            return File.Exists(featureFilePath) ? (featureFilePath, File.ReadAllText(featureFilePath)) : (featureFilePath, null);
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
            var fileJson = JsonNode.Parse(fileContent, JsonOptions.JsonNodeDefaultOptions, JsonOptions.JsonDocumentDefaultOptions).AsObject();

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
        private FeatureListStored GetFeaturesFromOldFormat(JsonObject json)
        {
            var features = new FeatureListStored();

            var fs = (string)json["Entity"]["Attributes"]["Custom"]["Features"]["*"];
            var oldFeatures = JsonNode.Parse(fs, JsonOptions.JsonNodeDefaultOptions, JsonOptions.JsonDocumentDefaultOptions).AsObject();

            // update finger print
            //features.Fingerprint = (string)oldFeatures["fingerprint"];
            features.Fingerprint = _fingerprint.Value.GetFingerprint();

            foreach (var f in oldFeatures["features"].AsArray())
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
            var wrapLog = Log.Fn<bool>($"f:{features?.Features?.Count ?? -1}");

            try
            {
                // when null, prepare empty features
                if (features == null) features = new FeatureListStored();

                // update to latest fingerprint
                features.Fingerprint = _fingerprint.Value.GetFingerprint();

                // save new format (v13)
                var fileContent = JsonSerializer.Serialize(features, JsonOptions.FeaturesJson);

                var configurationsPath = _globalConfiguration.Value.ConfigFolder;

                // ensure that path to store files already exits
                Directory.CreateDirectory(configurationsPath);

                var filePath = Path.Combine(configurationsPath, FeatureConstants.FeaturesJson);

                return wrapLog.Return(SaveFile(filePath, fileContent), "features new saved");
            }
            catch (Exception e)
            {
                Log.Ex(e);
                return wrapLog.ReturnFalse("save features failed:" + e.Message);
            }
        }


        /// <summary>
        /// Update existing features config in "features.json". 
        /// </summary>
        private bool SaveFile(string filePath, string fileContent)
        {
            var wrapLog = Log.Fn<bool>($"fp={filePath}");

            try
            {
                File.WriteAllText(filePath, fileContent);
                return wrapLog.ReturnTrue("ok, file saved");
            }
            catch (Exception e)
            {
                Log.Ex(e);
                return wrapLog.ReturnFalse("save file failed:" + e.Message);
            }
        }


        /// <summary>
        /// Update existing features config and save. 
        /// </summary>
        internal bool SaveFeaturesUpdate(List<FeatureManagementChange> changes)
        {
            var wrapLog = Log.Fn<bool>($"c:{changes?.Count ?? -1}");

            try
            {
                var (filePath, fileContent) = LoadFeaturesFile();
                
                // if features.json is missing, we still need empty list of stored features so we can create new one on save
                if (fileContent == null)
                    fileContent = JsonSerializer.Serialize(new FeatureListStored(), JsonOptions.FeaturesJson);

                // handle old 'features.json' format
                var stored = ConvertOldFeaturesFile(filePath, fileContent);
                if (stored != null) // if old features are converted, load fileContent features in new format
                    (filePath, fileContent) = LoadFeaturesFile();

                var fileJson = JsonNode.Parse(fileContent, JsonOptions.JsonNodeDefaultOptions, JsonOptions.JsonDocumentDefaultOptions).AsObject();
                foreach (var change in changes)
                {
                    var feature = (fileJson["features"].AsArray()).FirstOrDefault(f => (Guid)f["id"] == change.FeatureGuid);
                    if (feature == null) // insert
                    {
                        var featureConfig = JsonSerializer.Serialize(FeatureConfigBuilder(change), JsonOptions.FeaturesJson);
                        (fileJson["features"].AsArray()).Add(JsonNode.Parse(featureConfig, JsonOptions.JsonNodeDefaultOptions, JsonOptions.JsonDocumentDefaultOptions).AsObject());
                    }
                    else
                    {
                        if (change.Enabled.HasValue) // update
                            feature["enabled"] = change.Enabled.Value;
                        else // delete
                            fileJson["features"].AsArray().Remove(feature);
                    }
                }

                // update to latest fingerprint
                fileJson["fingerprint"] = _fingerprint.Value.GetFingerprint();

                return wrapLog.Return(SaveFile(filePath, fileJson.ToString()), "features saved");
            }
            catch (Exception e)
            {
                Log.Ex(e);
                return wrapLog.ReturnFalse("save features failed:" + e.Message);
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
