using System.Collections.Immutable;
using ToSic.Eav.Data;
using ToSic.Eav.Logging;

namespace ToSic.Eav.Types
{
    public interface IGlobalTypes: IHasLog
    {
        /// <summary>
        /// Dictionary of code-provided content-types, caches after first scan
        /// </summary>
        /// <returns></returns>
        ImmutableDictionary<string, IContentType> AllContentTypes();

        IContentType FindContentType(string name);

        /// <summary>
        /// This is the real initializer - implemented as a virtual method, because some
        /// long-living objects must actively prevent logs from being attached. 
        /// </summary>
        /// <remarks>this one can be overridden by outside sources, like the cache which should never allow attaching logs at runtime</remarks>
        /// <param name="name"></param>
        /// <param name="parentLog"></param>
        /// <param name="initialMessage"></param>
        void InitLog(string name, ILog parentLog = null, string initialMessage = null);

        /// <summary>
        /// This starts the loading process - this should only ever be called once!
        /// </summary>
        void StartUp(ILog targetLogger);
    }
}
