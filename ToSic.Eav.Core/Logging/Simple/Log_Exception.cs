using System;

namespace ToSic.Eav.Logging.Simple
{
    public partial class Log
    {
        public void Exception(Exception ex) => this?.ExceptionInternal(ex);
    }
}
