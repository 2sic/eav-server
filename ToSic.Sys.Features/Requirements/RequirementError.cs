namespace ToSic.Sys.Requirements;

[ShowApiWhenReleased(ShowApiMode.Never)]
public record RequirementError(Requirement Requirement, string Message);