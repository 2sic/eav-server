using ToSic.Eav.Internal.Requirements;
using ToSic.Eav.SysData;
using ToSic.Lib.Features.Tests.RequirementChecks.Mocks;
using static ToSic.Lib.Features.Tests.RequirementChecks.Mocks.MockAnimalRequirementsCheck;

namespace ToSic.Lib.Features.Tests.RequirementsServiceTests.AnimalRequirements;

public class RequirementsServiceAnimals(RequirementsService requirementsService)
{
    static Requirement ElephantRequirement => new(Animal, Elephant);
    static Requirement ZebraRequirement => new(Animal, "Zebra");

    [Fact]
    public void HasOneMoreChecker()
        => Equal(LibFeaturesTestsConstants.ExpectedCheckersInDefaultSetup + 1, requirementsService.Checkers.Value.AllServices.Count);

    [Fact]
    public void HasAnimalChecker()
        => True(requirementsService.Checkers.Value.ByNameId(Animal) is not null);

    [Fact]
    public void HasNoForestChecker()
        => False(requirementsService.Checkers.Value.ByNameId("forest") is not null);

    [Fact]
    public void ReqOneElephantIsOk()
        => Null(requirementsService.CheckTac(ElephantRequirement));

    [Fact]
    public void ReqOneZebraIsNotOk()
        => NotNull(requirementsService.CheckTac(ZebraRequirement));

    [Fact]
    public void ReqListElephantIsOk()
        => Empty(requirementsService.CheckTac([ElephantRequirement]));

    [Fact]
    public void ReqListZebraIsNotOk()
        => Single(requirementsService.CheckTac([ZebraRequirement]));

    [Fact]
    public void ReqListElephantAndZebraIsNotOk()
        => Single(requirementsService.CheckTac([ElephantRequirement, ZebraRequirement]));

    [Fact]
    public void ReqListZebraX2IsStillOne()
        => Single(requirementsService.CheckTac([ZebraRequirement, ZebraRequirement]));

    [Fact]
    public void ReqHasRequirementsElephantIsOk()
        => Empty(requirementsService.CheckTac(new MockHasRequirements([ElephantRequirement])));

    [Fact]
    public void ReqHasRequirementsZebraIsNotOk()
        => Single(requirementsService.CheckTac(new MockHasRequirements([ZebraRequirement])));

    [Fact]
    public void ReqHasRequirementsElephantAndZebraIsNotOk()
        => Single(requirementsService.CheckTac(new MockHasRequirements([ElephantRequirement, ZebraRequirement])));

    [Fact]
    public void ReqHasRequirementsElephantAndZebraX2IsStillSingle()
        => Single(requirementsService.CheckTac(new MockHasRequirements([ElephantRequirement, ZebraRequirement, ZebraRequirement])));

    [Fact]
    public void ReqHasRequirementsManyIdenticalIsStillSingle()
        => Single(requirementsService.CheckTac([
            new MockHasRequirements([ElephantRequirement, ZebraRequirement, ZebraRequirement]),
            new MockHasRequirements([ZebraRequirement]),
        ]));

}