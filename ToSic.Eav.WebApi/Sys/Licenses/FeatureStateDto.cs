using ToSic.Eav.SysData;
using ToSic.Eav.WebApi.Context;

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

    [JsonPropertyName("enabledByDefault")]
    public bool EnabledByDefault => state.EnabledByDefault;

    [JsonPropertyName("enabledInConfiguration")]
    public bool? EnabledInConfiguration => state.EnabledInConfiguration;

    [JsonPropertyName("expiration")]
    public DateTime Expiration => state.Expiration;

    [JsonPropertyName("security")]
    public FeatureSecurity Security => state.Security;

    [JsonPropertyName("link")]
    public string Link => state.Aspect.Link;

    [JsonPropertyName("isConfigurable")]
    public bool IsConfigurable => state.Aspect.IsConfigurable;
}