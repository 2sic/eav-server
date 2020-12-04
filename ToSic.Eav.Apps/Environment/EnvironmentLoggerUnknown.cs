using System;

namespace ToSic.Eav.Apps.Environment
{
    public sealed class EnvironmentLoggerUnknown: IEnvironmentLogger
    {
        public void LogException(Exception ex)
        {
            // do nothing
        }
    }
}
