using ToSic.Sys.Capabilities.Aspects;
using ToSic.Sys.Capabilities.Features;
using ToSic.Sys.Requirements;

namespace ToSic.Sys.Capabilities.SysFeatures;

[ShowApiWhenReleased(ShowApiMode.Never)]
public class SysFeatureRequirementCheck(LazySvc<ILibFeaturesService> sysFeatsSvc) : RequirementCheckBase
{
    public override string NameId => FeatureConstants.RequirementSysCapability;

    public override bool IsOk(Requirement requirement)
        => sysFeatsSvc.Value.IsEnabled(requirement.NameId);

    public override string InfoIfNotOk(Requirement requirement) 
        => $"The feature '{requirement.NameId}' is not enabled - see https://go.2sxc.org/features.";

    protected override Aspect GetAspect(Requirement requirement)
        => sysFeatsSvc.Value.Get(requirement.NameId)?.Aspect ?? Aspect.UnknownAspect(FeatureConstants.RequirementSysCapability, requirement.NameId);
}