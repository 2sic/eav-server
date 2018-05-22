using ToSic.Eav.Logging.Internals;
using ToSic.Eav.Logging.Simple;

namespace ToSic.Eav.Logging
{
    public class HistorySet
    {
        public HistorySet(int size)
        {
            Log = new FixedSizedQueue<Log>(size);
        }

        public FixedSizedQueue<Log> Log { get; }

        public bool Paused { get; set; } = false;
    }
}
