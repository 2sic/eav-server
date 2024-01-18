using ToSic.Eav.SysData;
using ToSic.Eav.WebApi.Context;

namespace ToSic.Eav.WebApi.Sys.Licenses;

public class FeatureStateDto(FeatureState state) : FeatureDto(state)
{
    //License = state.License?.Name;
    //LicenseEnabled = state.AllowedByLicense;

    public Guid Guid { get; } = state.Aspect.Guid;

    public string Description { get; } = state.Aspect.Description;

    public string EnabledReason { get; } = state.EnabledReason;

    public string EnabledReasonDetailed { get; } = state.EnabledReasonDetailed;

    public bool EnabledByDefault { get; } = state.EnabledByDefault;

    public bool? EnabledInConfiguration { get; } = state.EnabledInConfiguration;

    public DateTime Expiration { get; } = state.Expiration;

    //public string License { get; }

    //public bool LicenseEnabled { get; }

    public FeatureSecurity Security { get; } = state.Security;

    //public bool Public { get; }

    //public bool Ui { get; }

    public string Link { get; } = state.Aspect.Link;

    public bool IsConfigurable { get; } = state.Aspect.IsConfigurable;
}