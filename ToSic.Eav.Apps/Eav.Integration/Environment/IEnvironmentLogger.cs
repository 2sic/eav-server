namespace ToSic.Eav.Integration.Environment;

[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IEnvironmentLogger
{
    void LogException(Exception ex);
}