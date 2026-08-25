using ToSic.Sys.Capabilities.Aspects;

namespace ToSic.Sys.Requirements;

/// <inheritdoc cref="IRequirementsService"/>
[ShowApiWhenReleased(ShowApiMode.Never)]
internal sealed class RequirementsService(Generator<IRequirementCheck> checkGenerator)
    : ServiceBase("Lib.ReqSvc", connect: [checkGenerator]), IRequirementsService
{
    /// <inheritdoc/>
    public IEnumerable<RequirementStatus> Status(IEnumerable<IHasRequirements> withRequirements)
    {
        var l = Log.Fn<IEnumerable<RequirementStatus>>();

        var list = Status(withRequirements.SelectMany(r => r.Requirements))
            .ToListOpt();
        
        return l.Return(list, $"{list.Count} requirements failed");
    }
    

    /// <inheritdoc/>
    public IEnumerable<RequirementStatus> Status(IHasRequirements hasRequirements) 
        => Status(hasRequirements.Requirements);

    /// <inheritdoc/>
    public IEnumerable<RequirementStatus> Status(IEnumerable<Requirement> requirements)
        => requirements
            .Select(StatusInternal)
            .Distinct()
            .ToListOpt();

    /// <inheritdoc/>
    public IEnumerable<RequirementStatus> Issues(IEnumerable<IHasRequirements> withRequirements)
        => Status(withRequirements).Where(r => !r.IsOk);
    
    /// <inheritdoc/>
    public IEnumerable<RequirementStatus> Issues(IHasRequirements hasRequirements)
        => Status(hasRequirements).Where(r => !r.IsOk);
    
    /// <inheritdoc/>
    public IEnumerable<RequirementStatus> Issues(IEnumerable<Requirement> requirements)
        => Status(requirements).Where(r => !r.IsOk);

    /// <summary>
    /// Check a single requirement.
    /// </summary>
    /// <remarks>
    /// IMPORTANT: This must remain private, but internal for testing.
    /// All calls should only use the signature which returns lists of possible issues,
    /// so that they never need to do null checks.
    /// </remarks>
    /// <returns>An error object or `null`</returns>
    internal RequirementStatus StatusInternal(Requirement requirement)
    {
        var checker = checkGenerator.TryNew(requirement.Type);

        // TODO: ERROR IF CHECKER NOT FOUND
        // Must wait till we implement all checkers, ATM just feature
        // Once other checkers like LicenseChecker are implemented
        // We may refactor the license to just be a requirement
        return checker?.Status(requirement)
            ?? new RequirementStatus(false, requirement, Aspect.UnknownChecker(requirement.Type), $"No checker found for requirement type {requirement.Type}");
    }
}
