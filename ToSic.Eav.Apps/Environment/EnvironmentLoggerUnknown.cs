using System;
using ToSic.Eav.Internal.Unknown;

namespace ToSic.Eav.Apps.Environment;

internal sealed class EnvironmentLoggerUnknown: IEnvironmentLogger, IIsUnknown
{
    public EnvironmentLoggerUnknown(WarnUseOfUnknown<EnvironmentLoggerUnknown> _) { }

    public void LogException(Exception ex)
    {
        // do nothing
    }
}