namespace ToSic.Sys.Requirements;

/// <summary>
/// Internal service to check if a requirement has been met
/// </summary>
[ShowApiWhenReleased(ShowApiMode.Never)]
public class RequirementsService(Generator<IRequirementCheck> checkGenerator)
    : ServiceBase("Lib.ReqSvc", connect: [checkGenerator])
{
    public IEnumerable<RequirementError> Check(IEnumerable<IHasRequirements> withRequirements)
    {
        var l = Log.Fn<IEnumerable<RequirementError>>();

        var list = Check(withRequirements
                .SelectMany(r => r.Requirements)
                .ToListOpt())
            .ToListOpt();
        
        return l.Return(list, $"{list.Count} requirements failed");
    }

    /// <summary>
    /// Check all requirements of an object implementing IHasRequirements
    /// </summary>
    /// <param name="hasRequirements"></param>
    /// <returns></returns>
    public IEnumerable<RequirementError> Check(IHasRequirements hasRequirements) 
        => Check(hasRequirements.Requirements);

    /// <summary>
    /// Check a list of requirements.
    /// </summary>
    /// <param name="requirements"></param>
    /// <returns>A list of error objects or an empty list if all is ok</returns>
    public IEnumerable<RequirementError> Check(IEnumerable<Requirement> requirements)
        => requirements
            .Select(Check)
            .OfType<RequirementError>()
            .Distinct()
            .ToListOpt();

    /// <summary>
    /// Check a single requirement.
    /// </summary>
    /// <remarks>
    /// IMPORTANT: This should remain internal.
    /// All calls should only use the signature which returns lists of possible issues,
    /// so that they never need to do null checks.
    /// </remarks>
    /// <returns>An error object or `null`</returns>
    internal RequirementError? Check(Requirement requirement)
    {
        var checker = checkGenerator.TryNew(requirement.Type);

        // TODO: ERROR IF CHECKER NOT FOUND
        // Must wait till we implement all checkers, ATM just feature
        // Once other checkers like LicenseChecker are implemented
        // We may refactor the license to just be a requirement
        if (checker == null)
            return null;

        if (checker.IsOk(requirement))
            return null;

        return new(requirement,
            $"Condition '{requirement.Type}.{requirement.NameId}' is not met. " + checker.InfoIfNotOk(requirement));
    }
}