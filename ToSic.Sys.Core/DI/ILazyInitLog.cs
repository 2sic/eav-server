namespace ToSic.Lib.DI;

[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
public interface ILazyInitLog
{
    void SetLog(ILog parentLog);
}