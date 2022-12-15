using System;
using System.Runtime.CompilerServices;

namespace ToSic.Lib.Logging
{

    public static partial class LogExtensions
    {

        #region Intercept

        /// <summary>
        /// Intercept the result of an inner method, log it, then pass result on
        /// </summary>
        /// <returns></returns>
// TODO: This should be renamed to DoAndReturn() for clarity
// TODO: Not Null Safe! must fix
        public static T Return<T>(this ILog log,
            Func<T> generate,
            [CallerFilePath] string cPath = null,
            [CallerMemberName] string cName = null,
            [CallerLineNumber] int cLine = 0
        ) => log.Return(null, generate, cPath, cName, cLine);


        /// <summary>
        /// Intercept the result of an inner method, log it, then pass result on
        /// </summary>
        /// <returns></returns>
        /// <remarks>Is null-safe, so if there is no log, things still work and it still returns a valid <see cref="LogCall"/> </remarks>
// TODO: This should be renamed to DoAndReturn() for clarity
        public static T Return<T>(this ILog log,
            string message, 
            Func<T> generate,
            [CallerFilePath] string cPath = null,
            [CallerMemberName] string cName = null,
            [CallerLineNumber] int cLine = 0
        )
        {
            var callLog = log.Fn<T>(message: message, code: new CodeRef(cPath, cName, cLine));
            var result = generate();
            return callLog.ReturnAndLog(result);
        }
        #endregion
    }
}
