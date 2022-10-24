using System;
using System.Runtime.CompilerServices;

namespace ToSic.Lib.Logging.Simple
{
    public partial class Log
    {

        /// <inheritdoc />
        [Obsolete("Will remove soon - probably v15 as it's probably internal only")]
        public string Add(string message,
            [CallerFilePath] string cPath = null,
            [CallerMemberName] string cName = null,
            [CallerLineNumber] int cLine = 0)
        {
            this.AddInternal(message, new CodeRef(cPath, cName, cLine));
            return message;
        }

        [Obsolete("Will remove soon - probably v14 as it's probably internal only")]
        public void Warn(string message) => this.W(message);

        /// <inheritdoc />
        [Obsolete("Will remove soon - probably v15 as it's probably internal only")]
        public void Add(Func<string> messageMaker,
            [CallerFilePath] string cPath = null,
            [CallerMemberName] string cName = null,
            [CallerLineNumber] int cLine = 0) 
            => this.A(messageMaker, cPath, cName, cLine);

    }
}
