using System.Collections.Generic;
using ToSic.Eav.Interfaces;
using ToSic.Eav.Logging.Simple;

namespace ToSic.Eav.Types
{
    public partial class Global
    {

        /// <summary>
        /// All content-types available in Reflection; will cache after first scan
        /// </summary>
        /// <returns></returns>
        internal static IEnumerable<IContentType> ContentTypesInRuntime(Log log)
        {
            if (_runtimeCache != null) return _runtimeCache;

            log.Add("ContentTypesInRuntime() loading");
            var runtime = Factory.Resolve<IRuntime>();
            runtime?.LinkLog(log);
            _runtimeCache = runtime?.LoadGlobalContentTypes() ?? new List<IContentType>();
            return _runtimeCache;
        }

        private static IEnumerable<IContentType> _runtimeCache;


    }
}
