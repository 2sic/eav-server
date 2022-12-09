using ToSic.Lib.Logging;

namespace ToSic.Eav.DI
{
    public interface ILazyInitLog
    {
        void SetLog(ILog parentLog);
    }
}
