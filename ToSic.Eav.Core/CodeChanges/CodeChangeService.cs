using ToSic.Lib.DI;
using ToSic.Lib.Services;

namespace ToSic.Eav.CodeChanges
{
    public partial class CodeChangeService: ServiceBase
    {
        private readonly LazySvc<CodeChangesInScope> _scope;

        public CodeChangeService(LazySvc<CodeChangesInScope> scope) : base(EavLogs.Eav + ".CodeCh")
        {
            _scope = scope;
        }

        public void Warn(ICodeChangeInfo change)
        {
            if (change is null) return;
            var use = change as CodeChangeUse ?? new CodeChangeUse(change);
            var logged = LogObsolete(use);
            _scope.Value.Add(logged);
        }

    }
}
