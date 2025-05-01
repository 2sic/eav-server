using System.IO;
using System.Text.Json.Nodes;
using ToSic.Eav.SysData;

namespace ToSic.Eav.Internal.Features;

public partial class FeaturePersistenceService
{

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
    internal FeatureStatesPersisted ConvertOldFeaturesFile(string filePath, string fileContent)
    {
        var fileJson = JsonToObject(fileContent);

        // check json format in "features.json" to find is it old version (v12)
        if (fileJson["_"]?["V"] != null && (int)fileJson["_"]["V"] == 1) // detect old "features.json" format (v12)
        {
            // rename old file format "features.json" to "features.json.v12.bak"
            var oldFeatureFilePathForBackup = $"{filePath}.v12.bak";
            if (File.Exists(oldFeatureFilePathForBackup))
                File.Delete(oldFeatureFilePathForBackup);
            File.Move(filePath, oldFeatureFilePathForBackup);

            // get stored features from old format
            var stored = GetFeaturesFromOldFormat(fileJson);

            // save features in new format
            SaveFeaturesFile(stored);

            return stored;
        }
        return null;
    }


    /// <summary>
    /// Get features from json old format (v12)
    /// </summary>
    /// <param name="json"></param>
    private FeatureStatesPersisted GetFeaturesFromOldFormat(JsonObject json)
    {
        var features = new FeatureStatesPersisted();

        var fs = (string)json["Entity"]["Attributes"]["Custom"]["Features"]["*"];
        var oldFeatures = JsonToObject(fs);

        // update finger print
        //features.Fingerprint = (string)oldFeatures["fingerprint"];
        features = features with { Fingerprint = fingerprint.Value.GetFingerprint() };

        foreach (var f in oldFeatures["features"].AsArray())
        {
            features.Features.Add(new()
            {
                Id = (Guid)f["id"],
                Enabled = (bool)f["enabled"],
                Expires = (DateTime)f["expires"],
            });
        }

        return features;
    }


}