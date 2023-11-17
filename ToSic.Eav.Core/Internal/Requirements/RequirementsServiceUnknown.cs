using System.Collections.Generic;
using ToSic.Eav.Data;
using ToSic.Eav.Run.Unknown;

namespace ToSic.Eav.Internal.Requirements
{
    internal class RequirementsServiceUnknown: IRequirementsService
    {
        public RequirementsServiceUnknown(WarnUseOfUnknown<RequirementsServiceUnknown> _) {
        }

        public List<RequirementStatus> UnfulfilledRequirements(IEnumerable<IEntity> requirements)
        {
            return new List<RequirementStatus>();
        }
    }
}
