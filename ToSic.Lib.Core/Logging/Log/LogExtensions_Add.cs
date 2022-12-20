using System;
using System.Runtime.CompilerServices;
using ToSic.Lib.Documentation;

namespace ToSic.Lib.Logging
{

    public static partial class LogExtensions
    {
        /// <remarks>Is null-safe, so if there is no log, things still work</remarks>
        public static void A(this ILog log,
            string message,
            [CallerFilePath] string cPath = default,
            [CallerMemberName] string cName = default,
            [CallerLineNumber] int cLine = default
        ) => log?.AddInternal(message, CodeRef.Create(cPath, cName, cLine));


        /// <remarks>Is null-safe, so if there is no log, things still work</remarks>
        public static void A(this ILog log,
            Func<string> messageMaker,
            [CallerFilePath] string cPath = default,
            [CallerMemberName] string cName = default,
            [CallerLineNumber] int cLine = default
        ) => log?.AddInternal(LogExtensionsInternal.Try(messageMaker), CodeRef.Create(cPath, cName, cLine));

        [PrivateApi]
        /// <remarks>Is null-safe, so if there is no log, things still work</remarks>
        // TODO: @2dm - I think this is a very niche use case, we should probably remove this API
        public static string AddAndReuse(this ILog log,
            string message,
            [CallerFilePath] string cPath = default,
            [CallerMemberName] string cName = default,
            [CallerLineNumber] int cLine = default
        ) => log?.AddInternalReuse(message, CodeRef.Create(cPath, cName, cLine)).Message;


        [PrivateApi]
        /// <remarks>Is null-safe, so if there is no log, things still work</remarks>
        public static void A(this ILog log,
            bool enabled, 
            string message,
            [CallerFilePath] string cPath = default,
            [CallerMemberName] string cName = default,
            [CallerLineNumber] int cLine = default
        ) { if (enabled) log?.AddInternal(message, CodeRef.Create(cPath, cName, cLine)); }

        /// <remarks>Is null-safe, so if there is no log, things still work</remarks>
        public static void W(this ILog log,
            string message,
            [CallerFilePath] string cPath = default,
            [CallerMemberName] string cName = default,
            [CallerLineNumber] int cLine = default
        ) => log.AddInternal(LogConstants.WarningPrefix + message, CodeRef.Create(cPath, cName, cLine));

        /// <remarks>Is null-safe, so if there is no log, things still work</remarks>
        public static void E(this ILog log,
            string message,
            [CallerFilePath] string cPath = default,
            [CallerMemberName] string cName = default,
            [CallerLineNumber] int cLine = default
        ) => log.AddInternal(LogConstants.ErrorPrefix + message, CodeRef.Create(cPath, cName, cLine));

    }
}
