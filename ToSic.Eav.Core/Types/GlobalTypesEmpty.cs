using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using ToSic.Eav.Data;
using ToSic.Eav.Logging;

namespace ToSic.Eav.Types
{
    /// <summary>
    /// Empty Global Types - always returning empty lists.
    /// </summary>
    /// <remarks>
    /// WIP for the File AppState Loader
    /// </remarks>
    public class GlobalTypesEmpty: HasLog, IGlobalTypes
    {
        public GlobalTypesEmpty() : base(LogNames.Eav + ".GlbEmp") { }

        public ImmutableDictionary<string, IContentType> AllContentTypes() => _empty;
        private ImmutableDictionary<string, IContentType> _empty = new Dictionary<string, IContentType>().ToImmutableDictionary();

        public IContentType FindContentType(string name) => null;

        public new void InitLog(string name, ILog parentLog = null, string initialMessage = null)
        {
            Log.LinkTo(parentLog, name);
            if(initialMessage != null) Log.Add(initialMessage);
        }

        public void StartUp(ILog targetLogger)
        {
            // Do nothing
        }
    }
}
