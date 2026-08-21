namespace ToSic.Sys.Requirements;

/// <inheritdoc cref="IRequirementsService"/>
[ShowApiWhenReleased(ShowApiMode.Never)]
internal sealed class RequirementsService(Generator<IRequirementCheck> checkGenerator)
    : ServiceBase("Lib.ReqSvc", connect: [checkGenerator]), IRequirementsService
{
    /// <inheritdoc/>
    public IEnumerable<RequirementIssue> Check(IEnumerable<IHasRequirements> withRequirements)
    {
        var l = Log.Fn<IEnumerable<RequirementIssue>>();

        var list = Check(withRequirements
                .SelectMany(r => r.Requirements)
                .ToListOpt())
            .ToListOpt();
        
        return l.Return(list, $"{list.Count} requirements failed");
    }

    /// <inheritdoc/>
    public IEnumerable<RequirementIssue> Check(IHasRequirements hasRequirements) 
        => Check(hasRequirements.Requirements);

    /// <inheritdoc/>
    public IEnumerable<RequirementIssue> Check(IEnumerable<Requirement> requirements)
        => requirements
            .Select(CheckOneInternal)
            .OfType<RequirementIssue>()
            .Distinct()
            .ToListOpt();

    /// <summary>
    /// Check a single requirement.
    /// </summary>
    /// <remarks>
    /// IMPORTANT: This must remain internal.
    /// All calls should only use the signature which returns lists of possible issues,
    /// so that they never need to do null checks.
    /// </remarks>
    /// <returns>An error object or `null`</returns>
    internal RequirementIssue? CheckOneInternal(Requirement requirement)
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
