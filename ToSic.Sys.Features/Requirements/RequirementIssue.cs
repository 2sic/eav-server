namespace ToSic.Sys.Requirements;

/// <summary>
/// A requirement issue is a specific problem with a requirement.
/// Any issue means it has not been met.
/// </summary>
/// <param name="Requirement">The requirement that has an issue.</param>
/// <param name="Message">A message describing the issue with the requirement.</param>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
public record RequirementIssue(Requirement Requirement, string Message);