using ToSic.Eav.Internal.Requirements;
using ToSic.Eav.SysData;
using ToSic.Lib.DI;

namespace ToSic.Eav.Internal.Features;

public class FeatureRequirementCheck(LazySvc<ILibFeaturesService> features) : RequirementCheckBase
{
    public override string NameId => FeatureConstants.ConditionIsFeature;

    public override bool IsOk(Requirement requirement) => features.Value.IsEnabled(requirement.NameId);

    public override string InfoIfNotOk(Requirement requirement) 
        => $"The feature '{requirement.NameId}' is not enabled - see https://go.2sxc.org/features.";
}