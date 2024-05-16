using ToSic.Eav.SysData;
using ToSic.Lib.DI;
using ToSic.Lib.Services;

namespace ToSic.Eav.Internal.Requirements;

/// <summary>
/// Internal service to check if a requirement has been met
/// </summary>
public class RequirementsService(LazySvc<ServiceSwitcher<IRequirementCheck>> checkers)
    : ServiceBase(EavLogs.Eav + "ReqSvc", connect: [checkers])
{
    protected LazySvc<ServiceSwitcher<IRequirementCheck>> Checkers { get; } = checkers;

    public List<RequirementError> Check(IEnumerable<IHasRequirements> withRequirements) => Log.Func(timer: true, func: () =>
    {
        var result = withRequirements?.SelectMany(Check).ToList() ?? [];
        return (result, $"{result.Count} requirements failed");
    });

    public List<RequirementError> Check(IHasRequirements withRequirements) 
        => Check(withRequirements?.Requirements);

    public List<RequirementError> Check(List<Requirement> requirements)
    {
        if (requirements == null || requirements.Count == 0) return [];
        return requirements.Select(Check).Where(c => c != null).ToList();
    }

    public RequirementError Check(Requirement requirement)
    {
        if (requirement == null) return null;

        var checker = Checkers.Value.ByNameId(requirement.Type);

        // TODO: ERROR IF CHECKER NOT FOUND
        // Must wait till we implement all checkers, ATM just feature
        // Once other checkers like LicenseChecker are implemented
        // We may refactor the license to just be a requirement
        if (checker == null) return null;

        if (checker.IsOk(requirement)) return null;

        return new(requirement,
            $"Condition '{requirement.Type}.{requirement.NameId}' is not met. " + checker.InfoIfNotOk(requirement));
    }
}