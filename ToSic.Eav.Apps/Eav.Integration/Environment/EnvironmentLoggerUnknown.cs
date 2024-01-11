using ToSic.Eav.Internal.Unknown;

namespace ToSic.Eav.Integration.Environment;

internal sealed class EnvironmentLoggerUnknown: IEnvironmentLogger, IIsUnknown
{
    public EnvironmentLoggerUnknown(WarnUseOfUnknown<EnvironmentLoggerUnknown> _) { }

    public void LogException(Exception ex)
    {
        // do nothing
    }
}