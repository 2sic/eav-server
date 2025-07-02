namespace ToSic.Sys.DI;

[ShowApiWhenReleased(ShowApiMode.Never)]
public interface ILazyInitLog
{
    void SetLog(ILog? parentLog);
}