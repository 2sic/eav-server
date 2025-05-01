using System.Text.Json.Serialization;

namespace ToSic.Eav.SysData;

/// <summary>
/// This stores the enabled / expiry of a feature
/// </summary>
[PrivateApi("no good reason to publish this")]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public class FeatureStatePersisted
{
    /// <summary>
    /// Feature GUID
    /// </summary>
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    /// <summary>
    /// Feature is enabled and hasn't expired yet
    /// </summary>
    /// <remarks>by default all features are disabled</remarks>
    [JsonPropertyName("enabled")]
    public bool Enabled
    {
        get => field && Expires > DateTime.Now;
        set => field = value;
    }

    /// <summary>
    /// Expiry of this feature
    /// </summary>
    [JsonPropertyName("expires")]
    public DateTime Expires { get; set; } = DateTime.MaxValue;

}