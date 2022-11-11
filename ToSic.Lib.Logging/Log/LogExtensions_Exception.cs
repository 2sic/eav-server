using System;

namespace ToSic.Lib.Logging
{
    public static partial class LogExtensions
    {
        public static void Ex(this ILog log, Exception ex) => log?.ExceptionInternal(ex);
    }
}
