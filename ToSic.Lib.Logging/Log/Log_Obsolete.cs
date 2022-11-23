//using System;
//using System.Runtime.CompilerServices;

//namespace ToSic.Lib.Logging
//{
//    public partial class Log
//    {
//        /// <inheritdoc />
//        [Obsolete("Will remove soon - probably v15 as it's probably internal only")]
//        public string Add(string message,
//            [CallerFilePath] string cPath = null,
//            [CallerMemberName] string cName = null,
//            [CallerLineNumber] int cLine = 0)
//        {
//            this.AddInternal(message, new CodeRef(cPath, cName, cLine));
//            return message;
//        }

//        public void Exception(Exception ex) => this?.Ex(ex);

//        /// <inheritdoc />
//        [Obsolete("Do not use any more! Use extension method Fn instead")]
//        public Action<string> Call(string parameters = null, string message = null,
//            bool useTimer = false,
//            [CallerFilePath] string cPath = null,
//            [CallerMemberName] string cName = null,
//            [CallerLineNumber] int cLine = 0)
//            => Wrapper(
//                $"{cName}({parameters}) {message}",
//                useTimer,
//                new CodeRef(cPath, cName, cLine)
//            );

//    }
//}
