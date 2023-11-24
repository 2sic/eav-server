using System;
using System.Text.Json.Serialization;
using ToSic.Lib.Documentation;

namespace ToSic.Eav.SysData;

/// <summary>
/// This stores the enabled / expiry of a feature
/// </summary>
[PrivateApi("no good reason to publish this")]
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
        get => _enabled && Expires > DateTime.Now;
        set => _enabled = value;
    }
    private bool _enabled;

    /// <summary>
    /// Expiry of this feature
    /// </summary>
    [JsonPropertyName("expires")]
    public DateTime Expires { get; set; } = DateTime.MaxValue;

}