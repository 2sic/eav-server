using ToSic.Sys.Requirements;

namespace ToSic.Sys.Features.Requirements;

// ReSharper disable once InconsistentNaming
public class RequirementsService_DefaultSetup(IRequirementsService requirementsService)
{
    public class Startup() : QuickStartup(sc => sc.AddSysCapabilitiesAndSysCore());

    [Fact]
    public void ByDefault_EmptyRequirements_ReturnsEmpty()
    {
        // Arrange
        var requirementsList = new List<IHasRequirements>();
        // Act
        var result = requirementsService.CheckTac(requirementsList);
        // Assert
        Empty(result);
    }
}