using ToSic.Sys.Requirements;

namespace ToSic.Sys.Features.Requirements.Animals;

internal class MockHasRequirements(List<Requirement> requirements): IHasRequirements
{
    public List<Requirement> Requirements { get; } = requirements;
}