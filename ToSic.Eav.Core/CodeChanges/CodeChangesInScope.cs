using System;
using System.Collections.Generic;
using ToSic.Eav.Plumbing;
using ToSic.Lib.Helpers;

namespace ToSic.Eav.CodeChanges
{
    public class CodeChangesInScope
    {
        public readonly CodeChangeStats CodeChangeStats;
        public CodeChangesInScope(CodeChangeStats codeChangeStats)
        {
            CodeChangeStats = codeChangeStats;
        }

        public IEnumerable<CodeChangeLogged> List => _list;
        private readonly List<CodeChangeLogged> _list = new List<CodeChangeLogged>();

        /// <summary>
        /// Add it to the list and ensure that any known specs are also included
        /// </summary>
        /// <param name="codeChangeUse"></param>
        internal void Add(CodeChangeLogged codeChangeUse)
        {
            if (codeChangeUse == null) return;
            codeChangeUse.EntryOrNull?.UpdateSpecs(Specs);
            _list.Add(codeChangeUse);
            CodeChangeStats.Register(codeChangeUse.EntryOrNull);
        }

        /// <summary>
        /// Add context information and update anything that was previously added
        /// </summary>
        /// <param name="specsFactory"></param>
        public void AddContext(Func<IDictionary<string, string>> specsFactory, string entryPoint = default)
        {
            if (entryPoint != null) EntryPoint = entryPoint;
            if (specsFactory == null) return;
            _specsFactory = specsFactory;
            _specs.Reset();

            // If nothing to add, ignore.
            if (!_list.SafeAny()) return;

            // We could use some specs, so let's get them
            var specs = Specs;
            foreach (var logged in _list)
                logged?.EntryOrNull?.UpdateSpecs(specs);
        }
        internal string EntryPoint { get; private set; }

        private IDictionary<string, string> Specs => _specs.Get(() =>
        {
            try
            {
                return _specsFactory?.Invoke();
            }
            catch
            {
                return null;
            }
        });

        
        private readonly GetOnce<IDictionary<string, string>> _specs = new GetOnce<IDictionary<string, string>>();
        private Func<IDictionary<string, string>> _specsFactory;
    }
}
