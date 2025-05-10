namespace ToSic.Lib.DI;

[ShowApiWhenReleased(ShowApiMode.Never)]
public interface ILazyInitLog
{
    void SetLog(ILog? parentLog);
}