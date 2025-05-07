using ToSic.Eav.SysData;

namespace ToSic.Eav.Internal.Requirements;

public record RequirementError(Requirement Requirement, string Message);