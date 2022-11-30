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
            [CallerFilePath] string cPath = null,
            [CallerMemberName] string cName = null,
            [CallerLineNumber] int cLine = 0
        ) => log?.ExceptionInternal(ex, new CodeRef(cPath, cName, cLine));

    }
}
