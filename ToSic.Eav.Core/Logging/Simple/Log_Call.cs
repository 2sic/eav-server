using System;
using System.Runtime.CompilerServices;

namespace ToSic.Eav.Logging.Simple
{
    public partial class Log
    {
        /// <inheritdoc />
        public Action<string> Call(string parameters = null, string message = null,
            bool useTimer = false,
            [CallerFilePath] string cPath = null,
            [CallerMemberName] string cName = null,
            [CallerLineNumber] int cLine = 0)
            => Wrapper(
                $"{cName}({parameters}) {message}",
                useTimer,
                new CodeRef(cPath, cName, cLine)
            );

        /// <inheritdoc />
        public Action<string> Call(Func<string> parameters, Func<string> message = null,
            bool useTimer = false,
            [CallerFilePath] string cPath = null,
            [CallerMemberName] string cName = null,
            [CallerLineNumber] int cLine = 0)
            => Call(parameters: Try(parameters),
                message: message != null ? Try(message) : null,
                useTimer: useTimer,
                cPath: cPath,
                cName: cName,
                cLine: cLine
            );



        /// <summary>
        /// Add a log entry for method call, returning a method to call when done
        /// </summary>
        public Func<string, T, T> Call<T>(string methodName, string @params = null, string message = null,
            bool useTimer = false,
            [CallerFilePath] string cPath = null,
            [CallerMemberName] string cName = null,
            [CallerLineNumber] int cLine = 0
        )
            => Wrapper<T>($"{methodName}({@params}) {message}", useTimer,
                new CodeRef(cPath, cName, cLine)
            );


    }
}
