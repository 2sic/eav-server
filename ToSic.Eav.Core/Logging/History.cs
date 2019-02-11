using System.Collections.Concurrent;
using System.Linq;
using ToSic.Eav.Logging.Internals;
using ToSic.Eav.Logging.Simple;

namespace ToSic.Eav.Logging
{
    public static class History
    {
        public static int Size { get; set; } = 25;
        public const int MaxCollect = 250;

        public static bool Pause
        {
            get => _pause;
            set
            {
                _pause = value;
                Count = 0;
            } 
        }
        private static bool _pause;
        public static int Count { get; private set; }

        public static ConcurrentDictionary<string, FixedSizedQueue<Log>> Logs = new ConcurrentDictionary<string, FixedSizedQueue<Log>>();

        public static void Add(string key, Log log)
        {
            // only add if not paused
            if (Pause) return;

            // auto-pause after 1000 logs were run through this, till someone decides to unpause again
            if (Count++ > MaxCollect) Pause = true;

            // make sure we have a queue
            if (!Logs.ContainsKey(key))
                Logs.TryAdd(key, new FixedSizedQueue<Log>(Size));

            // add the current item
            if (Logs.TryGetValue(key, out var queue))
            {
                // add if it's already in the queue
                if (!queue.ToArray().Contains(log))
                    queue.Enqueue(log);
            }
        }



        public static void Flush(string key)
        {
            if (Logs.ContainsKey(key))
                Logs.TryRemove(key, out var _);
        }
    }


}
