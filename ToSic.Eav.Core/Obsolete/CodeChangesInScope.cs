using System;
using System.Collections.Generic;
using ToSic.Eav.Plumbing;

namespace ToSic.Eav.Obsolete
{
    public class CodeChangesInScope
    {
        public IEnumerable<CodeChangeLogged> List => _list;
        private readonly List<CodeChangeLogged> _list = new List<CodeChangeLogged>();


        internal void Add(CodeChangeLogged codeChangeUse) => _list.Add(codeChangeUse);

        public void AddContext(Func<IDictionary<string, string>> specsFactory)
        {
            // If nothing to add, ignore.
            if (!_list.SafeAny() || specsFactory == null) return;

            // We could use some specs, so let's get them
            try
            {
                var specs = specsFactory();
                foreach (var logged in _list)
                    logged?.EntryOrNull?.UpdateSpecs(specs);
            }
            catch
            {
                /* ignore */
            }
        }
    }
}
