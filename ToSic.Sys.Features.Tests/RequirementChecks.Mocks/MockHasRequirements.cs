using ToSic.Sys.Requirements;

namespace ToSic.Sys.Features.Tests.RequirementChecks.Mocks;

internal class MockHasRequirements(List<Requirement> requirements): IHasRequirements
{
    public List<Requirement> Requirements { get; } = requirements;
}