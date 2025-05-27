using ToSic.Sys.Requirements;

namespace ToSic.Lib.Features.Tests.RequirementChecks.Mocks;

internal class MockHasRequirements(List<Requirement> requirements): IHasRequirements
{
    public List<Requirement> Requirements { get; } = requirements;
}