using ToSic.Eav.SysData;
using ToSic.Lib.DI;
using ToSic.Lib.Services;

namespace ToSic.Eav.Internal.Requirements;

/// <summary>
/// Internal service to check if a requirement has been met
/// </summary>
public class RequirementsService(LazySvc<ServiceSwitcher<IRequirementCheck>> checkers)
    : ServiceBase("Lib.ReqSvc", connect: [checkers])
{
    protected LazySvc<ServiceSwitcher<IRequirementCheck>> Checkers { get; } = checkers;

    public List<RequirementError> Check(IEnumerable<IHasRequirements> withRequirements)
    {
        var l = Log.Fn<List<RequirementError>>();
        var result = withRequirements
                         ?.SelectMany(Check)
                         .ToList()
                     ?? [];
        return l.Return(result, $"{result.Count} requirements failed");
    }

    public List<RequirementError> Check(IHasRequirements withRequirements) 
        => Check(withRequirements?.Requirements);

    public List<RequirementError> Check(List<Requirement> requirements)
    {
        if (requirements == null || requirements.Count == 0)
            return [];
        return requirements.Select(Check)
            .Where(c => c != null)
            .ToList();
    }

    public RequirementError Check(Requirement requirement)
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