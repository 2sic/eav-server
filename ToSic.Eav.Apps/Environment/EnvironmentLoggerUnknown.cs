using System;
using ToSic.Eav.Run;

namespace ToSic.Eav.Apps.Environment
{
    public sealed class EnvironmentLoggerUnknown: IEnvironmentLogger, IIsUnknown
    {
        public void LogException(Exception ex)
        {
            // do nothing
        }
    }
}
