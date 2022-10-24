using System.Collections.Concurrent;
using System.Linq;
using ToSic.Lib.Logging.Internals;

namespace ToSic.Lib.Logging
{
    /// <summary>
    /// DI Version of the history object to ensure that things are added to the global log-history
    /// </summary>
    // Todo
    // let run for a while, and ca. 2sxc 13 rename back to "History" once we drop that global/static object
    public class LogHistory
    {
        public const string WarningsPrefix = "warnings-";

        // Note: this is treated like a constant for now...
        public readonly int MaxCollect = 500;

        public int Size
        {
            get => _size; 
            set => _size = value;
        } 
        private static int _size = 100;

        public ConcurrentDictionary<string, FixedSizedQueue<ILog>> Logs  => _logs;
        private static readonly ConcurrentDictionary<string, FixedSizedQueue<ILog>> _logs = new ConcurrentDictionary<string, FixedSizedQueue<ILog>>();

        #region Pause

        public bool Pause
        {
            get => _pause;
            set
            {
                _pause = value;
                Count = 0;
            }
        }
        private static bool _pause;

        #endregion

        public int Count { get; private set; }

        #region Add

        public void Add(string key, ILog log) => AddInternal(key, log, false);

        public void ForceAdd(string key, ILog log) => AddInternal(key, log, true);

        private void AddInternal(string key, ILog log, bool force)
        {
            // Check exit clauses if not forced
            if (!force)
            {
                // only add if not paused
                if (Pause) return;

                // don't keep in journal if it shouldn't be preserved
                if (!log.Preserve) return;
            }

            // auto-pause after 1000 logs were run through this, till someone decides to unpause again
            if (Count++ > MaxCollect) Pause = true;

            // make sure we have a queue
            if (!Logs.ContainsKey(key))
                Logs.TryAdd(key, new FixedSizedQueue<ILog>(Size));

            // add the current item if it's not already in the queue
            if (Logs.TryGetValue(key, out var queue) && !queue.ToArray().Contains(log)) 
                queue.Enqueue(log);
        }


        #endregion

        public void Flush(string key)
        {
            if (Logs.ContainsKey(key))
                Logs.TryRemove(key, out var _);
        }
    }
}
