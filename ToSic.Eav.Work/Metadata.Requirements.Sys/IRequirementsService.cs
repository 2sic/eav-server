using ToSic.Sys.Capabilities.SysFeatures;
using ToSic.Sys.Requirements;

namespace ToSic.Eav.Metadata.Requirements.Sys;

[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IRequirementsService
{
    ICollection<RequirementStatus> UnfulfilledRequirements(IEnumerable<SysFeature> requirements);

    ICollection<RequirementStatus> UnfulfilledRequirements(IEnumerable<IEntity> requirements);

}