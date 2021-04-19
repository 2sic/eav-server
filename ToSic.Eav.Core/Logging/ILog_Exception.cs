using System;

namespace ToSic.Eav.Logging
{
    public partial interface ILog
    {
        void Exception(Exception ex);
    }
}
