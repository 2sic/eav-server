using ToSic.Sys.Capabilities.Aspects;

namespace ToSic.Sys.Requirements;

[ShowApiWhenReleased(ShowApiMode.Never)]
public record RequirementStatus(bool IsOk, Aspect Aspect, string? Message);