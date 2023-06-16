using ToSic.Lib.Documentation;

namespace ToSic.Lib.Logging
{
    [PrivateApi]
    internal static partial class LogExtensionsInternal
    {
        /// <summary>
        /// Get the real log object - either the object passed in, or if it's a LogCall, then the underlying log object.
        /// </summary>
        /// <param name="log"></param>
        /// <returns>The target Log object OR null</returns>
        internal static ILog GetRealLog(this ILog log) => log as Log ?? (log as ILogLike)?.Log ?? log;


        /// <summary>
        /// Add a message
        /// </summary>
        internal static Entry AddInternal(this ILog log, string message, CodeRef code, EntryOptions options = default)
        {
            // Null-check
            if (!(log.GetRealLog() is ILogInternal realLog)) return null;
            return realLog.CreateAndAdd(message, code, options);
        }

        internal static Entry AddInternalReuse(this ILog log, string message, CodeRef code) 
            => log.AddInternal(message, code) ?? new Entry(null, null, 0, code);
    }
}
