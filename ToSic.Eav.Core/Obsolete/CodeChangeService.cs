using System;
using ToSic.Lib.DI;
using ToSic.Lib.Logging;
using ToSic.Lib.Services;

namespace ToSic.Eav.Obsolete
{
    public partial class CodeChangeService: ServiceBase
    {
        private readonly LazySvc<CodeChangesInScope> _scope;

        public CodeChangeService(LazySvc<CodeChangesInScope> scope) : base(EavLogs.Eav + ".CodeCh")
        {
            _scope = scope;
        }

        public void Warn(ICodeChangeInfo change, /*int appId = default, string specificId = default,*/ Action<ILog> addMore = default)
        {
            if (change is null) return;
            var use = change as CodeChangeUse ?? new CodeChangeUse(change);
            LogObsolete(use/*, specificId, addMore*/);
            _scope.Value.Add(use/*, appId*/);

        }

    }
}
