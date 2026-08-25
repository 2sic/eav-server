using ToSic.Sys.Capabilities.Aspects;

namespace ToSic.Sys.Requirements;

/// <summary>
/// Feedback about a requirement check, including the requirement itself, the aspect of the requirement, and an optional message if the requirement is not met.
/// </summary>
/// <param name="IsOk">Indicates whether the requirement is met.</param>
/// <param name="Requirement">The requirement being checked.</param>
/// <param name="Aspect">The aspect of the requirement.</param>
/// <param name="Message">An optional message if the requirement is not met.</param>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
public record RequirementStatus(bool IsOk, Requirement Requirement, Aspect Aspect, string? Message);