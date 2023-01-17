using System;
using System.Runtime.CompilerServices;
using ToSic.Lib.Documentation;

namespace ToSic.Lib.Logging
{
    // ReSharper disable once InconsistentNaming
    public static partial class ILog_Add
    {
        [PrivateApi("not in use yet, better keep secret for now")]
        internal static void ExWithCode(this ILog log,
            Exception exception,
            CodeRef codeRef
            ) => log?.ExceptionInternal(exception, codeRef);

        /// <summary>
        /// Add a **Exception** to the log.
        /// </summary>
        /// <param name="log">The log object (or null)</param>
        /// <param name="exception">The exception object.</param>
        /// <param name="cPath">Code file path, auto-added by compiler</param>
        /// <param name="cName">Code method name, auto-added by compiler</param>
        /// <param name="cLine">Code line number, auto-added by compiler</param>
        /// <remarks>Is null-safe, so if there is no log, things still work</remarks>
        public static TException Ex<TException>(this ILog log,
            TException exception,
            [CallerFilePath] string cPath = default,
            [CallerMemberName] string cName = default,
            [CallerLineNumber] int cLine = default
        ) where TException : Exception
            => log?.ExceptionInternal(exception, CodeRef.Create(cPath, cName, cLine));

    }
}
