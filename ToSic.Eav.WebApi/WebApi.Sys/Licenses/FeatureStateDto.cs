using ToSic.Eav.WebApi.Sys.Context;
using ToSic.Sys.Capabilities.Features;

namespace ToSic.Eav.WebApi.Sys.Licenses;

public class FeatureStateDto(FeatureState state) : FeatureDto(state, false)
{
    // ReSharper disable once InconsistentNaming
    private readonly FeatureState state = state;

    [JsonPropertyName("guid")]
    public Guid Guid => state.Aspect.Guid;

    [JsonPropertyName("description")]
    public string Description => state.Aspect.Description;

    [JsonPropertyName("enabledReason")]
    public string EnabledReason => state.EnabledReason;

    [JsonPropertyName("enabledReasonDetailed")]
    public string EnabledReasonDetailed => state.EnabledReasonDetailed;

    /// <inheritdoc cref="FeatureState.EnabledByDefault"/>
    [JsonPropertyName("enabledByDefault")]
    public bool EnabledByDefault => state.EnabledByDefault;

    /// <inheritdoc cref="FeatureState.EnabledInConfiguration"/>
    [JsonPropertyName("enabledInConfiguration")]
    public bool? EnabledInConfiguration => state.EnabledInConfiguration;

    [JsonPropertyName("expiration")]
    public DateTime Expiration => state.Expiration;

    [JsonPropertyName("security")]
    public FeatureSecurity Security => state.Security;

    [JsonPropertyName("link")]
    public string Link => state.Aspect.Link;

    /// <inheritdoc cref="Feature.IsConfigurable"/>
    [JsonPropertyName("isConfigurable")]
    public bool IsConfigurable => state.Aspect.IsConfigurable;

    /// <summary>
    /// Additional configuration from the json file.
    /// </summary>
    [JsonPropertyName("configuration")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public Dictionary<string, object> Configuration => state.Configuration;

    [JsonPropertyName("configurationContentType")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string ConfigurationContentType => state.Aspect.ConfigurationContentType;
}