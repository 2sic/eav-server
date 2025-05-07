using ToSic.Eav.Internal.Requirements;
using ToSic.Eav.SysData;

namespace ToSic.Lib.Features.Tests.RequirementsServiceTests.NoMockCheckers;

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

    /// <summary>
    /// By default, it only has 1 feature checker
    /// </summary>
    [Fact]
    public void HasOneChecker()
        => Single(requirementsService.Checkers.Value.AllServices);
}