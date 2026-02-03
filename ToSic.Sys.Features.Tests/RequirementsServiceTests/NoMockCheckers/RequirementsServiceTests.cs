using ToSic.Sys.Requirements;

namespace ToSic.Sys.Features.Tests.RequirementsServiceTests.NoMockCheckers;

public class RequirementsServiceTests(RequirementsService requirementsService)
{
    [Fact]
    public void Check_EmptyRequirements_ReturnsEmptyList()
    {
        // Arrange
        var requirements = new List<IHasRequirements>();
        // Act
        var result = requirementsService.Check(requirements);
        // Assert
        Empty(result);
    }

    [Fact]
    public void HasExpectedDefaultCheckers()
        => Equal(LibFeaturesTestsConstants.ExpectedCheckersInDefaultSetup, requirementsService.Checkers.Value.AllServices.Count);
}