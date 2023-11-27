using System.Collections.Concurrent;

namespace ToSic.Lib.Logging.Internals;
// code found here https://stackoverflow.com/questions/5852863/fixed-size-queue-which-automatically-dequeues-old-values-upon-new-enques

public class FixedSizedQueue<T> : ConcurrentQueue<T>
{

    public int Size { get; }

    public FixedSizedQueue(int size)
    {
        Size = size;
    }

    //private readonly object _syncObject = new object();
    public new void Enqueue(T obj)
    {
        base.Enqueue(obj);
        // 2018-03 removed lock statement because it's recommended against here https://stackoverflow.com/questions/5852863/fixed-size-queue-which-automatically-dequeues-old-values-upon-new-enques
        //lock (_syncObject)
        //{
        while (Count > Size)
            TryDequeue(out var _);

        //}
    }
}