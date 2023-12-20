using System.Collections.Generic;
using ToSic.Eav.Data;
using ToSic.Eav.Internal.Unknown;
using ToSic.Eav.SysData;

namespace ToSic.Eav.Internal.Requirements;

internal class RequirementsServiceUnknown: IRequirementsService
{
    public RequirementsServiceUnknown(WarnUseOfUnknown<RequirementsServiceUnknown> _) {
    }

    public List<RequirementStatus> UnfulfilledRequirements(IEnumerable<IEntity> requirements) => new();

    public List<RequirementStatus> UnfulfilledRequirements(IEnumerable<SysFeature> requirements) => new();
}