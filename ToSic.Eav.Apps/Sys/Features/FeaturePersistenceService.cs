using System.Text.Json;
using System.Text.Json.Nodes;
using ToSic.Eav.Internal.Configuration;
using ToSic.Eav.Serialization;
using ToSic.Sys.Capabilities.Features;
using ToSic.Sys.Capabilities.Fingerprints;
using ToSic.Sys.Configuration;

namespace ToSic.Eav.Internal.Features;

public class FeaturePersistenceService(
    LazySvc<IGlobalConfiguration> globalConfiguration,
    LazySvc<FeaturesIoHelper> featuresIo,
    LazySvc<SystemFingerprint> fingerprint)
    : ServiceBase("FeatCfgMng", connect: [globalConfiguration, featuresIo, fingerprint])
{
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
    internal bool ApplyUpdatesAndSave(List<FeatureStateChange> changes)
    {
        var changeCount = changes?.Count ?? -1;
        var l = Log.Fn<bool>($"c:{changeCount}");
        if (changes == null || changeCount <= 0)
            return l.ReturnTrue("no changes");

        try
        {
            var (_, fileContent) = featuresIo.Value.Load();
            
            // In case the file is empty, create a dummy so the remaining code works
            fileContent ??= JsonSerializer.Serialize(new FeatureStatesPersisted(), JsonOptions.FeaturesJson);
            var fileJson = JsonToObject(fileContent);

            var featureArray = fileJson["features"]
                .AsArray();
            foreach (var change in changes)
            {
                var feature = featureArray.FirstOrDefault(f => (Guid)f["id"] == change.FeatureGuid);
                // Insert (not yet configured)
                if (feature == null)
                {
                    var featObj = FeatureStatePersisted.FromChange(change);
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
                // Delete, but only if the change does not have a configuration and there was no previous configuration
                else
                    featureArray.Remove(feature);
            }

            // update to latest fingerprint
            fileJson["fingerprint"] = fingerprint.Value.GetFingerprint();

            var saved = featuresIo.Value.Save(fileJson.ToString());
            return l.Return(saved, $"features saved: {saved}");
        }
        catch (Exception e)
        {
            l.Ex(e);
            return l.ReturnFalse("save features failed:" + e.Message);
        }
    }
}