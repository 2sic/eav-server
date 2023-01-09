using System;
using System.Runtime.CompilerServices;
using ToSic.Lib.Documentation;

namespace ToSic.Lib.Logging
{
    // ReSharper disable once InconsistentNaming
    public static partial class ILogExtensions
    {
        [PrivateApi("not in use yet, better keep secret for now")]
        public static void Ex(this ILog log,
            Exception exception,
            CodeRef codeRef
            ) => log?.ExceptionInternal(exception, codeRef);

        /// <summary>
        /// Add a **Exception** to the log.
        /// </summary>
        /// <param name="log">The log object (or null)</param>
        /// <param name="exception">The exception object.</param>
        /// <param name="cPath">Caller file path - automatically added by the compiler</param>
        /// <param name="cName">Caller method/property name, automatically added by the compiler</param>
        /// <param name="cLine">The line number in the code, automatically added by the compiler</param>
        /// <remarks>Is null-safe, so if there is no log, things still work</remarks>
        public static void Ex(this ILog log,
            Exception exception,
            [CallerFilePath] string cPath = default,
            [CallerMemberName] string cName = default,
            [CallerLineNumber] int cLine = default
        ) => log?.ExceptionInternal(exception, CodeRef.Create(cPath, cName, cLine));

    }
}
