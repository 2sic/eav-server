using ToSic.Sys.DI;
using ToSic.Sys.Requirements;
using static ToSic.Sys.Features.Requirements.Animals.MockAnimalRequirementsCheck;

namespace ToSic.Sys.Features.Requirements.Animals;

public class RequirementsServiceAnimals(IRequirementsService requirementsService, IServiceProvider provider, Generator<IRequirementCheck> generator)
{
    public class Startup() : QuickStartup(services => services
        .AddSysCapabilitiesAndSysCore()
        // V22 - moving requirement checks to keyed services
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
        => True(requirementsService.StatusInternalTac(RequiresElephant).IsOk);

    [Fact]
    public void Requirement_Zebra_IsNotOk()
        => NotNull(requirementsService.StatusInternalTac(RequiresZebra));

    [Fact]
    public void RequirementList_Elephant_IsOk()
        => Empty(requirementsService.StatusTac([RequiresElephant]));

    [Fact]
    public void RequirementList_Zebra_IsNotOk()
        => Single(requirementsService.StatusTac([RequiresZebra]));

    [Fact]
    public void RequirementList_ElephantAndZebra_IsNotOk()
        => Single(requirementsService.StatusTac([RequiresElephant, RequiresZebra]));

    [Fact]
    public void RequirementList_ZebraX2_ReturnSingleError()
        => Single(requirementsService.StatusTac([RequiresZebra, RequiresZebra]));

    [Fact]
    public void HasRequirements_Elephant_IsOk()
        => Empty(requirementsService.StatusTac(new MockHasRequirements([RequiresElephant])));

    [Fact]
    public void HasRequirements_Zebra_IsNotOk()
        => Single(requirementsService.StatusTac(new MockHasRequirements([RequiresZebra])));

    [Fact]
    public void HasRequirements_ElephantAndZebra_IsNotOk()
        => Single(requirementsService.StatusTac(new MockHasRequirements([RequiresElephant, RequiresZebra])));

    [Fact]
    public void HasRequirements_ElephantAndZebraX2_ReturnSingleError()
        => Single(requirementsService.StatusTac(new MockHasRequirements([RequiresElephant, RequiresZebra, RequiresZebra])));

    [Fact]
    public void HasRequirements_ManyIdentical_ReturnSingleError()
        => Single(requirementsService.StatusTac([
            new MockHasRequirements([RequiresElephant, RequiresZebra, RequiresZebra]),
            new MockHasRequirements([RequiresZebra]),
        ]));

}