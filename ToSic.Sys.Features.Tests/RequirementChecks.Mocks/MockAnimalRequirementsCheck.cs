﻿using ToSic.Sys.Capabilities.Aspects;
using ToSic.Sys.Requirements;

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

    protected override Aspect GetAspect(Requirement requirement)
        => Aspect.Custom(requirement.NameId, Guid.Empty);

}