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
        public void Warn(CodeChangeUse use)
        {
            if (use is null) return;
            var logged = WarnObsolete(use);
            _scope.Value.AddObsolete(logged);
        }

        public void Warn(ICodeChangeInfo change) => Warn(change == null ? null : new CodeChangeUse(change));
        //{
        //    if (change is null) return;
        //    var use = new CodeChangeUse(change);
        //    var logged = WarnObsolete(use);
        //    _scope.Value.AddObsolete(logged);
        //}

        /// <summary>
        /// Quick helper to warn and return an object. 
        /// </summary>
        public T GetAndWarn<T>(ICodeChangeInfo change, T result)
        {
            Warn(change);
            return result;;
        }
    }
}
