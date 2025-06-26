namespace ToSic.Eav.ImportExport.Integration;

internal sealed class EnvironmentLoggerUnknown: IEnvironmentLogger, IIsUnknown
{
    public EnvironmentLoggerUnknown(WarnUseOfUnknown<EnvironmentLoggerUnknown> _) { }

    public void LogException(Exception ex)
    {
        // do nothing
    }
}