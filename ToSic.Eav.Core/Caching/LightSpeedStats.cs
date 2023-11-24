using System.Collections.Concurrent;

namespace ToSic.Eav.Caching;

public class LightSpeedStats
{
    public ConcurrentDictionary<int, int> ItemsCount { get; } = new();
    public ConcurrentDictionary<int, long> Size { get; } = new();

    public void Add(int appId, int size)
    {
        ItemsCount.AddOrUpdate(appId, 1, (id, count) => count + 1);
        Size.AddOrUpdate(appId, size, (id, before) => before + size);
    }

    public void Remove(int appId, int size)
    {
        ItemsCount.AddOrUpdate(appId, /*1*/ 0 /* this is probably more correct*/, (id, count) => count - 1);
        Size.AddOrUpdate(appId, 0, (id, before) => before - size);
    }
}