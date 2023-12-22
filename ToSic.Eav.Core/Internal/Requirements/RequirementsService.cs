using System.Collections.Generic;
using System.Linq;
using ToSic.Eav.SysData;
using ToSic.Lib.DI;
using ToSic.Lib.Logging;
using ToSic.Lib.Services;

namespace ToSic.Eav.Internal.Requirements;

/// <summary>
/// Internal service to check if a requirement has been met
/// </summary>
public class RequirementsService: ServiceBase
{
    public RequirementsService(LazySvc<ServiceSwitcher<IRequirementCheck>> checkers) : base(EavLogs.Eav + "ReqSvc")
    {
        ConnectServices(
            Checkers = checkers
        );
    }

    protected LazySvc<ServiceSwitcher<IRequirementCheck>> Checkers { get; }

    public List<RequirementError> Check(IEnumerable<IHasRequirements> withRequirements) => Log.Func(timer: true, func: () =>
    {
        var result = withRequirements?.SelectMany(Check).ToList() ?? new List<RequirementError>();
        return (result, $"{result.Count} requirements failed");
    });

    public List<RequirementError> Check(IHasRequirements withRequirements) 
        => Check(withRequirements?.Requirements);

    public List<RequirementError> Check(List<Requirement> requirements)
    {
        if (requirements == null || requirements.Count == 0) return new List<RequirementError>();
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

        return new RequirementError(requirement,
            $"Condition '{requirement.Type}.{requirement.NameId}' is not met. " + checker.InfoIfNotOk(requirement));
    }
}