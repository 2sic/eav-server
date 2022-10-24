using System;

namespace ToSic.Lib.Logging.Simple
{
    public partial class Log
    {
        public void Exception(Exception ex) => this?.ExceptionInternal(ex);
    }
}
