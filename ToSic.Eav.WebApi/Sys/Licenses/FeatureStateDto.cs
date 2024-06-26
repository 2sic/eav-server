using ToSic.Eav.SysData;
using ToSic.Eav.WebApi.Context;

namespace ToSic.Eav.WebApi.Sys.Licenses;

public class FeatureStateDto(FeatureState state) : FeatureDto(state)
{
    //License = state.License?.Name;
    //LicenseEnabled = state.AllowedByLicense;

    [JsonPropertyName("guid")]
    public Guid Guid { get; } = state.Aspect.Guid;

    [JsonPropertyName("description")]
    public string Description { get; } = state.Aspect.Description;

    [JsonPropertyName("enabledReason")]
    public string EnabledReason { get; } = state.EnabledReason;

    [JsonPropertyName("enabledReasonDetailed")]
    public string EnabledReasonDetailed { get; } = state.EnabledReasonDetailed;

    [JsonPropertyName("enabledByDefault")]
    public bool EnabledByDefault { get; } = state.EnabledByDefault;

    [JsonPropertyName("enabledInConfiguration")]
    public bool? EnabledInConfiguration { get; } = state.EnabledInConfiguration;

    [JsonPropertyName("expiration")]
    public DateTime Expiration { get; } = state.Expiration;

    //public string License { get; }

    //public bool LicenseEnabled { get; }

    [JsonPropertyName("security")]
    public FeatureSecurity Security { get; } = state.Security;

    //public bool Public { get; }

    //public bool Ui { get; }

    [JsonPropertyName("link")]
    public string Link { get; } = state.Aspect.Link;

    [JsonPropertyName("isConfigurable")]
    public bool IsConfigurable { get; } = state.Aspect.IsConfigurable;
}