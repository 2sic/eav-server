using System.Text.Json.Serialization;
using ToSic.Eav.Internal.Features;


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

    public static FeatureStatePersisted FromState(FeatureState featureState) =>
        new()
        {
            Id = featureState.Feature.Guid,
            Enabled = featureState.IsEnabled
        };


    public static FeatureStatePersisted FromChange(FeatureStateChange change) =>
        new()
        {
            Id = change.FeatureGuid,
            Enabled = change.Enabled ?? false
        };

}