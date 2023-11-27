using ToSic.Lib.Logging;

namespace ToSic.Lib.DI;

public interface ILazyInitLog
{
    void SetLog(ILog parentLog);
}