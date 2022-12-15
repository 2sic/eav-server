using System;
using System.Runtime.CompilerServices;

namespace ToSic.Lib.Logging
{
    public static partial class LogExtensions
    {
        public static void Ex(this ILog log,
            Exception ex,
            CodeRef codeRef
            ) => log?.ExceptionInternal(ex, codeRef);

        public static void Ex(this ILog log,
            Exception ex,
            [CallerFilePath] string cPath = default,
            [CallerMemberName] string cName = default,
            [CallerLineNumber] int cLine = default
        ) => log?.ExceptionInternal(ex, new CodeRef(cPath, cName, cLine));

    }
}
