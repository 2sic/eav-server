using System;

namespace ToSic.Eav.Logging
{
    public partial interface ILog
    {
        //void Ex(Exception ex);

        [Obsolete("Will remove soon")]
        void Exception(Exception ex);
    }
}
