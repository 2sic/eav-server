using System;

namespace ToSic.Lib.Logging
{
    internal static partial class LogExtensionsInternal
    {
        private const int MaxExceptionRecursion = 100;

        internal static void ExceptionInternal(this ILog log, Exception ex, CodeRef codeRef)
        {
            // Null-check
            if (!(log?._RealLog is Log realLog)) return;

            var wrapLog = realLog.Fn(message: "Will log Exception Details next", code: codeRef);
            var recursion = 1;
            while (true)
            {
                // avoid infinite loops
                if (recursion >= MaxExceptionRecursion)
                {
                    wrapLog.Done("max-depth reached");
                    return;
                }
                
                if (ex == null)
                {
                    wrapLog.Done("Exception is null");
                    return;
                }

                realLog.A($"Depth {recursion} in {ex.Source}: {ex}"); // use the default ToString of an exception

                if (ex.InnerException != null)
                {
                    ex = ex.InnerException;
                    recursion++;
                    continue;
                }

                break;
            }
            wrapLog.Done();
        }
    }
}
