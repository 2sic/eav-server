using ToSic.Eav.SysData;
using ToSic.Lib.DI;

namespace ToSic.Eav.Internal.Requirements;

public interface IRequirementCheck: ISwitchableService
{
    bool IsOk(Requirement requirement);

    string InfoIfNotOk(Requirement requirement);

    RequirementStatus Status(Requirement requirement);
}