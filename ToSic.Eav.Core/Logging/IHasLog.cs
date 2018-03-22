using ToSic.Eav.Logging.Simple;

namespace ToSic.Eav.Logging
{
    public interface IHasLog
    {
        Log Log { get; }
        void LinkLog(Log parentLog);
    }
}