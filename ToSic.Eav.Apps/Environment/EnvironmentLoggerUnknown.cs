using System;

namespace ToSic.Eav.Apps.Environment
{
    public sealed class UnknownEnvironmentLogger: IEnvironmentLogger
    {
        public void LogException(Exception ex)
        {
            // do nothing
        }
    }
}
