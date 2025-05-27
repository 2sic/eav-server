using ToSic.Lib.DI;
using ToSic.Sys.Capabilities.Aspects;
using ToSic.Sys.Requirements;

namespace ToSic.Sys.Capabilities.SysFeatures;

public class SysFeatureRequirementCheck(LazySvc<SysFeaturesService> sysFeatsSvc) : RequirementCheckBase
{
    public override string NameId => FeatureConstants.RequirementSysCapability;

    public override bool IsOk(Requirement requirement)
        => sysFeatsSvc.Value.IsEnabled(requirement.NameId);

    public override string InfoIfNotOk(Requirement requirement) 
        => $"The feature '{requirement.NameId}' is not enabled - see https://go.2sxc.org/features.";

    protected override Aspect GetAspect(Requirement requirement)
        => sysFeatsSvc.Value.GetDef(requirement.NameId) ?? Aspect.None;
}