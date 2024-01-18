namespace ToSic.Eav.Integration.Environment;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public interface IEnvironmentLogger
{
    void LogException(Exception ex);
}