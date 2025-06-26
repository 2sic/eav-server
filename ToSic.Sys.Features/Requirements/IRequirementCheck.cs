using ToSic.Lib.DI;

namespace ToSic.Sys.Requirements;

public interface IRequirementCheck: ISwitchableService
{
    bool IsOk(Requirement requirement);

    string InfoIfNotOk(Requirement requirement);

    RequirementStatus Status(Requirement requirement);
}