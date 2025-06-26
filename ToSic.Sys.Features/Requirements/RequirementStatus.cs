using ToSic.Sys.Capabilities.Aspects;

namespace ToSic.Sys.Requirements;

public record RequirementStatus(bool IsOk, Aspect Aspect, string? Message);