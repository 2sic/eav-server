using System;
using System.Runtime.CompilerServices;
using ToSic.Lib.Documentation;

namespace ToSic.Lib.Logging
{
    // ReSharper disable once InconsistentNaming
    public static partial class ILogExtensions
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
            [CallerFilePath] string cPath = default,
            [CallerMemberName] string cName = default,
            [CallerLineNumber] int cLine = default
        ) => log.Return(null, generate, cPath, cName, cLine);


        /// <summary>
        /// Intercept the result of an inner method, log it, then pass result on
        /// </summary>
        /// <returns></returns>
        [PrivateApi]
// TODO: This should be renamed to DoAndReturn() for clarity
        public static T Return<T>(this ILog log,
            string message, 
            Func<T> generate,
            [CallerFilePath] string cPath = default,
            [CallerMemberName] string cName = default,
            [CallerLineNumber] int cLine = default
        )
        {
            var callLog = log.GetRealLog().Fn<T>(message: message, cPath: cPath, cName: cName, cLine: cLine);
            var result = generate();
            return callLog.ReturnAndLog(result);
        }
        #endregion
    }
}
