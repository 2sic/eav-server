using System;
using ToSic.Eav.Internal.Unknown;
using ToSic.Eav.Run;

namespace ToSic.Eav.Apps.Environment;

public sealed class EnvironmentLoggerUnknown: IEnvironmentLogger, IIsUnknown
{
    public EnvironmentLoggerUnknown(WarnUseOfUnknown<EnvironmentLoggerUnknown> _) { }

    public void LogException(Exception ex)
    {
        // do nothing
    }
}