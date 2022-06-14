using System;
using System.Runtime.CompilerServices;

namespace ToSic.Eav.Logging.Simple
{
    public partial class Log
    {
        /// <inheritdoc />
        [Obsolete("Do not use any more! Use extension method Fn instead")]
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
        [Obsolete("Do not use any more! Use extension method Fn instead")]
        public Action<string> Call(Func<string> parameters, Func<string> message = null,
            bool useTimer = false,
            [CallerFilePath] string cPath = null,
            [CallerMemberName] string cName = null,
            [CallerLineNumber] int cLine = 0)
            => Call(parameters: LogExtensionsInternal.Try(parameters),
                message: message != null ? LogExtensionsInternal.Try(message) : null,
                useTimer: useTimer,
                cPath: cPath,
                cName: cName,
                cLine: cLine
            );



        /// <inheritdoc />
        [Obsolete("Do not use any more! Use extension method Fn instead")]
        public Func<string, T, T> Call<T>(string parameters = null,
            string message = null,
            bool useTimer = false,
            [CallerFilePath] string cPath = null,
            [CallerMemberName] string cName = null,
            [CallerLineNumber] int cLine = 0)
            => Wrapper<T>($"{cName}({parameters}) {message}", 
                useTimer,
                new CodeRef(cPath, cName, cLine)
            );


    }
}
