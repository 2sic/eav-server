using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using ToSic.Lib.Documentation;

namespace ToSic.Lib.Logging
{
    /// <summary>
    /// Various extensions for <see cref="ILog"/> objects to add logs.
    /// They are all implemented as extension methods, so that they will not fail even if the log object is null.
    /// </summary>
    [PublicApi]
    // ReSharper disable once InconsistentNaming
    public static partial class ILog_Add
    {
        /// <summary>
        /// Add a message to the log.
        /// </summary>
        /// <param name="log">The log object (or null)</param>
        /// <param name="message">The message to add</param>
        /// <param name="cPath">Code file path, auto-added by compiler</param>
        /// <param name="cName">Code method name, auto-added by compiler</param>
        /// <param name="cLine">Code line number, auto-added by compiler</param>
        /// <remarks>Is null-safe, so if there is no log, things still work</remarks>
        public static void A(this ILog log,
            string message,
            [CallerFilePath] string cPath = default,
            [CallerMemberName] string cName = default,
            [CallerLineNumber] int cLine = default,
            EntryOptions options = default
        ) => log.AddInternal(message, CodeRef.Create(cPath, cName, cLine), options);


        /// <summary>
        /// Add a message to the log.
        /// </summary>
        /// <param name="log">The log object (or null)</param>
        /// <param name="enabled">If true, will add the message, otherwise not</param>
        /// <param name="message">The message to add</param>
        /// <param name="cPath">Code file path, auto-added by compiler</param>
        /// <param name="cName">Code method name, auto-added by compiler</param>
        /// <param name="cLine">Code line number, auto-added by compiler</param>
        /// <remarks>Is null-safe, so if there is no log, things still work</remarks>
        [PrivateApi]
        public static void A(this ILog log,
            bool enabled, 
            string message,
            [CallerFilePath] string cPath = default,
            [CallerMemberName] string cName = default,
            [CallerLineNumber] int cLine = default,
            EntryOptions options = default
        ) { if (enabled) log.AddInternal(message, CodeRef.Create(cPath, cName, cLine), options); }

        /// <summary>
        /// Add a **warning** to the log.
        /// </summary>
        /// <param name="log">The log object (or null)</param>
        /// <param name="message">The message to add</param>
        /// <param name="cPath">Code file path, auto-added by compiler</param>
        /// <param name="cName">Code method name, auto-added by compiler</param>
        /// <param name="cLine">Code line number, auto-added by compiler</param>
        /// <remarks>Is null-safe, so if there is no log, things still work</remarks>
        public static void W(this ILog log,
            string message,
            [CallerFilePath] string cPath = default,
            [CallerMemberName] string cName = default,
            [CallerLineNumber] int cLine = default
        ) => log.AddInternal(LogConstants.WarningPrefix + message, CodeRef.Create(cPath, cName, cLine));

        /// <summary>
        /// Add an **error** to the log.
        /// </summary>
        /// <param name="log">The log object (or null)</param>
        /// <param name="message">The message to add</param>
        /// <param name="cPath">Code file path, auto-added by compiler</param>
        /// <param name="cName">Code method name, auto-added by compiler</param>
        /// <param name="cLine">Code line number, auto-added by compiler</param>
        /// <remarks>Is null-safe, so if there is no log, things still work</remarks>
        public static void E(this ILog log,
            string message,
            [CallerFilePath] string cPath = default,
            [CallerMemberName] string cName = default,
            [CallerLineNumber] int cLine = default
        ) => log.AddInternal(LogConstants.ErrorPrefix + message, CodeRef.Create(cPath, cName, cLine));

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
