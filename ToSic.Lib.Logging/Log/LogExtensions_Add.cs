using System;
using System.Runtime.CompilerServices;

namespace ToSic.Lib.Logging
{

    public static partial class LogExtensions
    {
        public static void A(this ILog log,
            string message,
            [CallerFilePath] string cPath = null,
            [CallerMemberName] string cName = null,
            [CallerLineNumber] int cLine = 0
        ) => log?.AddInternal(message, new CodeRef(cPath, cName, cLine));


        /// <remarks>Is null-safe, so if there is no log, things still work</remarks>
        public static void A(this ILog log,
            Func<string> messageMaker,
            [CallerFilePath] string cPath = null,
            [CallerMemberName] string cName = null,
            [CallerLineNumber] int cLine = 0
        ) => log?.AddInternal(LogExtensionsInternal.Try(messageMaker), new CodeRef(cPath, cName, cLine));

        /// <remarks>Is null-safe, so if there is no log, things still work</remarks>
        public static string AddAndReuse(this ILog log,
            string message,
            [CallerFilePath] string cPath = null,
            [CallerMemberName] string cName = null,
            [CallerLineNumber] int cLine = 0
        ) => log?.AddInternalReuse(message, new CodeRef(cPath, cName, cLine)).Message;


        /// <remarks>Is null-safe, so if there is no log, things still work</remarks>
        public static void A(this ILog log,
            bool enabled, 
            string message,
            [CallerFilePath] string cPath = null,
            [CallerMemberName] string cName = null,
            [CallerLineNumber] int cLine = 0
        ) { if (enabled) log?.AddInternal(message, new CodeRef(cPath, cName, cLine)); }

        /// <remarks>Is null-safe, so if there is no log, things still work</remarks>
        public static void W(this ILog log,
            string message,
            [CallerFilePath] string cPath = null,
            [CallerMemberName] string cName = null,
            [CallerLineNumber] int cLine = 0
        )
            => log.A("WARNING: " + message, cPath, cName, cLine);
    }
}
