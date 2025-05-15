using System.Text.Json.Serialization;


namespace ToSic.Eav.SysData;

/// <summary>
/// This stores the enabled / expiry of a feature
/// </summary>
[PrivateApi("no good reason to publish this")]
[ShowApiWhenReleased(ShowApiMode.Never)]
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
        init;
    }

    /// <summary>
    /// Expiry of this feature
    /// </summary>
    [JsonPropertyName("expires")]
    public DateTime Expires { get; init; } = DateTime.MaxValue;


    [JsonPropertyName("configuration")]
    public Dictionary<string, object>? Configuration
    {
        get;
        init => field = value == null
            ? null
            : new(value, comparer: StringComparer.InvariantCultureIgnoreCase);
    }
}