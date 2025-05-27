using ToSic.Eav.Data;
using ToSic.Eav.SysData;
using ToSic.Sys.Capabilities.SysFeatures;
using ToSic.Sys.Requirements;

namespace ToSic.Eav.Internal.Requirements;

public interface IRequirementsService
{
    List<RequirementStatus> UnfulfilledRequirements(IEnumerable<SysFeature> requirements);

    List<RequirementStatus> UnfulfilledRequirements(IEnumerable<IEntity> requirements);

}