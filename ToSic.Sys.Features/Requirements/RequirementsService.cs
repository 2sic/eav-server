using ToSic.Lib.DI;
using ToSic.Lib.Services;
using ToSic.Sys.Performance;

namespace ToSic.Sys.Requirements;

/// <summary>
/// Internal service to check if a requirement has been met
/// </summary>
public class RequirementsService(LazySvc<ServiceSwitcher<IRequirementCheck>> checkers)
    : ServiceBase("Lib.ReqSvc", connect: [checkers])
{
    /// <summary>
    /// List of all checkers.
    /// Internal so it can be unit tested.
    /// </summary>
    internal LazySvc<ServiceSwitcher<IRequirementCheck>> Checkers { get; } = checkers;


    public IEnumerable<RequirementError> Check(IEnumerable<IHasRequirements>? withRequirements)
    {
        var l = Log.Fn<IEnumerable<RequirementError>>();
        var list = withRequirements
                       ?.SelectMany(Check)
                       .Distinct()
                       .ToListOpt()
                   ?? [];
        return l.Return(list, $"{list.Count} requirements failed");
    }

    /// <summary>
    /// Check all requirements of an object implementing IHasRequirements
    /// </summary>
    /// <param name="hasRequirements"></param>
    /// <returns></returns>
    public IEnumerable<RequirementError> Check(IHasRequirements? hasRequirements) 
        => Check(hasRequirements?.Requirements);

    /// <summary>
    /// Check a list of requirements.
    /// </summary>
    /// <param name="requirements"></param>
    /// <returns>A list of error objects or an empty list if all is ok</returns>
    public IEnumerable<RequirementError> Check(IEnumerable<Requirement>? requirements)
    {
        var list = requirements?.ToListOpt();
        if (list == null || !list.Any())
            return [];

        return list
            .Select(Check)
            .Where(c => c != null)
            .Distinct()
            .ToListOpt()!;
    }

    /// <summary>
    /// Check a single requirement.
    /// </summary>
    /// <param name="requirement"></param>
    /// <returns>An error object or `null`</returns>
    public RequirementError? Check(Requirement? requirement)
    {
        if (requirement == null)
            return null;

        var checker = Checkers.Value.ByNameId(requirement.Type);

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