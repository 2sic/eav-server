using System.Collections.Concurrent;

namespace ToSic.Eav.Caching
{
    public class LightSpeedStats
    {
        public ConcurrentDictionary<int, int> ItemsCount { get; } = new ConcurrentDictionary<int, int>();
    }
}
