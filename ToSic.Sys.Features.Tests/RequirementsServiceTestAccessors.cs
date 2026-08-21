using ToSic.Sys.Requirements;

namespace ToSic.Sys.Features;

internal static class RequirementsServiceTestAccessors
{
    public static RequirementIssue? CheckOneInternalTac(this IRequirementsService reqSvc, Requirement requirement)
        => ((RequirementsService)reqSvc).CheckOneInternal(requirement);

    public static IEnumerable<RequirementIssue> CheckTac(this IRequirementsService reqSvc, IEnumerable<Requirement> requirements)
        => reqSvc.Check(requirements?.ToList());

    public static IEnumerable<RequirementIssue> CheckTac(this IRequirementsService reqSvc, IHasRequirements withRequirements)
        => reqSvc.Check(withRequirements);

    public static IEnumerable<RequirementIssue> CheckTac(this IRequirementsService reqSvc, List<IHasRequirements> withRequirements)
        => reqSvc.Check(withRequirements);
}