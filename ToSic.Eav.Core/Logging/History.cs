using System;
using ToSic.Eav.Documentation;

namespace ToSic.Eav.Logging
{
    [PrivateApi]
    public static class History
    {
        //public static int Size { get; set; } = 100;
        //public const int MaxCollect = 500;

        //public static int Count => 0;

        //public static ConcurrentDictionary<string, FixedSizedQueue<ILog>> Logs = LogHistory.Logs;

        [Obsolete("Will be removed in 2sxc 13 - just kept temporarily in case external DLLs are using this")]
        public static void Add(string key, ILog log)
        {
            new LogHistory().Add(key, log);
            //// only add if not paused
            //if (Pause) return;

            //// don't keep in journal if it shouldn't be preserved
            //if (!log.Preserve) return;

            //// auto-pause after 1000 logs were run through this, till someone decides to unpause again
            //if (Count++ > MaxCollect) Pause = true;

            //// make sure we have a queue
            //if (!Logs.ContainsKey(key))
            //    Logs.TryAdd(key, new FixedSizedQueue<ILog>(Size));

            //// add the current item
            //if (Logs.TryGetValue(key, out var queue))
            //{
            //    // add if it's already in the queue
            //    if (!queue.ToArray().Contains(log))
            //        queue.Enqueue(log);
            //}
        }
    }


}
