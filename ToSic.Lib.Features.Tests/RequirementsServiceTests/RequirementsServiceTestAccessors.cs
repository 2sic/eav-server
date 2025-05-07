using ToSic.Eav.Internal.Requirements;
using ToSic.Eav.SysData;

namespace ToSic.Lib.Features.Tests.RequirementsServiceTests;

internal static class RequirementsServiceTestAccessors
{
    public static RequirementError? CheckTac(this RequirementsService reqSvc, Requirement requirement)
        => reqSvc.Check(requirement);

    public static IEnumerable<RequirementError> CheckTac(this RequirementsService reqSvc, IEnumerable<Requirement> requirements)
        => reqSvc.Check(requirements?.ToList());

    public static IEnumerable<RequirementError> CheckTac(this RequirementsService reqSvc, IHasRequirements withRequirements)
        => reqSvc.Check(withRequirements);

    public static IEnumerable<RequirementError> CheckTac(this RequirementsService reqSvc, List<IHasRequirements> withRequirements)
        => reqSvc.Check(withRequirements);
}