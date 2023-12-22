using System;
using ToSic.Eav.SysData;
using ToSic.Eav.WebApi.Context;

namespace ToSic.Eav.WebApi.Sys.Licenses;

public class FeatureStateDto: FeatureDto
{

    public FeatureStateDto(FeatureState state) : base(state)
    {
        Guid = state.Aspect.Guid;
        Description = state.Aspect.Description;
        EnabledReason = state.EnabledReason;
        EnabledReasonDetailed = state.EnabledReasonDetailed;
        EnabledByDefault = state.EnabledByDefault;
        EnabledInConfiguration = state.EnabledInConfiguration;
        Expiration = state.Expiration;
        //License = state.License?.Name;
        //LicenseEnabled = state.AllowedByLicense;

        Security = state.Security;
        Link = state.Aspect.Link;
        IsConfigurable = state.Aspect.IsConfigurable;
    }

    public Guid Guid { get; }

    public string Description { get; }

    public string EnabledReason { get; }

    public string EnabledReasonDetailed { get; }

    public bool EnabledByDefault { get; }

    public bool? EnabledInConfiguration { get; }

    public DateTime Expiration { get; }

    //public string License { get; }

    //public bool LicenseEnabled { get; }

    public FeatureSecurity Security { get; }

    //public bool Public { get; }

    //public bool Ui { get; }

    public string Link { get; }

    public bool IsConfigurable { get; }


}