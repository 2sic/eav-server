using ToSic.Eav.Internal.Requirements;
using ToSic.Eav.SysData;
using ToSic.Lib.DI;

namespace ToSic.Eav.Internal.Features;

public class FeatureRequirementCheck(LazySvc<IEavFeaturesService> features) : RequirementCheckBase
{
    public const string ConditionIsFeature = "feature";

    private LazySvc<IEavFeaturesService> Features { get; } = features;

    public override string NameId => ConditionIsFeature;

    public override bool IsOk(Requirement requirement) => Features.Value.IsEnabled(requirement.NameId);

    public override string InfoIfNotOk(Requirement requirement) 
        => $"The feature '{requirement.NameId}' is not enabled - see https://go.2sxc.org/features.";
}