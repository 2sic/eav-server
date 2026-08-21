using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ToSic.Sys.Requirements;
using static ToSic.Sys.Features.Requirements.Animals.MockAnimalRequirementsCheck;

namespace ToSic.Sys.Features.Requirements.Animals;

public class RequirementsServiceAnimals(RequirementsService requirementsService)
{
    public class Startup() : QuickStartup(services => services
        .AddSysCapabilitiesAndSysCore()
        .AddTransient<IRequirementCheck, MockAnimalRequirementsCheck>()
        // V22 - WIP - moving requirement checks to keyed services
        .TryAddKeyedTransient<IRequirementCheck, MockAnimalRequirementsCheck>(Animal)
    );

    private static Requirement RequiresElephant => new(Animal, Elephant);
    private static Requirement RequiresZebra => new(Animal, "Zebra");

    [Fact]
    public void VerifySetup_HasOneMoreChecker()
        => Equal(StartupHelpers.RequirementChecksInDiByDefault + 1, requirementsService.Checkers.Value.AllServices.Count);

    [Fact]
    public void VerifySetup_HasAnimalChecker()
        => NotNull(requirementsService.Checkers.Value.ByNameId(Animal));

    [Fact]
    public void VerifySetup_NoForestChecker()
        => Null(requirementsService.Checkers.Value.ByNameId("forest"));

    [Fact]
    public void Requirement_Elephant_IsOk()
        => Null(requirementsService.CheckTac(RequiresElephant));

    [Fact]
    public void Requirement_Zebra_IsNotOk()
        => NotNull(requirementsService.CheckTac(RequiresZebra));

    [Fact]
    public void RequirementList_Elephant_IsOk()
        => Empty(requirementsService.CheckTac([RequiresElephant]));

    [Fact]
    public void RequirementList_Zebra_IsNotOk()
        => Single(requirementsService.CheckTac([RequiresZebra]));

    [Fact]
    public void RequirementList_ElephantAndZebra_IsNotOk()
        => Single(requirementsService.CheckTac([RequiresElephant, RequiresZebra]));

    [Fact]
    public void RequirementList_ZebraX2_ReturnSingleError()
        => Single(requirementsService.CheckTac([RequiresZebra, RequiresZebra]));

    [Fact]
    public void HasRequirements_Elephant_IsOk()
        => Empty(requirementsService.CheckTac(new MockHasRequirements([RequiresElephant])));

    [Fact]
    public void HasRequirements_Zebra_IsNotOk()
        => Single(requirementsService.CheckTac(new MockHasRequirements([RequiresZebra])));

    [Fact]
    public void HasRequirements_ElephantAndZebra_IsNotOk()
        => Single(requirementsService.CheckTac(new MockHasRequirements([RequiresElephant, RequiresZebra])));

    [Fact]
    public void HasRequirements_ElephantAndZebraX2_ReturnSingleError()
        => Single(requirementsService.CheckTac(new MockHasRequirements([RequiresElephant, RequiresZebra, RequiresZebra])));

    [Fact]
    public void HasRequirements_ManyIdentical_ReturnSingleError()
        => Single(requirementsService.CheckTac([
            new MockHasRequirements([RequiresElephant, RequiresZebra, RequiresZebra]),
            new MockHasRequirements([RequiresZebra]),
        ]));

}