using System.Text.Json.Serialization;

namespace ToSic.Eav.SysData;

[PrivateApi("no good reason to publish this")]
[ShowApiWhenReleased(ShowApiMode.Never)]
public record FeatureStatesPersisted
{
    [JsonPropertyName("features")]
    public List<FeatureStatePersisted> Features { get; init; }= [];

    /// <summary>
    /// Fingerprint must be included for ensuring integrity of the data
    /// </summary>
    [JsonPropertyName("fingerprint")]
    public string Fingerprint { get; init; }
}