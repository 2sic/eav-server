namespace ToSic.Eav.ImportExport.Integration;

[ShowApiWhenReleased(ShowApiMode.Never)]
public interface IEnvironmentLogger
{
    void LogException(Exception ex);
}