using ToSic.Sys.Capabilities.SysFeatures;
using ToSic.Sys.Requirements;

namespace ToSic.Metadata.Requirements.Sys;

public interface IRequirementsService
{
    List<RequirementStatus> UnfulfilledRequirements(IEnumerable<SysFeature> requirements);

    List<RequirementStatus> UnfulfilledRequirements(IEnumerable<IEntity> requirements);

}