using ToSic.Eav.Internal.Unknown;
using ToSic.Sys.Capabilities.SysFeatures;
using ToSic.Sys.Requirements;

namespace ToSic.Metadata.Requirements.Sys;

internal class RequirementsServiceUnknown: IRequirementsService
{
    public RequirementsServiceUnknown(WarnUseOfUnknown<RequirementsServiceUnknown> _) {
    }

    public List<RequirementStatus> UnfulfilledRequirements(IEnumerable<IEntity> requirements) => [];

    public List<RequirementStatus> UnfulfilledRequirements(IEnumerable<SysFeature> requirements) => [];
}