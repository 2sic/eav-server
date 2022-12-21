using System.Collections.Generic;
using System.Linq;
using ToSic.Lib.DI;
using ToSic.Lib.Logging;
using ToSic.Lib.Services;

namespace ToSic.Eav.Configuration
{
    /// <summary>
    /// Internal service to check if a requirement has been met
    /// </summary>
    public class RequirementsService: ServiceBase
    {
        public RequirementsService(LazyInit<ServiceSwitcher<IRequirementCheck>> checkers) : base(LogNames.Eav + "ReqSvc")
        {
            ConnectServices(
                Checkers = checkers
            );
        }

        protected LazyInit<ServiceSwitcher<IRequirementCheck>> Checkers { get; }

        public List<ConditionError> Check(IEnumerable<IHasRequirements> withRequirements)
        {
            var wrapLog = Log.Fn<List<ConditionError>>(startTimer: true);
            var result = withRequirements?.SelectMany(Check).ToList() ?? new List<ConditionError>();
            return wrapLog.Return(result, $"{result.Count} requirements failed");
        }

        public List<ConditionError> Check(IHasRequirements withRequirements) 
            => Check(withRequirements?.Requirements);

        public List<ConditionError> Check(List<Condition> requirements)
        {
            if (requirements == null || requirements.Count == 0) return new List<ConditionError>();
            return requirements.Select(Check).Where(c => c != null).ToList();
        }

        public ConditionError Check(Condition condition)
        {
            if (condition == null) return null;

            var checker = Checkers.Value.ByNameId(condition.Type);

            // TODO: ERROR IF CHECKER NOT FOUND
            // Must wait till we implement all checkers, ATM just feature
            // Once other checkers like LicenseChecker are implemented
            // We may refactor the license to just be a requirement
            if (checker == null) return null;

            if (checker.IsOk(condition)) return null;

            return new ConditionError(condition,
                $"Condition '{condition.Type}.{condition.NameId}' is not met. " + checker.InfoIfNotOk(condition));
        }
    }
}
