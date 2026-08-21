namespace ToSic.Sys.Requirements;

/// <summary>
/// Internal service to check if certain requirements have been met.
/// </summary>
[InternalApi_DoNotUse_MayChangeWithoutNotice]
public interface IRequirementsService
{
    /// <summary>
    /// Check requirements of multiple objects implementing IHasRequirements.
    /// </summary>
    /// <param name="withRequirements">A list of objects implementing IHasRequirements</param>
    /// <returns>A list of issues or an empty list if all is ok</returns>
    IEnumerable<RequirementIssue> Check(IEnumerable<IHasRequirements> withRequirements);

    /// <summary>
    /// Check all requirements of an object implementing IHasRequirements
    /// </summary>
    /// <param name="hasRequirements"></param>
    /// <returns>A list of issues or an empty list if all is ok</returns>
    IEnumerable<RequirementIssue> Check(IHasRequirements hasRequirements);

    /// <summary>
    /// Check a list of requirements.
    /// </summary>
    /// <param name="requirements"></param>
    /// <returns>A list of issues or an empty list if all is ok</returns>
    IEnumerable<RequirementIssue> Check(IEnumerable<Requirement> requirements);
}
