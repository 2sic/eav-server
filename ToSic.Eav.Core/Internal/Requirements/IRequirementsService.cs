using ToSic.Eav.Data;
using ToSic.Eav.SysData;

namespace ToSic.Eav.Internal.Requirements;

public interface IRequirementsService
{
    List<RequirementStatus> UnfulfilledRequirements(IEnumerable<IEntity> requirements);

    List<RequirementStatus> UnfulfilledRequirements(IEnumerable<SysFeature> requirements);
}