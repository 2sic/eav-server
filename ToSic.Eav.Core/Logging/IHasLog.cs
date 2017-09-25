using ToSic.Eav.Logging.Simple;

namespace ToSic.Eav.Logging
{
    public interface IHasLog
    {
        void LinkLog(Log parentLog);
    }
}