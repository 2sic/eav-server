using Microsoft.Extensions.DependencyInjection;
using ToSic.Sys.DI;
using ToSic.Sys.Requirements;
using static ToSic.Sys.Features.Requirements.Animals.MockAnimalRequirementsCheck;

namespace ToSic.Sys.Features.Requirements.Animals;

public class RequirementsServiceAnimals(IRequirementsService requirementsService, IServiceProvider provider, Generator<IRequirementCheck> generator)
{
    public class Startup() : QuickStartup(services => services
        .AddSysCapabilitiesAndSysCore()
        .AddTransient<IRequirementCheck, MockAnimalRequirementsCheck>()
        // V22 - WIP - moving requirement checks to keyed services
        .AddKeyedTransientWithMarker<IRequirementCheck, MockAnimalRequirementsCheck>(Animal)
    );

    private static Requirement RequiresElephant => new(Animal, Elephant);
    private static Requirement RequiresZebra => new(Animal, "Zebra");

    [Fact]
    public void VerifySetup_HasOneMoreKeyedChecker()
        => Equal(StartupHelpers.RequirementChecksInDiByDefault + 1, provider.GetAllKeysForService<IRequirementCheck>().Count());

    [Fact]
    public void VerifySetup_HasAnimalChecker()
        => NotNull(generator.TryNew(Animal));

    [Fact]
    public void VerifySetup_NoForestChecker()
        => Null(generator.TryNew("forest"));

    [Fact]
    public void Requirement_Elephant_IsOk()
        => Null(requirementsService.CheckOneInternalTac(RequiresElephant));

    [Fact]
    public void Requirement_Zebra_IsNotOk()
        => NotNull(requirementsService.CheckOneInternalTac(RequiresZebra));

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