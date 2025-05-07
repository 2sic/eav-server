using System.Text.Json.Serialization;
// ReSharper disable RedundantAccessorBody

namespace ToSic.Eav.SysData;

/// <summary>
/// This stores the enabled / expiry of a feature
/// </summary>
[PrivateApi("no good reason to publish this")]
[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public record FeatureStatePersisted
{
    /// <summary>
    /// Feature GUID
    /// </summary>
    [JsonPropertyName("id")]
    public Guid Id { get; init; }

    /// <summary>
    /// Feature is enabled and hasn't expired yet
    /// </summary>
    /// <remarks>by default all features are disabled</remarks>
    [JsonPropertyName("enabled")]
    public bool Enabled
    {
        get => field && Expires > DateTime.Now;
        init => field = value;
    }

    /// <summary>
    /// Expiry of this feature
    /// </summary>
    [JsonPropertyName("expires")]
    public DateTime Expires { get; init; } = DateTime.MaxValue;


    [JsonPropertyName("configuration")]
    public Dictionary<string, object>? Configuration
    {
        get => field;
        init => field = value == null
            ? null
            : new(value, comparer: StringComparer.InvariantCultureIgnoreCase);
    }
}