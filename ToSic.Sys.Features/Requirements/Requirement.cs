namespace ToSic.Sys.Requirements;

/// <summary>
/// 
/// </summary>
/// <param name="Type">The type of requirement, such as `feature` or ``</param>
/// <param name="NameId">
/// The string identifier of this condition such as `CSharp12`
/// </param>
[ShowApiWhenReleased(ShowApiMode.Never)]
public record Requirement(string Type, string NameId);