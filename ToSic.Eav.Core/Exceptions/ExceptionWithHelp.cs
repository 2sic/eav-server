using System;
using ToSic.Eav.Code.Help;

namespace ToSic.Eav
{
    public class ExceptionWithHelp : Exception, IExceptionWithHelp
    {
        public ExceptionWithHelp(CodeHelp help, Exception inner = null) : base(help.ErrorMessage, inner)
        {
            Help = help;
        }

        public CodeHelp Help { get; }

        public ExceptionWithHelp(string message, Exception inner) : base(message, inner)
        { }
    }
}
