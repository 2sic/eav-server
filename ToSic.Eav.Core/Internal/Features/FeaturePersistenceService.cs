using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using ToSic.Eav.Internal.Configuration;
using ToSic.Eav.Security.Fingerprint;
using ToSic.Eav.Serialization;
using ToSic.Eav.SysData;
using ToSic.Lib.DI;
using ToSic.Lib.Services;

namespace ToSic.Eav.Internal.Features;

public class FeaturePersistenceService(
    LazySvc<IGlobalConfiguration> globalConfiguration,
    LazySvc<SystemFingerprint> fingerprint)
    : ServiceBase("FeatCfgMng", connect: [globalConfiguration, fingerprint])
{

    #region File Operations

    /// <summary>
    /// Return content from 'features.json' file.
    /// Also return full path to 'features.json'.
    /// </summary>
    /// <returns>file path and content as string</returns>
    internal (string filePath, string fileContent) LoadFeaturesFile()
    {
        // folder with "features.json"
        var configurationsPath = globalConfiguration.Value.ConfigFolder();

        // ensure that path to store files already exits
        Directory.CreateDirectory(configurationsPath);

        var featureFilePath = Path.Combine(configurationsPath, FeatureConstants.FeaturesJson);

        return File.Exists(featureFilePath)
            ? (featureFilePath, File.ReadAllText(featureFilePath))
            : (featureFilePath, null);
    }

    /// <summary>
    /// Update existing features config in "features.json". 
    /// </summary>
    private bool SaveFile(string filePath, string fileContent)
    {
        var l = Log.Fn<bool>($"fp={filePath}");
        try
        {
            File.WriteAllText(filePath, fileContent);
            return l.ReturnTrue("ok, file saved");
        }
        catch (Exception e)
        {
            l.Ex(e);
            return l.ReturnFalse($"save file failed:{e.Message}");
        }
    }

    #endregion


    #region JSON serialization

    private static JsonObject JsonToObject(string json)
        => JsonNode.Parse(json ?? "{}", JsonOptions.JsonNodeDefaultOptions, JsonOptions.JsonDocumentDefaultOptions)
            .AsObject();

    private static string FeatToJson(object obj)
        => JsonSerializer.Serialize(obj, JsonOptions.FeaturesJson);

    #endregion





    /// <summary>
    /// Update existing features config and save. 
    /// </summary>
    internal bool SaveFeaturesUpdate(List<FeatureManagementChange> changes)
    {
        var changeCount = changes?.Count ?? -1;
        var l = Log.Fn<bool>($"c:{changeCount}");
        if (changes == null || changeCount <= 0)
            return l.ReturnTrue("no changes");

        try
        {
            var (filePath, fileContent) = LoadFeaturesFile();
            var fileJson = JsonToObject(fileContent);

            var featureArray = fileJson["features"]
                .AsArray();
            foreach (var change in changes)
            {
                var feature = featureArray.FirstOrDefault(f => (Guid)f["id"] == change.FeatureGuid);
                // Insert (not yet configured)
                if (feature == null)
                {
                    var featObj = FeatureConfigBuilder(change);
                    if (change.Configuration != null)
                        featObj = featObj with { Configuration = change.Configuration };
                    featureArray.Add(JsonToObject(FeatToJson(featObj)));
                }
                // Update existing config, but only if:
                // - there is a true/false config
                // - it already had a config (which should be preserved)
                // - it has a new config, despite not being enabled
                else if (change.Enabled.HasValue || feature["configuration"] != null || change.Configuration != null)
                {
                    // Note: as of v20 this can also result in a null, when having to preserve the underlying configuration
                    feature["enabled"] = change.Enabled;
                    if (change.Configuration != null)
                        feature["configuration"] = JsonToObject(FeatToJson(change.Configuration));
                }
                // Delete
                // TODO: in future if it has a configuration, probably better just null the "enabled" and not delete
                else
                    featureArray.Remove(feature);
            }

            // update to latest fingerprint
            fileJson["fingerprint"] = fingerprint.Value.GetFingerprint();

            var saved = SaveFile(filePath, fileJson.ToString());
            return l.Return(saved, $"features saved: {saved}");
        }
        catch (Exception e)
        {
            l.Ex(e);
            return l.ReturnFalse("save features failed:" + e.Message);
        }
    }


    internal static FeatureStatePersisted FeatureConfigBuilder(FeatureState featureState) =>
        new()
        {
            Id = featureState.Aspect.Guid,
            Enabled = featureState.IsEnabled
        };

        
    internal static FeatureStatePersisted FeatureConfigBuilder(FeatureManagementChange change) =>
        new()
        {
            Id = change.FeatureGuid,
            Enabled = change.Enabled ?? false
        };
}