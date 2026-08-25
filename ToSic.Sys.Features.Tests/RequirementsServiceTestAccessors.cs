using ToSic.Sys.Requirements;

namespace ToSic.Sys.Features;

internal static class RequirementsServiceTestAccessors
{
    extension(IRequirementsService reqSvc)
    {
        public RequirementStatus StatusInternalTac(Requirement requirement)
            => ((RequirementsService)reqSvc).StatusInternal(requirement);

        public IEnumerable<RequirementStatus> StatusTac(IEnumerable<Requirement> requirements)
            => reqSvc.Issues(requirements.ToList());

        public IEnumerable<RequirementStatus> StatusTac(IHasRequirements withRequirements)
            => reqSvc.Issues(withRequirements);

        public IEnumerable<RequirementStatus> StatusTac(List<IHasRequirements> withRequirements)
            => reqSvc.Issues(withRequirements);
    }
}