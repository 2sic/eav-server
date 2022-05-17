using System;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Logging
{
    [PrivateApi]
    public static class History
    {
        [Obsolete("Will be removed in 2sxc 13 - just kept temporarily in case external DLLs are using this")]
        public static void Add(string key, ILog log)
        {
            // TODO: ADD Obsolete WARNING IF THIS IS CALLED
            new LogHistory().Add(key, log);
        }
    }


}
