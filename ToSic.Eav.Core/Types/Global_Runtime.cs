using System.Collections.Generic;
using ToSic.Eav.Interfaces;

namespace ToSic.Eav.Types
{
    public partial class Global
    {

        /// <summary>
        /// All content-types available in Reflection; will cache after first scan
        /// </summary>
        /// <returns></returns>
        internal static IEnumerable<IContentType> ContentTypesInRuntime()
        {
            if (_runtimeCache != null) return _runtimeCache;

            var runtime = Factory.Resolve<IRuntime>();
            _runtimeCache = runtime?.LoadGlobalContentTypes() ?? new List<IContentType>();
            return _runtimeCache;
        }

        private static IEnumerable<IContentType> _runtimeCache;


    }
}
