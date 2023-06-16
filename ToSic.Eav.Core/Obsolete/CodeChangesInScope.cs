using System.Collections.Generic;

namespace ToSic.Eav.Obsolete
{
    public class CodeChangesInScope
    {
        public CodeChangesInScope() { }

        internal void Add(ICodeChangeInfo change, int appId)
        {
            _list.Add(new CodeChangeUse(change, appId));
        }

        internal void Add(CodeChangeUse codeChangeUse) => _list.Add(codeChangeUse);

        public IEnumerable<CodeChangeUse> List => _list;
        private readonly List<CodeChangeUse> _list = new List<CodeChangeUse>();
    }
}
