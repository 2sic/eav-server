using System.Collections.Generic;
using ToSic.Eav.Data;

namespace ToSic.Eav.Internal.Requirements;

public interface IRequirementsService
{
    List<RequirementStatus> UnfulfilledRequirements(IEnumerable<IEntity> requirements);
}