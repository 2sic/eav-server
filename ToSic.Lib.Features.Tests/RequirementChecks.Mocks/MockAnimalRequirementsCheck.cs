using ToSic.Eav.Internal.Requirements;
using ToSic.Eav.SysData;

namespace ToSic.Lib.Features.Tests.RequirementChecks.Mocks;

internal class MockAnimalRequirementsCheck : RequirementCheckBase, IRequirementCheck
{
    public const string Animal = "animal";
    public const string Elephant = "elephant";
    public override string NameId => Animal;

    public override bool IsOk(Requirement requirement)
        => requirement.NameId == Elephant;

    public override string InfoIfNotOk(Requirement requirement)
        => $"The feature '{requirement.NameId}' is not enabled.";
}