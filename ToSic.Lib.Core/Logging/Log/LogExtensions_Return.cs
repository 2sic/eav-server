using System;
using System.Runtime.CompilerServices;
using ToSic.Lib.Documentation;

namespace ToSic.Lib.Logging
{

    public static partial class LogExtensions
    {

        #region Return / ToDo: rename

        /// <summary>
        /// Intercept the result of an inner method, log it, then pass result on
        /// </summary>
        /// <returns></returns>
        // TODO: This should be renamed to DoAndReturn() for clarity
        // TODO: Not Null Safe! must fix
        [PrivateApi]
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
        [PrivateApi]
// TODO: This should be renamed to DoAndReturn() for clarity
        public static T Return<T>(this ILog log,
            string message, 
            Func<T> generate,
            [CallerFilePath] string cPath = null,
            [CallerMemberName] string cName = null,
            [CallerLineNumber] int cLine = 0
        )
        {
            var callLog = (log?._RealLog).Fn<T>(message: message, code: CodeRef.Create(cPath, cName, cLine));
            var result = generate();
            return callLog.ReturnAndLog(result);
        }
        #endregion
    }
}
