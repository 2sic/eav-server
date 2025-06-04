using System.Text.Json.Serialization;

namespace ToSic.Sys.Capabilities.Features;

public class FeatureStateChange
{
    [JsonPropertyName("featureGuid")]
    public Guid FeatureGuid { get; set; }

    /// <summary>
    /// Feature can be enabled, disabled or null.
    /// Null feature are removed from features stored.
    /// </summary>
    [JsonPropertyName("enabled")]
    public bool? Enabled { get; set; }

    [JsonPropertyName("configuration")]
    public Dictionary<string, object>? Configuration { get; set; }
}