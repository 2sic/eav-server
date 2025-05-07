using ToSic.Eav.SysData;

namespace ToSic.Eav.Internal.Requirements;

public record RequirementStatus(bool IsOk, Aspect Aspect, string? Message);