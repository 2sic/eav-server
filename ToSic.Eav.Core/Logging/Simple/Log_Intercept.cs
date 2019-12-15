using System;
using System.Runtime.CompilerServices;

namespace ToSic.Eav.Logging.Simple
{
    public partial class Log
    {
        /// <summary>
        /// Intercept the result of an inner method, log it, then pass result on
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message"></param>
        /// <param name="generate"></param>
        /// <returns></returns>
        public T Intercept<T>(string message, Func<T> generate,
            [CallerFilePath] string cPath = null,
            [CallerMemberName] string cName = null,
            [CallerLineNumber] int cLine = 0
            )
        {
            var result = generate();
            var e = AddInternal($"{message}", new CodeRef(cPath, cName, cLine));
            e.AppendResult($"{result}");
            return result;
        }
    }
}
