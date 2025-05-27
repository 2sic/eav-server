using ToSic.Lib.DI;
using ToSic.Sys.Capabilities.Aspects;
using ToSic.Sys.Requirements;

namespace ToSic.Sys.Capabilities.Features;

public class FeatureRequirementCheck(LazySvc<ILibFeaturesService> features) : RequirementCheckBase
{
    public override string NameId => FeatureConstants.RequirementFeature;

    public override bool IsOk(Requirement requirement)
        => features.Value.IsEnabled(requirement.NameId);

    public override string InfoIfNotOk(Requirement requirement) 
        => $"The feature '{requirement.NameId}' is not enabled - see https://go.2sxc.org/features.";

    protected override Aspect GetAspect(Requirement requirement)
        => features.Value.Get(requirement.NameId)?.Aspect ?? Aspect.None;

}