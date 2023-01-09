using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using ToSic.Lib.Documentation;

namespace ToSic.Lib.Logging
{
    // ReSharper disable once InconsistentNaming
    public static partial class ILogExtensions
    {
        /// <summary>
        /// Add a message to the log.
        /// </summary>
        /// <param name="log">The log object (or null)</param>
        /// <param name="message">The message to add</param>
        /// <param name="cPath">Caller file path - automatically added by the compiler</param>
        /// <param name="cName">Caller method/property name, automatically added by the compiler</param>
        /// <param name="cLine">The line number in the code, automatically added by the compiler</param>
        /// <remarks>Is null-safe, so if there is no log, things still work</remarks>
        public static void A(this ILog log,
            string message,
            [CallerFilePath] string cPath = default,
            [CallerMemberName] string cName = default,
            [CallerLineNumber] int cLine = default
        ) => log.AddInternal(message, CodeRef.Create(cPath, cName, cLine));


        ///// <remarks>Is null-safe, so if there is no log, things still work</remarks>
        //[PrivateApi("advanced case, probably don't publish or rework to use a helper...")]
        //public static void A(this ILog log,
        //    Func<string> messageMaker,
        //    [CallerFilePath] string cPath = default,
        //    [CallerMemberName] string cName = default,
        //    [CallerLineNumber] int cLine = default
        //) => log.AddInternal(LogExtensionsInternal.Try(messageMaker), CodeRef.Create(cPath, cName, cLine));


        [PrivateApi]
        /// <remarks>Is null-safe, so if there is no log, things still work</remarks>
        public static void A(this ILog log,
            bool enabled, 
            string message,
            [CallerFilePath] string cPath = default,
            [CallerMemberName] string cName = default,
            [CallerLineNumber] int cLine = default
        ) { if (enabled) log.AddInternal(message, CodeRef.Create(cPath, cName, cLine)); }

        /// <summary>
        /// Add a **warning** to the log.
        /// </summary>
        /// <param name="log">The log object (or null)</param>
        /// <param name="message">The message to add</param>
        /// <param name="cPath">Caller file path - automatically added by the compiler</param>
        /// <param name="cName">Caller method/property name, automatically added by the compiler</param>
        /// <param name="cLine">The line number in the code, automatically added by the compiler</param>
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

        ///// <summary>
        ///// WIP - maybe not relevant
        ///// </summary>
        ///// <param name="log"></param>
        //[PrivateApi]
        //public static void AddTimestamp(this ILog log) => log.A(DateTime.UtcNow.Dump());

        [PrivateApi]
        public static string Dump(this DateTime dateTime)
        {
            var utc = dateTime.ToUniversalTime();
            return $"Timestamp - Date/Time: {utc:o}; Ticks: {utc.Ticks.ToString("N0", AposThousandSeparator())}";
        }
        [PrivateApi]
        private static NumberFormatInfo AposThousandSeparator()
        {
            if (_aposSeparator != null) return _aposSeparator;
            _aposSeparator = CultureInfo.InvariantCulture.NumberFormat.Clone() as NumberFormatInfo;
            _aposSeparator.NumberGroupSeparator = "'";
            return _aposSeparator;
        }

        private static NumberFormatInfo _aposSeparator;
    }
}
