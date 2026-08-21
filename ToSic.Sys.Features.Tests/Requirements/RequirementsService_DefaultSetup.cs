using ToSic.Sys.Requirements;

namespace ToSic.Sys.Features.Requirements;

// ReSharper disable once InconsistentNaming
public class RequirementsService_DefaultSetup(RequirementsService requirementsService)
{
    public class Startup() : QuickStartup(sc => sc.AddSysCapabilitiesAndSysCore());

    [Fact]
    public void ByDefault_EmptyRequirements_ReturnsEmpty()
    {
        // Arrange
        var requirementsList = new List<IHasRequirements>();
        // Act
        var result = requirementsService.Check(requirementsList);
        // Assert
        Empty(result);
    }

    [Fact]
    public void ByDefault_Has2Checkers()
        => Equal(StartupHelpers.RequirementChecksInDiByDefault, requirementsService.Checkers.Value.AllServices.Count);
}